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
using System.Runtime.CompilerServices;
using Orion.Core.World;
using Orion.Core.World.Tiles;

namespace Orion.Launcher.World
{
    internal sealed partial class OrionWorld : IWorld, IDisposable
    {
        // An adapter class to make a `Tile` reference compatible with `OTAPI.Tile.ITile`. Unfortunately, this means we
        // generate a lot of garbage, but this is the best we can really do.
        private sealed unsafe class TileAdapter : OTAPI.Tile.ITile
        {
            private readonly Tile* _tile;

            public TileAdapter(ref Tile tile)
            {
                _tile = (Tile*)Unsafe.AsPointer(ref tile);
            }

            public ushort type
            {
                get => (ushort)_tile->BlockId;
                set => _tile->BlockId = (BlockId)value;
            }

            public ushort wall
            {
                get => (ushort)_tile->WallId;
                set => _tile->WallId = (WallId)value;
            }

            public byte liquid
            {
                get => _tile->LiquidAmount;
                set => _tile->LiquidAmount = value;
            }

            public short frameX
            {
                get => _tile->BlockFrameX;
                set => _tile->BlockFrameX = value;
            }

            public short frameY
            {
                get => _tile->BlockFrameY;
                set => _tile->BlockFrameY = value;
            }

            public short sTileHeader
            {
                get => Unsafe.ReadUnaligned<short>(((byte*)_tile) + 9);
                set => Unsafe.WriteUnaligned(((byte*)_tile) + 9, value);
            }

            public byte bTileHeader
            {
                get => *((byte*)_tile + 11);
                set => *((byte*)_tile + 11) = value;
            }

            public byte bTileHeader3
            {
                get => *((byte*)_tile + 12);
                set => *((byte*)_tile + 12) = value;
            }

            // No-ops since these are never used.
            [ExcludeFromCodeCoverage]
            public int collisionType => 0;

            public byte bTileHeader2
            {
                [ExcludeFromCodeCoverage]
                get => 0;
                [ExcludeFromCodeCoverage]
                set { }
            }

            public byte color() => (byte)_tile->BlockColor;
            public void color(byte color) => _tile->BlockColor = (PaintColor)color;
            public bool active() => _tile->IsBlockActive;
            public void active(bool active) => _tile->IsBlockActive = active;
            public bool inActive() => _tile->IsBlockActuated;
            public void inActive(bool inActive) => _tile->IsBlockActuated = inActive;
            public bool nactive() => (sTileHeader & 0x60) == 0x20;
            public bool wire() => _tile->HasRedWire;
            public void wire(bool wire) => _tile->HasRedWire = wire;

            public bool wire2() => _tile->HasBlueWire;
            public void wire2(bool wire2) => _tile->HasBlueWire = wire2;
            public bool wire3() => _tile->HasGreenWire;
            public void wire3(bool wire3) => _tile->HasGreenWire = wire3;
            public bool halfBrick() => _tile->IsBlockHalved;
            public void halfBrick(bool halfBrick) => _tile->IsBlockHalved = halfBrick;
            public bool actuator() => _tile->HasActuator;
            public void actuator(bool actuator) => _tile->HasActuator = actuator;
            public byte slope() => (byte)_tile->Slope;
            public void slope(byte slope) => _tile->Slope = (Slope)slope;

            public byte wallColor() => (byte)_tile->WallColor;
            public void wallColor(byte wallColor) => _tile->WallColor = (PaintColor)wallColor;
            public bool lava() => (bTileHeader & 0x20) == 0x20;

            public void lava(bool lava)
            {
                if (lava)
                {
                    bTileHeader = (byte)((bTileHeader & 0x9F) | 0x20);
                }
                else
                {
                    bTileHeader &= 223;
                }
            }

            public bool honey() => (bTileHeader & 0x40) == 0x40;

            public void honey(bool honey)
            {
                if (honey)
                {
                    bTileHeader = (byte)((bTileHeader & 0x9F) | 0x40);
                }
                else
                {
                    bTileHeader &= 191;
                }
            }

            public byte liquidType() => (byte)_tile->Liquid;
            public void liquidType(int liquidType) => _tile->Liquid = (Liquid)liquidType;
            public bool wire4() => _tile->HasYellowWire;
            public void wire4(bool wire4) => _tile->HasYellowWire = wire4;

            public byte frameNumber() => _tile->BlockFrameNumber;
            public void frameNumber(byte frameNumber) => _tile->BlockFrameNumber = frameNumber;
            public bool checkingLiquid() => _tile->IsCheckingLiquid;
            public void checkingLiquid(bool checkingLiquid) => _tile->IsCheckingLiquid = checkingLiquid;
            public bool skipLiquid() => _tile->ShouldSkipLiquid;
            public void skipLiquid(bool skipLiquid) => _tile->ShouldSkipLiquid = skipLiquid;

            public void CopyFrom(OTAPI.Tile.ITile from)
            {
                if (from is null)
                {
                    ClearEverything();
                    return;
                }

                if (from is TileAdapter adapter)
                {
                    Unsafe.CopyBlockUnaligned(_tile, adapter._tile, 13);
                }
                else
                {
                    type = from.type;
                    wall = from.wall;
                    liquid = from.liquid;
                    sTileHeader = from.sTileHeader;
                    bTileHeader = from.bTileHeader;
                    bTileHeader3 = from.bTileHeader3;
                    frameX = from.frameX;
                    frameY = from.frameY;
                }
            }

            public bool isTheSameAs(OTAPI.Tile.ITile compTile)
            {
                if (compTile is null)
                {
                    return false;
                }

                if (compTile is TileAdapter adapter)
                {
                    var mask = liquid == 0 ?
                        0b_00000000_11111111_11111111_11111111 :
                        0b_00000000_10011111_11111111_11111111;
                    if ((_tile->Header & mask) != (adapter._tile->Header & mask))
                    {
                        return false;
                    }

                    if (active())
                    {
                        if (type != adapter.type)
                        {
                            return false;
                        }

                        if (_tile->BlockId.HasFrames() && (frameX != adapter.frameX || frameY != adapter.frameY))
                        {
                            return false;
                        }
                    }

                    if (wall != adapter.wall || liquid != adapter.liquid)
                    {
                        return false;
                    }
                }
                else
                {
                    if (sTileHeader != compTile.sTileHeader)
                    {
                        return false;
                    }

                    if (active())
                    {
                        if (type != compTile.type)
                        {
                            return false;
                        }

                        if (_tile->BlockId.HasFrames() && (frameX != compTile.frameX || frameY != compTile.frameY))
                        {
                            return false;
                        }
                    }

                    if (wall != compTile.wall || liquid != compTile.liquid)
                    {
                        return false;
                    }

                    if (liquid == 0)
                    {
                        if (wallColor() != compTile.wallColor())
                        {
                            return false;
                        }

                        if (wire4() != compTile.wire4())
                        {
                            return false;
                        }
                    }
                    else if (bTileHeader != compTile.bTileHeader)
                    {
                        return false;
                    }
                }

                return true;
            }

            public void ClearEverything() => Unsafe.InitBlockUnaligned(_tile, 0, 13);
            public void ClearMetadata() => Unsafe.InitBlockUnaligned(((byte*)_tile) + 4, 0, 9);

            public void ClearTile()
            {
                active(false);
                inActive(false);
                slope(0);
                halfBrick(false);
            }

            public void Clear(Terraria.DataStructures.TileDataType types)
            {
                if ((types & Terraria.DataStructures.TileDataType.Tile) != 0)
                {
                    type = 0;
                    active(false);
                    frameX = 0;
                    frameY = 0;
                }

                if ((types & Terraria.DataStructures.TileDataType.TilePaint) != 0)
                {
                    color(0);
                }

                if ((types & Terraria.DataStructures.TileDataType.Wall) != 0)
                {
                    wall = 0;
                }

                if ((types & Terraria.DataStructures.TileDataType.WallPaint) != 0)
                {
                    wallColor(0);
                }

                if ((types & Terraria.DataStructures.TileDataType.Liquid) != 0)
                {
                    liquid = 0;
                    liquidType(0);
                    checkingLiquid(false);
                }

                if ((types & Terraria.DataStructures.TileDataType.Wiring) != 0)
                {
                    wire(false);
                    wire2(false);
                    wire3(false);
                    wire4(false);
                }

                if ((types & Terraria.DataStructures.TileDataType.Actuator) != 0)
                {
                    actuator(false);
                    inActive(false);
                }

                if ((types & Terraria.DataStructures.TileDataType.Slope) != 0)
                {
                    slope(0);
                    halfBrick(false);
                }
            }

            public void ResetToType(ushort type)
            {
                ClearMetadata();
                this.type = type;
                active(true);
            }

            public bool topSlope() => IsSlope(Slope.TopRight, Slope.TopLeft);
            public bool bottomSlope() => IsSlope(Slope.BottomRight, Slope.BottomLeft);
            public bool leftSlope() => IsSlope(Slope.TopLeft, Slope.BottomLeft);
            public bool rightSlope() => IsSlope(Slope.TopRight, Slope.BottomRight);
            public bool HasSameSlope(OTAPI.Tile.ITile tile) => (sTileHeader & 29696) == (tile.sTileHeader & 29696);

            public int blockType()
            {
                if (halfBrick())
                {
                    return 1;
                }

                var slope = this.slope();
                return slope > 0 ? slope + 1 : 0;
            }

            // No-ops since these are never used.
            [ExcludeFromCodeCoverage]
            public object Clone() => MemberwiseClone();

            [ExcludeFromCodeCoverage]
            public Microsoft.Xna.Framework.Color actColor(Microsoft.Xna.Framework.Color oldColor) => default;

            [ExcludeFromCodeCoverage]
            public void actColor(ref Microsoft.Xna.Framework.Vector3 oldColor) { }

            [ExcludeFromCodeCoverage] public byte wallFrameNumber() => 0;
            [ExcludeFromCodeCoverage] public void wallFrameNumber(byte wallFrameNumber) { }
            [ExcludeFromCodeCoverage] public int wallFrameX() => 0;
            [ExcludeFromCodeCoverage] public void wallFrameX(int wallFrameX) { }
            [ExcludeFromCodeCoverage] public int wallFrameY() => 0;
            [ExcludeFromCodeCoverage] public void wallFrameY(int wallFrameY) { }

            private bool IsSlope(Slope slope1, Slope slope2)
            {
                var slope = (Slope)this.slope();
                return slope == slope1 || slope == slope2;
            }
        }
    }
}
