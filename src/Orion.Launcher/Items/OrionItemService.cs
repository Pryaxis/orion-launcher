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
using Orion.Core.DataStructures;
using Orion.Core.Events;
using Orion.Core.Events.Items;
using Orion.Core.Framework;
using Orion.Core.Items;
using Orion.Launcher.Collections;
using Serilog;

namespace Orion.Launcher.Items
{
    [Binding("orion-items", Author = "Pryaxis", Priority = BindingPriority.Lowest)]
    internal sealed class OrionItemService : IItemService, IDisposable
    {
        private readonly IEventManager _events;
        private readonly ILogger _log;
        private readonly IReadOnlyList<IItem> _items;

        private readonly object _lock = new object();

        public OrionItemService(IEventManager events, ILogger log)
        {
            Debug.Assert(events != null);
            Debug.Assert(log != null);

            _events = events;
            _log = log;

            // Note that the last item should be ignored, as it is not a real item.
            _items = new WrappedReadOnlyList<OrionItem, Terraria.Item>(
                Terraria.Main.item.AsMemory(..^1),
                (itemIndex, terrariaItem) => new OrionItem(itemIndex, terrariaItem));

            OTAPI.Hooks.Item.PreSetDefaultsById = PreSetDefaultsByIdHandler;
            OTAPI.Hooks.Item.PreUpdate = PreUpdateHandler;
        }

        public IItem this[int index] => _items[index];

        public int Count => _items.Count;

        public IEnumerator<IItem> GetEnumerator() => _items.GetEnumerator();

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IItem SpawnItem(ItemStack itemStack, Vector2f position)
        {
            // Not localized because this string is developer-facing.
            Log.Debug("Spawning {ItemStack} at {Position}", itemStack);

            lock (_lock)
            {
                var itemIndex = Terraria.Item.NewItem(
                    (int)position.X, (int)position.Y, 0, 0, (int)itemStack.Id, itemStack.StackSize, false,
                    (int)itemStack.Prefix);
                Debug.Assert(itemIndex >= 0 && itemIndex < Count);

                return this[itemIndex];
            }
        }

        public void Dispose()
        {
            OTAPI.Hooks.Item.PreSetDefaultsById = null;
            OTAPI.Hooks.Item.PreUpdate = null;
        }

        // =============================================================================================================
        // OTAPI hooks
        //

        private OTAPI.HookResult PreSetDefaultsByIdHandler(
            Terraria.Item terrariaItem, ref int itemId, ref bool noMatCheck)
        {
            Debug.Assert(terrariaItem != null);

            var item = GetItem(terrariaItem);
            var evt = new ItemDefaultsEvent(item) { Id = (ItemId)itemId };
            _events.Raise(evt, _log);
            if (evt.IsCanceled)
            {
                return OTAPI.HookResult.Cancel;
            }

            itemId = (int)evt.Id;
            return OTAPI.HookResult.Continue;
        }

        private OTAPI.HookResult PreUpdateHandler(Terraria.Item terrariaItem, ref int itemIndex)
        {
            Debug.Assert(terrariaItem != null);
            Debug.Assert(itemIndex >= 0 && itemIndex < Count);

            // Set `whoAmI` since this is never done in the vanilla server, and we depend on this field being set in
            // `GetItem`.
            terrariaItem.whoAmI = itemIndex;

            var evt = new ItemTickEvent(this[itemIndex]);
            _events.Raise(evt, _log);
            return evt.IsCanceled ? OTAPI.HookResult.Cancel : OTAPI.HookResult.Continue;
        }

        // Gets an `IItem` which corresponds to the given Terraria item. Retrieves the `IItem` from the `Items` array,
        // if possible.
        private IItem GetItem(Terraria.Item terrariaItem)
        {
            Debug.Assert(terrariaItem != null);

            var itemIndex = terrariaItem.whoAmI;
            Debug.Assert(itemIndex >= 0 && itemIndex < Count);

            var isConcrete = ReferenceEquals(terrariaItem, Terraria.Main.item[itemIndex]);
            return isConcrete ? this[itemIndex] : new OrionItem(terrariaItem);
        }
    }
}
