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
using Orion.Core;
using Orion.Core.DataStructures;
using Orion.Core.Events;
using Orion.Core.Events.Items;
using Orion.Core.Items;
using Serilog;
using Xunit;

namespace Orion.Launcher.Items
{
    // These tests depend on Terraria state.
    [Collection("TerrariaTestsCollection")]
    public class OrionItemServiceTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(10000)]
        public void Items_Item_GetInvalidIndex_ThrowsIndexOutOfRangeException(int index)
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var itemService = new OrionItemService(server, log);

            Assert.Throws<IndexOutOfRangeException>(() => itemService.Items[index]);
        }

        [Fact]
        public void Items_Item_Get()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var itemService = new OrionItemService(server, log);

            var item = itemService.Items[1];

            Assert.Equal(1, item.Index);
            Assert.Same(Terraria.Main.item[1], ((OrionItem)item).Wrapped);
        }

        [Fact]
        public void Items_Item_GetMultipleTimes_ReturnsSameInstance()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var itemService = new OrionItemService(server, log);

            var item = itemService.Items[0];
            var item2 = itemService.Items[0];

            Assert.Same(item, item2);
        }

        [Fact]
        public void Items_GetEnumerator()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var itemService = new OrionItemService(server, log);

            var items = itemService.Items.ToList();

            for (var i = 0; i < items.Count; ++i)
            {
                Assert.Same(Terraria.Main.item[i], ((OrionItem)items[i]).Wrapped);
            }
        }

        [Fact]
        public void ItemDefaults_EventTriggered()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var itemService = new OrionItemService(server, log);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<ItemDefaultsEvent>(
                        evt => ((OrionItem)evt.Item).Wrapped == Terraria.Main.item[0] &&
                            evt.Id == ItemId.Sdmg),
                    log));

            Terraria.Main.item[0].SetDefaults((int)ItemId.Sdmg);

            Assert.Equal(ItemId.Sdmg, (ItemId)Terraria.Main.item[0].type);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void ItemDefaults_AbstractItemEventTriggered()
        {
            var terrariaItem = new Terraria.Item();

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var itemService = new OrionItemService(server, log);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<ItemDefaultsEvent>(
                        evt => ((OrionItem)evt.Item).Wrapped == terrariaItem &&
                            evt.Id == ItemId.Sdmg),
                    log));

            terrariaItem.SetDefaults((int)ItemId.Sdmg);

            Assert.Equal(ItemId.Sdmg, (ItemId)terrariaItem.type);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void ItemDefaults_EventModified()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var itemService = new OrionItemService(server, log);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<ItemDefaultsEvent>(), log))
                .Callback<ItemDefaultsEvent, ILogger>((evt, log) => evt.Id = ItemId.DirtBlock);

            Terraria.Main.item[0].SetDefaults((int)ItemId.Sdmg);

            Assert.Equal(ItemId.DirtBlock, (ItemId)Terraria.Main.item[0].type);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void ItemDefaults_EventCanceled()
        {
            // Clear the item so that we know it's empty.
            Terraria.Main.item[0] = new Terraria.Item { whoAmI = 0 };

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var itemService = new OrionItemService(server, log);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<ItemDefaultsEvent>(), log))
                .Callback<ItemDefaultsEvent, ILogger>((evt, log) => evt.Cancel());

            Terraria.Main.item[0].SetDefaults((int)ItemId.Sdmg);

            Assert.Equal(ItemId.None, (ItemId)Terraria.Main.item[0].type);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void ItemTick_EventTriggered()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var itemService = new OrionItemService(server, log);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(
                    It.Is<ItemTickEvent>(
                        evt => ((OrionItem)evt.Item).Wrapped == Terraria.Main.item[0]),
                    log));

            Terraria.Main.item[0].UpdateItem(0);

            Mock.Get(server.Events).VerifyAll();
        }

        [Fact]
        public void ItemTick_EventCanceled()
        {
            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var itemService = new OrionItemService(server, log);

            Mock.Get(server.Events)
                .Setup(em => em.Raise(It.IsAny<ItemTickEvent>(), log))
                .Callback<ItemTickEvent, ILogger>((evt, log) => evt.Cancel());

            Terraria.Main.item[0].UpdateItem(0);

            Mock.Get(server.Events).VerifyAll();
        }

        [Theory]
        [InlineData(ItemId.StoneBlock, 100, ItemPrefix.None)]
        [InlineData(ItemId.Sdmg, 1, ItemPrefix.Unreal)]
        [InlineData(ItemId.Meowmere, 1, ItemPrefix.Legendary)]
        public void SpawnItem(ItemId id, int stackSize, ItemPrefix prefix)
        {
            // Set up an empty slot for the item to appear at.
            Terraria.Main.item[0] = new Terraria.Item { whoAmI = 0 };

            var server = Mock.Of<IServer>(s => s.Events == Mock.Of<IEventManager>());
            var log = Mock.Of<ILogger>();
            using var itemService = new OrionItemService(server, log);

            var item = itemService.SpawnItem(new ItemStack(id, stackSize, prefix), Vector2f.Zero);

            Assert.Equal(Terraria.Main.item[0], ((OrionItem)item).Wrapped);
            Assert.Equal(id, item.Id);
            Assert.Equal(stackSize, item.StackSize);
            Assert.Equal(prefix, item.Prefix);
        }
    }
}
