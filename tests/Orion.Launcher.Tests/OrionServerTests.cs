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

using System.Reflection;
using Orion.Core;
using Serilog;
using Serilog.Core;
using Xunit;

namespace Orion.Launcher
{
    public class OrionServerTests
    {
        [Fact]
        public void Events_Get()
        {
            using var server = new OrionServer(Logger.None);
            server.Initialize();

            Assert.NotNull(server.Events);
        }

        [Fact]
        public void Items_Get()
        {
            using var server = new OrionServer(Logger.None);
            server.Initialize();

            Assert.NotNull(server.Items);
        }

        [Fact]
        public void Npcs_Get()
        {
            using var server = new OrionServer(Logger.None);
            server.Initialize();

            Assert.NotNull(server.Npcs);
        }

        [Fact]
        public void Players_Get()
        {
            using var server = new OrionServer(Logger.None);
            server.Initialize();

            Assert.NotNull(server.Players);
        }

        [Fact]
        public void Projectiles_Get()
        {
            using var server = new OrionServer(Logger.None);
            server.Initialize();

            Assert.NotNull(server.Projectiles);
        }

        [Fact]
        public void Chests_Get()
        {
            using var server = new OrionServer(Logger.None);
            server.Initialize();

            Assert.NotNull(server.Chests);
        }

        [Fact]
        public void Signs_Get()
        {
            using var server = new OrionServer(Logger.None);
            server.Initialize();

            Assert.NotNull(server.Signs);
        }

        [Fact]
        public void World_Get()
        {
            using var server = new OrionServer(Logger.None);
            server.Initialize();

            Assert.NotNull(server.World);
        }

        [Fact]
        public void Load_Initialize()
        {
            using var server = new OrionServer(Logger.None);

            server.Load(Assembly.GetExecutingAssembly());

            server.Initialize();

            Assert.IsType<TestService>(TestOrionPlugin.SingletonService);
            Assert.IsType<TestService2>(TestOrionPlugin.TransientService);

            Assert.Equal(100, TestOrionPlugin.Value);
        }

        [Service(ServiceScope.Singleton)]
        public interface ITestSingletonService { }

        [Service(ServiceScope.Transient)]
        public interface ITestTransientService { }

        [Binding("test-service")]
        internal class TestService : ITestSingletonService, ITestTransientService { }

        [Binding("test-service-2", Priority = BindingPriority.Highest)]
        internal class TestService2 : ITestTransientService { }

        [Plugin("test-plugin")]
        public class TestOrionPlugin : OrionPlugin
        {
            public TestOrionPlugin(
                IServer server, ILogger log,
                ITestSingletonService singletonService,
                ITestTransientService transientService) : base(server, log)
            {
                SingletonService = singletonService;
                TransientService = transientService;
                Value = 100;
            }

            public static ITestSingletonService SingletonService { get; private set; } = null!;
            public static ITestTransientService TransientService { get; private set; } = null!;
            public static int Value { get; set; }

            public override void Dispose() => Value = -100;
        }
    }
}
