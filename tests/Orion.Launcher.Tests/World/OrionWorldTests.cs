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
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.World.Tiles;
using Orion.Core.Packets.World.Tiles;
using Orion.Core.Players;
using Serilog;
using Xunit;
using Orion.Launcher.World;
using Orion.Core.Events.World;
using Orion.Core.World.Tiles;
using Orion.Core.World;
using System.Diagnostics.CodeAnalysis;

namespace Orion.Launcher
{
    // These tests depend on Terraria state.
    [Collection("TerrariaTestsCollection")]
    [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Testing")]
    public class OrionWorldTests
    {
        [Fact]
        public void Item_Mutate()
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
        public void Evil_Set_Corruption()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world.Evil = WorldEvil.Corruption;

            Assert.False(Terraria.WorldGen.crimson);
        }

        [Fact]
        public void Evil_Set_Crimson()
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
        public void Main_tile_Width_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(Terraria.Main.maxTilesX, Terraria.Main.tile.Width);
        }

        [Fact]
        public void Main_tile_Height_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(Terraria.Main.maxTilesY, Terraria.Main.tile.Height);
        }

        [Fact]
        public void Main_tile_type_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockId = BlockId.Stone };

            Assert.Equal(BlockId.Stone, (BlockId)Terraria.Main.tile[0, 0].type);
        }

        [Fact]
        public void Main_tile_type_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].type = (ushort)BlockId.Stone;

            Assert.Equal(BlockId.Stone, world[0, 0].BlockId);
        }

        [Fact]
        public void Main_tile_wall_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallId = WallId.Stone };

            Assert.Equal(WallId.Stone, (WallId)Terraria.Main.tile[0, 0].wall);
        }

        [Fact]
        public void Main_tile_wall_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].wall = (ushort)WallId.Stone;

            Assert.Equal(WallId.Stone, world[0, 0].WallId);
        }

        [Fact]
        public void Main_tile_liquid_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { LiquidAmount = 100 };

            Assert.Equal(100, Terraria.Main.tile[0, 0].liquid);
        }

        [Fact]
        public void Main_tile_liquid_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].liquid = 100;

            Assert.Equal(100, world[0, 0].LiquidAmount);
        }

        [Fact]
        public void Main_tile_sTileHeader_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Header = 0x00001234u };

            Assert.Equal(0x1234, Terraria.Main.tile[0, 0].sTileHeader);
        }

        [Fact]
        public void Main_tile_sTileHeader_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].sTileHeader = 0x1234;

            Assert.Equal(0x00001234u, world[0, 0].Header & 0x0000ffffu);
        }

        [Fact]
        public void Main_tile_bTileHeader_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Header = 0x00120000u };

            Assert.Equal(0x12, Terraria.Main.tile[0, 0].bTileHeader);
        }

        [Fact]
        public void Main_tile_bTileHeader_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].bTileHeader = 0x12;

            Assert.Equal(0x00120000u, world[0, 0].Header & 0x00ff0000u);
        }

        [Fact]
        public void Main_tile_bTileHeader3_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Header = 0x12000000u };

            Assert.Equal(0x12, Terraria.Main.tile[0, 0].bTileHeader3);
        }

        [Fact]
        public void Main_tile_bTileHeader3_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].bTileHeader3 = 0x12;

            Assert.Equal(0x12000000u, world[0, 0].Header & 0xff000000u);
        }

        [Fact]
        public void Main_tile_frameX_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockFrameX = 12345 };

            Assert.Equal(12345, Terraria.Main.tile[0, 0].frameX);
        }

        [Fact]
        public void Main_tile_frameX_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].frameX = 12345;

            Assert.Equal(12345, world[0, 0].BlockFrameX);
        }

        [Fact]
        public void Main_tile_frameY_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockFrameY = 12345 };

            Assert.Equal(12345, Terraria.Main.tile[0, 0].frameY);
        }

        [Fact]
        public void Main_tile_frameY_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].frameY = 12345;

            Assert.Equal(12345, world[0, 0].BlockFrameY);
        }

        [Fact]
        public void Main_tile_color()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockColor = PaintColor.Red };

            Assert.Equal((byte)PaintColor.Red, Terraria.Main.tile[0, 0].color());

            Terraria.Main.tile[0, 0].color((byte)PaintColor.DeepRed);

            Assert.Equal(PaintColor.DeepRed, world[0, 0].BlockColor);
        }

        [Fact]
        public void Main_tile_active()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { IsBlockActive = true };

            Assert.True(Terraria.Main.tile[0, 0].active());

            Terraria.Main.tile[0, 0].active(false);

            Assert.False(world[0, 0].IsBlockActive);
        }

        [Fact]
        public void Main_tile_inActive()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { IsBlockActuated = true };

            Assert.True(Terraria.Main.tile[0, 0].inActive());

            Terraria.Main.tile[0, 0].inActive(false);

            Assert.False(world[0, 0].IsBlockActuated);
        }

        [Fact]
        public void Main_tile_nactive()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { IsBlockActive = true };

            Assert.True(Terraria.Main.tile[0, 0].nactive());

            world[0, 0].IsBlockActuated = true;

            Assert.False(Terraria.Main.tile[0, 0].nactive());
        }

        [Fact]
        public void Main_tile_wire()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasRedWire = true };

            Assert.True(Terraria.Main.tile[0, 0].wire());

            Terraria.Main.tile[0, 0].wire(false);

            Assert.False(world[0, 0].HasRedWire);
        }

        [Fact]
        public void Main_tile_wire2()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasBlueWire = true };

            Assert.True(Terraria.Main.tile[0, 0].wire2());

            Terraria.Main.tile[0, 0].wire2(false);

            Assert.False(world[0, 0].HasBlueWire);
        }

        [Fact]
        public void Main_tile_wire3()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasGreenWire = true };

            Assert.True(Terraria.Main.tile[0, 0].wire3());

            Terraria.Main.tile[0, 0].wire3(false);

            Assert.False(world[0, 0].HasGreenWire);
        }

        [Fact]
        public void Main_tile_halfBrick()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { IsBlockHalved = true };

            Assert.True(Terraria.Main.tile[0, 0].halfBrick());

            Terraria.Main.tile[0, 0].halfBrick(false);

            Assert.False(world[0, 0].IsBlockHalved);
        }

        [Fact]
        public void Main_tile_actuator()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasActuator = true };

            Assert.True(Terraria.Main.tile[0, 0].actuator());

            Terraria.Main.tile[0, 0].actuator(false);

            Assert.False(world[0, 0].HasActuator);
        }

        [Fact]
        public void Main_tile_slope()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Slope = Slope.BottomRight };

            Assert.Equal((byte)Slope.BottomRight, Terraria.Main.tile[0, 0].slope());

            Terraria.Main.tile[0, 0].slope((byte)Slope.BottomLeft);

            Assert.Equal(Slope.BottomLeft, world[0, 0].Slope);
        }

        [Fact]
        public void Main_tile_wallColor()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallColor = PaintColor.Red };

            Assert.Equal((byte)PaintColor.Red, Terraria.Main.tile[0, 0].wallColor());

            Terraria.Main.tile[0, 0].wallColor((byte)PaintColor.DeepRed);

            Assert.Equal(PaintColor.DeepRed, world[0, 0].WallColor);
        }

        [Fact]
        public void Main_tile_lava()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Liquid = Liquid.Lava };

            Assert.True(Terraria.Main.tile[0, 0].lava());

            Terraria.Main.tile[0, 0].lava(false);

            Assert.NotEqual(Liquid.Lava, world[0, 0].Liquid);

            Terraria.Main.tile[0, 0].lava(true);

            Assert.Equal(Liquid.Lava, world[0, 0].Liquid);
        }

        [Fact]
        public void Main_tile_honey()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Liquid = Liquid.Honey };

            Assert.True(Terraria.Main.tile[0, 0].honey());

            Terraria.Main.tile[0, 0].honey(false);

            Assert.NotEqual(Liquid.Honey, world[0, 0].Liquid);

            Terraria.Main.tile[0, 0].honey(true);

            Assert.Equal(Liquid.Honey, world[0, 0].Liquid);
        }

        [Fact]
        public void Main_tile_liquidType()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Liquid = Liquid.Lava };

            Assert.Equal((byte)Liquid.Lava, Terraria.Main.tile[0, 0].liquidType());

            Terraria.Main.tile[0, 0].liquidType((int)Liquid.Honey);

            Assert.Equal(Liquid.Honey, world[0, 0].Liquid);
        }

        [Fact]
        public void Main_tile_wire4()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasYellowWire = true };

            Assert.True(Terraria.Main.tile[0, 0].wire4());

            Terraria.Main.tile[0, 0].wire4(false);

            Assert.False(world[0, 0].HasYellowWire);
        }

        [Fact]
        public void Main_tile_frameNumber()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockFrameNumber = 7 };

            Assert.Equal(7, Terraria.Main.tile[0, 0].frameNumber());

            Terraria.Main.tile[0, 0].frameNumber(5);

            Assert.Equal(5, world[0, 0].BlockFrameNumber);
        }

        [Fact]
        public void Main_tile_checkingLiquid()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { IsCheckingLiquid = true };

            Assert.True(Terraria.Main.tile[0, 0].checkingLiquid());

            Terraria.Main.tile[0, 0].checkingLiquid(false);

            Assert.False(world[0, 0].IsCheckingLiquid);
        }

        [Fact]
        public void Main_tile_skipLiquid()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { ShouldSkipLiquid = true };

            Assert.True(Terraria.Main.tile[0, 0].skipLiquid());

            Terraria.Main.tile[0, 0].skipLiquid(false);

            Assert.False(world[0, 0].ShouldSkipLiquid);
        }

        [Fact]
        public void Main_tile_CopyFrom_NullTile()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = (BlockId)1,
                WallId = (WallId)2,
                LiquidAmount = 3,
                Header = 12345678u,
                BlockFrameX = 4,
                BlockFrameY = 5
            };

            Terraria.Main.tile[0, 0].CopyFrom(null);

            Assert.Equal(BlockId.Dirt, world[0, 0].BlockId);
            Assert.Equal(WallId.None, world[0, 0].WallId);
            Assert.Equal(0, world[0, 0].LiquidAmount);
            Assert.Equal(0u, world[0, 0].Header);
            Assert.Equal(0, world[0, 0].BlockFrameX);
            Assert.Equal(0, world[0, 0].BlockFrameY);
        }

        [Fact]
        public void Main_tile_CopyFrom_TileAdapter()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                LiquidAmount = 3,
                Header = 12345678u,
                BlockFrameX = 4,
                BlockFrameY = 5
            };

            Terraria.Main.tile[0, 1].CopyFrom(Terraria.Main.tile[0, 0]);

            Assert.Equal(BlockId.Stone, world[0, 1].BlockId);
            Assert.Equal(WallId.Dirt, world[0, 1].WallId);
            Assert.Equal(3, world[0, 1].LiquidAmount);
            Assert.Equal(12345678u, world[0, 1].Header);
            Assert.Equal(4, world[0, 1].BlockFrameX);
            Assert.Equal(5, world[0, 1].BlockFrameY);
        }

        [Fact]
        public void Main_tile_isTheSameAs_NullTile_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(null));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentHeader_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { IsBlockActive = true };
            world[0, 1] = new Tile();

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentHeader2_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                LiquidAmount = 1,
                Header = 0x00010000u
            };
            world[0, 1] = new Tile
            {
                LiquidAmount = 1,
                Header = 0x00020000u
            };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentBlockIdAndBlockActive_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Stone
            };
            world[0, 1] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Dirt
            };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentBlockIdButNotBlockActive_ReturnsTrue()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockId = BlockId.Stone };
            world[0, 1] = new Tile { BlockId = BlockId.Dirt };

            Assert.True(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentBlockFrameXAndHasFrames_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Torches,
                BlockFrameX = 1
            };
            world[0, 1] = new Tile
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
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Torches,
                BlockFrameY = 1
            };
            world[0, 1] = new Tile
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
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                IsBlockActive = true,
                BlockId = BlockId.Stone,
                BlockFrameX = 1,
                BlockFrameY = 1
            };
            world[0, 1] = new Tile
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
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallId = WallId.Stone };
            world[0, 1] = new Tile { WallId = WallId.NaturalDirt };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentLiquidAmount_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { LiquidAmount = 1 };
            world[0, 1] = new Tile { LiquidAmount = 2 };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentWallColor_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallColor = PaintColor.Red };
            world[0, 1] = new Tile { WallColor = PaintColor.DeepRed };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_TileAdapterDifferentYellowWire_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasYellowWire = true };
            world[0, 1] = new Tile();

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentHeader_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { IsBlockActive = true };
            var tile = Mock.Of<OTAPI.Tile.ITile>();

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentHeader2_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                LiquidAmount = 1,
                Header = 0x00010000u
            };
            var tile = Mock.Of<OTAPI.Tile.ITile>(t => t.liquid == 1 && t.bTileHeader == 2);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentBlockIdAndBlockActive_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
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
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockId = BlockId.Stone };
            var tile = Mock.Of<OTAPI.Tile.ITile>(t => t.type == (ushort)BlockId.Dirt);

            Assert.True(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentBlockFrameXAndHasFrames_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
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
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
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
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
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
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallId = WallId.Stone };
            var tile = Mock.Of<OTAPI.Tile.ITile>(t => t.wall == (ushort)WallId.NaturalDirt);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentLiquidAmount_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { LiquidAmount = 1 };
            var tile = Mock.Of<OTAPI.Tile.ITile>(t => t.liquid == 2);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentWallColor_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallColor = PaintColor.Red };
            var tile = Mock.Of<OTAPI.Tile.ITile>(t => t.bTileHeader == 13);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_isTheSameAs_ITileDifferentYellowWire_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasYellowWire = true };
            var tile = Mock.Of<OTAPI.Tile.ITile>();

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(tile));
        }

        [Fact]
        public void Main_tile_ClearEverything()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                LiquidAmount = 3,
                Header = 12345678u,
                BlockFrameX = 4,
                BlockFrameY = 5
            };

            Terraria.Main.tile[0, 0].ClearEverything();

            Assert.Equal(BlockId.Dirt, world[0, 0].BlockId);
            Assert.Equal(WallId.None, world[0, 0].WallId);
            Assert.Equal(0, world[0, 0].LiquidAmount);
            Assert.Equal(0u, world[0, 0].Header);
            Assert.Equal(0, world[0, 0].BlockFrameX);
            Assert.Equal(0, world[0, 0].BlockFrameY);
        }

        [Fact]
        public void Main_tile_ClearMetadata()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                LiquidAmount = 3,
                Header = 12345678u,
                BlockFrameX = 4,
                BlockFrameY = 5
            };

            Terraria.Main.tile[0, 0].ClearMetadata();

            Assert.Equal(BlockId.Stone, world[0, 0].BlockId);
            Assert.Equal(WallId.Dirt, world[0, 0].WallId);
            Assert.Equal(0, world[0, 0].LiquidAmount);
            Assert.Equal(0u, world[0, 0].Header);
            Assert.Equal(0, world[0, 0].BlockFrameX);
            Assert.Equal(0, world[0, 0].BlockFrameY);
        }

        [Fact]
        public void Main_tile_ClearTile()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                Slope = Slope.BottomRight,
                IsBlockHalved = true,
                IsBlockActive = true,
                IsBlockActuated = true
            };

            Terraria.Main.tile[0, 0].ClearTile();

            Assert.Equal(Slope.None, world[0, 0].Slope);
            Assert.False(world[0, 0].IsBlockHalved);
            Assert.False(world[0, 0].IsBlockActive);
            Assert.False(world[0, 0].IsBlockActuated);
        }

        [Fact]
        public void Main_tile_Clear_Tile()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                IsBlockActive = true,
                BlockFrameX = 1,
                BlockFrameY = 2
            };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Tile);

            Assert.Equal(BlockId.Dirt, world[0, 0].BlockId);
            Assert.False(world[0, 0].IsBlockActive);
            Assert.Equal(0, world[0, 0].BlockFrameX);
            Assert.Equal(0, world[0, 0].BlockFrameX);
        }

        [Fact]
        public void Main_tile_Clear_TilePaint()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockColor = PaintColor.Red };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.TilePaint);

            Assert.Equal(PaintColor.None, world[0, 0].BlockColor);
        }

        [Fact]
        public void Main_tile_Clear_Wall()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallId = WallId.Dirt };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Wall);

            Assert.Equal(WallId.None, world[0, 0].WallId);
        }

        [Fact]
        public void Main_tile_Clear_WallPaint()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallColor = PaintColor.Red };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.WallPaint);

            Assert.Equal(PaintColor.None, world[0, 0].WallColor);
        }

        [Fact]
        public void Main_tile_Clear_Liquid()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                LiquidAmount = 100,
                Liquid = Liquid.Honey,
                IsCheckingLiquid = true
            };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Liquid);

            Assert.Equal(0, world[0, 0].LiquidAmount);
            Assert.Equal(Liquid.Water, world[0, 0].Liquid);
            Assert.False(world[0, 0].IsCheckingLiquid);
        }

        [Fact]
        public void Main_tile_Clear_Wiring()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                HasRedWire = true,
                HasBlueWire = true,
                HasGreenWire = true,
                HasYellowWire = true
            };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Wiring);

            Assert.False(world[0, 0].HasRedWire);
            Assert.False(world[0, 0].HasBlueWire);
            Assert.False(world[0, 0].HasGreenWire);
            Assert.False(world[0, 0].HasYellowWire);
        }

        [Fact]
        public void Main_tile_Clear_Actuator()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                HasActuator = true,
                IsBlockActuated = true
            };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Actuator);

            Assert.False(world[0, 0].HasActuator);
            Assert.False(world[0, 0].IsBlockActuated);
        }

        [Fact]
        public void Main_tile_Clear_Slope()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                Slope = Slope.TopLeft,
                IsBlockHalved = true
            };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Slope);

            Assert.Equal(Slope.None, world[0, 0].Slope);
            Assert.False(world[0, 0].IsBlockHalved);
        }

        [Fact]
        public void Main_tile_ResetToType()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                LiquidAmount = 3,
                Header = 12345678u,
                BlockFrameX = 4,
                BlockFrameY = 5
            };

            Terraria.Main.tile[0, 0].ResetToType((ushort)BlockId.Stone);

            Assert.Equal(BlockId.Stone, world[0, 0].BlockId);
            Assert.Equal(WallId.Dirt, world[0, 0].WallId);
            Assert.Equal(0, world[0, 0].LiquidAmount);
            Assert.Equal(32u, world[0, 0].Header);
            Assert.Equal(0, world[0, 0].BlockFrameX);
            Assert.Equal(0, world[0, 0].BlockFrameY);
        }

        [Theory]
        [InlineData(Slope.None, false)]
        [InlineData(Slope.TopLeft, true)]
        [InlineData(Slope.TopRight, true)]
        [InlineData(Slope.BottomLeft, false)]
        [InlineData(Slope.BottomRight, false)]
        public void Main_tile_topSlope(Slope slope, bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Slope = slope };

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
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Slope = slope };

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
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Slope = slope };

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
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Slope = slope };

            Assert.Equal(value, Terraria.Main.tile[0, 0].rightSlope());
        }

        [Fact]
        public void Main_tile_HasSameSlope()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Slope = Slope.BottomRight };
            world[0, 1] = new Tile { Slope = Slope.BottomRight };

            Assert.True(Terraria.Main.tile[0, 0].HasSameSlope(Terraria.Main.tile[0, 1]));

            world[0, 1].Slope = Slope.BottomLeft;

            Assert.False(Terraria.Main.tile[0, 0].HasSameSlope(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_blockType()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile();

            Assert.Equal(0, Terraria.Main.tile[0, 0].blockType());
        }

        [Fact]
        public void Main_tile_blockType_Halved()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { IsBlockHalved = true };

            Assert.Equal(1, Terraria.Main.tile[0, 0].blockType());
        }

        [Fact]
        public void Main_tile_blockType_Slope()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Slope = Slope.BottomRight };

            Assert.Equal((int)Slope.BottomRight + 1, Terraria.Main.tile[0, 0].blockType());
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
        public void PacketReceive_TileModifyPacket_BreakBlock_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakBlock };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<BlockBreakEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && !evt.IsItemless),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakBlock_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakBlock };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<BlockBreakEvent>(), log))
                .Callback<BlockBreakEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakBlockFailure_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

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

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<BlockBreakEvent>(), log), Times.Never);
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_PlaceBlock_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

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

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<BlockPlaceEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == BlockId.Torches && evt.Style == 1 && !evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_PlaceBlock_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

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

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<BlockPlaceEvent>(), log))
                .Callback<BlockPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakWall_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakWall };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<WallBreakEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 && evt.Y == 256),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakWall_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakWall };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WallBreakEvent>(), log))
                .Callback<WallBreakEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakWallFailure_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

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

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<WallBreakEvent>(), log), Times.Never);
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_PlaceWall_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.PlaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<WallPlaceEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == WallId.Stone && !evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_PlaceWall_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.PlaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WallPlaceEvent>(), log))
                .Callback<WallPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakBlockItemless_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakBlockItemless };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<BlockBreakEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.IsItemless),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakBlockItemless_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket { X = 100, Y = 256, Modification = TileModification.BreakBlockItemless };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<BlockBreakEvent>(), log))
                .Callback<BlockBreakEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_BreakBlockItemlessFailure_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

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

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<BlockBreakEvent>(), log), Times.Never);
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_ReplaceBlock_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.ReplaceBlock,
                BlockId = BlockId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<BlockPlaceEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == BlockId.Stone && evt.Style == 0 && evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_ReplaceBlock_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.ReplaceBlock,
                BlockId = BlockId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<BlockPlaceEvent>(), log))
                .Callback<BlockPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_ReplaceWall_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.ReplaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<WallPlaceEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == WallId.Stone && evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_ReplaceWall_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModifyPacket
            {
                X = 100,
                Y = 256,
                Modification = TileModification.ReplaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModifyPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WallPlaceEvent>(), log))
                .Callback<WallPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModifyPacket_InvalidModification()
        {
            Action<PacketReceiveEvent<TileModifyPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModifyPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModifyPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

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

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileSquarePacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileSquarePacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileSquarePacket { X = 100, Y = 256, Tiles = new TileSlice(3, 3) };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileSquarePacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<TileSquareEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Tiles.Width == 3 && evt.Tiles.Height == 3),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileSquarePacket_EventCanceled()
        {
            Action<PacketReceiveEvent<TileSquarePacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileSquarePacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileSquarePacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileSquarePacket { X = 100, Y = 256, Tiles = new TileSlice(3, 3) };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileSquarePacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<TileSquareEvent>(), log))
                .Callback<TileSquareEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileLiquidPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<TileLiquidPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileLiquidPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileLiquidPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileLiquidPacket { X = 100, Y = 256, LiquidAmount = 255, Liquid = Liquid.Honey };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileLiquidPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<TileLiquidEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.LiquidAmount == 255 && evt.Liquid == Liquid.Honey),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileLiquidPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<TileLiquidPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileLiquidPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileLiquidPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileLiquidPacket { X = 100, Y = 256, LiquidAmount = 255, Liquid = Liquid.Honey };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileLiquidPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<TileLiquidEvent>(), log))
                .Callback<TileLiquidEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WireActivatePacket_EventTriggered()
        {
            Action<PacketReceiveEvent<WireActivatePacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WireActivatePacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<WireActivatePacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new WireActivatePacket { X = 100, Y = 256 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WireActivatePacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<WiringActivateEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 && evt.Y == 256),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WireActivatePacket_EventCanceled()
        {
            Action<PacketReceiveEvent<WireActivatePacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WireActivatePacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<WireActivatePacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new WireActivatePacket { X = 100, Y = 256 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WireActivatePacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WiringActivateEvent>(), log))
                .Callback<WiringActivateEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_BlockPaintPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<BlockPaintPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<BlockPaintPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<BlockPaintPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new BlockPaintPacket { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<BlockPaintPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<BlockPaintEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Color == PaintColor.Red),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_BlockPaintPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<BlockPaintPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<BlockPaintPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<BlockPaintPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new BlockPaintPacket { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<BlockPaintPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<BlockPaintEvent>(), log))
                .Callback<BlockPaintEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WallPaintPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<WallPaintPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WallPaintPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<WallPaintPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new WallPaintPacket { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WallPaintPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<WallPaintEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Color == PaintColor.Red),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WallPaintPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<WallPaintPacket>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WallPaintPacket>>>(), log))
                .Callback<Action<PacketReceiveEvent<WallPaintPacket>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new WallPaintPacket { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WallPaintPacket>(ref packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WallPaintEvent>(), log))
                .Callback<WallPaintEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }
    }
}
