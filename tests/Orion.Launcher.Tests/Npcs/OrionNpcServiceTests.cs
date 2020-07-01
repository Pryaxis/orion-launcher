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
using System.Linq;
using System.Linq.Expressions;
using Moq;
using Orion.Core.Entities;
using Orion.Core.Events;
using Orion.Core.Events.Npcs;
using Orion.Core.Events.Packets;
using Orion.Core.Items;
using Orion.Core.Npcs;
using Orion.Core.Packets;
using Orion.Core.Packets.Npcs;
using Orion.Core.Players;
using Orion.Core.Utils;
using Serilog;
using Xunit;

namespace Orion.Launcher.Npcs
{
    // These tests depend on Terraria state.
    [Collection("TerrariaTestsCollection")]
    public class OrionNpcServiceTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(10000)]
        public void Item_GetIndexOutOfRange_ThrowsIndexOutOfRangeException(int index)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Assert.Throws<IndexOutOfRangeException>(() => npcService[index]);
        }

        [Fact]
        public void Item_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            var npc = npcService[1];

            Assert.Equal(1, npc.Index);
            Assert.Same(Terraria.Main.npc[1], ((OrionNpc)npc).Wrapped);
        }

        [Fact]
        public void Item_GetMultipleTimes_ReturnsSameInstance()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            var npc = npcService[0];
            var npc2 = npcService[0];

            Assert.Same(npc, npc2);
        }

        [Fact]
        public void GetEnumerator()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            var npcs = npcService.ToList();

            for (var i = 0; i < npcs.Count; ++i)
            {
                Assert.Same(Terraria.Main.npc[i], ((OrionNpc)npcs[i]).Wrapped);
            }
        }

        [Theory]
        [InlineData(NpcId.BlueSlime)]
        [InlineData(NpcId.GreenSlime)]
        public void NpcSetDefaults_EventTriggered(NpcId id)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<NpcDefaultsEvent>(evt => ((OrionNpc)evt.Npc).Wrapped == Terraria.Main.npc[0] && evt.Id == id),
                    log));

            Terraria.Main.npc[0].SetDefaults((int)id);

            Assert.Equal(id, (NpcId)Terraria.Main.npc[0].netID);

            Mock.Get(events).VerifyAll();
        }

        [Theory]
        [InlineData(NpcId.BlueSlime, NpcId.GreenSlime)]
        [InlineData(NpcId.BlueSlime, NpcId.None)]
        public void NpcSetDefaults_EventModified(NpcId oldId, NpcId newId)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<NpcDefaultsEvent>(), log))
                .Callback<NpcDefaultsEvent, ILogger>((evt, log) => evt.Id = newId);

            Terraria.Main.npc[0].SetDefaults((int)oldId);

            Assert.Equal(newId, (NpcId)Terraria.Main.npc[0].netID);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void NpcSetDefaults_EventCanceled()
        {
            // Clear the NPC so that we know it's empty.
            Terraria.Main.npc[0] = new Terraria.NPC { whoAmI = 0 };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<NpcDefaultsEvent>(), log))
                .Callback<NpcDefaultsEvent, ILogger>((evt, log) => evt.Cancel());

            Terraria.Main.npc[0].SetDefaults((int)NpcId.BlueSlime);

            Assert.Equal(NpcId.None, (NpcId)Terraria.Main.npc[0].netID);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void NpcSpawn_EventTriggered()
        {
            INpc? evtNpc = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<NpcSpawnEvent>(), log))
                .Callback<NpcSpawnEvent, ILogger>((evt, log) => evtNpc = evt.Npc);

            var npcIndex = Terraria.NPC.NewNPC(0, 0, (int)NpcId.BlueSlime);

            Assert.NotNull(evtNpc);
            Assert.Same(Terraria.Main.npc[npcIndex], ((OrionNpc)evtNpc!).Wrapped);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void NpcSpawn_EventCanceled()
        {
            // Clear the NPC so that we know it's empty.
            Terraria.Main.npc[0] = new Terraria.NPC { whoAmI = 0 };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<NpcSpawnEvent>(), log))
                .Callback<NpcSpawnEvent, ILogger>((evt, log) => evt.Cancel());

            var npcIndex = Terraria.NPC.NewNPC(0, 0, (int)NpcId.BlueSlime);

            Assert.Equal(npcService.Count, npcIndex);
            Assert.False(Terraria.Main.npc[0].active);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void NpcTick_EventTriggered()
        {
            // Clear the NPC so that we know it's empty.
            Terraria.Main.npc[0] = new Terraria.NPC { whoAmI = 0 };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<NpcTickEvent>(evt => ((OrionNpc)evt.Npc).Wrapped == Terraria.Main.npc[0]),
                    log));

            Terraria.Main.npc[0].UpdateNPC(0);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void NpcTick_EventCanceled()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<NpcTickEvent>(), log))
                .Callback<NpcTickEvent, ILogger>((evt, log) => evt.Cancel());

            Terraria.Main.npc[0].UpdateNPC(0);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void NpcKilled_EventTriggered()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<NpcKilledEvent>(evt => ((OrionNpc)evt.Npc).Wrapped == Terraria.Main.npc[0]),
                    log));

            Terraria.Main.npc[0].SetDefaults((int)NpcId.BlueSlime);
            Terraria.Main.npc[0].life = 0;

            Terraria.Main.npc[0].checkDead();

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void NpcLoot_EventTriggered()
        {
            // Clear the item so that we know it's empty.
            Terraria.Main.item[0] = new Terraria.Item { whoAmI = 0 };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<NpcLootEvent>(
                        evt => ((OrionNpc)evt.Npc).Wrapped == Terraria.Main.npc[0] && evt.Item.Id == ItemId.Gel &&
                            (evt.Item.StackSize >= 1 || evt.Item.StackSize <= 2) &&
                            evt.Item.Prefix == ItemPrefix.Random),
                    log));

            Terraria.Main.npc[0].SetDefaults((int)NpcId.BlueSlime);
            Terraria.Main.npc[0].life = 0;

            Terraria.Main.npc[0].checkDead();

            Assert.Equal(ItemId.Gel, (ItemId)Terraria.Main.item[0].type);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void NpcLoot_EventModified()
        {
            // Clear the item so that we know it's empty.
            Terraria.Main.item[0] = new Terraria.Item { whoAmI = 0 };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<NpcLootEvent>(), log))
                .Callback<NpcLootEvent, ILogger>((evt, log) =>
                {
                    evt.Item = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);
                });

            Terraria.Main.npc[0].SetDefaults((int)NpcId.BlueSlime);
            Terraria.Main.npc[0].life = 0;

            Terraria.Main.npc[0].checkDead();

            Assert.Equal(ItemId.Sdmg, (ItemId)Terraria.Main.item[0].type);
            Assert.Equal(1, Terraria.Main.item[0].stack);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)Terraria.Main.item[0].prefix);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void NpcLoot_EventCanceled()
        {
            // Clear the item so that we know it's empty.
            Terraria.Main.item[0] = new Terraria.Item { whoAmI = 0 };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<NpcLootEvent>(), log))
                .Callback<NpcLootEvent, ILogger>((evt, log) => evt.Cancel());

            Terraria.Main.npc[0].SetDefaults((int)NpcId.BlueSlime);
            Terraria.Main.npc[0].life = 0;

            Terraria.Main.npc[0].checkDead();

            Assert.NotEqual(ItemId.Gel, (ItemId)Terraria.Main.item[0].type);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_NpcAddBuff_EventTriggered()
        {
            var packet = new NpcAddBuff { NpcIndex = 1, Id = BuffId.Poisoned, Ticks = 60 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<NpcAddBuff, NpcAddBuffEvent>(packet, sender,
                evt => ((OrionNpc)evt.Npc).Wrapped == Terraria.Main.npc[1] && evt.Player == sender &&
                    evt.Buff == new Buff(BuffId.Poisoned, 60));
        }

        [Fact]
        public void PacketReceive_NpcBuffPacket_EventCanceled()
        {
            var packet = new NpcAddBuff { NpcIndex = 1, Id = BuffId.Poisoned, Ticks = 60 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<NpcAddBuff, NpcAddBuffEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_NpcCatchPacket_EventTriggered()
        {
            var packet = new NpcCatch { NpcIndex = 1 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<NpcCatch, NpcCatchEvent>(packet, sender,
                evt => ((OrionNpc)evt.Npc).Wrapped == Terraria.Main.npc[1]);
        }

        [Fact]
        public void PacketReceive_NpcCatchPacket_EventCanceled()
        {
            var packet = new NpcCatch { NpcIndex = 1 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<NpcCatch, NpcCatchEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_NpcFishPacket_EventTriggered()
        {
            var packet = new NpcFish { X = 100, Y = 256, Id = NpcId.HemogoblinShark };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<NpcFish, NpcFishEvent>(packet, sender,
                evt => evt.Player == sender && evt.Position == new Vector2f(1600, 4096) &&
                    evt.Id == NpcId.HemogoblinShark);
        }

        [Fact]
        public void PacketReceive_NpcFishPacket_EventCanceled()
        {
            var packet = new NpcFish { X = 100, Y = 256, Id = NpcId.HemogoblinShark };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<NpcFish, NpcFishEvent>(packet, sender);
        }

        [Fact]
        public void Spawn()
        {
            // Clear the NPC so that we know it's empty.
            Terraria.Main.npc[0] = new Terraria.NPC { whoAmI = 0 };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            var npc = npcService.Spawn(NpcId.BlueSlime, Vector2f.Zero);

            Assert.NotNull(npc);
            Assert.Equal(NpcId.BlueSlime, npc!.Id);
        }

        [Fact]
        public void Spawn_ReturnsNull()
        {
            // Fill up all of the NPC slots so that the spawn fails.
            for (var i = 0; i < Terraria.Main.maxNPCs; ++i)
            {
                Terraria.Main.npc[i] = new Terraria.NPC { whoAmI = i, active = true };
            }

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var npcService = new OrionNpcService(events, log);

            var npc = npcService.Spawn(NpcId.BlueSlime, Vector2f.Zero);

            Assert.Null(npc);
        }

        private void PacketReceive_EventTriggered<TPacket, TEvent>(
            TPacket packet, IPlayer sender, Expression<Func<TEvent, bool>> match)
            where TPacket : IPacket
            where TEvent : Event
        {
            Action<PacketReceiveEvent<TPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var npcService = new OrionNpcService(events, log);

            var evt = new PacketReceiveEvent<TPacket>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.Is(match), log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        private void PacketReceive_EventCanceled<TPacket, TEvent>(TPacket packet, IPlayer sender)
            where TPacket : IPacket
            where TEvent : Event
        {
            Action<PacketReceiveEvent<TPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var npcService = new OrionNpcService(events, log);

            var evt = new PacketReceiveEvent<TPacket>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<TEvent>(), log))
                .Callback<TEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }
    }
}
