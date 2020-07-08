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

namespace Orion.Launcher.World.TileEntities
{
    internal sealed partial class OrionChest
    {
        private sealed class ItemArray : IArray<ItemStack>
        {
            private readonly Terraria.Item[] _items;

            private readonly object _lock = new object();

            public ItemArray(Terraria.Item?[] items)
            {
                Debug.Assert(items != null);

                for (var i = 0; i < items.Length; ++i)
                {
                    items[i] ??= new Terraria.Item();
                }
                _items = items!;
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

            public int Count => _items.Length;

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

                return _items[index];
            }

            [ExcludeFromCodeCoverage]
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
