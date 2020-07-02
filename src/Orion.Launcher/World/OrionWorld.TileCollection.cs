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
using System.Runtime.CompilerServices;
using Orion.Core.World;

namespace Orion.Launcher.World
{
    internal sealed partial class OrionWorld : IWorld, IDisposable
    {
        private sealed class TileCollection : OTAPI.Tile.ITileCollection
        {
            private readonly IWorld _world;

            public TileCollection(IWorld world)
            {
                Debug.Assert(world != null);

                _world = world;
            }

            public unsafe OTAPI.Tile.ITile this[int x, int y]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new TileAdapter(ref _world[x, y]);

                // TODO: optimize this to not generate garbage.
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => this[x, y].CopyFrom(value);
            }

            public int Width => Terraria.Main.maxTilesX;

            public int Height => Terraria.Main.maxTilesY;
        }
    }
}
