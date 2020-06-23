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

using Xunit;

namespace Orion.Launcher
{
    public class TerrariaSynchronizationContextTests
    {
        [Fact]
        public void TryExecute_NoTasks()
        {
            using var context = new TerrariaSynchronizationContext();

            context.TryExecute();
        }

        [Fact]
        public void Post_TryExecute()
        {
            using var context = new TerrariaSynchronizationContext();
            object? state = null;

            context.Post(s => state = s, "test");

            context.TryExecute();

            Assert.Equal("test", state);
        }
    }
}
