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
using Orion.Core.Entities;
using Orion.Core.Events;
using Serilog;
using Xunit;

namespace Orion.Launcher.Players
{
    public partial class OrionPlayerTests
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        public void Buffs_Get_Item_GetIndexOutOfRange_ThrowsIndexOutOfRangeException(int index)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Throws<IndexOutOfRangeException>(() => player.Buffs[index]);
        }

        [Fact]
        public void Buffs_Get_Item_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.buffType[0] = (int)BuffId.ObsidianSkin;
            terrariaPlayer.buffTime[0] = 28800;

            Assert.Equal(new Buff(BuffId.ObsidianSkin, TimeSpan.FromMinutes(8)), player.Buffs[0]);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Buffs_Get_Item_InvalidTime_Get(int buffTime)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            terrariaPlayer.buffType[0] = (int)BuffId.ObsidianSkin;
            terrariaPlayer.buffTime[0] = buffTime;

            Assert.Equal(default, player.Buffs[0]);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(100)]
        public void Buffs_Get_Item_SetIndexOutOfRange_ThrowsIndexOutOfRangeException(int index)
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Throws<IndexOutOfRangeException>(() => player.Buffs[index] = default);
        }

        [Fact]
        public void Buffs_Get_Item_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            player.Buffs[0] = new Buff(BuffId.ObsidianSkin, TimeSpan.FromMinutes(8));

            Assert.Equal(BuffId.ObsidianSkin, (BuffId)terrariaPlayer.buffType[0]);
            Assert.Equal(28800, terrariaPlayer.buffTime[0]);
        }
        
        [Fact]
        public void Buffs_Get_Count_Get()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            Assert.Equal(22, player.Buffs.Count);
        }

        [Fact]
        public void Buffs_Get_GetEnumerator()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            var terrariaPlayer = new Terraria.Player();
            var player = new OrionPlayer(terrariaPlayer, events, log);

            for (var i = 0; i < Terraria.Player.maxBuffs; ++i)
            {
                terrariaPlayer.buffType[i] = i;
                terrariaPlayer.buffTime[i] = 60;
            }

            var buffs = player.Buffs.ToList();
            for (var i = 0; i < buffs.Count; ++i)
            {
                Assert.Equal(new Buff((BuffId)i, 60), buffs[i]);
            }
        }
    }
}
