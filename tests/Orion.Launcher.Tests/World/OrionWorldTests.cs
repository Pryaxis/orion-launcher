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
using Moq;
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.World.Tiles;
using Orion.Core.Players;
using Serilog;
using Xunit;
using Orion.Launcher.World;
using Orion.Core.Events.World;
using Orion.Core.World.Tiles;
using Orion.Core.World;
using System.Diagnostics.CodeAnalysis;
using Orion.Core.Packets.World.Tiles;
using Orion.Core.Packets.DataStructures;

namespace Orion.Launcher
{
    // These tests depend on Terraria state.
    [Collection("TerrariaTestsCollection")]
    [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Testing")]
    public partial class OrionWorldTests
    {
        [Fact]
        public void Item_Mutate()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world[0, 0].BlockId = BlockId.Stone;

            Assert.Equal(BlockId.Stone, world[0, 0].BlockId);
        }

        [Fact]
        public void Width_Get()
        {
            Terraria.Main.maxTilesX = 8400;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(8400, world.Width);
        }

        [Fact]
        public void Height_Get()
        {
            Terraria.Main.maxTilesY = 2400;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(2400, world.Height);
        }

        [Fact]
        public void Name_GetNullValue()
        {
            Terraria.Main.worldName = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal("", world.Name);
        }

        [Fact]
        public void Name_Get()
        {
            Terraria.Main.worldName = "test";

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal("test", world.Name);
        }

        [Fact]
        public void Evil_Get_ReturnsCorruption()
        {
            Terraria.WorldGen.crimson = false;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(WorldEvil.Corruption, world.Evil);
        }

        [Fact]
        public void Evil_Get_ReturnsCrimson()
        {
            Terraria.WorldGen.crimson = true;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(WorldEvil.Crimson, world.Evil);
        }

        [Fact]
        public void Evil_Set_Corruption()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world.Evil = WorldEvil.Corruption;

            Assert.False(Terraria.WorldGen.crimson);
        }

        [Fact]
        public void Evil_Set_Crimson()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world.Evil = WorldEvil.Crimson;

            Assert.True(Terraria.WorldGen.crimson);
        }

        [Fact]
        public void Difficulty_Get()
        {
            Terraria.Main.GameMode = (int)WorldDifficulty.Master;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Assert.Equal(WorldDifficulty.Master, world.Difficulty);
        }

        [Fact]
        public void Difficulty_Set()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            world.Difficulty = WorldDifficulty.Master;

            Assert.Equal(WorldDifficulty.Master, (WorldDifficulty)Terraria.Main.GameMode);
        }

        [Fact]
        public void WorldSave_EventTriggered()
        {
            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.Is<WorldSaveEvent>(evt => evt.World == world), log));

            Terraria.IO.WorldFile.SaveWorld(false, true);

            Assert.Equal(13500.0, Terraria.IO.WorldFile._tempTime);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void WorldSave_EventCanceled()
        {
            // Clear the time so we know it's 0.
            Terraria.IO.WorldFile._tempTime = 0.0;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            using var world = new OrionWorld(events, log);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WorldSaveEvent>(), log))
                .Callback<WorldSaveEvent, ILogger>((evt, log) => evt.Cancel());

            Terraria.IO.WorldFile.SaveWorld(false, true);

            Assert.Equal(0.0, Terraria.IO.WorldFile._tempTime);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlock_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify { X = 100, Y = 256, Modification = TileModify.TileModification.BreakBlock };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<BlockBreakEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && !evt.IsItemless),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlock_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify { X = 100, Y = 256, Modification = TileModify.TileModification.BreakBlock };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<BlockBreakEvent>(), log))
                .Callback<BlockBreakEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlockFailure_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.BreakBlock,
                IsFailure = true
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<BlockBreakEvent>(), log), Times.Never);
        }

        [Fact]
        public void PacketReceive_TileModify_PlaceBlock_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.PlaceBlock,
                BlockId = BlockId.Torches,
                BlockStyle = 1
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<BlockPlaceEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == BlockId.Torches && evt.Style == 1 && !evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_PlaceBlock_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.PlaceBlock,
                BlockId = BlockId.Torches,
                BlockStyle = 1
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<BlockPlaceEvent>(), log))
                .Callback<BlockPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_BreakWall_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify { X = 100, Y = 256, Modification = TileModify.TileModification.BreakWall };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<WallBreakEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 && evt.Y == 256),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_BreakWall_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify { X = 100, Y = 256, Modification = TileModify.TileModification.BreakWall };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WallBreakEvent>(), log))
                .Callback<WallBreakEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_BreakWallFailure_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.BreakWall,
                IsFailure = true
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<WallBreakEvent>(), log), Times.Never);
        }

        [Fact]
        public void PacketReceive_TileModify_PlaceWall_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.PlaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<WallPlaceEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == WallId.Stone && !evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_PlaceWall_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.PlaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WallPlaceEvent>(), log))
                .Callback<WallPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlockItemless_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.BreakBlockItemless
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<BlockBreakEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.IsItemless),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlockItemless_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.BreakBlockItemless
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<BlockBreakEvent>(), log))
                .Callback<BlockBreakEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_BreakBlockItemlessFailure_EventNotTriggered()
        {
            for (var i = 0; i < Terraria.Sign.maxSigns; ++i)
            {
                Terraria.Main.sign[i] = new Terraria.Sign();
            }

            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.BreakBlockItemless,
                IsFailure = true
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events)
                .Verify(em => em.Raise(It.IsAny<BlockBreakEvent>(), log), Times.Never);
        }

        [Fact]
        public void PacketReceive_TileModify_ReplaceBlock_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.ReplaceBlock,
                BlockId = BlockId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<BlockPlaceEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == BlockId.Stone && evt.Style == 0 && evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_ReplaceBlock_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.ReplaceBlock,
                BlockId = BlockId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<BlockPlaceEvent>(), log))
                .Callback<BlockPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_ReplaceWall_EventTriggered()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.ReplaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<WallPlaceEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Id == WallId.Stone && evt.IsReplacement),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_ReplaceWall_EventCanceled()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = TileModify.TileModification.ReplaceWall,
                WallId = WallId.Stone
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WallPlaceEvent>(), log))
                .Callback<WallPlaceEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileModify_InvalidModification()
        {
            Action<PacketReceiveEvent<TileModify>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileModify>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileModify>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileModify
            {
                X = 100,
                Y = 256,
                Modification = (TileModify.TileModification)255
            };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileModify>(packet, sender);

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);
        }

        [Fact]
        public void PacketReceive_TileSquarePacket_EventTriggered()
        {
            Action<PacketReceiveEvent<TileSquare>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileSquare>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileSquare>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileSquare { X = 100, Y = 256, Tiles = new NetworkTileSlice(3, 3) };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileSquare>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<TileSquareEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Tiles.Width == 3 && evt.Tiles.Height == 3),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileSquarePacket_EventCanceled()
        {
            Action<PacketReceiveEvent<TileSquare>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileSquare>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileSquare>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileSquare { X = 100, Y = 256, Tiles = new NetworkTileSlice(3, 3) };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileSquare>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<TileSquareEvent>(), log))
                .Callback<TileSquareEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileLiquidPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<TileLiquid>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileLiquid>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileLiquid>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileLiquid { X = 100, Y = 256, LiquidAmount = 255, LiquidType = LiquidType.Honey };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileLiquid>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<TileLiquidEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Liquid == new Liquid(LiquidType.Honey, 255)),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_TileLiquidPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<TileLiquid>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<TileLiquid>>>(), log))
                .Callback<Action<PacketReceiveEvent<TileLiquid>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new TileLiquid { X = 100, Y = 256, LiquidAmount = 255, LiquidType = LiquidType.Honey };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<TileLiquid>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<TileLiquidEvent>(), log))
                .Callback<TileLiquidEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WireActivatePacket_EventTriggered()
        {
            Action<PacketReceiveEvent<WireActivate>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WireActivate>>>(), log))
                .Callback<Action<PacketReceiveEvent<WireActivate>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new WireActivate { X = 100, Y = 256 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WireActivate>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<WiringActivateEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 && evt.Y == 256),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WireActivatePacket_EventCanceled()
        {
            Action<PacketReceiveEvent<WireActivate>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WireActivate>>>(), log))
                .Callback<Action<PacketReceiveEvent<WireActivate>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new WireActivate { X = 100, Y = 256 };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WireActivate>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WiringActivateEvent>(), log))
                .Callback<WiringActivateEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_BlockPaintPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<BlockPaint>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<BlockPaint>>>(), log))
                .Callback<Action<PacketReceiveEvent<BlockPaint>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new BlockPaint { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<BlockPaint>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<BlockPaintEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Color == PaintColor.Red),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_BlockPaintPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<BlockPaint>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<BlockPaint>>>(), log))
                .Callback<Action<PacketReceiveEvent<BlockPaint>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new BlockPaint { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<BlockPaint>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<BlockPaintEvent>(), log))
                .Callback<BlockPaintEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WallPaintPacket_EventTriggered()
        {
            Action<PacketReceiveEvent<WallPaint>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WallPaint>>>(), log))
                .Callback<Action<PacketReceiveEvent<WallPaint>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new WallPaint { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WallPaint>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(
                    It.Is<WallPaintEvent>(
                        evt => evt.World == world && evt.Player == sender && evt.X == 100 &&
                            evt.Y == 256 && evt.Color == PaintColor.Red),
                    log));

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Mock.Get(events).VerifyAll();
        }

        [Fact]
        public void PacketReceive_WallPaintPacket_EventCanceled()
        {
            Action<PacketReceiveEvent<WallPaint>>? registeredHandler = null;

            var events = Mock.Of<IEventManager>();
            var log = Mock.Of<ILogger>();
            Mock.Get(events)
                .Setup(em => em.RegisterHandler(It.IsAny<Action<PacketReceiveEvent<WallPaint>>>(), log))
                .Callback<Action<PacketReceiveEvent<WallPaint>>, ILogger>(
                    (handler, log) => registeredHandler = handler);

            using var world = new OrionWorld(events, log);

            var packet = new WallPaint { X = 100, Y = 256, Color = PaintColor.Red };
            var sender = Mock.Of<IPlayer>();
            var evt = new PacketReceiveEvent<WallPaint>(packet, sender);

            Mock.Get(events)
                .Setup(em => em.Raise(It.IsAny<WallPaintEvent>(), log))
                .Callback<WallPaintEvent, ILogger>((evt, log) => evt.Cancel());

            Assert.NotNull(registeredHandler);
            registeredHandler!(evt);

            Assert.True(evt.IsCanceled);

            Mock.Get(events).VerifyAll();
        }
    }
}
