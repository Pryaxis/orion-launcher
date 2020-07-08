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
using Xunit;

namespace Orion.Launcher.World.TileEntities
{
    [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Testing")]
    public partial class OrionChestTests
    {
        [Fact]
        public void Name_GetNullValue()
        {
            var terrariaChest = new Terraria.Chest { x = 256, y = 100, name = null };
            var chest = new OrionChest(terrariaChest);

            Assert.Equal(string.Empty, chest.Name);
        }

        [Fact]
        public void Name_Get()
        {
            var terrariaChest = new Terraria.Chest { x = 256, y = 100, name = "test" };
            var chest = new OrionChest(terrariaChest);

            Assert.Equal("test", chest.Name);
        }

        [Fact]
        public void Name_SetNullValue_ThrowsArgumentNullException()
        {
            var terrariaChest = new Terraria.Chest();
            var chest = new OrionChest(terrariaChest);

            Assert.Throws<ArgumentNullException>(() => chest.Name = null!);
        }

        [Fact]
        public void Name_Set()
        {
            var terrariaChest = new Terraria.Chest();
            var chest = new OrionChest(terrariaChest);

            chest.Name = "test";

            Assert.Equal("test", terrariaChest.name);
        }

        [Fact]
        public void Index_Get()
        {
            var terrariaChest = new Terraria.Chest();
            var chest = new OrionChest(1, terrariaChest);

            Assert.Equal(1, chest.Index);
        }

        [Fact]
        public void IsActive_Get_ReturnsFalse()
        {
            var chest = new OrionChest(null);

            Assert.False(chest.IsActive);
        }

        [Fact]
        public void IsActive_Get_ReturnsTrue()
        {
            var terrariaChest = new Terraria.Chest();
            var chest = new OrionChest(terrariaChest);

            Assert.True(chest.IsActive);
        }

        [Fact]
        public void X_Get()
        {
            var terrariaChest = new Terraria.Chest { x = 256, y = 100, name = "test" };
            var chest = new OrionChest(terrariaChest);

            Assert.Equal(256, chest.X);
        }

        [Fact]
        public void X_Set()
        {
            var terrariaChest = new Terraria.Chest();
            var chest = new OrionChest(terrariaChest);

            chest.X = 256;

            Assert.Equal(256, terrariaChest.x);
        }

        [Fact]
        public void Y_Get()
        {
            var terrariaChest = new Terraria.Chest { x = 256, y = 100, name = "test" };
            var chest = new OrionChest(terrariaChest);

            Assert.Equal(100, chest.Y);
        }

        [Fact]
        public void Y_Set()
        {
            var terrariaChest = new Terraria.Chest();
            var chest = new OrionChest(terrariaChest);

            chest.Y = 100;

            Assert.Equal(100, terrariaChest.y);
        }
    }
}
