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
using Moq;
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.World.Chests;
using Orion.Core.Items;
using Orion.Core.Packets.World.Chests;
using Orion.Core.Players;
using Serilog;
using Xunit;

namespace Orion.Launcher.World.Chests
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

            Action<PacketReceiveEvent<ChestOpenPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ChestOpenPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<ChestOpenPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var chestService = new OrionChestService(events, log);

            var packet = new ChestOpenPacket { X = 256, Y = 100 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ChestOpenPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<ChestOpenEvent>(evt => evt.Player == sender && evt.Chest == chestService[0]), log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_ChestOpen_EventCanceled()
        {
            Terraria.Main.chest[0] = new Terraria.Chest { x = 256, y = 100, name = "test" };

            Action<PacketReceiveEvent<ChestOpenPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ChestOpenPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<ChestOpenPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var chestService = new OrionChestService(events, log);

            var packet = new ChestOpenPacket { X = 256, Y = 100 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ChestOpenPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<ChestOpenEvent>(), log))
                .Callback<ChestOpenEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_ChestOpen_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            Action<PacketReceiveEvent<ChestOpenPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ChestOpenPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<ChestOpenPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var chestService = new OrionChestService(events, log);

            var packet = new ChestOpenPacket { X = 256, Y = 100 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ChestOpenPacket>(ref packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<ChestOpenEvent>(), log), Times.Never);
        }

        [Fact]
        public void PacketReceive_ChestInventory_EventTriggered()
        {
            Terraria.Main.chest[5] = new Terraria.Chest();

            Action<PacketReceiveEvent<ChestInventoryPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ChestInventoryPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<ChestInventoryPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var chestService = new OrionChestService(events, log);

            var packet = new ChestInventoryPacket
            {
                ChestIndex = 5,
                Id = ItemId.Sdmg,
                StackSize = 1,
                Prefix = ItemPrefix.Unreal
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ChestInventoryPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<ChestInventoryEvent>(
                        evt => evt.Player == sender && evt.Chest == chestService[5] &&
                            evt.ItemStack == new ItemStack(ItemId.Sdmg, 1, ItemPrefix.Unreal)),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_ChestInventory_EventCanceled()
        {
            Action<PacketReceiveEvent<ChestInventoryPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ChestInventoryPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<ChestInventoryPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var chestService = new OrionChestService(events, log);

            var packet = new ChestInventoryPacket
            {
                ChestIndex = 5,
                Id = ItemId.Sdmg,
                StackSize = 1,
                Prefix = ItemPrefix.Unreal
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ChestInventoryPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<ChestInventoryEvent>(), log))
                .Callback<ChestInventoryEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }
    }
}
