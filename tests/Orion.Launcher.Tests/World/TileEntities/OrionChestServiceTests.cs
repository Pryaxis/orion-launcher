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
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.World.TileEntities;
using Orion.Core.Items;
using Orion.Core.Packets;
using Orion.Core.Packets.World.TileEntities;
using Orion.Core.Players;
using Serilog;
using Xunit;

namespace Orion.Launcher.World.TileEntities
{
    // These tests depend on Terraria state.
    [Collection("TerrariaTestsCollection")]
    public class OrionChestServiceTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(10000)]
        public void Item_GetInvalidIndex_ThrowsIndexOutOfRangeException(int index)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var chestService = new OrionChestService(events, log);

            Assert.Throws<IndexOutOfRangeException>(() => chestService[index]);
        }

        [Fact]
        public void Item_Get()
        {
            Terraria.Main.chest[1] = new Terraria.Chest();

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var chestService = new OrionChestService(events, log);

            var chest = chestService[1];

            Assert.Equal(1, chest.Index);
            Assert.Same(Terraria.Main.chest[1], ((OrionChest)chest).Wrapped);
        }

        [Fact]
        public void Item_GetMultipleTimes_ReturnsSameInstance()
        {
            Terraria.Main.chest[0] = new Terraria.Chest();

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var chestService = new OrionChestService(events, log);

            var chest = chestService[0];
            var chest2 = chestService[0];

            Assert.Same(chest, chest2);
        }

        [Fact]
        public void Count_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var chestService = new OrionChestService(events, log);

            Assert.Equal(Terraria.Main.maxChests, chestService.Count);
        }

        [Fact]
        public void GetEnumerator()
        {
            for (var i = 0; i < Terraria.Main.maxChests; ++i)
            {
                Terraria.Main.chest[i] = new Terraria.Chest();
            }

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var chestService = new OrionChestService(events, log);

            var chests = chestService.ToList();

            for (var i = 0; i < chests.Count; ++i)
            {
                Assert.Same(Terraria.Main.chest[i], ((OrionChest)chests[i]).Wrapped);
            }
        }

        [Fact]
        public void PacketReceive_ChestOpen_EventTriggered()
        {
            Terraria.Main.chest[0] = new Terraria.Chest { x = 256, y = 100, name = "test" };

            var packet = new ChestOpen { X = 256, Y = 100 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<ChestOpen, ChestOpenEvent>(packet, sender,
                evt => evt.Player == sender && ((OrionChest)evt.Chest).Wrapped == Terraria.Main.chest[0]);
        }

        [Fact]
        public void PacketReceive_ChestOpen_EventCanceled()
        {
            Terraria.Main.chest[0] = new Terraria.Chest { x = 256, y = 100, name = "test" };

            var packet = new ChestOpen { X = 256, Y = 100 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<ChestOpen, ChestOpenEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_ChestOpen_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Main.maxChests; ++i)
            {
                Terraria.Main.chest[i] = new Terraria.Chest();
            }

            var packet = new ChestOpen { X = 256, Y = 100 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventNotTriggered<ChestOpen, ChestOpenEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_ChestInventory_EventTriggered()
        {
            Terraria.Main.chest[5] = new Terraria.Chest();

            var packet = new ChestInventory
            {
                ChestIndex = 5,
                Id = ItemId.Sdmg,
                StackSize = 1,
                Prefix = ItemPrefix.Unreal
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<ChestInventory, ChestInventoryEvent>(packet, sender,
                evt => evt.Player == sender && ((OrionChest)evt.Chest).Wrapped == Terraria.Main.chest[5] &&
                    evt.Item == new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1));
        }

        [Fact]
        public void PacketReceive_ChestInventory_EventCanceled()
        {
            var packet = new ChestInventory
            {
                ChestIndex = 5,
                Id = ItemId.Sdmg,
                StackSize = 1,
                Prefix = ItemPrefix.Unreal
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<ChestInventory, ChestInventoryEvent>(packet, sender);
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

            using var chestService = new OrionChestService(events, log);

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

            using var chestService = new OrionChestService(events, log);

            var evt = new PacketReceiveEvent<TPacket>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<TEvent>(), log))
                .Callback<TEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        private void PacketReceive_EventNotTriggered<TPacket, TEvent>(TPacket packet, IPlayer sender)
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

            using var chestService = new OrionChestService(events, log);

            var evt = new PacketReceiveEvent<TPacket>(packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<TEvent>(), log), Times.Never);
        }
    }
}
