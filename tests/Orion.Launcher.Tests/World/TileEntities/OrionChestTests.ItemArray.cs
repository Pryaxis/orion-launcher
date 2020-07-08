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
using System.Linq;
using Orion.Core.Items;
using Xunit;

namespace Orion.Launcher.World.TileEntities
{
    public partial class OrionChestTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        public void Items_Get_Item_GetInvalidIndex_ThrowsIndexOutOfRangeException(int index)
        {
            var terrariaChest = new Terraria.Chest();
            var chest = new OrionChest(terrariaChest);

            Assert.Throws<IndexOutOfRangeException>(() => chest.Items[index]);
        }

        [Fact]
        public void Items_Get_Item_Get()
        {
            var terrariaChest = new Terraria.Chest();
            terrariaChest.item[0] = new Terraria.Item
            {
                type = (int)ItemId.Sdmg,
                prefix = (byte)ItemPrefix.Unreal,
                stack = 1
            };

            var chest = new OrionChest(terrariaChest);

            Assert.Equal(new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1), chest.Items[0]);
        }

        [Fact]
        public void Items_Get_Item_Get_Null()
        {
            var terrariaChest = new Terraria.Chest();
            var chest = new OrionChest(terrariaChest);

            Assert.Equal(default, chest.Items[0]);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        public void Items_Get_Item_SetInvalidIndex_ThrowsIndexOutOfRangeException(int index)
        {
            var terrariaChest = new Terraria.Chest();
            var chest = new OrionChest(terrariaChest);

            Assert.Throws<IndexOutOfRangeException>(() => chest.Items[index] = default);
        }

        [Fact]
        public void Items_Get_Item_Set()
        {
            var terrariaChest = new Terraria.Chest();
            terrariaChest.item[0] = new Terraria.Item();
            var chest = new OrionChest(terrariaChest);

            chest.Items[0] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaChest.item[0].type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaChest.item[0].prefix);
            Assert.Equal(1, terrariaChest.item[0].stack);
        }

        [Fact]
        public void Items_Get_Item_Set_Null()
        {
            var terrariaChest = new Terraria.Chest();
            var chest = new OrionChest(terrariaChest);

            chest.Items[0] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaChest.item[0].type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaChest.item[0].prefix);
            Assert.Equal(1, terrariaChest.item[0].stack);
        }

        [Fact]
        public void Items_GetEnumerator()
        {
            var terrariaChest = new Terraria.Chest();
            var chest = new OrionChest(terrariaChest);

            for (var i = 0; i < chest.Items.Count; ++i)
            {
                chest.Items[i] = new ItemStack((ItemId)i, ItemPrefix.None, (short)i);
            }

            var items = chest.Items.ToList();
            for (var i = 0; i < items.Count; ++i)
            {
                Assert.Equal(new ItemStack((ItemId)i, ItemPrefix.None, (short)i), items[i]);
            }
        }
    }
}
