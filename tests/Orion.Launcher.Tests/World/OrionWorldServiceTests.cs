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
using Moq;
using Orion.Core;
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.World;
using Orion.Core.Events.World.Tiles;
using Orion.Core.Packets.World.Tiles;
using Orion.Core.Players;
using Orion.Core.World.Tiles;
using Serilog;
using Xunit;

namespace Orion.Launcher.World
{
    // These tests depend on Terraria state.
    [Collection("TerrariaTestsCollection")]
    public class OrionWorldServiceTests
    {
        private static readonly byte[] _breakBlockBytes = { 11, 0, 17, 0, 100, 0, 0, 1, 0, 0, 0 };
        private static readonly byte[] _breakBlockFailureBytes = { 11, 0, 17, 0, 100, 0, 0, 1, 1, 0, 0 };
        private static readonly byte[] _placeBlockBytes = { 11, 0, 17, 1, 100, 0, 0, 1, 4, 0, 1 };
        private static readonly byte[] _breakWallBytes = { 11, 0, 17, 2, 100, 0, 0, 1, 0, 0, 0 };
        private static readonly byte[] _breakWallFailureBytes = { 11, 0, 17, 2, 100, 0, 0, 1, 1, 0, 0 };
        private static readonly byte[] _placeWallBytes = { 11, 0, 17, 3, 100, 0, 0, 1, 1, 0, 0 };
        private static readonly byte[] _breakBlockItemlessBytes = { 11, 0, 17, 4, 100, 0, 0, 1, 0, 0, 0 };
        private static readonly byte[] _breakBlockItemlessFailureBytes = { 11, 0, 17, 4, 100, 0, 0, 1, 1, 0, 0 };
        private static readonly byte[] _replaceBlockBytes = { 11, 0, 17, 21, 100, 0, 0, 1, 1, 0, 0 };
        private static readonly byte[] _replaceWallBytes = { 11, 0, 17, 22, 100, 0, 0, 1, 1, 0, 0 };

        private static readonly byte[] _tileSquareBytes =
        {
            41, 0, 20, 3, 0, 100, 0, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 4, 0, 1, 0, 2, 0, 4, 0, 1, 0, 8, 0, 255, 1, 0, 4, 1,
            0, 8, 1, 240, 131, 0, 0
        };

        private static readonly byte[] _tileLiquidBytes = { 9, 0, 48, 0, 1, 100, 0, 255, 2 };
        private static readonly byte[] _wireActivateBytes = { 7, 0, 59, 0, 1, 100, 0 };
        private static readonly byte[] _blockPaintBytes = { 8, 0, 63, 0, 1, 100, 0, 1 };
        private static readonly byte[] _wallPaintBytes = { 8, 0, 64, 0, 1, 100, 0, 1 };

        [Fact]
        public void Main_tile_Width_Get()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Assert.Equal(Terraria.Main.maxTilesX, Terraria.Main.tile.Width);
        }

        [Fact]
        public void Main_tile_Height_Get()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Assert.Equal(Terraria.Main.maxTilesY, Terraria.Main.tile.Height);
        }

        [Fact]
        public void Main_tile_type_Get()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { BlockId = BlockId.Stone };

            Assert.Equal(BlockId.Stone, (BlockId)Terraria.Main.tile[0, 0].type);
        }

        [Fact]
        public void Main_tile_type_Set()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Terraria.Main.tile[0, 0].type = (ushort)BlockId.Stone;

            Assert.Equal(BlockId.Stone, worldService.World[0, 0].BlockId);
        }

        [Fact]
        public void Main_tile_wall_Get()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { WallId = WallId.Stone };

            Assert.Equal(WallId.Stone, (WallId)Terraria.Main.tile[0, 0].wall);
        }

        [Fact]
        public void Main_tile_wall_Set()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Terraria.Main.tile[0, 0].wall = (ushort)WallId.Stone;

            Assert.Equal(WallId.Stone, worldService.World[0, 0].WallId);
        }

        [Fact]
        public void Main_tile_liquid_Get()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { LiquidAmount = 100 };

            Assert.Equal(100, Terraria.Main.tile[0, 0].liquid);
        }

        [Fact]
        public void Main_tile_liquid_Set()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Terraria.Main.tile[0, 0].liquid = 100;

            Assert.Equal(100, worldService.World[0, 0].LiquidAmount);
        }

        [Fact]
        public void Main_tile_sTileHeader_Get()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { HeaderPart = 12345 };

            Assert.Equal(12345, Terraria.Main.tile[0, 0].sTileHeader);
        }

        [Fact]
        public void Main_tile_sTileHeader_Set()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Terraria.Main.tile[0, 0].sTileHeader = 12345;

            Assert.Equal(12345, worldService.World[0, 0].HeaderPart);
        }

        [Fact]
        public void Main_tile_bTileHeader_Get()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { HeaderPart2 = 100 };

            Assert.Equal(100, Terraria.Main.tile[0, 0].bTileHeader);
        }

        [Fact]
        public void Main_tile_bTileHeader_Set()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Terraria.Main.tile[0, 0].bTileHeader = 100;

            Assert.Equal(100, worldService.World[0, 0].HeaderPart2);
        }

        [Fact]
        public void Main_tile_bTileHeader3_Get()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { HeaderPart3 = 100 };

            Assert.Equal(100, Terraria.Main.tile[0, 0].bTileHeader3);
        }

        [Fact]
        public void Main_tile_bTileHeader3_Set()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Terraria.Main.tile[0, 0].bTileHeader3 = 100;

            Assert.Equal(100, worldService.World[0, 0].HeaderPart3);
        }

        [Fact]
        public void Main_tile_frameX_Get()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { BlockFrameX = 12345 };

            Assert.Equal(12345, Terraria.Main.tile[0, 0].frameX);
        }

        [Fact]
        public void Main_tile_frameX_Set()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Terraria.Main.tile[0, 0].frameX = 12345;

            Assert.Equal(12345, worldService.World[0, 0].BlockFrameX);
        }

        [Fact]
        public void Main_tile_frameY_Get()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { BlockFrameY = 12345 };

            Assert.Equal(12345, Terraria.Main.tile[0, 0].frameY);
        }

        [Fact]
        public void Main_tile_frameY_Set()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Terraria.Main.tile[0, 0].frameY = 12345;

            Assert.Equal(12345, worldService.World[0, 0].BlockFrameY);
        }

        [Fact]
        public void Main_tile_color()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { BlockColor = PaintColor.Red };

            Assert.Equal((byte)PaintColor.Red, Terraria.Main.tile[0, 0].color());

            Terraria.Main.tile[0, 0].color((byte)PaintColor.DeepRed);

            Assert.Equal(PaintColor.DeepRed, worldService.World[0, 0].BlockColor);
        }

        [Fact]
        public void Main_tile_active()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { IsBlockActive = true };

            Assert.True(Terraria.Main.tile[0, 0].active());

            Terraria.Main.tile[0, 0].active(false);

            Assert.False(worldService.World[0, 0].IsBlockActive);
        }

        [Fact]
        public void Main_tile_inActive()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { IsBlockActuated = true };

            Assert.True(Terraria.Main.tile[0, 0].inActive());

            Terraria.Main.tile[0, 0].inActive(false);

            Assert.False(worldService.World[0, 0].IsBlockActuated);
        }

        [Fact]
        public void Main_tile_nactive()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { IsBlockActive = true };

            Assert.True(Terraria.Main.tile[0, 0].nactive());

            worldService.World[0, 0].IsBlockActuated = true;

            Assert.False(Terraria.Main.tile[0, 0].nactive());
        }

        [Fact]
        public void Main_tile_wire()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { HasRedWire = true };

            Assert.True(Terraria.Main.tile[0, 0].wire());

            Terraria.Main.tile[0, 0].wire(false);

            Assert.False(worldService.World[0, 0].HasRedWire);
        }

        [Fact]
        public void Main_tile_wire2()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { HasBlueWire = true };

            Assert.True(Terraria.Main.tile[0, 0].wire2());

            Terraria.Main.tile[0, 0].wire2(false);

            Assert.False(worldService.World[0, 0].HasBlueWire);
        }

        [Fact]
        public void Main_tile_wire3()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { HasGreenWire = true };

            Assert.True(Terraria.Main.tile[0, 0].wire3());

            Terraria.Main.tile[0, 0].wire3(false);

            Assert.False(worldService.World[0, 0].HasGreenWire);
        }

        [Fact]
        public void Main_tile_halfBrick()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { IsBlockHalved = true };

            Assert.True(Terraria.Main.tile[0, 0].halfBrick());

            Terraria.Main.tile[0, 0].halfBrick(false);

            Assert.False(worldService.World[0, 0].IsBlockHalved);
        }

        [Fact]
        public void Main_tile_actuator()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { HasActuator = true };

            Assert.True(Terraria.Main.tile[0, 0].actuator());

            Terraria.Main.tile[0, 0].actuator(false);

            Assert.False(worldService.World[0, 0].HasActuator);
        }

        [Fact]
        public void Main_tile_slope()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { Slope = Slope.BottomRight };

            Assert.Equal((byte)Slope.BottomRight, Terraria.Main.tile[0, 0].slope());

            Terraria.Main.tile[0, 0].slope((byte)Slope.BottomLeft);

            Assert.Equal(Slope.BottomLeft, worldService.World[0, 0].Slope);
        }

        [Fact]
        public void Main_tile_wallColor()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { WallColor = PaintColor.Red };

            Assert.Equal((byte)PaintColor.Red, Terraria.Main.tile[0, 0].wallColor());

            Terraria.Main.tile[0, 0].wallColor((byte)PaintColor.DeepRed);

            Assert.Equal(PaintColor.DeepRed, worldService.World[0, 0].WallColor);
        }

        [Fact]
        public void Main_tile_lava()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { Liquid = Liquid.Lava };

            Assert.True(Terraria.Main.tile[0, 0].lava());

            Terraria.Main.tile[0, 0].lava(false);

            Assert.NotEqual(Liquid.Lava, worldService.World[0, 0].Liquid);

            Terraria.Main.tile[0, 0].lava(true);

            Assert.Equal(Liquid.Lava, worldService.World[0, 0].Liquid);
        }

        [Fact]
        public void Main_tile_honey()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { Liquid = Liquid.Honey };

            Assert.True(Terraria.Main.tile[0, 0].honey());

            Terraria.Main.tile[0, 0].honey(false);

            Assert.NotEqual(Liquid.Honey, worldService.World[0, 0].Liquid);

            Terraria.Main.tile[0, 0].honey(true);

            Assert.Equal(Liquid.Honey, worldService.World[0, 0].Liquid);
        }

        [Fact]
        public void Main_tile_liquidType()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { Liquid = Liquid.Lava };

            Assert.Equal((byte)Liquid.Lava, Terraria.Main.tile[0, 0].liquidType());

            Terraria.Main.tile[0, 0].liquidType((int)Liquid.Honey);

            Assert.Equal(Liquid.Honey, worldService.World[0, 0].Liquid);
        }

        [Fact]
        public void Main_tile_wire4()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { HasYellowWire = true };

            Assert.True(Terraria.Main.tile[0, 0].wire4());

            Terraria.Main.tile[0, 0].wire4(false);

            Assert.False(worldService.World[0, 0].HasYellowWire);
        }

        [Fact]
        public void Main_tile_frameNumber()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { BlockFrameNumber = 7 };

            Assert.Equal(7, Terraria.Main.tile[0, 0].frameNumber());

            Terraria.Main.tile[0, 0].frameNumber(5);

            Assert.Equal(5, worldService.World[0, 0].BlockFrameNumber);
        }

        [Fact]
        public void Main_tile_checkingLiquid()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { IsCheckingLiquid = true };

            Assert.True(Terraria.Main.tile[0, 0].checkingLiquid());

            Terraria.Main.tile[0, 0].checkingLiquid(false);

            Assert.False(worldService.World[0, 0].IsCheckingLiquid);
        }

        [Fact]
        public void Main_tile_skipLiquid()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { ShouldSkipLiquid = true };

            Assert.True(Terraria.Main.tile[0, 0].skipLiquid());

            Terraria.Main.tile[0, 0].skipLiquid(false);

            Assert.False(worldService.World[0, 0].ShouldSkipLiquid);
        }

        [Fact]
        public void Main_tile_CopyFrom_NullTile()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                BlockId = (BlockId)1,
                WallId = (WallId)2,
                LiquidAmount = 3,
                HeaderPart = 12345,
                HeaderPart2 = 100,
                HeaderPart3 = 101,
                BlockFrameX = 4,
                BlockFrameY = 5
            };

            Terraria.Main.tile[0, 0].CopyFrom(null);

            Assert.Equal(BlockId.Dirt, worldService.World[0, 0].BlockId);
            Assert.Equal(WallId.None, worldService.World[0, 0].WallId);
            Assert.Equal(0, worldService.World[0, 0].LiquidAmount);
            Assert.Equal(0, worldService.World[0, 0].HeaderPart);
            Assert.Equal(0, worldService.World[0, 0].HeaderPart2);
            Assert.Equal(0, worldService.World[0, 0].HeaderPart3);
            Assert.Equal(0, worldService.World[0, 0].BlockFrameX);
            Assert.Equal(0, worldService.World[0, 0].BlockFrameY);
        }

        [Fact]
        public void Main_tile_CopyFrom_TileAdapter()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                LiquidAmount = 3,
                HeaderPart = 12345,
                HeaderPart2 = 100,
                HeaderPart3 = 101,
                BlockFrameX = 4,
                BlockFrameY = 5
            };

            Terraria.Main.tile[0, 1].CopyFrom(Terraria.Main.tile[0, 0]);

            Assert.Equal(BlockId.Stone, worldService.World[0, 1].BlockId);
            Assert.Equal(WallId.Dirt, worldService.World[0, 1].WallId);
            Assert.Equal(3, worldService.World[0, 1].LiquidAmount);
            Assert.Equal(12345, worldService.World[0, 1].HeaderPart);
            Assert.Equal(100, worldService.World[0, 1].HeaderPart2);
            Assert.Equal(101, worldService.World[0, 1].HeaderPart3);
            Assert.Equal(4, worldService.World[0, 1].BlockFrameX);
            Assert.Equal(5, worldService.World[0, 1].BlockFrameY);
        }

        [Fact]
        public void Main_tile_isTheSameAs_NullTile_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(null));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentHeader_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { IsBlockActive = true };
            worldService.World[0, 1] = new Tile();

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentHeader2_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                LiquidAmount = 1,
                HeaderPart2 = 1
            };
            worldService.World[0, 1] = new Tile
            {
                LiquidAmount = 1,
                HeaderPart2 = 2
            };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentBlockIdAndBlockActive_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Stone
            };
            worldService.World[0, 1] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Dirt
            };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentBlockIdButNotBlockActive_ReturnsTrue()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { BlockId = BlockId.Stone };
            worldService.World[0, 1] = new Tile { BlockId = BlockId.Dirt };

            Assert.True(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentBlockFrameXAndHasFrames_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Torches,
                BlockFrameX = 1
            };
            worldService.World[0, 1] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Torches,
                BlockFrameX = 2
            };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentBlockFrameYAndHasFrames_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Torches,
                BlockFrameY = 1
            };
            worldService.World[0, 1] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Torches,
                BlockFrameY = 2
            };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentBlockFramesButNotHasFrames_ReturnsTrue()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Stone,
                BlockFrameX = 1,
                BlockFrameY = 1
            };
            worldService.World[0, 1] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Stone,
                BlockFrameX = 2,
                BlockFrameY = 2
            };

            Assert.True(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentWallId_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { WallId = WallId.Stone };
            worldService.World[0, 1] = new Tile { WallId = WallId.NaturalDirt };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentLiquidAmount_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { LiquidAmount = 1 };
            worldService.World[0, 1] = new Tile { LiquidAmount = 2 };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentWallColor_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { WallColor = PaintColor.Red };
            worldService.World[0, 1] = new Tile { WallColor = PaintColor.DeepRed };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentYellowWire_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { HasYellowWire = true };
            worldService.World[0, 1] = new Tile();

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentHeader_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { IsBlockActive = true };
            var tile = Mock.Of<OTAPI.Tile.ITile>();

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentHeader2_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                LiquidAmount = 1,
                HeaderPart2 = 1
            };
            var tile = Mock.Of<OTAPI.Tile.ITile>(t => t.liquid == 1 && t.bTileHeader == 2);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentBlockIdAndBlockActive_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Stone
            };
            var tile = Mock.Of<OTAPI.Tile.ITile>(t => t.sTileHeader == 32 && t.type == (ushort)BlockId.Dirt);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentBlockIdButNotBlockActive_ReturnsTrue()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { BlockId = BlockId.Stone };
            var tile = Mock.Of<OTAPI.Tile.ITile>(t => t.type == (ushort)BlockId.Dirt);

            Assert.True(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentBlockFrameXAndHasFrames_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Torches,
                BlockFrameX = 1
            };
            var tile = Mock.Of<OTAPI.Tile.ITile>(
                t => t.sTileHeader == 32 && t.type == (ushort)BlockId.Torches && t.frameX == 2);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentBlockFrameYAndHasFrames_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Torches,
                BlockFrameY = 1
            };
            var tile = Mock.Of<OTAPI.Tile.ITile>(
                t => t.sTileHeader == 32 && t.type == (ushort)BlockId.Torches && t.frameY == 2);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentBlockFramesButNotHasFrames_ReturnsTrue()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Stone,
                BlockFrameX = 1,
                BlockFrameY = 1
            };
            var tile = Mock.Of<OTAPI.Tile.ITile>(
                t => t.sTileHeader == 32 && t.type == (ushort)BlockId.Stone && t.frameX == 2 && t.frameY == 2);

            Assert.True(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentWallId_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { WallId = WallId.Stone };
            var tile = Mock.Of<OTAPI.Tile.ITile>(t => t.wall == (ushort)WallId.NaturalDirt);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentLiquidAmount_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { LiquidAmount = 1 };
            var tile = Mock.Of<OTAPI.Tile.ITile>(t => t.liquid == 2);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentWallColor_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { WallColor = PaintColor.Red };
            var tile = Mock.Of<OTAPI.Tile.ITile>(t => t.bTileHeader == 13);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentYellowWire_ReturnsFalse()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { HasYellowWire = true };
            var tile = Mock.Of<OTAPI.Tile.ITile>();

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_ClearEverything()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                LiquidAmount = 3,
                HeaderPart = 12345,
                HeaderPart2 = 100,
                HeaderPart3 = 101,
                BlockFrameX = 4,
                BlockFrameY = 5
            };

            Terraria.Main.tile[0, 0].ClearEverything();

            Assert.Equal(BlockId.Dirt, worldService.World[0, 0].BlockId);
            Assert.Equal(WallId.None, worldService.World[0, 0].WallId);
            Assert.Equal(0, worldService.World[0, 0].LiquidAmount);
            Assert.Equal(0, worldService.World[0, 0].HeaderPart);
            Assert.Equal(0, worldService.World[0, 0].HeaderPart2);
            Assert.Equal(0, worldService.World[0, 0].HeaderPart3);
            Assert.Equal(0, worldService.World[0, 0].BlockFrameX);
            Assert.Equal(0, worldService.World[0, 0].BlockFrameY);
        }

        [Fact]
        public void Main_tile_ClearMetadata()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                LiquidAmount = 3,
                HeaderPart = 12345,
                HeaderPart2 = 100,
                HeaderPart3 = 101,
                BlockFrameX = 4,
                BlockFrameY = 5
            };

            Terraria.Main.tile[0, 0].ClearMetadata();

            Assert.Equal(BlockId.Stone, worldService.World[0, 0].BlockId);
            Assert.Equal(WallId.Dirt, worldService.World[0, 0].WallId);
            Assert.Equal(0, worldService.World[0, 0].LiquidAmount);
            Assert.Equal(0, worldService.World[0, 0].HeaderPart);
            Assert.Equal(0, worldService.World[0, 0].HeaderPart2);
            Assert.Equal(0, worldService.World[0, 0].HeaderPart3);
            Assert.Equal(0, worldService.World[0, 0].BlockFrameX);
            Assert.Equal(0, worldService.World[0, 0].BlockFrameY);
        }

        [Fact]
        public void Main_tile_ClearTile()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                Slope = Slope.BottomRight,
                IsBlockHalved = true,
                IsBlockActive = true,
                IsBlockActuated = true
            };

            Terraria.Main.tile[0, 0].ClearTile();

            Assert.Equal(Slope.None, worldService.World[0, 0].Slope);
            Assert.False(worldService.World[0, 0].IsBlockHalved);
            Assert.False(worldService.World[0, 0].IsBlockActive);
            Assert.False(worldService.World[0, 0].IsBlockActuated);
        }

        [Fact]
        public void Main_tile_Clear_Tile()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                IsBlockActive = true,
                BlockFrameX = 1,
                BlockFrameY = 2
            };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Tile);

            Assert.Equal(BlockId.Dirt, worldService.World[0, 0].BlockId);
            Assert.False(worldService.World[0, 0].IsBlockActive);
            Assert.Equal(0, worldService.World[0, 0].BlockFrameX);
            Assert.Equal(0, worldService.World[0, 0].BlockFrameX);
        }

        [Fact]
        public void Main_tile_Clear_TilePaint()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { BlockColor = PaintColor.Red };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.TilePaint);

            Assert.Equal(PaintColor.None, worldService.World[0, 0].BlockColor);
        }

        [Fact]
        public void Main_tile_Clear_Wall()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { WallId = WallId.Dirt };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Wall);

            Assert.Equal(WallId.None, worldService.World[0, 0].WallId);
        }

        [Fact]
        public void Main_tile_Clear_WallPaint()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { WallColor = PaintColor.Red };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.WallPaint);

            Assert.Equal(PaintColor.None, worldService.World[0, 0].WallColor);
        }

        [Fact]
        public void Main_tile_Clear_Liquid()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                LiquidAmount = 100,
                Liquid = Liquid.Honey,
                IsCheckingLiquid = true
            };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Liquid);

            Assert.Equal(0, worldService.World[0, 0].LiquidAmount);
            Assert.Equal(Liquid.Water, worldService.World[0, 0].Liquid);
            Assert.False(worldService.World[0, 0].IsCheckingLiquid);
        }

        [Fact]
        public void Main_tile_Clear_Wiring()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                HasRedWire = true,
                HasBlueWire = true,
                HasGreenWire = true,
                HasYellowWire = true
            };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Wiring);

            Assert.False(worldService.World[0, 0].HasRedWire);
            Assert.False(worldService.World[0, 0].HasBlueWire);
            Assert.False(worldService.World[0, 0].HasGreenWire);
            Assert.False(worldService.World[0, 0].HasYellowWire);
        }

        [Fact]
        public void Main_tile_Clear_Actuator()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                HasActuator = true,
                IsBlockActuated = true
            };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Actuator);

            Assert.False(worldService.World[0, 0].HasActuator);
            Assert.False(worldService.World[0, 0].IsBlockActuated);
        }

        [Fact]
        public void Main_tile_Clear_Slope()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                Slope = Slope.TopLeft,
                IsBlockHalved = true
            };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Slope);

            Assert.Equal(Slope.None, worldService.World[0, 0].Slope);
            Assert.False(worldService.World[0, 0].IsBlockHalved);
        }

        [Fact]
        public void Main_tile_ResetToType()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                LiquidAmount = 3,
                HeaderPart = 12345,
                HeaderPart2 = 100,
                HeaderPart3 = 101,
                BlockFrameX = 4,
                BlockFrameY = 5
            };

            Terraria.Main.tile[0, 0].ResetToType((ushort)BlockId.Stone);

            Assert.Equal(BlockId.Stone, worldService.World[0, 0].BlockId);
            Assert.Equal(WallId.Dirt, worldService.World[0, 0].WallId);
            Assert.Equal(0, worldService.World[0, 0].LiquidAmount);
            Assert.Equal(32, worldService.World[0, 0].HeaderPart);
            Assert.Equal(0, worldService.World[0, 0].HeaderPart2);
            Assert.Equal(0, worldService.World[0, 0].HeaderPart3);
            Assert.Equal(0, worldService.World[0, 0].BlockFrameX);
            Assert.Equal(0, worldService.World[0, 0].BlockFrameY);
        }

        [Theory]
        [InlineData(Slope.None, false)]
        [InlineData(Slope.TopLeft, true)]
        [InlineData(Slope.TopRight, true)]
        [InlineData(Slope.BottomLeft, false)]
        [InlineData(Slope.BottomRight, false)]
        public void Main_tile_topSlope(Slope slope, bool value)
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { Slope = slope };

            Assert.Equal(value, Terraria.Main.tile[0, 0].topSlope());
        }

        [Theory]
        [InlineData(Slope.None, false)]
        [InlineData(Slope.TopLeft, false)]
        [InlineData(Slope.TopRight, false)]
        [InlineData(Slope.BottomLeft, true)]
        [InlineData(Slope.BottomRight, true)]
        public void Main_tile_bottomSlope(Slope slope, bool value)
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { Slope = slope };

            Assert.Equal(value, Terraria.Main.tile[0, 0].bottomSlope());
        }

        [Theory]
        [InlineData(Slope.None, false)]
        [InlineData(Slope.TopLeft, true)]
        [InlineData(Slope.TopRight, false)]
        [InlineData(Slope.BottomLeft, true)]
        [InlineData(Slope.BottomRight, false)]
        public void Main_tile_leftSlope(Slope slope, bool value)
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { Slope = slope };

            Assert.Equal(value, Terraria.Main.tile[0, 0].leftSlope());
        }

        [Theory]
        [InlineData(Slope.None, false)]
        [InlineData(Slope.TopLeft, false)]
        [InlineData(Slope.TopRight, true)]
        [InlineData(Slope.BottomLeft, false)]
        [InlineData(Slope.BottomRight, true)]
        public void Main_tile_rightSlope(Slope slope, bool value)
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { Slope = slope };

            Assert.Equal(value, Terraria.Main.tile[0, 0].rightSlope());
        }

        [Fact]
        public void Main_tile_HasSameSlope()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { Slope = Slope.BottomRight };
            worldService.World[0, 1] = new Tile { Slope = Slope.BottomRight };

            Assert.True(Terraria.Main.tile[0, 0].HasSameSlope(Terraria.Main.tile[0, 1]));

            worldService.World[0, 1].Slope = Slope.BottomLeft;

            Assert.False(Terraria.Main.tile[0, 0].HasSameSlope(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_blockType()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile();

            Assert.Equal(0, Terraria.Main.tile[0, 0].blockType());
        }

        [Fact]
        public void Main_tile_blockType_Halved()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { IsBlockHalved = true };

            Assert.Equal(1, Terraria.Main.tile[0, 0].blockType());
        }

        [Fact]
        public void Main_tile_blockType_Slope()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            worldService.World[0, 0] = new Tile { Slope = Slope.BottomRight };

            Assert.Equal((int)Slope.BottomRight + 1, Terraria.Main.tile[0, 0].blockType());
        }

        [Fact]
        public void WorldSave_EventTriggered()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.Is<WorldSaveEvent>(evt => evt.World == worldService.World), log));

            Terraria.IO.WorldFile.SaveWorld(false, true);

            Assert.Equal(13500.0, Terraria.IO.WorldFile._tempTime);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void WorldSave_EventCanceled()
        {
            // Clear the time so we know it's 0.
            Terraria.IO.WorldFile._tempTime = 0.0;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var worldService = new OrionWorldService(server, log);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<WorldSaveEvent>(), log))
                .Callback<WorldSaveEvent, ILogger>((evt, log) => evt.Cancel());

            Terraria.IO.WorldFile.SaveWorld(false, true);

            Assert.Equal(0.0, Terraria.IO.WorldFile._tempTime);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakBlock_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakBlock };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<BlockBreakEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && !evt.IsItemless),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakBlock_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakBlock };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<BlockBreakEvent>(), log))
                .Callback<BlockBreakEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakBlockFailure_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.BreakBlock,
                IsFailure = true
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events)
                .Verify(em => em.Raise(It.IsAny<BlockBreakEvent>(), log), Times.Never);
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_PlaceBlock_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.PlaceBlock,
                BlockId = BlockId.Torches,
                BlockStyle = 1
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<BlockPlaceEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == BlockId.Torches && evt.Style == 1 && !evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_PlaceBlock_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.PlaceBlock,
                BlockId = BlockId.Torches,
                BlockStyle = 1
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<BlockPlaceEvent>(), log))
                .Callback<BlockPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakWall_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakWall };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<WallBreakEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 && evt.Y == 256),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakWall_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakWall };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<WallBreakEvent>(), log))
                .Callback<WallBreakEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakWallFailure_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.BreakWall,
                IsFailure = true
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events)
                .Verify(em => em.Raise(It.IsAny<WallBreakEvent>(), log), Times.Never);
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_PlaceWall_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.PlaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<WallPlaceEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == WallId.Stone && !evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_PlaceWall_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.PlaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<WallPlaceEvent>(), log))
                .Callback<WallPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakBlockItemless_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakBlockItemless };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<BlockBreakEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.IsItemless),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakBlockItemless_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakBlockItemless };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<BlockBreakEvent>(), log))
                .Callback<BlockBreakEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakBlockItemlessFailure_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.BreakBlockItemless,
                IsFailure = true
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events)
                .Verify(em => em.Raise(It.IsAny<BlockBreakEvent>(), log), Times.Never);
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_ReplaceBlock_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.ReplaceBlock,
                BlockId = BlockId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<BlockPlaceEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == BlockId.Stone && evt.Style == 0 && evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_ReplaceBlock_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.ReplaceBlock,
                BlockId = BlockId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<BlockPlaceEvent>(), log))
                .Callback<BlockPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_ReplaceWall_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.ReplaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<WallPlaceEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == WallId.Stone && evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_ReplaceWall_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.ReplaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<WallPlaceEvent>(), log))
                .Callback<WallPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_InvalidModification()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = (TileModification)255
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);
        }

        [Fact]
        public void PacketReceive_TileSquarePacket_EventTriggered()
        {
            Action<PacketReceiveEvent<TileSquarePacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileSquarePacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileSquarePacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileSquarePacket { X = 100, Y = 256, Tiles = new TileSlice(3, 3) };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileSquarePacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<TileSquareEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Tiles.Width == 3 && evt.Tiles.Height == 3),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileSquarePacket_EventCanceled()
        {
            Action<PacketReceiveEvent<TileSquarePacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileSquarePacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileSquarePacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileSquarePacket { X = 100, Y = 256, Tiles = new TileSlice(3, 3) };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileSquarePacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<TileSquareEvent>(), log))
                .Callback<TileSquareEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileLiquidPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<TileLiquidPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileLiquidPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileLiquidPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileLiquidPacket { X = 100, Y = 256, LiquidAmount = 255, Liquid = Liquid.Honey };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileLiquidPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<TileLiquidEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.LiquidAmount == 255 && evt.Liquid == Liquid.Honey),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileLiquidPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<TileLiquidPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileLiquidPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileLiquidPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new TileLiquidPacket { X = 100, Y = 256, LiquidAmount = 255, Liquid = Liquid.Honey };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileLiquidPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<TileLiquidEvent>(), log))
                .Callback<TileLiquidEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WireActivatePacket_EventTriggered()
        {
            Action<PacketReceiveEvent<WireActivatePacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WireActivatePacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<WireActivatePacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new WireActivatePacket { X = 100, Y = 256 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WireActivatePacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<WiringActivateEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 && evt.Y == 256),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WireActivatePacket_EventCanceled()
        {
            Action<PacketReceiveEvent<WireActivatePacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WireActivatePacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<WireActivatePacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new WireActivatePacket { X = 100, Y = 256 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WireActivatePacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<WiringActivateEvent>(), log))
                .Callback<WiringActivateEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_BlockPaintPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<BlockPaintPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<BlockPaintPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<BlockPaintPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new BlockPaintPacket { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<BlockPaintPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<BlockPaintEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Color == PaintColor.Red),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_BlockPaintPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<BlockPaintPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<BlockPaintPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<BlockPaintPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new BlockPaintPacket { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<BlockPaintPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<BlockPaintEvent>(), log))
                .Callback<BlockPaintEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WallPaintPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<WallPaintPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WallPaintPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<WallPaintPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new WallPaintPacket { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WallPaintPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<WallPaintEvent>(
                        evt => evt.World == worldService.World && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Color == PaintColor.Red),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WallPaintPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<WallPaintPacket>>? registeredHandler = null;

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            Mock.Get(server.Events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WallPaintPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<WallPaintPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var worldService = new OrionWorldService(server, log);

            var packet = new WallPaintPacket { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WallPaintPacket>(ref packet, sender);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<WallPaintEvent>(), log))
                .Callback<WallPaintEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(server.Events).VerifyAll();
        }
    }
}
