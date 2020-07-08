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
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.Players;
using Orion.Core.Packets;
using Orion.Core.Packets.DataStructures.Modules;
using Orion.Core.Packets.Players;
using Orion.Core.Packets.Server;
using Orion.Core.Players;
using Orion.Core.Utils;
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
            var packet = new ClientConnect { Version = "Terraria" + Terraria.Main.curRelease };
            var packetLength = packet.Write(bytes, PacketContext.Client);

            _serverConnectPacketBytes = bytes[..packetLength];
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(10000)]
        public void Item_GetInvalidIndex_ThrowsIndexOutOfRangeException(int index)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Assert.Throws<IndexOutOfRangeException>(() => playerService[index]);
        }

        [Fact]
        public void Item_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            var player = playerService[1];

            Assert.Equal(1, player.Index);
            Assert.Equal(Terraria.Main.player[1], ((OrionPlayer)player).Wrapped);
        }

        [Fact]
        public void Item_GetMultipleTimes_ReturnsSameInstance()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            var player = playerService[0];
            var player2 = playerService[0];

            Assert.Same(player2, player);
        }

        [Fact]
        public void Count_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Assert.Equal(Terraria.Main.maxPlayers, playerService.Count);
        }

        [Fact]
        public void GetEnumerator()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            var players = playerService.ToList();

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
                    It.Is<PacketReceiveEvent<ClientConnect>>(
                        evt => ((OrionPlayer)evt.Sender).Wrapped == Terraria.Main.player[5]),
                    log))
                .Callback<PacketReceiveEvent<ClientConnect>, ILogger>((evt, log) =>
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
                .Setup(em => em.Raise(It.IsAny<PacketReceiveEvent<ClientConnect>>(), log))
                .Callback<PacketReceiveEvent<ClientConnect>, ILogger>(
                    (evt, log) => evt.Packet = new ClientConnect { Version = "Terraria1" });

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
                .Setup(em => em.Raise(It.IsAny<PacketReceiveEvent<ClientConnect>>(), log))
                .Callback<PacketReceiveEvent<ClientConnect>, ILogger>((evt, log) => evt.Cancel());

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
                    Assert.Equal(0, evt.Packet.Data.Length);
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
                    Assert.Equal(0, evt.Packet.Module.Data.Length);
                });

            using var playerService = new OrionPlayerService(events, log);

            TestUtils.FakeReceiveBytes(5, new byte[] { 5, 0, 82, 255, 255 });

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerJoinPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<PlayerJoin>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerJoin>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerJoin>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerJoin();
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerJoin>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.Is<PlayerJoinEvent>(evt => evt.Player == sender), log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerJoinPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<PlayerJoin>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerJoin>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerJoin>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerJoin();
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerJoin>(packet, sender);

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
            Action<PacketReceiveEvent<PlayerHealth>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerHealth>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerHealth>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerHealth { Health = 100, MaxHealth = 500 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerHealth>(packet, sender);

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
            Action<PacketReceiveEvent<PlayerHealth>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerHealth>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerHealth>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerHealth { Health = 100, MaxHealth = 500 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerHealth>(packet, sender);

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
            Action<PacketReceiveEvent<PlayerPvp>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerPvp>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerPvp>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerPvp { IsInPvp = true };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerPvp>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.Is<PlayerPvpEvent>(evt => evt.Player == sender && evt.IsInPvp), log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerPvpPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<PlayerPvp>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerPvp>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerPvp>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerPvp { IsInPvp = true };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerPvp>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PlayerPvpEvent>(), log))
                .Callback<PlayerPvpEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);
        }

        [Fact]
        public void PacketReceive_PasswordResponsePacket_EventTriggered()
        {
            Action<PacketReceiveEvent<PasswordResponse>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PasswordResponse>>>(), log))
                .Callback<Action<PacketReceiveEvent<PasswordResponse>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PasswordResponse { Password = "Terraria" };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PasswordResponse>(packet, sender);

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
            Action<PacketReceiveEvent<PasswordResponse>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PasswordResponse>>>(), log))
                .Callback<Action<PacketReceiveEvent<PasswordResponse>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PasswordResponse { Password = "Terraria" };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PasswordResponse>(packet, sender);

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
            Action<PacketReceiveEvent<PlayerMana>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerMana>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerMana>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerMana { Mana = 100, MaxMana = 200 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerMana>(packet, sender);

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
            Action<PacketReceiveEvent<PlayerMana>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerMana>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerMana>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerMana { Mana = 100, MaxMana = 200 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerMana>(packet, sender);

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
            Action<PacketReceiveEvent<PlayerTeam>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerTeam>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerTeam>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerTeam { Team = Team.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerTeam>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PlayerTeamEvent>(evt => evt.Player == sender && evt.Team == Team.Red), log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_PlayerTeamPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<PlayerTeam>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<PlayerTeam>>>(), log))
                .Callback<Action<PacketReceiveEvent<PlayerTeam>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new PlayerTeam { Team = Team.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<PlayerTeam>(packet, sender);

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
            Action<PacketReceiveEvent<ClientUuid>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ClientUuid>>>(), log))
                .Callback<Action<PacketReceiveEvent<ClientUuid>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new ClientUuid { Uuid = "Terraria" };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ClientUuid>(packet, sender);

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
            Action<PacketReceiveEvent<ClientUuid>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ClientUuid>>>(), log))
                .Callback<Action<PacketReceiveEvent<ClientUuid>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new ClientUuid { Uuid = "Terraria" };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ClientUuid>(packet, sender);

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
            Action<PacketReceiveEvent<ModulePacket<Chat>>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ModulePacket<Chat>>>>(), log))
                .Callback<Action<PacketReceiveEvent<ModulePacket<Chat>>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new ModulePacket<Chat>
            {
                Module = new Chat { ClientCommand = "Say", ClientMessage = "/command test" }
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ModulePacket<Chat>>(packet, sender);

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
            Action<PacketReceiveEvent<ModulePacket<Chat>>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<ModulePacket<Chat>>>>(), log))
                .Callback<Action<PacketReceiveEvent<ModulePacket<Chat>>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var playerService = new OrionPlayerService(events, log);

            var packet = new ModulePacket<Chat>
            {
                Module = new Chat { ClientCommand = "Say", ClientMessage = "/command test" }
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<ModulePacket<Chat>>(packet, sender);

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
                    (data, offset, size, callback, state) =>
                    {
                        sendData = data[offset..size];
                        callback(state);
                    });

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PacketSendEvent<ClientConnect>>(
                        evt => ((OrionPlayer)evt.Receiver).Wrapped == Terraria.Main.player[5]),
                    log))
                .Callback<PacketSendEvent<ClientConnect>, ILogger>((evt, log) =>
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
                    (data, offset, size, callback, state) =>
                    {
                        sendData = data[offset..size];
                        callback(state);
                    });

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
                    Assert.Equal(0, evt.Packet.Data.Length);
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
                    (data, offset, size, callback, state) =>
                    {
                        sendData = data[offset..size];
                        callback(state);
                    });

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ClientConnect>>(), log))
                .Callback<PacketSendEvent<ClientConnect>, ILogger>((evt, log) => evt.Packet = new ClientConnect());

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
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ClientConnect>>(), log))
                .Callback<PacketSendEvent<ClientConnect>, ILogger>((evt, log) => evt.Cancel());

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
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ClientConnect>>(), log));

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
                    (data, offset, size, callback, state) =>
                    {
                        sendData = data[offset..size];
                        callback(state);
                    });

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PacketSendEvent<ModulePacket<Chat>>>(
                        evt => ((OrionPlayer)evt.Receiver).Wrapped == Terraria.Main.player[5]),
                    log))
                .Callback<PacketSendEvent<ModulePacket<Chat>>, ILogger>((evt, log) =>
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
                    (data, offset, size, callback, state) =>
                    {
                        sendData = data[offset..size];
                        callback(state);
                    });

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
                    Assert.Equal(4, evt.Packet.Module.Data.Length);
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
                    (data, offset, size, callback, state) =>
                    {
                        sendData = data[offset..size];
                        callback(state);
                    });

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var playerService = new OrionPlayerService(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ModulePacket<Chat>>>(), log))
                .Callback<PacketSendEvent<ModulePacket<Chat>>, ILogger>(
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
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ModulePacket<Chat>>>(), log))
                .Callback<PacketSendEvent<ModulePacket<Chat>>, ILogger>((evt, log) => evt.Cancel());

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
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<ModulePacket<Chat>>>(), log));

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
