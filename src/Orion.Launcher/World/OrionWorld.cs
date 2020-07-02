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
using Destructurama.Attributed;
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
    [LogAsScalar]
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

                AllocateUnmanaged();

                return ref _tiles[y * Width + x];
            }
        }

        public int Width => Terraria.Main.maxTilesX;

        public int Height => Terraria.Main.maxTilesY;

        public string Name => Terraria.Main.worldName ?? string.Empty;

        public WorldEvil Evil
        {
            get => Terraria.WorldGen.crimson ? WorldEvil.Crimson : WorldEvil.Corruption;
            set => Terraria.WorldGen.crimson = value == WorldEvil.Crimson;
        }

        public WorldDifficulty Difficulty
        {
            get => (WorldDifficulty)Terraria.Main.GameMode;
            set => Terraria.Main.GameMode = (int)value;
        }

        public unsafe void Dispose()
        {
            DisposeUnmanaged();
            GC.SuppressFinalize(this);

            // HACK: replace the original `Terraria.Main.tile` implementation using reflection.
            Terraria.Main.tile =
                (OTAPI.Tile.ITileCollection)typeof(OTAPI.Hooks).Assembly
                    .GetType("OTAPI.Callbacks.Terraria.Collection")!
                    .GetMethod("Create")!
                    .Invoke(null, null)!;

            OTAPI.Hooks.World.IO.PostLoadWorld = null;
            OTAPI.Hooks.World.IO.PreSaveWorld = null;

            _events.DeregisterHandlers(this, _log);
        }

        private unsafe void AllocateUnmanaged()
        {
            if (_tiles is null)
            {
                // Allocate the `Tile` array in unmanaged memory so that it doesn't need to be pinned. The bounds are
                // increased by 1 to fix some OOB issues in world generation code.
                var size = sizeof(Tile) * (Width + 1) * (Height + 1);

                _tiles = (Tile*)Marshal.AllocHGlobal(size);
                GC.AddMemoryPressure(size);
            }
        }

        private unsafe void DisposeUnmanaged()
        {
            if (_tiles != null)
            {
                var size = sizeof(Tile) * (Width + 1) * (Height + 1);

                Marshal.FreeHGlobal((IntPtr)_tiles);
                GC.RemoveMemoryPressure(size);
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
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnTileModify(PacketReceiveEvent<TileModify> evt)
        {
            var packet = evt.Packet;

            var newEvt = packet.Modification switch
            {
                TileModify.TileModification.BreakBlock => RaiseBlockBreak(packet, false),
                TileModify.TileModification.PlaceBlock => RaiseBlockPlace(packet, false),
                TileModify.TileModification.BreakWall => RaiseWallBreak(packet),
                TileModify.TileModification.PlaceWall => RaiseWallPlace(packet, false),
                TileModify.TileModification.BreakBlockItemless => RaiseBlockBreak(packet, true),
                TileModify.TileModification.ReplaceBlock => RaiseBlockPlace(packet, true),
                TileModify.TileModification.ReplaceWall => RaiseWallPlace(packet, true),

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

            Event? RaiseBlockBreak(TileModify packet, bool isItemless) =>
                packet.IsFailure ? null : Raise(new BlockBreakEvent(this, evt.Sender, packet.X, packet.Y, isItemless));

            Event RaiseBlockPlace(TileModify packet, bool isReplacement) =>
                Raise(new BlockPlaceEvent(
                    this, evt.Sender, packet.X, packet.Y, packet.BlockId, packet.BlockStyle, isReplacement));

            Event? RaiseWallBreak(TileModify packet) =>
                packet.IsFailure ? null : Raise(new WallBreakEvent(this, evt.Sender, packet.X, packet.Y));

            Event RaiseWallPlace(TileModify packet, bool isReplacement) =>
                Raise(new WallPlaceEvent(this, evt.Sender, packet.X, packet.Y, packet.WallId, isReplacement));
        }

        [EventHandler("orion-world", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnTileSquare(PacketReceiveEvent<TileSquare> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new TileSquareEvent(this, evt.Sender, packet.X, packet.Y, packet.Tiles), _log);
        }

        [EventHandler("orion-world", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnTileLiquid(PacketReceiveEvent<TileLiquid> evt)
        {
            var packet = evt.Packet;
            var liquid = new Liquid(packet.LiquidType, packet.LiquidAmount);

            _events.Forward(evt, new TileLiquidEvent(this, evt.Sender, packet.X, packet.Y, liquid), _log);
        }

        [EventHandler("orion-world", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnWireActivate(PacketReceiveEvent<WireActivate> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new WiringActivateEvent(this, evt.Sender, packet.X, packet.Y), _log);
        }

        [EventHandler("orion-world", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnBlockPaint(PacketReceiveEvent<BlockPaint> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new BlockPaintEvent(this, evt.Sender, packet.X, packet.Y, packet.Color), _log);
        }

        [EventHandler("orion-world", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicit usage")]
        private void OnWallPaint(PacketReceiveEvent<WallPaint> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new WallPaintEvent(this, evt.Sender, packet.X, packet.Y, packet.Color), _log);
        }
    }
}
