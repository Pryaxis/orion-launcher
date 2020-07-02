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
using System.Linq;
using Orion.Core;
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.World.Chests;
using Orion.Core.Items;
using Orion.Core.Packets.World.Chests;
using Orion.Core.World.Chests;
using Orion.Launcher.Utils;
using Serilog;

namespace Orion.Launcher.World.Chests
{
    [Binding("orion-chests", Author = "Pryaxis", Priority = BindingPriority.Lowest)]
    internal sealed class OrionChestService : IChestService, IDisposable
    {
        private readonly IEventManager _events;
        private readonly ILogger _log;
        private readonly IReadOnlyList<IChest> _chests;

        public OrionChestService(IEventManager events, ILogger log)
        {
            Debug.Assert(events != null);
            Debug.Assert(log != null);

            _events = events;
            _log = log;

            _chests = new WrappedReadOnlyList<OrionChest, Terraria.Chest?>(
                Terraria.Main.chest, (chestIndex, terrariaChest) => new OrionChest(chestIndex, terrariaChest));

            _events.RegisterHandlers(this, _log);
        }

        public IChest this[int index] => _chests[index];

        public int Count => _chests.Count;

        public IEnumerator<IChest> GetEnumerator() => _chests.GetEnumerator();

        public void Dispose()
        {
            _events.DeregisterHandlers(this, _log);
        }

        private IChest? FindChest(int x, int y) => this.FirstOrDefault(s => s.IsActive && s.X == x && s.Y == y);

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // =============================================================================================================
        // Chest event publishers
        //

        [EventHandler("orion-chests", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnChestOpen(PacketReceiveEvent<ChestOpen> evt)
        {
            var packet = evt.Packet;
            var chest = FindChest(packet.X, packet.Y);
            if (chest is null)
            {
                return;
            }

            _events.Forward(evt, new ChestOpenEvent(chest, evt.Sender), _log);
        }

        [EventHandler("orion-chests", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnChestInventory(PacketReceiveEvent<ChestInventory> evt)
        {
            var packet = evt.Packet;
            var item = new ItemStack(packet.Id, packet.Prefix, packet.StackSize);

            _events.Forward(evt, new ChestInventoryEvent(this[packet.ChestIndex], evt.Sender, packet.Slot, item), _log);
        }
    }
}
