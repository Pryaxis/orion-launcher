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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Orion.Core.Entities;
using Orion.Core.Utils;

namespace Orion.Launcher.Players
{
    internal sealed partial class OrionPlayer
    {
        private sealed class BuffArray : IArray<Buff>
        {
            private readonly Terraria.Player _wrapped;

            private readonly object _lock = new object();

            public BuffArray(Terraria.Player terrariaPlayer)
            {
                Debug.Assert(terrariaPlayer != null);

                _wrapped = terrariaPlayer;
            }

            public Buff this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                    {
                        throw new IndexOutOfRangeException($"Index out of range (expected: 0 to {Count - 1})");
                    }

                    lock (_lock)
                    {
                        var id = (BuffId)_wrapped.buffType[index];
                        var ticks = _wrapped.buffTime[index];
                        return ticks > 0 ? new Buff(id, ticks) : default;
                    }
                }

                set
                {
                    if (index < 0 || index >= Count)
                    {
                        throw new IndexOutOfRangeException($"Index out of range (expected: 0 to {Count - 1})");
                    }

                    lock (_lock)
                    {
                        _wrapped.buffType[index] = (int)value.Id;
                        _wrapped.buffTime[index] = value.Ticks;
                    }
                }
            }

            public int Count => Terraria.Player.maxBuffs;

            public IEnumerator<Buff> GetEnumerator()
            {
                for (var i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            [ExcludeFromCodeCoverage]
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
