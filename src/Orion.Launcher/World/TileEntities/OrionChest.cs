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
using System.Diagnostics.Contracts;
using Destructurama.Attributed;
using Orion.Core.Entities;
using Orion.Core.Items;
using Orion.Core.Utils;
using Orion.Core.World.TileEntities;
using Orion.Launcher.Utils;

namespace Orion.Launcher.World.TileEntities
{
    [LogAsScalar]
    internal sealed partial class OrionChest : AnnotatableObject, IChest, IWrapping<Terraria.Chest>
    {
        public OrionChest(int chestIndex, Terraria.Chest? terrariaChest)
        {
            Index = chestIndex;
            IsActive = terrariaChest != null;
            Wrapped = terrariaChest ?? new Terraria.Chest();
            Items = new ItemArray(Wrapped.item);
        }

        public OrionChest(Terraria.Chest? terrariaChest) : this(-1, terrariaChest)
        {
        }

        public string Name
        {
            get => Wrapped.name ?? string.Empty;
            set => Wrapped.name = value ?? throw new ArgumentNullException(nameof(value));
        }

        public IArray<ItemStack> Items { get; }

        public int Index { get; }

        public bool IsActive { get; }

        public int X
        {
            get => Wrapped.x;
            set => Wrapped.x = value;
        }

        public int Y
        {
            get => Wrapped.y;
            set => Wrapped.y = value;
        }

        public Terraria.Chest Wrapped { get; }

        [Pure, ExcludeFromCodeCoverage]
        public override string ToString() => this.IsConcrete() ? $"<(index: {Index})>" : "<abstract instance>";
    }
}
