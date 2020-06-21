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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Moq;
using Orion.Core.Buffs;
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Packets;
using Orion.Core.Packets.Client;
using Orion.Core.Players;
using Serilog;
using Xunit;

namespace Orion.Launcher.Players
{
    // These tests depend on Terraria state.
    [Collection("TerrariaTestsCollection")]
    [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Testing")]
    public class OrionPlayerTests
    {
        [Fact]
        public void Name_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { name = "test" };
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Equal("test", player.Name);
        }

        [Fact]
        public void Name_SetNullValue_ThrowsArgumentNullException()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Throws<ArgumentNullException>(() => player.Name = null!);
        }

        [Fact]
        public void Name_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Name = "test";

            Assert.Equal("test", terrariaPlayer.name);
        }

        [Fact]
        public void Health_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { statLife = 100 };
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Equal(100, player.Health);
        }

        [Fact]
        public void Health_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Health = 100;

            Assert.Equal(100, terrariaPlayer.statLife);
        }

        [Fact]
        public void MaxHealth_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { statLifeMax = 500 };
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Equal(500, player.MaxHealth);
        }

        [Fact]
        public void MaxHealth_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.MaxHealth = 500;

            Assert.Equal(500, terrariaPlayer.statLifeMax);
        }

        [Fact]
        public void Mana_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { statMana = 100 };
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Equal(100, player.Mana);
        }

        [Fact]
        public void Mana_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Mana = 100;

            Assert.Equal(100, terrariaPlayer.statMana);
        }

        [Fact]
        public void MaxMana_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { statManaMax = 200 };
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Equal(200, player.MaxMana);
        }

        [Fact]
        public void MaxMana_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.MaxMana = 200;

            Assert.Equal(200, terrariaPlayer.statManaMax);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        public void Buffs_Get_Index_GetInvalidIndex_ThrowsIndexOutOfRangeException(int index)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Throws<IndexOutOfRangeException>(() => player.Buffs[index]);
        }

        [Fact]
        public void Buffs_Get_Index_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.buffType[0] = (int)BuffId.ObsidianSkin;
            terrariaPlayer.buffTime[0] = 28800;

            Assert.Equal(new Buff(BuffId.ObsidianSkin, TimeSpan.FromMinutes(8)), player.Buffs[0]);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Buffs_Get_Index_InvalidTime_Get(int buffTime)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.buffType[0] = (int)BuffId.ObsidianSkin;
            terrariaPlayer.buffTime[0] = buffTime;

            Assert.Equal(default, player.Buffs[0]);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        public void Buffs_Get_Index_SetInvalidIndex_ThrowsIndexOutOfRangeException(int index)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Throws<IndexOutOfRangeException>(() => player.Buffs[index] = default);
        }

        [Fact]
        public void Buffs_Get_Index_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Buffs[0] = new Buff(BuffId.ObsidianSkin, TimeSpan.FromMinutes(8));

            Assert.Equal(BuffId.ObsidianSkin, (BuffId)terrariaPlayer.buffType[0]);
            Assert.Equal(28800, terrariaPlayer.buffTime[0]);
        }

        [Fact]
        public void Difficulty_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { difficulty = (byte)CharacterDifficulty.Journey };
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Equal(CharacterDifficulty.Journey, player.Difficulty);
        }

        [Fact]
        public void Difficulty_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Difficulty = CharacterDifficulty.Journey;

            Assert.Equal(CharacterDifficulty.Journey, (CharacterDifficulty)terrariaPlayer.difficulty);
        }

        [Fact]
        public void IsInPvp_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { hostile = true };
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.True(player.IsInPvp);
        }

        [Fact]
        public void IsInPvp_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.IsInPvp = true;

            Assert.True(terrariaPlayer.hostile);
        }

        [Fact]
        public void Team_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { team = (int)PlayerTeam.Red };
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Equal(PlayerTeam.Red, player.Team);
        }

        [Fact]
        public void Team_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Team = PlayerTeam.Red;

            Assert.Equal(1, terrariaPlayer.team);
        }

        [Fact]
        public void ReceivePacket_EventTriggered()
        {
            // Clear out the password so we know it's empty.
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient { Id = 5 };
            Terraria.Netplay.ServerPassword = string.Empty;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { whoAmI = 5 };
            var player = new OrionPlayer(5, terrariaPlayer, events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PacketReceiveEvent<ClientConnectPacket>>(
                        evt => ((OrionPlayer)evt.Sender).Wrapped == terrariaPlayer),
                    log))
                .Callback<PacketReceiveEvent<ClientConnectPacket>, ILogger>((evt, log) =>
                {
                    Assert.Equal("Terraria" + Terraria.Main.curRelease, evt.Packet.Version);
                });

            var packet = new ClientConnectPacket { Version = "Terraria" + Terraria.Main.curRelease };
            player.ReceivePacket(ref packet);

            Assert.Equal(1, Terraria.Netplay.Clients[5].State);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void ReceivePacket_EventModified()
        {
            // Clear out the password so we know it's empty.
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient { Id = 5 };
            Terraria.Netplay.ServerPassword = string.Empty;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { whoAmI = 5 };
            var player = new OrionPlayer(5, terrariaPlayer, events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketReceiveEvent<ClientConnectPacket>>(), log))
                .Callback<PacketReceiveEvent<ClientConnectPacket>, ILogger>(
                    (evt, log) => evt.Packet.Version = "Terraria1");

            var packet = new ClientConnectPacket { Version = "Terraria" + Terraria.Main.curRelease };
            player.ReceivePacket(ref packet);

            Assert.Equal(0, Terraria.Netplay.Clients[5].State);
        }

        [Fact]
        public void ReceivePacket_EventCanceled()
        {
            // Clear out the password so we know it's empty.
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient { Id = 5 };
            Terraria.Netplay.ServerPassword = string.Empty;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { whoAmI = 5 };
            var player = new OrionPlayer(5, terrariaPlayer, events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketReceiveEvent<ClientConnectPacket>>(), log))
                .Callback<PacketReceiveEvent<ClientConnectPacket>, ILogger>((evt, log) => evt.Cancel());

            var packet = new ClientConnectPacket { Version = "Terraria" + Terraria.Main.curRelease };
            player.ReceivePacket(ref packet);

            Assert.Equal(0, Terraria.Netplay.Clients[5].State);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void SendPacket_NotConnected()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => !s.IsConnected())
            };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { whoAmI = 5 };
            var player = new OrionPlayer(5, terrariaPlayer, events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<TestPacket>>(), log));

            var packet = new TestPacket { Value = 100 };
            player.SendPacket(ref packet);

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<PacketSendEvent<TestPacket>>(), log), Times.Never);
        }

        [Fact]
        public void SendPacket_EventTriggered()
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
            var terrariaPlayer = new Terraria.Player { whoAmI = 5 };
            var player = new OrionPlayer(5, terrariaPlayer, events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<PacketSendEvent<TestPacket>>(
                        evt => ((OrionPlayer)evt.Receiver).Wrapped == terrariaPlayer),
                    log))
                .Callback<PacketSendEvent<TestPacket>, ILogger>((evt, log) =>
                {
                    Assert.Equal(100, evt.Packet.Value);
                });

            var packet = new TestPacket { Value = 100 };
            player.SendPacket(ref packet);

            Assert.NotNull(sendData);
            Assert.Equal(new byte[] { 4, 0, 255, 100 }, sendData!);

            Mock.Get(Terraria.Netplay.Clients[5].Socket).VerifyAll();
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void SendPacket_EventModified()
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
            var terrariaPlayer = new Terraria.Player { whoAmI = 5 };
            var player = new OrionPlayer(5, terrariaPlayer, events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<TestPacket>>(), log))
                .Callback<PacketSendEvent<TestPacket>, ILogger>((evt, log) => evt.Packet.Value = 200);

            var packet = new TestPacket { Value = 100 };
            player.SendPacket(ref packet);

            Assert.NotNull(sendData);
            Assert.Equal(new byte[] { 4, 0, 255, 200 }, sendData!);

            Mock.Get(Terraria.Netplay.Clients[5].Socket).VerifyAll();
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void SendPacket_EventCanceled()
        {
            Terraria.Netplay.Clients[5] = new Terraria.RemoteClient
            {
                Id = 5,
                Socket = Mock.Of<Terraria.Net.Sockets.ISocket>(s => s.IsConnected())
            };

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player { whoAmI = 5 };
            var player = new OrionPlayer(5, terrariaPlayer, events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<TestPacket>>(), log))
                .Callback<PacketSendEvent<TestPacket>, ILogger>((evt, log) => evt.Cancel());

            var packet = new TestPacket { Value = 100 };
            player.SendPacket(ref packet);

            Mock.Get(Terraria.Netplay.Clients[5].Socket)
                .Verify(
                    s => s.AsyncSend(
                        It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(),
                        It.IsAny<Terraria.Net.Sockets.SocketSendCallback>(), It.IsAny<object>()),
                    Times.Never);
            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void SendPacket_AsyncSendThrowsIOException()
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
            var terrariaPlayer = new Terraria.Player { whoAmI = 5 };
            var player = new OrionPlayer(5, terrariaPlayer, events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<PacketSendEvent<TestPacket>>(), log));

            var packet = new TestPacket { Value = 100 };
            player.SendPacket(ref packet);

            Mock.Get(Terraria.Netplay.Clients[5].Socket).VerifyAll();
            Mock.Get(events).VerifyAll();
        }

        private struct TestPacket : IPacket
        {
            public PacketId Id => (PacketId)255;

            public byte Value { get; set; }

            public int Read(Span<byte> span, PacketContext context) => throw new NotImplementedException();

            public int Write(Span<byte> span, PacketContext context)
            {
                span[0] = Value;
                return 1;
            }
        }
    }
}
