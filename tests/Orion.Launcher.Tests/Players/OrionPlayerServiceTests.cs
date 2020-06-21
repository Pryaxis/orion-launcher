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
using System.IO;
using System.Linq;
using Moq;
using Orion.Core.DataStructures;
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.Players;
using Orion.Core.Packets;
using Orion.Core.Packets.Client;
using Orion.Core.Packets.Modules;
using Orion.Core.Packets.Players;
using Orion.Core.Players;
using Serilog;
using Xunit;

namespace Orion.Launcher.Players
{
    // These tests depend on Terraria state.
    [Collection("TerrariaTestsCollection")]
    public class OrionPlayerServiceTests
    {
        private static readonly byte[] _serverConnectPacketBytes;

        static OrionPlayerServiceTests()
        {
            var bytes = new byte[100];
            var packet = new ClientConnectPacket { Version = "Terraria" + Terraria.Main.curRelease };
            var packetLength = packet.WriteWithHeader(bytes, PacketContext.Client);

            _serverConnectPacketBytes = bytes[..packetLength];
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(10000)]
        public void Players_Item_GetInvalidIndex_ThrowsIndexOutOfRangeException(int index)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Assert.Throws<IndexOutOfRangeException>(() => playerService.Players[index]);
        }

        [Fact]
        public void Players_Item_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            var player = playerService.Players[1];

            Assert.Equal(1, player.Index);
            Assert.Equal(Terraria.Main.player[1], ((OrionPlayer)player).Wrapped);
        }

        [Fact]
        public void Players_Item_GetMultipleTimes_ReturnsSameInstance()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            var player = playerService.Players[0];
            var player2 = playerService.Players[0];

            Assert.Same(player2, player);
        }

        [Fact]
        public void Players_GetEnumerator()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            var players = playerService.Players.ToList();

            for (var i = 0; i < players.Count; ++i)
            {
                Assert.Equal(Terraria.Main.player[i], ((OrionPlayer)players[i]).Wrapped);
            }
        }

        [Fact]
        public void PlayerTick_EventTriggered()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PlayerTickEvent>(
                        evt => ((OrionPlayer)evt.Player).Wrapped == Terraria.Main.player[0]),
                    log));

            Terraria.Main.player[0].Update(0);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PlayerTick_EventCanceled()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PlayerTickEvent>(), log))
                .Callback<PlayerTickEvent, ILogger>((evt, log) => evt.Cancel());

            Terraria.Main.player[0].Update(0);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void ResetClient_EventTriggered()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient { Id = 5, IsActive = true };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PlayerQuitEvent>(
                        evt => ((OrionPlayer)evt.Player).Wrapped == Terraria.Main.player[5]),
                    log));

            Terraria.Netplay.Clients[5].Reset();

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void ResetClient_EventNotTriggered()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient { Id = 5 };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Terraria.Netplay.Clients[5].Reset();

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<PlayerQuitEvent>(), log), Times.Never);
        }

        [Fact]
        public void PacketReceive_EventTriggered()
        {
            // Clear out the password so we know it's empty.
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient { Id = 5 };
            Terraria.Netplay.ServerPassword = string.Empty;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PacketReceiveEvent<ClientConnectPacket>>(
                        evt => ((OrionPlayer)evt.Sender).Wrapped == Terraria.Main.player[5]),
                    log))
                .Callback<PacketReceiveEvent<ClientConnectPacket>, ILogger>((evt, log) =>
                {
                    Assert.Equal("Terraria" + Terraria.Main.curRelease, evt.Packet.Version);
                });

            using var playerService = new OrionPlayerService(events, log);

            TestUtils.FakeReceiveBytes(5, _serverConnectPacketBytes);

            Assert.Equal(1, Terraria.Netplay.Clients[5].State);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_EventModified()
        {
            // Clear out the password so we know it's empty.
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient { Id = 5 };
            Terraria.Netplay.ServerPassword = string.Empty;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketReceiveEvent<ClientConnectPacket>>(), log))
                .Callback<PacketReceiveEvent<ClientConnectPacket>, ILogger>(
                    (evt, log) => evt.Packet.Version = "Terraria1");

            using var playerService = new OrionPlayerService(events, log);

            TestUtils.FakeReceiveBytes(5, _serverConnectPacketBytes);

            Assert.Equal(0, Terraria.Netplay.Clients[5].State);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_EventCanceled()
        {
            // Clear out the password so we know it's empty.
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient { Id = 5 };
            Terraria.Netplay.ServerPassword = string.Empty;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketReceiveEvent<ClientConnectPacket>>(), log))
                .Callback<PacketReceiveEvent<ClientConnectPacket>, ILogger>((evt, log) => evt.Cancel());

            using var playerService = new OrionPlayerService(events, log);

            TestUtils.FakeReceiveBytes(5, _serverConnectPacketBytes);

            Assert.Equal(0, Terraria.Netplay.Clients[5].State);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_UnknownPacket()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PacketReceiveEvent<UnknownPacket>>(
                        evt => ((OrionPlayer)evt.Sender).Wrapped == Terraria.Main.player[5]),
                    log))
                .Callback<PacketReceiveEvent<UnknownPacket>, ILogger>((evt, log) =>
                {
                    Assert.Equal((PacketId)255, evt.Packet.Id);
                    Assert.Equal(0, evt.Packet.Length);
                });

            using var playerService = new OrionPlayerService(events, log);

            TestUtils.FakeReceiveBytes(5, new byte[] { 3, 0, 255 });

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_UnknownModule()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PacketReceiveEvent<ModulePacket<UnknownModule>>>(
                        evt => ((OrionPlayer)evt.Sender).Wrapped == Terraria.Main.player[5]),
                    log))
                .Callback<PacketReceiveEvent<ModulePacket<UnknownModule>>, ILogger>((evt, log) =>
                {
                    Assert.Equal((ModuleId)65535, evt.Packet.Module.Id);
                    Assert.Equal(0, evt.Packet.Module.Length);
                });

            using var playerService = new OrionPlayerService(events, log);

            TestUtils.FakeReceiveBytes(5, new byte[] { 5, 0, 82, 255, 255 });

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerJoinPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<PlayerJoinPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerJoinPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerJoinPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerJoinPacket();
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerJoinPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.Is<PlayerJoinEvent>(evt => evt.Player == sender), log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerJoinPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<PlayerJoinPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerJoinPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerJoinPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerJoinPacket();
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerJoinPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PlayerJoinEvent>(), log))
                .Callback<PlayerJoinEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerHealthPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<PlayerHealthPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerHealthPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerHealthPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerHealthPacket { Health = 100, MaxHealth = 500 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerHealthPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PlayerHealthEvent>(
                        evt => evt.Player == sender && evt.Health == 100 && evt.MaxHealth == 500),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerHealthPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<PlayerHealthPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerHealthPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerHealthPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerHealthPacket { Health = 100, MaxHealth = 500 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerHealthPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PlayerHealthEvent>(), log))
                .Callback<PlayerHealthEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerPvpPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<PlayerPvpPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerPvpPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerPvpPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerPvpPacket { IsInPvp = true };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerPvpPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.Is<PlayerPvpEvent>(evt => evt.Player == sender && evt.IsInPvp), log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerPvpPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<PlayerPvpPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerPvpPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerPvpPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerPvpPacket { IsInPvp = true };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerPvpPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PlayerPvpEvent>(), log))
                .Callback<PlayerPvpEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);
        }

        [Fact]
        public void PacketReceive_ClientPasswordPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<ClientPasswordPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ClientPasswordPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<ClientPasswordPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new ClientPasswordPacket { Password = "Terraria" };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ClientPasswordPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PlayerPasswordEvent>(evt => evt.Player == sender && evt.Password == "Terraria"), log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_ClientPasswordPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<ClientPasswordPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ClientPasswordPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<ClientPasswordPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new ClientPasswordPacket { Password = "Terraria" };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ClientPasswordPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PlayerPasswordEvent>(), log))
                .Callback<PlayerPasswordEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);
        }

        [Fact]
        public void PacketReceive_PlayerManaPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<PlayerManaPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerManaPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerManaPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerManaPacket { Mana = 100, MaxMana = 200 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerManaPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PlayerManaEvent>(evt => evt.Player == sender && evt.Mana == 100 && evt.MaxMana == 200), log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerManaPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<PlayerManaPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerManaPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerManaPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerManaPacket { Mana = 100, MaxMana = 200 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerManaPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PlayerManaEvent>(), log))
                .Callback<PlayerManaEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerTeamPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<PlayerTeamPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerTeamPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerTeamPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerTeamPacket { Team = PlayerTeam.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerTeamPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PlayerTeamEvent>(evt => evt.Player == sender && evt.Team == PlayerTeam.Red), log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerTeamPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<PlayerTeamPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerTeamPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerTeamPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerTeamPacket { Team = PlayerTeam.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerTeamPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PlayerTeamEvent>(), log))
                .Callback<PlayerTeamEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_ClientUuidPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<ClientUuidPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ClientUuidPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<ClientUuidPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new ClientUuidPacket { Uuid = "Terraria" };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ClientUuidPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PlayerUuidEvent>(evt => evt.Player == sender && evt.Uuid == "Terraria"), log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_ClientUuidPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<ClientUuidPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ClientUuidPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<ClientUuidPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new ClientUuidPacket { Uuid = "Terraria" };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ClientUuidPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PlayerUuidEvent>(), log))
                .Callback<PlayerUuidEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_ChatModulePacket_EventTriggered()
        {
            Action<PacketReceiveEvent<ModulePacket<ChatModule>>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ModulePacket<ChatModule>>>>(), log))
                .Callback<Action<PacketReceiveEvent<ModulePacket<ChatModule>>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new ModulePacket<ChatModule>
            {
                Module = new ChatModule { ClientCommand = "Say", ClientMessage = "/command test" }
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ModulePacket<ChatModule>>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PlayerChatEvent>(
                        evt => evt.Player == sender && evt.Command == "Say" && evt.Message == "/command test"),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_ChatModulePacket_EventCanceled()
        {
            Action<PacketReceiveEvent<ModulePacket<ChatModule>>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ModulePacket<ChatModule>>>>(), log))
                .Callback<Action<PacketReceiveEvent<ModulePacket<ChatModule>>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new ModulePacket<ChatModule>
            {
                Module = new ChatModule { ClientCommand = "Say", ClientMessage = "/command test" }
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ModulePacket<ChatModule>>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PlayerChatEvent>(), log))
                .Callback<PlayerChatEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketSend_EventTriggered()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => s.IsConnected())
            };

            byte[]? sendData = null;
            Mock.Get(Terraria.Netplay.Clients[5].Socket)
                .Setup(s => s.AsyncSend(
                    It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Terraria.Net.Sockets.SocketSendCallback>(), It.IsAny<object>()))
                .Callback<byte[], int, int, Terraria.Net.Sockets.SocketSendCallback, object>(
                    (data, offset, size, callback, state) => sendData = data[offset..size]);

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PacketSendEvent<ClientConnectPacket>>(
                        evt => ((OrionPlayer)evt.Receiver).Wrapped == Terraria.Main.player[5]),
                    log))
                .Callback<PacketSendEvent<ClientConnectPacket>, ILogger>((evt, log) =>
                {
                    Assert.Equal("Terraria" + Terraria.Main.curRelease, evt.Packet.Version);
                });

            Terraria.NetMessage.SendData((byte)PacketId.ClientConnect, 5);

            Assert.NotNull(sendData);
            Assert.Equal(_serverConnectPacketBytes, sendData!);

            Mock.Get(Terraria.Netplay.Clients[5].Socket).VerifyAll();
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketSend_UnknownPacket_EventTriggered()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => s.IsConnected())
            };

            byte[]? sendData = null;
            Mock.Get(Terraria.Netplay.Clients[5].Socket)
                .Setup(s => s.AsyncSend(
                    It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Terraria.Net.Sockets.SocketSendCallback>(), It.IsAny<object>()))
                .Callback<byte[], int, int, Terraria.Net.Sockets.SocketSendCallback, object>(
                    (data, offset, size, callback, state) => sendData = data[offset..size]);

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PacketSendEvent<UnknownPacket>>(
                        evt => ((OrionPlayer)evt.Receiver).Wrapped == Terraria.Main.player[5]),
                    log))
                .Callback<PacketSendEvent<UnknownPacket>, ILogger>((evt, log) =>
                {
                    Assert.Equal((PacketId)25, evt.Packet.Id);
                    Assert.Equal(0, evt.Packet.Length);
                });

            Terraria.NetMessage.SendData(25, 5);

            Assert.NotNull(sendData);
            Assert.Equal(new byte[] { 3, 0, 25 }, sendData!);

            Mock.Get(Terraria.Netplay.Clients[5].Socket).VerifyAll();
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketSend_EventModified()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => s.IsConnected())
            };

            byte[]? sendData = null;
            Mock.Get(Terraria.Netplay.Clients[5].Socket)
                .Setup(s => s.AsyncSend(
                    It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Terraria.Net.Sockets.SocketSendCallback>(), It.IsAny<object>()))
                .Callback<byte[], int, int, Terraria.Net.Sockets.SocketSendCallback, object>(
                    (data, offset, size, callback, state) => sendData = data[offset..size]);

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ClientConnectPacket>>(), log))
                .Callback<PacketSendEvent<ClientConnectPacket>, ILogger>((evt, log) => evt.Packet.Version = "");

            Terraria.NetMessage.SendData((byte)PacketId.ClientConnect, 5);

            Assert.NotNull(sendData);
            Assert.NotEqual(_serverConnectPacketBytes, sendData!);

            Mock.Get(Terraria.Netplay.Clients[5].Socket).VerifyAll();
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketSend_EventCanceled()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => s.IsConnected())
            };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ClientConnectPacket>>(), log))
                .Callback<PacketSendEvent<ClientConnectPacket>, ILogger>((evt, log) => evt.Cancel());

            Terraria.NetMessage.SendData((byte)PacketId.ClientConnect, 5);

            Mock.Get(Terraria.Netplay.Clients[5].Socket)
                .Verify(
                    s => s.AsyncSend(
                        It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                        It.IsAny<Terraria.Net.Sockets.SocketSendCallback>(), It.IsAny<object>()),
                    Times.Never);
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketSend_AsyncSendThrowsIOException()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => s.IsConnected())
            };

            Mock.Get(Terraria.Netplay.Clients[5].Socket)
                .Setup(s => s.AsyncSend(
                    It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Terraria.Net.Sockets.SocketSendCallback>(), It.IsAny<object>()))
                .Throws<IOException>();

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ClientConnectPacket>>(), log));

            Terraria.NetMessage.SendData((byte)PacketId.ClientConnect, 5);

            Mock.Get(Terraria.Netplay.Clients[5].Socket).VerifyAll();
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void ModuleSend_EventTriggered()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => s.IsConnected())
            };

            byte[]? sendData = null;
            Mock.Get(Terraria.Netplay.Clients[5].Socket)
                .Setup(s => s.AsyncSend(
                    It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Terraria.Net.Sockets.SocketSendCallback>(), It.IsAny<object>()))
                .Callback<byte[], int, int, Terraria.Net.Sockets.SocketSendCallback, object>(
                    (data, offset, size, callback, state) => sendData = data[offset..size]);

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PacketSendEvent<ModulePacket<ChatModule>>>(
                        evt => ((OrionPlayer)evt.Receiver).Wrapped == Terraria.Main.player[5]),
                    log))
                .Callback<PacketSendEvent<ModulePacket<ChatModule>>, ILogger>((evt, log) =>
                {
                    Assert.Equal(1, evt.Packet.Module.ServerAuthorIndex);
                    Assert.Equal("test", evt.Packet.Module.ServerMessage);
                    Assert.Equal(Color3.White, evt.Packet.Module.ServerColor);
                });

            var packet = new Terraria.Net.NetPacket(1, 16);
            packet.Writer.Write((byte)1);
            Terraria.Localization.NetworkText.FromLiteral("test").Serialize(packet.Writer);
            Terraria.Utils.WriteRGB(packet.Writer, Microsoft.Xna.Framework.Color.White);
            Terraria.Net.NetManager.Instance.SendData(Terraria.Netplay.Clients[5].Socket, packet);

            Assert.NotNull(sendData);
            Assert.Equal(new byte[] { 15, 0, 82, 1, 0, 1, 0, 4, 116, 101, 115, 116, 255, 255, 255 }, sendData!);

            Mock.Get(Terraria.Netplay.Clients[5].Socket).VerifyAll();
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void ModuleSend_UnknownModule_EventTriggered()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => s.IsConnected())
            };

            byte[]? sendData = null;
            Mock.Get(Terraria.Netplay.Clients[5].Socket)
                .Setup(s => s.AsyncSend(
                    It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Terraria.Net.Sockets.SocketSendCallback>(), It.IsAny<object>()))
                .Callback<byte[], int, int, Terraria.Net.Sockets.SocketSendCallback, object>(
                    (data, offset, size, callback, state) => sendData = data[offset..size]);

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PacketSendEvent<ModulePacket<UnknownModule>>>(
                        evt => ((OrionPlayer)evt.Receiver).Wrapped == Terraria.Main.player[5]),
                    log))
                .Callback<PacketSendEvent<ModulePacket<UnknownModule>>, ILogger>((evt, log) =>
                {
                    Assert.Equal((ModuleId)65535, evt.Packet.Module.Id);
                    Assert.Equal(4, evt.Packet.Module.Length);
                });

            var packet = new Terraria.Net.NetPacket(65535, 10);
            packet.Writer.Write(1234);
            Terraria.Net.NetManager.Instance.SendData(Terraria.Netplay.Clients[5].Socket, packet);

            Assert.NotNull(sendData);
            Assert.Equal(new byte[] { 9, 0, 82, 255, 255, 210, 4, 0, 0 }, sendData!);

            Mock.Get(Terraria.Netplay.Clients[5].Socket).VerifyAll();
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void ModuleSend_EventModified()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => s.IsConnected())
            };

            byte[]? sendData = null;
            Mock.Get(Terraria.Netplay.Clients[5].Socket)
                .Setup(s => s.AsyncSend(
                    It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Terraria.Net.Sockets.SocketSendCallback>(), It.IsAny<object>()))
                .Callback<byte[], int, int, Terraria.Net.Sockets.SocketSendCallback, object>(
                    (data, offset, size, callback, state) => sendData = data[offset..size]);

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ModulePacket<ChatModule>>>(), log))
                .Callback<PacketSendEvent<ModulePacket<ChatModule>>, ILogger>(
                    (evt, log) => evt.Packet.Module.ServerColor = Color3.Black);

            var packet = new Terraria.Net.NetPacket(1, 16);
            packet.Writer.Write((byte)1);
            Terraria.Localization.NetworkText.FromLiteral("test").Serialize(packet.Writer);
            Terraria.Utils.WriteRGB(packet.Writer, Microsoft.Xna.Framework.Color.White);
            Terraria.Net.NetManager.Instance.SendData(Terraria.Netplay.Clients[5].Socket, packet);

            Assert.NotNull(sendData);
            Assert.Equal(new byte[] { 15, 0, 82, 1, 0, 1, 0, 4, 116, 101, 115, 116, 0, 0, 0 }, sendData!);

            Mock.Get(Terraria.Netplay.Clients[5].Socket).VerifyAll();
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void ModuleSend_EventCanceled()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => s.IsConnected())
            };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ModulePacket<ChatModule>>>(), log))
                .Callback<PacketSendEvent<ModulePacket<ChatModule>>, ILogger>((evt, log) => evt.Cancel());

            var packet = new Terraria.Net.NetPacket(1, 16);
            packet.Writer.Write((byte)1);
            Terraria.Localization.NetworkText.FromLiteral("test").Serialize(packet.Writer);
            Terraria.Utils.WriteRGB(packet.Writer, Microsoft.Xna.Framework.Color.White);
            Terraria.Net.NetManager.Instance.SendData(Terraria.Netplay.Clients[5].Socket, packet);

            Mock.Get(Terraria.Netplay.Clients[5].Socket)
                .Verify(
                    s => s.AsyncSend(
                        It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                        It.IsAny<Terraria.Net.Sockets.SocketSendCallback>(), It.IsAny<object>()),
                    Times.Never);
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void ModuleSend_AsyncSendThrowsIOException()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => s.IsConnected())
            };

            Mock.Get(Terraria.Netplay.Clients[5].Socket)
                .Setup(s => s.AsyncSend(
                    It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<Terraria.Net.Sockets.SocketSendCallback>(), It.IsAny<object>()))
                .Throws<IOException>();

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ModulePacket<ChatModule>>>(), log));

            var packet = new Terraria.Net.NetPacket(1, 16);
            packet.Writer.Write((byte)1);
            Terraria.Localization.NetworkText.FromLiteral("test").Serialize(packet.Writer);
            Terraria.Utils.WriteRGB(packet.Writer, Microsoft.Xna.Framework.Color.White);
            Terraria.Net.NetManager.Instance.SendData(Terraria.Netplay.Clients[5].Socket, packet);

            Mock.Get(Terraria.Netplay.Clients[5].Socket).VerifyAll();
            Mock.Get(events).VerifyAll();
        }
    }
}
