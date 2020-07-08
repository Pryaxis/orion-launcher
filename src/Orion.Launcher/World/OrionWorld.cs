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
using System.Collections.Generic;
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
        private delegate void TileModifyHandler(PacketReceiveEvent<TileModify> evt);

        private readonly IEventManager _events;
        private readonly ILogger _log;

        private readonly Dictionary<TileModify.TileModification, TileModifyHandler> _tileModifyHandlers;

        private unsafe Tile* _tiles;

        public OrionWorld(IEventManager events, ILogger log)
        {
            Debug.Assert(events != null);
            Debug.Assert(log != null);

            _events = events;
            _log = log;

            _tileModifyHandlers = new Dictionary<TileModify.TileModification, TileModifyHandler>
            {
                [TileModify.TileModification.BreakBlock] = OnTileModifyBreakBlock,
                [TileModify.TileModification.PlaceBlock] = OnTileModifyPlaceBlock,
                [TileModify.TileModification.BreakWall] = OnTileModifyBreakWall,
                [TileModify.TileModification.PlaceWall] = OnTileModifyPlaceWall,
                [TileModify.TileModification.BreakBlockItemless] = OnTileModifyBreakBlockItemless,
                [TileModify.TileModification.ReplaceBlock] = OnTileModifyReplaceBlock,
                [TileModify.TileModification.ReplaceWall] = OnTileModifyReplaceWall
            };

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
                Debug.Assert(0 <= x && x <= Width);
                Debug.Assert(0 <= y && y <= Height);

                AllocateUnmanaged();

                return ref _tiles[(y * Width) + x];
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

        public WorldEvil Evil
        {
            get => Terraria.WorldGen.crimson ? WorldEvil.Crimson : WorldEvil.Corruption;
            set => Terraria.WorldGen.crimson = value == WorldEvil.Crimson;
        }

        public AnglerQuest AnglerQuest
        {
            get => (AnglerQuest)Terraria.Main.anglerQuest;
            set => Terraria.Main.anglerQuest = (int)value;
        }

        public int BaseNpcSpawnRate
        {
            get => Terraria.NPC.defaultSpawnRate;
            set => Terraria.NPC.defaultSpawnRate = value;
        }

        public int BaseNpcSpawnLimit
        {
            get => Terraria.NPC.defaultMaxSpawns;
            set => Terraria.NPC.defaultMaxSpawns = value;
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

            if (_tileModifyHandlers.TryGetValue(packet.Modification, out var handler))
            {
                handler(evt);
            }
        }

        private void OnTileModifyBreakBlock(PacketReceiveEvent<TileModify> evt)
        {
            var packet = evt.Packet;
            if (packet.Data != 0)
            {
                return;
            }

            _events.Forward(evt, new BlockBreakEvent(this, evt.Sender, packet.X, packet.Y, false), _log);
        }

        private void OnTileModifyPlaceBlock(PacketReceiveEvent<TileModify> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt,
                new BlockPlaceEvent(
                    this, evt.Sender, packet.X, packet.Y, (BlockId)(packet.Data + 1), packet.Data2, false),
                _log);
        }

        private void OnTileModifyBreakWall(PacketReceiveEvent<TileModify> evt)
        {
            var packet = evt.Packet;
            if (packet.Data != 0)
            {
                return;
            }

            _events.Forward(evt, new WallBreakEvent(this, evt.Sender, packet.X, packet.Y), _log);
        }

        private void OnTileModifyPlaceWall(PacketReceiveEvent<TileModify> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new WallPlaceEvent(this, evt.Sender, packet.X, packet.Y, (WallId)packet.Data, false),
                _log);
        }

        private void OnTileModifyBreakBlockItemless(PacketReceiveEvent<TileModify> evt)
        {
            var packet = evt.Packet;
            if (packet.Data != 0)
            {
                return;
            }

            _events.Forward(evt, new BlockBreakEvent(this, evt.Sender, packet.X, packet.Y, true), _log);
        }

        private void OnTileModifyReplaceBlock(PacketReceiveEvent<TileModify> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt,
                new BlockPlaceEvent(
                    this, evt.Sender, packet.X, packet.Y, (BlockId)(packet.Data + 1), packet.Data2, true),
                _log);
        }

        private void OnTileModifyReplaceWall(PacketReceiveEvent<TileModify> evt)
        {
            var packet = evt.Packet;

            _events.Forward(evt, new WallPlaceEvent(this, evt.Sender, packet.X, packet.Y, (WallId)packet.Data, true),
                _log);
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
            var liquid = new Liquid(packet.Type, packet.Amount);

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
