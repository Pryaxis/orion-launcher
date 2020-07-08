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

using System.Diagnostics.CodeAnalysis;
using Moq;
using Orion.Core.Events;
using Orion.Core.World.Tiles;
using Serilog;
using Xunit;

namespace Orion.Launcher.World
{
    // These tests depend on Terraria state.
    [Collection("TerrariaTestsCollection")]
    [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Testing")]
    public partial class OrionWorldTests
    {
        [Fact]
        public void Main_tile_Get_Width_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(Terraria.Main.maxTilesX, Terraria.Main.tile.Width);
        }

        [Fact]
        public void Main_tile_Get_Height_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(Terraria.Main.maxTilesY, Terraria.Main.tile.Height);
        }

        [Fact]
        public void Main_tile_Get_type_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockId = BlockId.Stone };

            Assert.Equal(BlockId.Stone, (BlockId)(Terraria.Main.tile[0, 0].type + 1));
        }

        [Fact]
        public void Main_tile_Get_type_GetNone()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockId = BlockId.None };

            Assert.Equal(0, Terraria.Main.tile[0, 0].type);
        }

        [Fact]
        public void Main_tile_Get_type_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].type = (ushort)(BlockId.Stone - 1);

            Assert.Equal(BlockId.Stone, world[0, 0].BlockId);
        }

        [Fact]
        public void Main_tile_Get_wall_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallId = WallId.Stone };

            Assert.Equal(WallId.Stone, (WallId)Terraria.Main.tile[0, 0].wall);
        }

        [Fact]
        public void Main_tile_Get_wall_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].wall = (ushort)WallId.Stone;

            Assert.Equal(WallId.Stone, world[0, 0].WallId);
        }

        [Fact]
        public void Main_tile_Get_liquid_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Liquid = new Liquid(LiquidType.Water, 100) };

            Assert.Equal(100, Terraria.Main.tile[0, 0].liquid);
        }

        [Fact]
        public void Main_tile_Get_liquid_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].liquid = 100;

            Assert.Equal(100, world[0, 0].Liquid.Amount);
        }

        [Fact]
        public void Main_tile_Get_frameX_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockFrameX = 12345 };

            Assert.Equal(12345, Terraria.Main.tile[0, 0].frameX);
        }

        [Fact]
        public void Main_tile_Get_frameX_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].frameX = 12345;

            Assert.Equal(12345, world[0, 0].BlockFrameX);
        }

        [Fact]
        public void Main_tile_Get_frameY_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockFrameY = 12345 };

            Assert.Equal(12345, Terraria.Main.tile[0, 0].frameY);
        }

        [Fact]
        public void Main_tile_Get_frameY_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].frameY = 12345;

            Assert.Equal(12345, world[0, 0].BlockFrameY);
        }

        [Fact]
        public void Main_tile_Get_active_Get_ReturnsTrue()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockId = BlockId.Dirt };

            Assert.True(Terraria.Main.tile[0, 0].active());
        }

        [Fact]
        public void Main_tile_Get_active_Get_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockId = BlockId.None };

            Assert.False(Terraria.Main.tile[0, 0].active());
        }

        [Fact]
        public void Main_tile_Get_active_SetTrue()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockId = BlockId.None };

            Terraria.Main.tile[0, 0].active(true);

            Assert.Equal(BlockId.Dirt, world[0, 0].BlockId);
        }

        [Fact]
        public void Main_tile_Get_active_SetFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].active(false);

            Assert.Equal(BlockId.None, world[0, 0].BlockId);
        }

        [Fact]
        public void Main_tile_Get_color_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockColor = PaintColor.Red };

            Assert.Equal((byte)PaintColor.Red, Terraria.Main.tile[0, 0].color());
        }

        [Fact]
        public void Main_tile_Get_color_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].color((byte)PaintColor.DeepRed);

            Assert.Equal(PaintColor.DeepRed, world[0, 0].BlockColor);
        }

        [Theory]
        [InlineData(BlockShape.Normal)]
        [InlineData(BlockShape.Halved)]
        [InlineData(BlockShape.TopLeft)]
        [InlineData(BlockShape.TopRight)]
        [InlineData(BlockShape.BottomLeft)]
        [InlineData(BlockShape.BottomRight)]
        public void Main_tile_Get_blockType_Get(BlockShape value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = value };

            Assert.Equal(value, (BlockShape)Terraria.Main.tile[0, 0].blockType());
        }

        [Fact]
        public void Main_tile_Get_halfBrick_Get_ReturnsTrue()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = BlockShape.Halved };

            Assert.True(Terraria.Main.tile[0, 0].halfBrick());
        }

        [Fact]
        public void Main_tile_Get_halfBrick_Get_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = BlockShape.TopRight };

            Assert.False(Terraria.Main.tile[0, 0].halfBrick());
        }

        [Fact]
        public void Main_tile_Get_halfBrick_SetTrue()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].halfBrick(true);

            Assert.Equal(BlockShape.Halved, world[0, 0].BlockShape);
        }

        [Fact]
        public void Main_tile_Get_halfBrick_SetFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = BlockShape.Halved };

            Terraria.Main.tile[0, 0].halfBrick(false);

            Assert.Equal(BlockShape.Normal, world[0, 0].BlockShape);
        }

        [Fact]
        public void Main_tile_Get_halfBrick_SetFalse_NoEffect()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = BlockShape.TopRight };

            Terraria.Main.tile[0, 0].halfBrick(false);

            Assert.Equal(BlockShape.TopRight, world[0, 0].BlockShape);
        }

        [Fact]
        public void Main_tile_Get_slope_GetNormal()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = BlockShape.Normal };

            Assert.Equal(0, Terraria.Main.tile[0, 0].slope());
        }

        [Fact]
        public void Main_tile_Get_slope_GetNotNormal()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = BlockShape.TopRight };

            Assert.Equal((int)(BlockShape.TopRight - 1), Terraria.Main.tile[0, 0].slope());
        }

        [Fact]
        public void Main_tile_Get_slope_SetNonZero()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].slope((byte)(BlockShape.TopRight - 1));

            Assert.Equal(BlockShape.TopRight, world[0, 0].BlockShape);
        }

        [Fact]
        public void Main_tile_Get_slope_SetZero()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = BlockShape.TopRight };

            Terraria.Main.tile[0, 0].slope(0);

            Assert.Equal(BlockShape.Normal, world[0, 0].BlockShape);
        }

        [Fact]
        public void Main_tile_Get_slope_SetZero_NoEffect()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = BlockShape.Halved };

            Terraria.Main.tile[0, 0].slope(0);

            Assert.Equal(BlockShape.Halved, world[0, 0].BlockShape);
        }

        [Theory]
        [InlineData(BlockShape.Normal, false)]
        [InlineData(BlockShape.TopLeft, true)]
        [InlineData(BlockShape.TopRight, true)]
        [InlineData(BlockShape.BottomLeft, false)]
        [InlineData(BlockShape.BottomRight, false)]
        public void Main_tile_Get_topSlope(BlockShape shape, bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = shape };

            Assert.Equal(value, Terraria.Main.tile[0, 0].topSlope());
        }

        [Theory]
        [InlineData(BlockShape.Normal, false)]
        [InlineData(BlockShape.TopLeft, false)]
        [InlineData(BlockShape.TopRight, false)]
        [InlineData(BlockShape.BottomLeft, true)]
        [InlineData(BlockShape.BottomRight, true)]
        public void Main_tile_Get_bottomSlope(BlockShape shape, bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = shape };

            Assert.Equal(value, Terraria.Main.tile[0, 0].bottomSlope());
        }

        [Theory]
        [InlineData(BlockShape.Normal, false)]
        [InlineData(BlockShape.TopLeft, true)]
        [InlineData(BlockShape.TopRight, false)]
        [InlineData(BlockShape.BottomLeft, true)]
        [InlineData(BlockShape.BottomRight, false)]
        public void Main_tile_Get_leftSlope(BlockShape shape, bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = shape };

            Assert.Equal(value, Terraria.Main.tile[0, 0].leftSlope());
        }

        [Theory]
        [InlineData(BlockShape.Normal, false)]
        [InlineData(BlockShape.TopLeft, false)]
        [InlineData(BlockShape.TopRight, true)]
        [InlineData(BlockShape.BottomLeft, false)]
        [InlineData(BlockShape.BottomRight, true)]
        public void Main_tile_Get_rightSlope(BlockShape shape, bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = shape };

            Assert.Equal(value, Terraria.Main.tile[0, 0].rightSlope());
        }

        [Fact]
        public void Main_tile_Get_HasSameSlope_SameSlopes()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = BlockShape.BottomRight };
            world[0, 1] = new Tile { BlockShape = BlockShape.BottomRight };

            Assert.True(Terraria.Main.tile[0, 0].HasSameSlope(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_Get_HasSameSlope_DifferentSlopes()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = BlockShape.BottomRight };
            world[0, 1] = new Tile { BlockShape = BlockShape.BottomLeft };

            Assert.False(Terraria.Main.tile[0, 0].HasSameSlope(Terraria.Main.tile[0, 1]));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_wire_Get(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasRedWire = value };

            Assert.Equal(value, Terraria.Main.tile[0, 0].wire());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_wire_Set(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].wire(value);

            Assert.Equal(value, world[0, 0].HasRedWire);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_wire2_Get(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasBlueWire = value };

            Assert.Equal(value, Terraria.Main.tile[0, 0].wire2());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_wire2_Set(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].wire2(value);

            Assert.Equal(value, world[0, 0].HasBlueWire);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_wire3_Get(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasGreenWire = value };

            Assert.Equal(value, Terraria.Main.tile[0, 0].wire3());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_wire3_Set(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].wire3(value);

            Assert.Equal(value, world[0, 0].HasGreenWire);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_wire4_Get(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasYellowWire = value };

            Assert.Equal(value, Terraria.Main.tile[0, 0].wire4());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_wire4_Set(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].wire4(value);

            Assert.Equal(value, world[0, 0].HasYellowWire);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_actuator_Get(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasActuator = value };

            Assert.Equal(value, Terraria.Main.tile[0, 0].actuator());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_actuator_Set(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].actuator(value);

            Assert.Equal(value, world[0, 0].HasActuator);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_inActive_Get(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { IsBlockActuated = value };

            Assert.Equal(value, Terraria.Main.tile[0, 0].inActive());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_inActive_Set(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].inActive(value);

            Assert.Equal(value, world[0, 0].IsBlockActuated);
        }

        [Theory]
        [InlineData(LiquidType.Water)]
        [InlineData(LiquidType.Honey)]
        [InlineData(LiquidType.Lava)]
        public void Main_tile_Get_liquidType_Get(LiquidType value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Liquid = new Liquid(value, 255) };

            Assert.Equal((byte)value, Terraria.Main.tile[0, 0].liquidType());
        }

        [Theory]
        [InlineData(LiquidType.Water)]
        [InlineData(LiquidType.Honey)]
        [InlineData(LiquidType.Lava)]
        public void Main_tile_Get_liquidType_Set(LiquidType value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].liquidType((int)value);

            Assert.Equal(value, world[0, 0].Liquid.Type);
        }

        [Fact]
        public void Main_tile_Get_lava_Get_ReturnsTrue()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Liquid = new Liquid(LiquidType.Lava, 255) };

            Assert.True(Terraria.Main.tile[0, 0].lava());
        }

        [Fact]
        public void Main_tile_Get_lava_Get_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Liquid = default };

            Assert.False(Terraria.Main.tile[0, 0].lava());
        }

        [Fact]
        public void Main_tile_Get_lava_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].lava(false);

            Assert.NotEqual(LiquidType.Lava, world[0, 0].Liquid.Type);

            Terraria.Main.tile[0, 0].lava(true);

            Assert.Equal(LiquidType.Lava, world[0, 0].Liquid.Type);
        }

        [Fact]
        public void Main_tile_Get_honey_Get_ReturnsTrue()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Liquid = new Liquid(LiquidType.Honey, 255) };

            Assert.True(Terraria.Main.tile[0, 0].honey());
        }

        [Fact]
        public void Main_tile_Get_honey_Get_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Liquid = default };

            Assert.False(Terraria.Main.tile[0, 0].honey());
        }

        [Fact]
        public void Main_tile_Get_honey_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].honey(false);

            Assert.NotEqual(LiquidType.Honey, world[0, 0].Liquid.Type);

            Terraria.Main.tile[0, 0].honey(true);

            Assert.Equal(LiquidType.Honey, world[0, 0].Liquid.Type);
        }

        [Fact]
        public void Main_tile_Get_nactive_Get_ReturnsTrue()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockId = BlockId.Dirt };

            Assert.True(Terraria.Main.tile[0, 0].nactive());
        }

        [Fact]
        public void Main_tile_Get_nactive_Get_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockId = BlockId.None };

            Assert.False(Terraria.Main.tile[0, 0].nactive());

            world[0, 0] = new Tile { BlockId = BlockId.Dirt, IsBlockActuated = true };

            Assert.False(Terraria.Main.tile[0, 0].nactive());
        }

        [Fact]
        public void Main_tile_Get_wallColor_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallColor = PaintColor.Red };

            Assert.Equal((byte)PaintColor.Red, Terraria.Main.tile[0, 0].wallColor());
        }

        [Fact]
        public void Main_tile_Get_wallColor_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].wallColor((byte)PaintColor.DeepRed);

            Assert.Equal(PaintColor.DeepRed, world[0, 0].WallColor);
        }

        [Fact]
        public void Main_tile_Get_frameNumber_Set_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].frameNumber(2);

            Assert.Equal(2, Terraria.Main.tile[0, 0].frameNumber());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_checkingLiquid_Set_Get(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].checkingLiquid(value);

            Assert.Equal(value, Terraria.Main.tile[0, 0].checkingLiquid());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Main_tile_Get_skipLiquid_Set_Get(bool value)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Terraria.Main.tile[0, 0].skipLiquid(value);

            Assert.Equal(value, Terraria.Main.tile[0, 0].skipLiquid());
        }

        [Fact]
        public void Main_tile_Get_CopyFrom_NullTile()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = (BlockId)1,
                WallId = (WallId)2,
                BlockFrameX = 3,
                BlockFrameY = 4,
                Liquid = new Liquid(LiquidType.Water, 5),
                HasRedWire = true
            };

            Terraria.Main.tile[0, 0].CopyFrom(null);

            Assert.Equal(BlockId.None, world[0, 0].BlockId);
            Assert.Equal(WallId.None, world[0, 0].WallId);
            Assert.Equal(0, world[0, 0].BlockFrameX);
            Assert.Equal(0, world[0, 0].BlockFrameY);
            Assert.Equal(default, world[0, 0].Liquid);
            Assert.False(world[0, 0].HasRedWire);
        }

        [Fact]
        public void Main_tile_Get_CopyFrom_TileAdapter()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                BlockFrameX = 3,
                BlockFrameY = 4,
                Liquid = new Liquid(LiquidType.Water, 5),
                HasRedWire = true
            };

            Terraria.Main.tile[0, 1].CopyFrom(Terraria.Main.tile[0, 0]);

            Assert.Equal(BlockId.Stone, world[0, 1].BlockId);
            Assert.Equal(WallId.Dirt, world[0, 1].WallId);
            Assert.Equal(3, world[0, 1].BlockFrameX);
            Assert.Equal(4, world[0, 1].BlockFrameY);
            Assert.Equal(new Liquid(LiquidType.Water, 5), world[0, 1].Liquid);
            Assert.True(world[0, 1].HasRedWire);
        }

        [Fact]
        public void Main_tile_Get_isTheSameAs_NullTile_ReturnsFalse()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(null));
        }

        [Fact]
        public void Main_tile_Get_isTheSameAs_TileAdapter()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockId = BlockId.Dirt };
            world[0, 1] = new Tile { BlockId = BlockId.Stone };

            Assert.False(Terraria.Main.tile[0, 0].isTheSameAs(Terraria.Main.tile[0, 1]));
        }

        [Fact]
        public void Main_tile_Get_ClearEverything()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                BlockFrameX = 3,
                BlockFrameY = 4,
                Liquid = new Liquid(LiquidType.Water, 5),
                HasRedWire = true
            };

            Terraria.Main.tile[0, 0].ClearEverything();

            Assert.Equal(BlockId.None, world[0, 0].BlockId);
            Assert.Equal(WallId.None, world[0, 0].WallId);
            Assert.Equal(0, world[0, 0].BlockFrameX);
            Assert.Equal(0, world[0, 0].BlockFrameY);
            Assert.Equal(default, world[0, 0].Liquid);
            Assert.False(world[0, 0].HasRedWire);
        }

        [Fact]
        public void Main_tile_Get_ClearMetadata()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                BlockFrameX = 3,
                BlockFrameY = 4,
                Liquid = new Liquid(LiquidType.Water, 5),
                HasRedWire = true
            };

            Terraria.Main.tile[0, 0].ClearMetadata();

            Assert.Equal(BlockId.Stone, world[0, 0].BlockId);
            Assert.Equal(WallId.Dirt, world[0, 0].WallId);
            Assert.Equal(0, world[0, 0].BlockFrameX);
            Assert.Equal(0, world[0, 0].BlockFrameY);
            Assert.Equal(default, world[0, 0].Liquid);
            Assert.False(world[0, 0].HasRedWire);
        }

        [Fact]
        public void Main_tile_Get_ClearTile()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                BlockShape = BlockShape.BottomRight,
                IsBlockActuated = true
            };

            Terraria.Main.tile[0, 0].ClearTile();

            Assert.Equal(BlockId.None, world[0, 0].BlockId);
            Assert.Equal(BlockShape.Normal, world[0, 0].BlockShape);
            Assert.False(world[0, 0].IsBlockActuated);
        }

        [Fact]
        public void Main_tile_Get_Clear_Tile()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                BlockFrameX = 1,
                BlockFrameY = 2
            };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Tile);

            Assert.Equal(BlockId.None, world[0, 0].BlockId);
            Assert.Equal(0, world[0, 0].BlockFrameX);
            Assert.Equal(0, world[0, 0].BlockFrameX);
        }

        [Fact]
        public void Main_tile_Get_Clear_TilePaint()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockColor = PaintColor.Red };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.TilePaint);

            Assert.Equal(PaintColor.None, world[0, 0].BlockColor);
        }

        [Fact]
        public void Main_tile_Get_Clear_Wall()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallId = WallId.Dirt };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Wall);

            Assert.Equal(WallId.None, world[0, 0].WallId);
        }

        [Fact]
        public void Main_tile_Get_Clear_WallPaint()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { WallColor = PaintColor.Red };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.WallPaint);

            Assert.Equal(PaintColor.None, world[0, 0].WallColor);
        }

        [Fact]
        public void Main_tile_Get_Clear_Liquid()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { Liquid = new Liquid(LiquidType.Honey, 255) };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Liquid);

            Assert.Equal(default, world[0, 0].Liquid);
        }

        [Fact]
        public void Main_tile_Get_Clear_Wiring()
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
        public void Main_tile_Get_Clear_Actuator()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { HasActuator = true, IsBlockActuated = true };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Actuator);

            Assert.False(world[0, 0].HasActuator);
            Assert.False(world[0, 0].IsBlockActuated);
        }

        [Fact]
        public void Main_tile_Get_Clear_Slope()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile { BlockShape = BlockShape.TopLeft };

            Terraria.Main.tile[0, 0].Clear(Terraria.DataStructures.TileDataType.Slope);

            Assert.Equal(BlockShape.Normal, world[0, 0].BlockShape);
        }

        [Fact]
        public void Main_tile_Get_ResetToType()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                BlockFrameX = 3,
                BlockFrameY = 4,
                Liquid = new Liquid(LiquidType.Water, 5),
                HasRedWire = true
            };

            Terraria.Main.tile[0, 0].ResetToType((ushort)(BlockId.Stone - 1));

            Assert.Equal(BlockId.Stone, world[0, 0].BlockId);
            Assert.Equal(WallId.Dirt, world[0, 0].WallId);
            Assert.Equal(0, world[0, 0].BlockFrameX);
            Assert.Equal(0, world[0, 0].BlockFrameY);
            Assert.Equal(default, world[0, 0].Liquid);
            Assert.False(world[0, 0].HasRedWire);
        }

        [Fact]
        public void Main_tile_Set_NullTile()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = (BlockId)1,
                WallId = (WallId)2,
                BlockFrameX = 3,
                BlockFrameY = 4,
                Liquid = new Liquid(LiquidType.Water, 5),
                HasRedWire = true
            };

            Terraria.Main.tile[0, 0] = null;

            Assert.Equal(BlockId.None, world[0, 0].BlockId);
            Assert.Equal(WallId.None, world[0, 0].WallId);
            Assert.Equal(0, world[0, 0].BlockFrameX);
            Assert.Equal(0, world[0, 0].BlockFrameY);
            Assert.Equal(default, world[0, 0].Liquid);
            Assert.False(world[0, 0].HasRedWire);
        }

        [Fact]
        public void Main_tile_Set_TileAdapter()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0] = new Tile
            {
                BlockId = BlockId.Stone,
                WallId = WallId.Dirt,
                BlockFrameX = 3,
                BlockFrameY = 4,
                Liquid = new Liquid(LiquidType.Water, 5),
                HasRedWire = true
            };

            Terraria.Main.tile[0, 1] = Terraria.Main.tile[0, 0];

            Assert.Equal(BlockId.Stone, world[0, 1].BlockId);
            Assert.Equal(WallId.Dirt, world[0, 1].WallId);
            Assert.Equal(3, world[0, 1].BlockFrameX);
            Assert.Equal(4, world[0, 1].BlockFrameY);
            Assert.Equal(new Liquid(LiquidType.Water, 5), world[0, 1].Liquid);
            Assert.True(world[0, 1].HasRedWire);
        }
    }
}
