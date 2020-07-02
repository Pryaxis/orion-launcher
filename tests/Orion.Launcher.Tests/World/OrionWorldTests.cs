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
using System.Linq.Expressions;
using Moq;
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.World;
using Orion.Core.Events.World.Tiles;
using Orion.Core.Packets;
using Orion.Core.Packets.DataStructures;
using Orion.Core.Packets.World.Tiles;
using Orion.Core.Players;
using Orion.Core.World;
using Orion.Core.World.Tiles;
using Orion.Launcher.World;
using Serilog;
using Xunit;

namespace Orion.Launcher
{
    // These tests depend on Terraria state.
    [Collection("TerrariaTestsCollection")]
    [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Testing")]
    public partial class OrionWorldTests
    {
        [Fact]
        public void Item_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0].BlockId = BlockId.Stone;

            Assert.Equal(BlockId.Stone, world[0, 0].BlockId);
        }

        [Fact]
        public void Width_Get()
        {
            Terraria.Main.maxTilesX = 8400;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(8400, world.Width);
        }

        [Fact]
        public void Height_Get()
        {
            Terraria.Main.maxTilesY = 2400;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(2400, world.Height);
        }

        [Fact]
        public void Name_GetNullValue()
        {
            Terraria.Main.worldName = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal("", world.Name);
        }

        [Fact]
        public void Name_Get()
        {
            Terraria.Main.worldName = "test";

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal("test", world.Name);
        }

        [Fact]
        public void Evil_Get_ReturnsCorruption()
        {
            Terraria.WorldGen.crimson = false;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(WorldEvil.Corruption, world.Evil);
        }

        [Fact]
        public void Evil_Get_ReturnsCrimson()
        {
            Terraria.WorldGen.crimson = true;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(WorldEvil.Crimson, world.Evil);
        }

        [Fact]
        public void Evil_SetCorruption()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world.Evil = WorldEvil.Corruption;

            Assert.False(Terraria.WorldGen.crimson);
        }

        [Fact]
        public void Evil_SetCrimson()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world.Evil = WorldEvil.Crimson;

            Assert.True(Terraria.WorldGen.crimson);
        }

        [Fact]
        public void Difficulty_Get()
        {
            Terraria.Main.GameMode = (int)WorldDifficulty.Master;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(WorldDifficulty.Master, world.Difficulty);
        }

        [Fact]
        public void Difficulty_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world.Difficulty = WorldDifficulty.Master;

            Assert.Equal(WorldDifficulty.Master, (WorldDifficulty)Terraria.Main.GameMode);
        }

        [Fact]
        public void WorldSave_EventTriggered()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.Is<WorldSaveEvent>(evt => evt.World == world), log));

            Terraria.IO.WorldFile.SaveWorld(false, true);

            Assert.Equal(13500.0, Terraria.IO.WorldFile._tempTime);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void WorldSave_EventCanceled()
        {
            // Clear the time so we know it's 0.
            Terraria.IO.WorldFile._tempTime = 0.0;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WorldSaveEvent>(), log))
                .Callback<WorldSaveEvent, ILogger>((evt, log) => evt.Cancel());

            Terraria.IO.WorldFile.SaveWorld(false, true);

            Assert.Equal(0.0, Terraria.IO.WorldFile._tempTime);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlock_EventTriggered()
        {
            var packet = new TileModify { X = 100, Y = 256, Modification = TileModify.TileModification.BreakBlock };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<TileModify, BlockBreakEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256 && !evt.IsItemless);
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlock_EventCanceled()
        {
            var packet = new TileModify { X = 100, Y = 256, Modification = TileModify.TileModification.BreakBlock };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<TileModify, BlockBreakEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlockFailure_EventNotTriggered()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.BreakBlock,
                IsFailure = true
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventNotTriggered<TileModify, BlockBreakEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileModify_PlaceBlock_EventTriggered()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.PlaceBlock,
                BlockId = BlockId.Torches,
                BlockStyle = 1
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<TileModify, BlockPlaceEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256 && evt.Id == BlockId.Torches &&
                    evt.Style == 1 && !evt.IsReplacement);
        }

        [Fact]
        public void PacketReceive_TileModify_PlaceBlock_EventCanceled()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.PlaceBlock,
                BlockId = BlockId.Torches,
                BlockStyle = 1
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<TileModify, BlockPlaceEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileModify_BreakWall_EventTriggered()
        {
            var packet = new TileModify { X = 100, Y = 256, Modification = TileModify.TileModification.BreakWall };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<TileModify, WallBreakEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256);
        }

        [Fact]
        public void PacketReceive_TileModify_BreakWall_EventCanceled()
        {
            var packet = new TileModify { X = 100, Y = 256, Modification = TileModify.TileModification.BreakWall };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<TileModify, WallBreakEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileModify_BreakWallFailure_EventNotTriggered()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.BreakWall,
                IsFailure = true
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventNotTriggered<TileModify, WallBreakEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileModify_PlaceWall_EventTriggered()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.PlaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<TileModify, WallPlaceEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256 && evt.Id == WallId.Stone &&
                    !evt.IsReplacement);
        }

        [Fact]
        public void PacketReceive_TileModify_PlaceWall_EventCanceled()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.PlaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<TileModify, WallPlaceEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlockItemless_EventTriggered()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.BreakBlockItemless
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<TileModify, BlockBreakEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256 && evt.IsItemless);
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlockItemless_EventCanceled()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.BreakBlockItemless
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<TileModify, BlockBreakEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlockItemlessFailure_EventNotTriggered()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.BreakBlockItemless,
                IsFailure = true
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventNotTriggered<TileModify, BlockBreakEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileModify_ReplaceBlock_EventTriggered()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.ReplaceBlock,
                BlockId = BlockId.Stone
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<TileModify, BlockPlaceEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256 && evt.Id == BlockId.Stone &&
                    evt.Style == 0 && evt.IsReplacement);
        }

        [Fact]
        public void PacketReceive_TileModify_ReplaceBlock_EventCanceled()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.ReplaceBlock,
                BlockId = BlockId.Stone
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<TileModify, BlockPlaceEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileModify_ReplaceWall_EventTriggered()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.ReplaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<TileModify, WallPlaceEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256 && evt.Id == WallId.Stone &&
                    evt.IsReplacement);
        }

        [Fact]
        public void PacketReceive_TileModify_ReplaceWall_EventCanceled()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.ReplaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<TileModify, WallPlaceEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileModify_InvalidModification()
        {
            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = (TileModify.TileModification)255
            };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventNotTriggered<TileModify, BlockBreakEvent>(packet, sender);
            PacketReceive_EventNotTriggered<TileModify, BlockPlaceEvent>(packet, sender);
            PacketReceive_EventNotTriggered<TileModify, WallBreakEvent>(packet, sender);
            PacketReceive_EventNotTriggered<TileModify, WallPlaceEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileSquarePacket_EventTriggered()
        {
            var packet = new TileSquare { X = 100, Y = 256, Tiles = new NetworkTileSlice(3, 3) };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<TileSquare, TileSquareEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256 && evt.Tiles.Width == 3 &&
                    evt.Tiles.Height == 3);
        }

        [Fact]
        public void PacketReceive_TileSquarePacket_EventCanceled()
        {
            var packet = new TileSquare { X = 100, Y = 256, Tiles = new NetworkTileSlice(3, 3) };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<TileSquare, TileSquareEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_TileLiquidPacket_EventTriggered()
        {
            var packet = new TileLiquid { X = 100, Y = 256, LiquidAmount = 255, LiquidType = LiquidType.Honey };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<TileLiquid, TileLiquidEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256 &&
                    evt.Liquid == new Liquid(LiquidType.Honey, 255));
        }

        [Fact]
        public void PacketReceive_TileLiquidPacket_EventCanceled()
        {
            var packet = new TileLiquid { X = 100, Y = 256, LiquidAmount = 255, LiquidType = LiquidType.Honey };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<TileLiquid, TileLiquidEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_WireActivatePacket_EventTriggered()
        {
            var packet = new WireActivate { X = 100, Y = 256 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<WireActivate, WiringActivateEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256);
        }

        [Fact]
        public void PacketReceive_WireActivatePacket_EventCanceled()
        {
            var packet = new WireActivate { X = 100, Y = 256 };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<WireActivate, WiringActivateEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_BlockPaintPacket_EventTriggered()
        {
            var packet = new BlockPaint { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<BlockPaint, BlockPaintEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256 && evt.Color == PaintColor.Red);
        }

        [Fact]
        public void PacketReceive_BlockPaintPacket_EventCanceled()
        {
            var packet = new BlockPaint { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<BlockPaint, BlockPaintEvent>(packet, sender);
        }

        [Fact]
        public void PacketReceive_WallPaintPacket_EventTriggered()
        {
            var packet = new WallPaint { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventTriggered<WallPaint, WallPaintEvent>(packet, sender,
                evt => evt.Player == sender && evt.X == 100 && evt.Y == 256 && evt.Color == PaintColor.Red);
        }

        [Fact]
        public void PacketReceive_WallPaintPacket_EventCanceled()
        {
            var packet = new WallPaint { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();

            PacketReceive_EventCanceled<WallPaint, WallPaintEvent>(packet, sender);
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

            using var world = new OrionWorld(events, log);

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

            using var world = new OrionWorld(events, log);

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

            using var world = new OrionWorld(events, log);

            var evt = new PacketReceiveEvent<TPacket>(packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<TEvent>(), log), Times.Never);
        }
    }
}
