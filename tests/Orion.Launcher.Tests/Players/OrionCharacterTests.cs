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

using System.Diagnostics.CodeAnalysis;
using Orion.Core.Players;
using Xunit;

namespace Orion.Launcher.Players
{
    [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Testing")]
    public class OrionCharacterTests
    {
        [Fact]
        public void Difficulty_Get()
        {
            var terrariaPlayer = new Terraria.Player { difficulty = (byte)CharacterDifficulty.Journey };
            var character = new OrionCharacter(terrariaPlayer);

            Assert.Equal(CharacterDifficulty.Journey, character.Difficulty);
        }

        [Fact]
        public void Difficulty_Set()
        {
            var terrariaPlayer = new Terraria.Player();
            var character = new OrionCharacter(terrariaPlayer);

            character.Difficulty = CharacterDifficulty.Journey;

            Assert.Equal(CharacterDifficulty.Journey, (CharacterDifficulty)terrariaPlayer.difficulty);
        }
    }
}
