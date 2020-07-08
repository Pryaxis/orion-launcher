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
using Moq;
using Orion.Core.Events;
using Orion.Core.Items;
using Serilog;
using Xunit;

namespace Orion.Launcher.Players
{
    public partial class OrionPlayerTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(1000)]
        public void Inventory_Get_Item_GetIndexOutOfRange_ThrowsIndexOutOfRangeException(int index)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Throws<IndexOutOfRangeException>(() => player.Inventory[index]);
        }

        [Fact]
        public void Inventory_Get_Item_GetInventory()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.inventory[6] = new Terraria.Item
            {
                type = (int)ItemId.Sdmg,
                prefix = (byte)ItemPrefix.Unreal,
                stack = 1
            };

            Assert.Equal(new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1), player.Inventory[6]);
        }

        [Fact]
        public void Inventory_Get_Item_GetArmor()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.armor[6] = new Terraria.Item
            {
                type = (int)ItemId.Sdmg,
                prefix = (byte)ItemPrefix.Unreal,
                stack = 1
            };

            Assert.Equal(new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1), player.Inventory[65]);
        }

        [Fact]
        public void Inventory_Get_Item_GetDye()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.dye[6] = new Terraria.Item
            {
                type = (int)ItemId.Sdmg,
                prefix = (byte)ItemPrefix.Unreal,
                stack = 1
            };

            Assert.Equal(new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1), player.Inventory[85]);
        }

        [Fact]
        public void Inventory_Get_Item_GetMiscEquips()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.miscEquips[3] = new Terraria.Item
            {
                type = (int)ItemId.Sdmg,
                prefix = (byte)ItemPrefix.Unreal,
                stack = 1
            };

            Assert.Equal(new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1), player.Inventory[92]);
        }

        [Fact]
        public void Inventory_Get_Item_GetMiscDyes()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.miscDyes[3] = new Terraria.Item
            {
                type = (int)ItemId.Sdmg,
                prefix = (byte)ItemPrefix.Unreal,
                stack = 1
            };

            Assert.Equal(new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1), player.Inventory[97]);
        }

        [Fact]
        public void Inventory_Get_Item_GetBank()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.bank.item[3] = new Terraria.Item
            {
                type = (int)ItemId.Sdmg,
                prefix = (byte)ItemPrefix.Unreal,
                stack = 1
            };

            Assert.Equal(new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1), player.Inventory[102]);
        }

        [Fact]
        public void Inventory_Get_Item_GetBank2()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.bank2.item[3] = new Terraria.Item
            {
                type = (int)ItemId.Sdmg,
                prefix = (byte)ItemPrefix.Unreal,
                stack = 1
            };

            Assert.Equal(new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1), player.Inventory[142]);
        }

        [Fact]
        public void Inventory_Get_Item_GetTrashItem()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.trashItem = new Terraria.Item
            {
                type = (int)ItemId.Sdmg,
                prefix = (byte)ItemPrefix.Unreal,
                stack = 1
            };

            Assert.Equal(new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1), player.Inventory[179]);
        }

        [Fact]
        public void Inventory_Get_Item_GetBank3()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.bank3.item[3] = new Terraria.Item
            {
                type = (int)ItemId.Sdmg,
                prefix = (byte)ItemPrefix.Unreal,
                stack = 1
            };

            Assert.Equal(new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1), player.Inventory[183]);
        }

        [Fact]
        public void Inventory_Get_Item_GetBank4()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.bank4.item[3] = new Terraria.Item
            {
                type = (int)ItemId.Sdmg,
                prefix = (byte)ItemPrefix.Unreal,
                stack = 1
            };

            Assert.Equal(new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1), player.Inventory[223]);
        }

        [Fact]
        public void Inventory_Get_Item_SetInventory()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Inventory[6] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaPlayer.inventory[6].type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaPlayer.inventory[6].prefix);
            Assert.Equal(1, terrariaPlayer.inventory[6].stack);
        }

        [Fact]
        public void Inventory_Get_Item_SetArmor()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Inventory[65] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaPlayer.armor[6].type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaPlayer.armor[6].prefix);
            Assert.Equal(1, terrariaPlayer.armor[6].stack);
        }

        [Fact]
        public void Inventory_Get_Item_SetDye()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Inventory[85] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaPlayer.dye[6].type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaPlayer.dye[6].prefix);
            Assert.Equal(1, terrariaPlayer.dye[6].stack);
        }

        [Fact]
        public void Inventory_Get_Item_SetMiscEquips()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Inventory[92] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaPlayer.miscEquips[3].type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaPlayer.miscEquips[3].prefix);
            Assert.Equal(1, terrariaPlayer.miscEquips[3].stack);
        }

        [Fact]
        public void Inventory_Get_Item_SetMiscDyes()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Inventory[97] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaPlayer.miscDyes[3].type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaPlayer.miscDyes[3].prefix);
            Assert.Equal(1, terrariaPlayer.miscDyes[3].stack);
        }

        [Fact]
        public void Inventory_Get_Item_SetBank()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Inventory[102] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaPlayer.bank.item[3].type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaPlayer.bank.item[3].prefix);
            Assert.Equal(1, terrariaPlayer.bank.item[3].stack);
        }

        [Fact]
        public void Inventory_Get_Item_SetBank2()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Inventory[142] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaPlayer.bank2.item[3].type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaPlayer.bank2.item[3].prefix);
            Assert.Equal(1, terrariaPlayer.bank2.item[3].stack);
        }

        [Fact]
        public void Inventory_Get_Item_SetTrashItem()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Inventory[179] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaPlayer.trashItem.type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaPlayer.trashItem.prefix);
            Assert.Equal(1, terrariaPlayer.trashItem.stack);
        }

        [Fact]
        public void Inventory_Get_Item_SetBank3()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Inventory[183] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaPlayer.bank3.item[3].type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaPlayer.bank3.item[3].prefix);
            Assert.Equal(1, terrariaPlayer.bank3.item[3].stack);
        }

        [Fact]
        public void Inventory_Get_Item_SetBank4()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Inventory[223] = new ItemStack(ItemId.Sdmg, ItemPrefix.Unreal, 1);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaPlayer.bank4.item[3].type);
            Assert.Equal(ItemPrefix.Unreal, (ItemPrefix)terrariaPlayer.bank4.item[3].prefix);
            Assert.Equal(1, terrariaPlayer.bank4.item[3].stack);
        }

        [Fact]
        public void Inventory_Get_Count_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Equal(260, player.Inventory.Count);
        }

        [Fact]
        public void Inventory_Get_GetEnumerator()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            for (var i = 0; i < player.Inventory.Count; ++i)
            {
                player.Inventory[i] = new ItemStack((ItemId)i, ItemPrefix.None, (short)i);
            }

            var inventory = player.Inventory.ToList();
            for (var i = 0; i < inventory.Count; ++i)
            {
                Assert.Equal(new ItemStack((ItemId)i, ItemPrefix.None, (short)i), inventory[i]);
            }
        }
    }
}
