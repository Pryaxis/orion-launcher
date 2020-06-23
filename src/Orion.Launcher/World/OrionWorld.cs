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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Orion.Core;
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.World;
using Orion.Core.Events.World.Tiles;
using Orion.Core.Packets.World.Tiles;
using Orion.Core.World;
using Orion.Core.World.Tiles;
using Serilog;

namespace Orion.Launcher.World
{
    [Binding("orion-world", Author = "Pryaxis", Priority = BindingPriority.Lowest)]
    internal sealed partial class OrionWorld : IWorld, IDisposable
    {
        private readonly IEventManager _events;
        private readonly ILogger _log;

        private unsafe Tile* _tiles;

        public OrionWorld(IEventManager events, ILogger log)
        {
            Debug.Assert(events != null);
            Debug.Assert(log != null);

            _events = events;
            _log = log;

            // Replace `Terraria.Main.tile` with our own implementation which involves using the `OrionWorld` class
            // along with an adapter for the `OTAPI.Tile.ITile` interface. This cuts down on the memory usage
            // significantly while not impacting speed very much.
            Terraria.Main.tile = new TileCollection(this);

            OTAPI.Hooks.World.IO.PostLoadWorld = PostLoadWorldHandler;
            OTAPI.Hooks.World.IO.PreSaveWorld = PreSaveWorldHandler;

            _events.RegisterHandlers(this, _log);
        }

        [ExcludeFromCodeCoverage]
        unsafe ~OrionWorld()
        {
            DisposeUnmanaged();
        }

        public unsafe ref Tile this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(x >= 0 && x <= Width);
                Debug.Assert(y >= 0 && y <= Height);

                if (_tiles is null)
                {
                    // Allocate the `Tile` array in unmanaged memory so that it doesn't need to be pinned. The bounds
                    // are increased by 1 to fix some OOB issues in world generation code.
                    _tiles = (Tile*)Marshal.AllocHGlobal(sizeof(Tile) * (Width + 1) * (Height + 1));
                }

                return ref _tiles[y * Width + x];
            }
        }

        public int Width => Terraria.Main.maxTilesX;
        public int Height => Terraria.Main.maxTilesY;

        public string Name => Terraria.Main.worldName ?? string.Empty;

        public WorldDifficulty Difficulty
        {
            get => (WorldDifficulty)Terraria.Main.GameMode;
            set => Terraria.Main.GameMode = (int)value;
        }

        public unsafe void Dispose()
        {
            DisposeUnmanaged();
            GC.SuppressFinalize(this);

            // Replace the original `Terraria.Main.tile` implementation using a reflection hack.
            Terraria.Main.tile =
                (OTAPI.Tile.ITileCollection)typeof(OTAPI.Hooks).Assembly
                    .GetType("OTAPI.Callbacks.Terraria.Collection")!
                    .GetMethod("Create")!
                    .Invoke(null, null)!;

            OTAPI.Hooks.World.IO.PostLoadWorld = null;
            OTAPI.Hooks.World.IO.PreSaveWorld = null;

            _events.DeregisterHandlers(this, _log);
        }

        private unsafe void DisposeUnmanaged()
        {
            if (_tiles != null)
            {
                Marshal.FreeHGlobal((IntPtr)_tiles);
            }
        }

        // =============================================================================================================
        // OTAPI hooks
        //

        private void PostLoadWorldHandler(bool loadFromCloud)
        {
            var evt = new WorldLoadedEvent(this);
            _events.Raise(evt, _log);
        }

        private OTAPI.HookResult PreSaveWorldHandler(ref bool useCloudSaving, ref bool resetTime)
        {
            var evt = new WorldSaveEvent(this);
            _events.Raise(evt, _log);
            return evt.IsCanceled ? OTAPI.HookResult.Cancel : OTAPI.HookResult.Continue;
        }

        // =============================================================================================================
        // World event publishers
        //

        [EventHandler("orion-world", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnTileModifyPacket(PacketReceiveEvent<TileModifyPacket> evt)
        {
            ref var packet = ref evt.Packet;

            var newEvt = packet.Modification switch
            {
                TileModification.BreakBlock => RaiseBlockBreak(ref packet, false),
                TileModification.PlaceBlock => RaiseBlockPlace(ref packet, false),
                TileModification.BreakWall => RaiseWallBreak(ref packet),
                TileModification.PlaceWall => RaiseWallPlace(ref packet, false),
                TileModification.BreakBlockItemless => RaiseBlockBreak(ref packet, true),
                TileModification.ReplaceBlock => RaiseBlockPlace(ref packet, true),
                TileModification.ReplaceWall => RaiseWallPlace(ref packet, true),

                _ => null
            };
            if (newEvt?.IsCanceled == true)
            {
                evt.Cancel(newEvt.CancellationReason);
            }

            Event Raise<TEvent>(TEvent newEvt) where TEvent : Event
            {
                _events.Raise(newEvt, _log);
                return newEvt;
            }

            Event? RaiseBlockBreak(ref TileModifyPacket packet, bool isItemless) =>
                packet.IsFailure ? null : Raise(new BlockBreakEvent(this, evt.Sender, packet.X, packet.Y, isItemless));

            Event RaiseBlockPlace(ref TileModifyPacket packet, bool isReplacement) =>
                Raise(new BlockPlaceEvent(
                    this, evt.Sender, packet.X, packet.Y, packet.BlockId, packet.BlockStyle, isReplacement));

            Event? RaiseWallBreak(ref TileModifyPacket packet) =>
                packet.IsFailure ? null : Raise(new WallBreakEvent(this, evt.Sender, packet.X, packet.Y));

            Event RaiseWallPlace(ref TileModifyPacket packet, bool isReplacement) =>
                Raise(new WallPlaceEvent(this, evt.Sender, packet.X, packet.Y, packet.WallId, isReplacement));
        }

        [EventHandler("orion-world", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnTileSquarePacket(PacketReceiveEvent<TileSquarePacket> evt)
        {
            ref var packet = ref evt.Packet;

            _events.Forward(evt, new TileSquareEvent(this, evt.Sender, packet.X, packet.Y, packet.Tiles), _log);
        }

        [EventHandler("orion-world", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnTileLiquidPacket(PacketReceiveEvent<TileLiquidPacket> evt)
        {
            ref var packet = ref evt.Packet;

            _events.Forward(
                evt, new TileLiquidEvent(this, evt.Sender, packet.X, packet.Y, packet.LiquidAmount, packet.Liquid),
                _log);
        }

        [EventHandler("orion-world", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnWireActivatePacket(PacketReceiveEvent<WireActivatePacket> evt)
        {
            ref var packet = ref evt.Packet;

            _events.Forward(evt, new WiringActivateEvent(this, evt.Sender, packet.X, packet.Y), _log);
        }

        [EventHandler("orion-world", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnBlockPaintPacket(PacketReceiveEvent<BlockPaintPacket> evt)
        {
            ref var packet = ref evt.Packet;

            _events.Forward(evt, new BlockPaintEvent(this, evt.Sender, packet.X, packet.Y, packet.Color), _log);
        }

        [EventHandler("orion-world", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnWallPaintPacket(PacketReceiveEvent<WallPaintPacket> evt)
        {
            ref var packet = ref evt.Packet;

            _events.Forward(evt, new WallPaintEvent(this, evt.Sender, packet.X, packet.Y, packet.Color), _log);
        }

        // Forwards `evt` as `newEvt`.
        private void ForwardEvent<TEvent>(Event evt, TEvent newEvt) where TEvent : Event
        {
            _events.Raise(newEvt, _log);
            if (newEvt.IsCanceled)
            {
                evt.Cancel(newEvt.CancellationReason);
            }
        }
    }
}
