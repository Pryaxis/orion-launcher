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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Orion.Core.Entities;
using Orion.Core.Utils;
using Orion.Launcher.Utils;

namespace Orion.Launcher.Entities
{
    internal abstract class OrionEntity<TTerrariaEntity> : AnnotatableObject, IEntity, IWrapping<TTerrariaEntity>
        where TTerrariaEntity : Terraria.Entity
    {
        protected OrionEntity(int entityIndex, TTerrariaEntity terrariaEntity)
        {
            Debug.Assert(terrariaEntity != null);

            Index = entityIndex;
            Wrapped = terrariaEntity;
        }

        public int Index { get; }

        public bool IsActive
        {
            get => Wrapped.active;
            set => Wrapped.active = value;
        }

        // `Terraria.Entity` does not provide this property so we need to declare this.
        public abstract string Name { get; set; }

        public Vector2f Position
        {
            get => new Vector2f(Wrapped.position.X, Wrapped.position.Y);
            set => Wrapped.position = new Microsoft.Xna.Framework.Vector2(value.X, value.Y);
        }

        public Vector2f Velocity
        {
            get => new Vector2f(Wrapped.velocity.X, Wrapped.velocity.Y);
            set => Wrapped.velocity = new Microsoft.Xna.Framework.Vector2(value.X, value.Y);
        }

        public Vector2f Size
        {
            get => new Vector2f(Wrapped.Size.X, Wrapped.Size.Y);
            set => Wrapped.Size = new Microsoft.Xna.Framework.Vector2(value.X, value.Y);
        }

        public TTerrariaEntity Wrapped { get; }

        [ExcludeFromCodeCoverage]
        public override string ToString() => this.IsConcrete() ? $"<{Name} (index: {Index})>" : "<abstract instance>";
    }
}
