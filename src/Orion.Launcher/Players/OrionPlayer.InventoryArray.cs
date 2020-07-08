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
using Orion.Core.Items;
using Orion.Core.Utils;

namespace Orion.Launcher.Players
{
    internal sealed partial class OrionPlayer
    {
        private sealed class InventoryArray : IArray<ItemStack>
        {
            private readonly Terraria.Player _wrapped;

            private readonly object _lock = new object();

            public InventoryArray(Terraria.Player terrariaPlayer)
            {
                Debug.Assert(terrariaPlayer != null);

                _wrapped = terrariaPlayer;
            }

            public ItemStack this[int index]
            {
                get
                {
                    var item = GetItem(index);

                    lock (_lock)
                    {
                        return new ItemStack((ItemId)item.type, (ItemPrefix)item.prefix, (short)item.stack);
                    }
                }

                set
                {
                    var item = GetItem(index);

                    lock (_lock)
                    {
                        item.type = (int)value.Id;
                        item.prefix = (byte)value.Prefix;
                        item.stack = value.StackSize;
                    }
                }
            }

            public int Count => 260;

            public IEnumerator<ItemStack> GetEnumerator()
            {
                for (var i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            private Terraria.Item GetItem(int index)
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException($"Index out of range (expected: 0 to {Count - 1})");
                }

                return index switch
                {
                    _ when index < 59 => _wrapped.inventory[index],
                    _ when index < 79 => _wrapped.armor[index - 59],
                    _ when index < 89 => _wrapped.dye[index - 79],
                    _ when index < 94 => _wrapped.miscEquips[index - 89],
                    _ when index < 99 => _wrapped.miscDyes[index - 94],
                    _ when index < 139 => _wrapped.bank.item[index - 99],
                    _ when index < 179 => _wrapped.bank2.item[index - 139],
                    _ when index == 179 => _wrapped.trashItem,
                    _ when index < 220 => _wrapped.bank3.item[index - 180],
                    _ => _wrapped.bank4.item[index - 220]
                };
            }

            [ExcludeFromCodeCoverage]
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
