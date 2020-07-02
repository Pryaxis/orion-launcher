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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TileAdapter(ref Tile tile)
            {
                _tile = (Tile*)Unsafe.AsPointer(ref tile);
            }

            public ushort type
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (ushort)_tile->BlockId;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _tile->BlockId = (BlockId)value;
            }

            public ushort wall
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (ushort)_tile->WallId;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _tile->WallId = (WallId)value;
            }

            public byte liquid
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => *((byte*)_tile + 4);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => *((byte*)_tile + 4) = value;
            }

            public short frameX
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _tile->BlockFrameX;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _tile->BlockFrameX = value;
            }

            public short frameY
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _tile->BlockFrameY;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _tile->BlockFrameY = value;
            }

            public short sTileHeader
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Unsafe.ReadUnaligned<short>(((byte*)_tile) + 9);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => Unsafe.WriteUnaligned(((byte*)_tile) + 9, value);
            }

            public byte bTileHeader
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => *((byte*)_tile + 11);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => *((byte*)_tile + 11) = value;
            }

            public byte bTileHeader3
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => *((byte*)_tile + 12);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte color() => (byte)_tile->BlockColor;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void color(byte color) => _tile->BlockColor = (PaintColor)color;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool active() => _tile->IsBlockActive;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void active(bool active) => _tile->IsBlockActive = active;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool inActive() => _tile->IsBlockActuated;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void inActive(bool inActive) => _tile->IsBlockActuated = inActive;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool nactive() => (sTileHeader & 0x0060) == 0x0020;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool wire() => _tile->HasRedWire;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void wire(bool wire) => _tile->HasRedWire = wire;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool wire2() => _tile->HasBlueWire;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void wire2(bool wire2) => _tile->HasBlueWire = wire2;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool wire3() => _tile->HasGreenWire;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void wire3(bool wire3) => _tile->HasGreenWire = wire3;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool halfBrick() => (sTileHeader & 0x0400) != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void halfBrick(bool halfBrick)
            {
                if (halfBrick)
                {
                    sTileHeader |= 0x0400;
                }
                else
                {
                    sTileHeader = (short)(sTileHeader & 0xFBFF);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool actuator() => _tile->HasActuator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void actuator(bool actuator) => _tile->HasActuator = actuator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte slope() => (byte)((sTileHeader & 0x7000) >> 12);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void slope(byte slope) => sTileHeader = (short)((sTileHeader & 0x8FFF) | ((slope & 7) << 12));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte wallColor() => (byte)_tile->WallColor;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void wallColor(byte wallColor) => _tile->WallColor = (PaintColor)wallColor;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool lava() => (bTileHeader & 0x20) != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void lava(bool lava)
            {
                if (lava)
                {
                    bTileHeader = (byte)((bTileHeader & 0x9F) | 0x20);
                }
                else
                {
                    bTileHeader &= 0xdf;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool honey() => (bTileHeader & 0x40) != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void honey(bool honey)
            {
                if (honey)
                {
                    bTileHeader = (byte)((bTileHeader & 0x9F) | 0x40);
                }
                else
                {
                    bTileHeader &= 0xbf;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte liquidType() => (byte)((bTileHeader & 0x60) >> 5);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void liquidType(int liquidType) => bTileHeader = (byte)((bTileHeader & 0x9f) | (liquidType << 5));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool wire4() => _tile->HasYellowWire;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void wire4(bool wire4) => _tile->HasYellowWire = wire4;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte frameNumber() => (byte)(bTileHeader3 & 0x03);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void frameNumber(byte frameNumber) => bTileHeader3 = (byte)((bTileHeader3 & 0xfc) | frameNumber);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool checkingLiquid() => (bTileHeader3 & 0x04) != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void checkingLiquid(bool checkingLiquid)
            {
                if (checkingLiquid)
                {
                    bTileHeader3 |= 0x04;
                }
                else
                {
                    bTileHeader3 &= 0xfb;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool skipLiquid() => (bTileHeader3 & 0x08) != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void skipLiquid(bool skipLiquid)
            {
                if (skipLiquid)
                {
                    bTileHeader3 |= 0x08;
                }
                else
                {
                    bTileHeader3 &= 0xf7;
                }
            }

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
                    var mask = liquid == 0
                        ? 0b_00000000_11111111_11111111_11111111
                        : 0b_00000000_10011111_11111111_11111111;
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ClearEverything() => Unsafe.InitBlockUnaligned(_tile, 0, 13);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ClearMetadata() => Unsafe.InitBlockUnaligned(((byte*)_tile) + 4, 0, 9);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ClearTile()
            {
                active(false);
                inActive(false);
                slope(0);
                halfBrick(false);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ResetToType(ushort type)
            {
                ClearMetadata();
                this.type = type;
                active(true);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool topSlope() => IsSlope(1, 2);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool bottomSlope() => IsSlope(3, 4);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool leftSlope() => IsSlope(2, 4);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool rightSlope() => IsSlope(1, 3);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool HasSameSlope(OTAPI.Tile.ITile tile) => (sTileHeader & 29696) == (tile.sTileHeader & 29696);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool IsSlope(byte slope1, byte slope2)
            {
                var slope = this.slope();
                return slope == slope1 || slope == slope2;
            }
        }
    }
}
