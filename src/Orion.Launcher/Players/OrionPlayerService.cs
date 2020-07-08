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
using System.Reflection;
using System.Runtime.CompilerServices;
using Orion.Core;
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.Players;
using Orion.Core.Packets;
using Orion.Core.Packets.DataStructures.Modules;
using Orion.Core.Packets.Players;
using Orion.Core.Packets.Server;
using Orion.Core.Players;
using Orion.Core.Utils;
using Orion.Launcher.Utils;
using Serilog;

namespace Orion.Launcher.Players
{
    [Binding("orion-players", Author = "Pryaxis", Priority = BindingPriority.Lowest)]
    internal sealed class OrionPlayerService : IPlayerService, IDisposable
    {
        private delegate void PacketHandler(int playerIndex, Span<byte> span);

        private static readonly MethodInfo _onReceivePacket =
            typeof(OrionPlayerService)
                .GetMethod(nameof(OnReceivePacket), BindingFlags.NonPublic | BindingFlags.Instance)!;

        private static readonly MethodInfo _onSendPacket =
            typeof(OrionPlayerService)
                .GetMethod(nameof(OnSendPacket), BindingFlags.NonPublic | BindingFlags.Instance)!;

        [ThreadStatic] internal static bool _ignoreGetData;

        private readonly IEventManager _events;
        private readonly ILogger _log;
        private readonly IReadOnlyList<IPlayer> _players;

        private readonly PacketHandler?[] _onReceivePacketHandlers = new PacketHandler?[256];
        private readonly PacketHandler?[] _onReceiveModuleHandlers = new PacketHandler?[65536];
        private readonly PacketHandler?[] _onSendPacketHandlers = new PacketHandler?[256];
        private readonly PacketHandler?[] _onSendModuleHandlers = new PacketHandler?[65536];

        public OrionPlayerService(IEventManager events, ILogger log)
        {
            Debug.Assert(events != null);
            Debug.Assert(log != null);

            _events = events;
            _log = log;

            // Note that the last player should be ignored, as it is not a real player.
            _players = new WrappedReadOnlyList<OrionPlayer, Terraria.Player>(
                Terraria.Main.player.AsMemory(..^1),
                (playerIndex, terrariaPlayer) => new OrionPlayer(playerIndex, terrariaPlayer, events, log));

            foreach (var packetId in (PacketId[])Enum.GetValues(typeof(PacketId)))
            {
                var packetType = packetId.Type();
                _onReceivePacketHandlers[(byte)packetId] = MakeOnReceivePacketHandler(packetType);
                _onSendPacketHandlers[(byte)packetId] = MakeOnSendPacketHandler(packetType);
            }

            foreach (var moduleId in (ModuleId[])Enum.GetValues(typeof(ModuleId)))
            {
                var packetType = typeof(ModulePacket<>).MakeGenericType(moduleId.Type());
                _onReceiveModuleHandlers[(ushort)moduleId] = MakeOnReceivePacketHandler(packetType);
                _onSendModuleHandlers[(ushort)moduleId] = MakeOnSendPacketHandler(packetType);
            }

            OTAPI.Hooks.Net.ReceiveData = ReceiveDataHandler;
            OTAPI.Hooks.Net.SendBytes = SendBytesHandler;
            OTAPI.Hooks.Net.SendNetData = SendNetDataHandler;
            OTAPI.Hooks.Player.PreUpdate = PreUpdateHandler;
            OTAPI.Hooks.Net.RemoteClient.PreReset = PreResetHandler;

            _events.RegisterHandlers(this, _log);

            PacketHandler MakeOnReceivePacketHandler(Type packetType) =>
                (PacketHandler)_onReceivePacket
                    .MakeGenericMethod(packetType)
                    .CreateDelegate(typeof(PacketHandler), this);

            PacketHandler MakeOnSendPacketHandler(Type packetType) =>
                (PacketHandler)_onSendPacket
                    .MakeGenericMethod(packetType)
                    .CreateDelegate(typeof(PacketHandler), this);
        }

        public IPlayer this[int index] => _players[index];

        public int Count => _players.Count;

        public IEnumerator<IPlayer> GetEnumerator() => _players.GetEnumerator();

        public void Dispose()
        {
            OTAPI.Hooks.Net.ReceiveData = null;
            OTAPI.Hooks.Net.SendBytes = null;
            OTAPI.Hooks.Net.SendNetData = null;
            OTAPI.Hooks.Player.PreUpdate = null;
            OTAPI.Hooks.Net.RemoteClient.PreReset = null;

            _events.DeregisterHandlers(this, _log);
        }

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // =============================================================================================================
        // OTAPI hooks
        //

        private OTAPI.HookResult ReceiveDataHandler(
            Terraria.MessageBuffer buffer, ref byte packetId, ref int readOffset, ref int start, ref int length)
        {
            Debug.Assert(buffer != null);
            Debug.Assert(buffer.whoAmI >= 0 && buffer.whoAmI < Count);
            Debug.Assert(start >= 0 && start + length <= buffer.readBuffer.Length);
            Debug.Assert(length > 0);

            // Check `_ignoreGetData` to prevent infinite loops.
            if (_ignoreGetData)
            {
                return OTAPI.HookResult.Continue;
            }

            PacketHandler handler;
            var span = buffer.readBuffer.AsSpan(start..(start + length));
            if (packetId == (byte)PacketId.Module)
            {
                if (span.Length < 3)
                {
                    return OTAPI.HookResult.Cancel;
                }

                var moduleId = Unsafe.ReadUnaligned<ushort>(ref span.At(1));
                handler = _onReceiveModuleHandlers[moduleId] ?? OnReceivePacket<ModulePacket<UnknownModule>>;
            }
            else
            {
                handler = _onReceivePacketHandlers[packetId] ?? OnReceivePacket<UnknownPacket>;
            }

            handler(buffer.whoAmI, span);
            return OTAPI.HookResult.Cancel;
        }

        private OTAPI.HookResult SendBytesHandler(
            ref int playerIndex, ref byte[] data, ref int offset, ref int size,
            ref Terraria.Net.Sockets.SocketSendCallback callback, ref object state)
        {
            Debug.Assert(playerIndex >= 0 && playerIndex < Count);
            Debug.Assert(data != null);
            Debug.Assert(offset >= 0 && offset + size <= data.Length);
            Debug.Assert(size >= 3);

            var span = data.AsSpan((offset + 2)..(offset + size));
            var packetId = span.At(0);

            // The `SendBytes` event is only triggered for non-module packets.
            var handler = _onSendPacketHandlers[packetId] ?? OnSendPacket<UnknownPacket>;
            handler(playerIndex, span);
            return OTAPI.HookResult.Cancel;
        }

        private OTAPI.HookResult SendNetDataHandler(
            Terraria.Net.NetManager manager, Terraria.Net.Sockets.ISocket socket, ref Terraria.Net.NetPacket packet)
        {
            Debug.Assert(socket != null);
            Debug.Assert(packet.Buffer.Data != null);
            Debug.Assert(packet.Writer.BaseStream.Position >= 5);

            // Since we don't have an index, scan through the clients to find the player index.
            //
            // TODO: optimize this using a hash map, if needed
            var playerIndex = -1;
            for (var i = 0; i < Terraria.Netplay.MaxConnections; ++i)
            {
                if (Terraria.Netplay.Clients[i].Socket == socket)
                {
                    playerIndex = i;
                    break;
                }
            }

            Debug.Assert(playerIndex >= 0 && playerIndex < Count);

            var span = packet.Buffer.Data.AsSpan(2..((int)packet.Writer.BaseStream.Position));
            var moduleId = Unsafe.ReadUnaligned<ushort>(ref span.At(1));

            // The `SendBytes` event is only triggered for module packets.
            var handler = _onSendModuleHandlers[moduleId] ?? OnSendPacket<ModulePacket<UnknownModule>>;
            handler(playerIndex, span);
            return OTAPI.HookResult.Cancel;
        }

        private OTAPI.HookResult PreUpdateHandler(Terraria.Player terrariaPlayer, ref int playerIndex)
        {
            Debug.Assert(playerIndex >= 0 && playerIndex < Count);

            var player = this[playerIndex];
            var evt = new PlayerTickEvent(player);
            _events.Raise(evt, _log);
            return evt.IsCanceled ? OTAPI.HookResult.Cancel : OTAPI.HookResult.Continue;
        }

        private OTAPI.HookResult PreResetHandler(Terraria.RemoteClient remoteClient)
        {
            Debug.Assert(remoteClient != null);
            Debug.Assert(remoteClient.Id >= 0 && remoteClient.Id < Count);

            // Check if the client was active since this gets called when setting up `RemoteClient` as well.
            if (!remoteClient.IsActive)
            {
                return OTAPI.HookResult.Continue;
            }

            var player = this[remoteClient.Id];
            var evt = new PlayerQuitEvent(player);
            _events.Raise(evt, _log);
            return OTAPI.HookResult.Continue;
        }

        // =============================================================================================================
        // Packet event publishers
        //

        private void OnReceivePacket<TPacket>(int playerIndex, Span<byte> span) where TPacket : IPacket
        {
            var packet = MakePacket<TPacket>(span);

            // Read the packet using the `Server` context since we're receiving this packet.
            var packetBodyLength = packet.ReadBody(span[1..], PacketContext.Server);
            Debug.Assert(packetBodyLength == span.Length - 1);

            this[playerIndex].ReceivePacket(packet);
        }

        private void OnSendPacket<TPacket>(int playerIndex, Span<byte> span) where TPacket : IPacket
        {
            var packet = MakePacket<TPacket>(span);

            // Read the packet using the `Client` context since we're sending this packet.
            var packetBodyLength = packet.ReadBody(span[1..], PacketContext.Client);
            Debug.Assert(packetBodyLength == span.Length - 1);

            this[playerIndex].SendPacket(packet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TPacket MakePacket<TPacket>(Span<byte> span) where TPacket : IPacket
        {
            TPacket packet = default;

            // `UnknownPacket` is a special case since it has no default constructor.
            if (typeof(TPacket) == typeof(UnknownPacket))
            {
                return (TPacket)(object)new UnknownPacket(span.Length - 1, (PacketId)span[0]);
            }
            else if (packet is null)
            {
                return (TPacket)Activator.CreateInstance(typeof(TPacket))!;
            }

            return packet;
        }

        // =============================================================================================================
        // Player event publishers
        //

        [EventHandler("orion-players", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnPlayerJoin(PacketReceiveEvent<PlayerJoin> evt)
        {
            _events.Forward(evt, new PlayerJoinEvent(evt.Sender), _log);
        }

        [EventHandler("orion-players", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnPlayerHealth(PacketReceiveEvent<PlayerHealth> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new PlayerHealthEvent(evt.Sender, packet.Health, packet.MaxHealth), _log);
        }

        [EventHandler("orion-players", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnPlayerPvp(PacketReceiveEvent<PlayerPvp> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new PlayerPvpEvent(evt.Sender, packet.IsInPvp), _log);
        }

        [EventHandler("orion-players", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnPasswordResponse(PacketReceiveEvent<PasswordResponse> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new PlayerPasswordEvent(evt.Sender, packet.Password), _log);
        }

        [EventHandler("orion-players", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnPlayerMana(PacketReceiveEvent<PlayerMana> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new PlayerManaEvent(evt.Sender, packet.Mana, packet.MaxMana), _log);
        }

        [EventHandler("orion-players", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnPlayerTeam(PacketReceiveEvent<PlayerTeam> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new PlayerTeamEvent(evt.Sender, packet.Team), _log);
        }

        [EventHandler("orion-players", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnClientUuid(PacketReceiveEvent<ClientUuid> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new PlayerUuidEvent(evt.Sender, packet.Uuid), _log);
        }

        [EventHandler("orion-players", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnChat(PacketReceiveEvent<ModulePacket<Chat>> evt)
        {
            var module = evt.Packet.Module;

            _events.Forward(evt, new PlayerChatEvent(evt.Sender, module.ClientCommand, module.ClientMessage), _log);
        }
    }
}
