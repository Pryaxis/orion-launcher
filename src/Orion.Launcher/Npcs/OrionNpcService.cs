// Copyright (c) 2020 Pryaxis & Orion Contributors
// 
// This file is part of Orion.
// 
// Orion is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Orion is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Orion.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Orion.Core.Buffs;
using Orion.Core.DataStructures;
using Orion.Core.Events;
using Orion.Core.Events.Npcs;
using Orion.Core.Events.Packets;
using Orion.Core.Framework;
using Orion.Core.Items;
using Orion.Core.Npcs;
using Orion.Core.Packets.Npcs;
using Orion.Launcher.Collections;
using Serilog;

namespace Orion.Launcher.Npcs
{
    [Binding("orion-npcs", Author = "Pryaxis", Priority = BindingPriority.Lowest)]
    internal sealed class OrionNpcService : INpcService, IDisposable
    {
        private readonly IEventManager _events;
        private readonly ILogger _log;
        private readonly IReadOnlyList<INpc> _npcs;

        private readonly object _lock = new object();
        private readonly ThreadLocal<int> _setDefaultsToIgnore = new ThreadLocal<int>();

        public OrionNpcService(IEventManager events, ILogger log)
        {
            Debug.Assert(events != null);
            Debug.Assert(log != null);

            _events = events;
            _log = log;

            // Note that the last NPC should be ignored, as it is not a real NPC.
            _npcs = new WrappedReadOnlyList<OrionNpc, Terraria.NPC>(
                Terraria.Main.npc.AsMemory(..^1),
                (npcIndex, terrariaNpc) => new OrionNpc(npcIndex, terrariaNpc));

            OTAPI.Hooks.Npc.PreSetDefaultsById = PreSetDefaultsByIdHandler;
            OTAPI.Hooks.Npc.Spawn = SpawnHandler;
            OTAPI.Hooks.Npc.PreUpdate = PreUpdateHandler;
            OTAPI.Hooks.Npc.Killed = KilledHandler;
            OTAPI.Hooks.Npc.PreDropLoot = PreDropLootHandler;

            _events.RegisterHandlers(this, _log);
        }

        public INpc this[int index] => _npcs[index];

        public int Count => _npcs.Count;

        public IEnumerator<INpc> GetEnumerator() => _npcs.GetEnumerator();

        public INpc? Spawn(NpcId id, Vector2f position)
        {
            // Not localized because this string is developer-facing.
            Log.Debug("Spawning {NpcId} at {Position}", id, position);

            lock (_lock)
            {
                var npcIndex = Terraria.NPC.NewNPC((int)position.X, (int)position.Y, (int)id);
                return npcIndex >= 0 && npcIndex < Count ? this[npcIndex] : null;
            }
        }

        public void Dispose()
        {
            _setDefaultsToIgnore.Dispose();

            OTAPI.Hooks.Npc.PreSetDefaultsById = null;
            OTAPI.Hooks.Npc.Spawn = null;
            OTAPI.Hooks.Npc.PreUpdate = null;
            OTAPI.Hooks.Npc.Killed = null;
            OTAPI.Hooks.Npc.PreDropLoot = null;

            _events.DeregisterHandlers(this, _log);
        }

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // =============================================================================================================
        // OTAPI hooks
        //

        private OTAPI.HookResult PreSetDefaultsByIdHandler(
            Terraria.NPC terrariaNpc, ref int npcId, ref Terraria.NPCSpawnParams spawnParams)
        {
            Debug.Assert(terrariaNpc != null);

            // Check `_setDefaultsToIgnore` to ignore spurious calls if `SetDefaultsById` is called with a negative ID.
            if (_setDefaultsToIgnore.Value > 0)
            {
                --_setDefaultsToIgnore.Value;
                return OTAPI.HookResult.Continue;
            }

            var npc = GetNpc(terrariaNpc);
            var evt = new NpcDefaultsEvent(npc) { Id = (NpcId)npcId };
            _events.Raise(evt, _log);
            if (evt.IsCanceled)
            {
                return OTAPI.HookResult.Cancel;
            }

            npcId = (int)evt.Id;
            if (npcId < 0)
            {
                _setDefaultsToIgnore.Value = 2;
            }
            return OTAPI.HookResult.Continue;
        }

        private OTAPI.HookResult SpawnHandler(ref int npcIndex)
        {
            Debug.Assert(npcIndex >= 0 && npcIndex < Count);

            var npc = this[npcIndex];
            var evt = new NpcSpawnEvent(npc);
            _events.Raise(evt, _log);
            if (evt.IsCanceled)
            {
                // To cancel the event, remove the NPC and return the failure index.
                npc.IsActive = false;
                npcIndex = Count;
                return OTAPI.HookResult.Cancel;
            }

            return OTAPI.HookResult.Continue;
        }

        private OTAPI.HookResult PreUpdateHandler(Terraria.NPC terrariaNpc, ref int npcIndex)
        {
            Debug.Assert(npcIndex >= 0 && npcIndex < Count);

            var evt = new NpcTickEvent(this[npcIndex]);
            _events.Raise(evt, _log);
            return evt.IsCanceled ? OTAPI.HookResult.Cancel : OTAPI.HookResult.Continue;
        }

        private void KilledHandler(Terraria.NPC terrariaNpc)
        {
            Debug.Assert(terrariaNpc != null);

            var npc = GetNpc(terrariaNpc);
            var evt = new NpcKilledEvent(npc);
            _events.Raise(evt, _log);
        }

        private OTAPI.HookResult PreDropLootHandler(
            Terraria.NPC terrariaNpc, ref int itemIndex, ref int x, ref int y, ref int width, ref int height,
            ref int itemId, ref int stackSize, ref bool noBroadcast, ref int prefix, ref bool noGrabDelay,
            ref bool reverseIndex)
        {
            Debug.Assert(terrariaNpc != null);

            var npc = GetNpc(terrariaNpc);
            var evt = new NpcLootEvent(npc) { ItemStack = new ItemStack((ItemId)itemId, stackSize, (ItemPrefix)prefix) };
            _events.Raise(evt, _log);
            if (evt.IsCanceled)
            {
                return OTAPI.HookResult.Cancel;
            }

            itemId = (int)evt.ItemStack.Id;
            stackSize = evt.ItemStack.StackSize;
            prefix = (int)evt.ItemStack.Prefix;
            return OTAPI.HookResult.Continue;
        }

        // Gets an `INpc` which corresponds to the given Terraria NPC. Retrieves the `INpc` from the `Npcs` array, if
        // possible.
        private INpc GetNpc(Terraria.NPC terrariaNpc)
        {
            var npcIndex = terrariaNpc.whoAmI;
            Debug.Assert(npcIndex >= 0 && npcIndex < Count);

            var isConcrete = terrariaNpc == Terraria.Main.npc[npcIndex];
            return isConcrete ? this[npcIndex] : new OrionNpc(terrariaNpc);
        }

        // =============================================================================================================
        // NPC event publishers
        //

        [EventHandler("orion-npcs", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnNpcBuffPacket(PacketReceiveEvent<NpcBuffPacket> evt)
        {
            ref var packet = ref evt.Packet;
            var buff = new Buff(packet.Id, packet.Ticks);

            _events.Forward(evt, new NpcBuffEvent(this[packet.NpcIndex], evt.Sender, buff), _log);
        }

        [EventHandler("orion-npcs", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnNpcCatchPacket(PacketReceiveEvent<NpcCatchPacket> evt)
        {
            ref var packet = ref evt.Packet;

            _events.Forward(evt, new NpcCatchEvent(this[packet.NpcIndex], evt.Sender), _log);
        }

        [EventHandler("orion-npcs", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnNpcFishPacket(PacketReceiveEvent<NpcFishPacket> evt)
        {
            ref var packet = ref evt.Packet;

            _events.Forward(evt, new NpcFishEvent(evt.Sender, packet.X, packet.Y, packet.Id), _log);
        }
    }
}
