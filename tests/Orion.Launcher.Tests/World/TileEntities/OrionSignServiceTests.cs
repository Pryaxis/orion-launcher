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
using Orion.Core.Packets;
using Orion.Core.Packets.World.TileEntities;
using Orion.Core.Players;
using Serilog;
using Xunit;

namespace Orion.Launcher.World.TileEntities
{
    // These tests depend on Terraria state.
    [Collection("TerrariaTestsCollection")]
    public class OrionSignServiceTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(10000)]
        public void Item_GetInvalidIndex_ThrowsIndexOutOfRangeException(int index)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var signService = new OrionSignService(events, log);

            Assert.Throws<IndexOutOfRangeException>(() => signService[index]);
        }

        [Fact]
        public void Item_Get()
        {
            Terraria.Main.sign[1] = new Terraria.Sign();

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var signService = new OrionSignService(events, log);

            var sign = signService[1];

            Assert.Equal(1, sign.Index);
            Assert.Same(Terraria.Main.sign[1], ((OrionSign)sign).Wrapped);
        }

        [Fact]
        public void Item_GetMultipleTimes_ReturnsSameInstance()
        {
            Terraria.Main.sign[0] = new Terraria.Sign();

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var signService = new OrionSignService(events, log);

            var sign = signService[0];
            var sign2 = signService[0];

            Assert.Same(sign, sign2);
        }

        [Fact]
        public void Count_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var signService = new OrionSignService(events, log);

            Assert.Equal(Terraria.Sign.maxSigns, signService.Count);
        }

        [Fact]
        public void GetEnumerator()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var signService = new OrionSignService(events, log);

            var signs = signService.ToList();

            for (var i = 0; i < signs.Count; ++i)
            {
                Assert.Same(Terraria.Main.sign[i], ((OrionSign)signs[i]).Wrapped);
            }
        }

        [Fact]
        public void PacketReceive_SignReadPacket_EventTriggered()
        {
            Terraria.Main.sign[0] = new Terraria.Sign { x = 256, y = 100, text = "test" };

            var packet = new SignRead { X = 256, Y = 100 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<SignRead, SignReadEvent>(packet, sender,
                evt => ((OrionSign)evt.Sign).Wrapped == Terraria.Main.sign[0] && evt.Player == sender);
        }

        [Fact]
        public void PacketReceive_SignReadPacket_EventCanceled()
        {
            Terraria.Main.sign[0] = new Terraria.Sign { x = 256, y = 100, text = "test" };

            var packet = new SignRead { X = 256, Y = 100 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<SignRead, SignReadEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_SignReadPacket_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            var packet = new SignRead { X = 256, Y = 100 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventNotTriggered<SignRead, SignReadEvent>(packet, sender);
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

            using var signService = new OrionSignService(events, log);

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

            using var signService = new OrionSignService(events, log);

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

            using var signService = new OrionSignService(events, log);

            var evt = new PacketReceiveEvent<TPacket>(packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<TEvent>(), log), Times.Never);
        }
    }
}
