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
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Text;
using Destructurama.Attributed;
using Orion.Core.Entities;
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Items;
using Orion.Core.Packets;
using Orion.Core.Players;
using Orion.Core.Utils;
using Orion.Launcher.Entities;
using Serilog;

namespace Orion.Launcher.Players
{
    [LogAsScalar]
    internal sealed partial class OrionPlayer : OrionEntity<Terraria.Player>, IPlayer
    {
        private readonly IEventManager _events;
        private readonly ILogger _log;

        public OrionPlayer(int playerIndex, Terraria.Player terrariaPlayer, IEventManager events, ILogger log)
            : base(playerIndex, terrariaPlayer)
        {
            Debug.Assert(events != null);
            Debug.Assert(log != null);

            _events = events;
            _log = log;

            Character = new OrionCharacter(terrariaPlayer);
            Buffs = new BuffArray(terrariaPlayer);
            Inventory = new InventoryArray(terrariaPlayer);
        }

        public OrionPlayer(Terraria.Player terrariaPlayer, IEventManager events, ILogger log)
            : this(-1, terrariaPlayer, events, log)
        {
        }

        public override string Name
        {
            get => Wrapped.name;
            set => Wrapped.name = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ICharacter Character { get; }

        public IArray<Buff> Buffs { get; }

        public int Health
        {
            get => Wrapped.statLife;
            set => Wrapped.statLife = value;
        }

        public int MaxHealth
        {
            get => Wrapped.statLifeMax;
            set => Wrapped.statLifeMax = value;
        }

        public int Mana
        {
            get => Wrapped.statMana;
            set => Wrapped.statMana = value;
        }

        public int MaxMana
        {
            get => Wrapped.statManaMax;
            set => Wrapped.statManaMax = value;
        }

        public IArray<ItemStack> Inventory { get; }

        public bool IsInPvp
        {
            get => Wrapped.hostile;
            set => Wrapped.hostile = value;
        }

        public Team Team
        {
            get => (Team)Wrapped.team;
            set => Wrapped.team = (int)value;
        }

        public void ReceivePacket<TPacket>(TPacket packet) where TPacket : IPacket
        {
            if (packet is null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            var evt = new PacketReceiveEvent<TPacket>(packet, this);
            _events.Raise(evt, _log);
            if (evt.IsCanceled)
            {
                return;
            }

            packet = evt.Packet;

            var buffer = Terraria.NetMessage.buffer[Index];

            // To simulate the receival of the packet, we must swap out the read buffer and reader, and call `GetData()`
            // while ensuring that there isn't an infinite loop.
            var oldReadBuffer = buffer.readBuffer;
            var oldReader = buffer.reader;

            var pool = ArrayPool<byte>.Shared;
            var receiveBuffer = pool.Rent(ushort.MaxValue);

            try
            {
                // Write the packet using the `Client` context since we're receiving this packet.
                var packetLength = packet.Write(receiveBuffer, PacketContext.Client);

                // Ignore the next `GetData` call so that there isn't an infinite loop.
                OrionPlayerService._ignoreGetData = true;
                buffer.readBuffer = receiveBuffer;
                buffer.reader = new BinaryReader(new MemoryStream(buffer.readBuffer), Encoding.UTF8);
                buffer.GetData(2, packetLength - 2, out _);
            }
            finally
            {
                pool.Return(receiveBuffer);

                OrionPlayerService._ignoreGetData = false;
                buffer.readBuffer = oldReadBuffer;
                buffer.reader = oldReader;
            }
        }

        public void SendPacket<TPacket>(TPacket packet) where TPacket : IPacket
        {
            if (packet is null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            var terrariaClient = Terraria.Netplay.Clients[Index];
            if (!terrariaClient.IsConnected())
            {
                return;
            }

            var evt = new PacketSendEvent<TPacket>(packet, this);
            _events.Raise(evt, _log);
            if (evt.IsCanceled)
            {
                return;
            }

            packet = evt.Packet;

            var pool = ArrayPool<byte>.Shared;
            var sendBuffer = pool.Rent(ushort.MaxValue);
            var wasSuccessful = false;

            try
            {
                // Write the packet using the `Server` context since we're sending this packet.
                var packetLength = packet.Write(sendBuffer, PacketContext.Server);

                terrariaClient.Socket.AsyncSend(sendBuffer, 0, packetLength, state =>
                {
                    try
                    {
                        terrariaClient.ServerWriteCallBack(null!);
                    }
                    finally
                    {
                        pool.Return((byte[])state);
                    }
                }, sendBuffer);

                wasSuccessful = true;
            }
            catch (IOException)
            {
                terrariaClient.Socket.Close();
            }
            finally
            {
                if (!wasSuccessful)
                {
                    pool.Return(sendBuffer);
                }
            }
        }
    }
}
