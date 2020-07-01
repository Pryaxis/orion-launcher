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
using System.Threading;
using System.Threading.Tasks;
using Orion.Core.Events;
using Serilog.Core;
using Xunit;

namespace Orion.Launcher.Events
{
    public class OrionEventManagerTests
    {
        [Fact]
        public void RegisterHandler_NullHandler_ThrowsArgumentNullException()
        {
            var manager = new OrionEventManager();

            Assert.Throws<ArgumentNullException>(() => manager.RegisterHandler<TestEvent>(null!, Logger.None));
        }

        [Fact]
        public void RegisterHandler_NullLog_ThrowsArgumentNullException()
        {
            var manager = new OrionEventManager();

            Assert.Throws<ArgumentNullException>(() => manager.RegisterHandler<TestEvent>(evt => { }, null!));
        }

        [Fact]
        public void RegisterAsyncHandler_NullHandler_ThrowsArgumentNullException()
        {
            var manager = new OrionEventManager();

            Assert.Throws<ArgumentNullException>(() => manager.RegisterAsyncHandler<TestEvent>(null!, Logger.None));
        }

        [Fact]
        public void RegisterAsyncHandler_NullLog_ThrowsArgumentNullException()
        {
            var manager = new OrionEventManager();

            Assert.Throws<ArgumentNullException>(
                () => manager.RegisterAsyncHandler<TestEvent>(async evt => await Task.Delay(100), null!));
        }

        [Fact]
        public void DeregisterHandler_NullHandler_ThrowsArgumentNullException()
        {
            var manager = new OrionEventManager();

            Assert.Throws<ArgumentNullException>(() => manager.DeregisterHandler<TestEvent>(null!, Logger.None));
        }

        [Fact]
        public void DeregisterHandler_NullLog_ThrowsArgumentNullException()
        {
            var manager = new OrionEventManager();

            Assert.Throws<ArgumentNullException>(() => manager.DeregisterHandler<TestEvent>(evt => { }, null!));
        }

        [Fact]
        public void DeregisterAsyncHandler_NullHandler_ThrowsArgumentNullException()
        {
            var manager = new OrionEventManager();

            Assert.Throws<ArgumentNullException>(() => manager.DeregisterAsyncHandler<TestEvent>(null!, Logger.None));
        }

        [Fact]
        public void DeregisterAsyncHandler_NullLog_ThrowsArgumentNullException()
        {
            var manager = new OrionEventManager();

            Assert.Throws<ArgumentNullException>(
                () => manager.DeregisterAsyncHandler<TestEvent>(async evt => await Task.Delay(100), null!));
        }

        [Fact]
        public void Raise_NullEvt_ThrowsArgumentNullException()
        {
            var manager = new OrionEventManager();

            Assert.Throws<ArgumentNullException>(() => manager.Raise<TestEvent>(null!, Logger.None));
        }

        [Fact]
        public void Raise_NullLog_ThrowsArgumentNullException()
        {
            var manager = new OrionEventManager();

            Assert.Throws<ArgumentNullException>(() => manager.Raise(new TestEvent(), null!));
        }

        [Fact]
        public void RegisterHandler_Raise()
        {
            var manager = new OrionEventManager();
            manager.RegisterHandler<TestEvent>(evt => evt.Value = 100, Logger.None);
            var evt = new TestEvent();

            manager.Raise(evt, Logger.None);

            Assert.Equal(100, evt.Value);
        }

        [Fact]
        public void RegisterAsyncHandler_Raise()
        {
            var manager = new OrionEventManager();
            manager.RegisterAsyncHandler<TestEvent>(async evt =>
            {
                await Task.Delay(100);

                evt.Value = 100;
            }, Logger.None);
            var evt = new TestEvent();

            manager.Raise(evt, Logger.None);

            // HACK: sleep here to check that the value is updated.
            Thread.Sleep(1000);

            Assert.Equal(100, evt.Value);
        }

        [Fact]
        public void DeregisterHandler()
        {
            static void Handler(TestEvent evt) => evt.Value = 100;

            var manager = new OrionEventManager();
            manager.RegisterHandler<TestEvent>(Handler, Logger.None);

            manager.DeregisterHandler<TestEvent>(Handler, Logger.None);

            var evt = new TestEvent();
            manager.Raise(evt, Logger.None);

            Assert.NotEqual(100, evt.Value);
        }

        [Fact]
        public void DeregisterHandler_NotRegistered()
        {
            var manager = new OrionEventManager();

            manager.DeregisterHandler<TestEvent>(evt => { }, Logger.None);
        }

        [Fact]
        public void DeregisterAsyncHandler()
        {
            static async Task Handler(TestEvent evt)
            {
                await Task.Delay(100);

                evt.Value = 100;
            }

            var manager = new OrionEventManager();
            manager.RegisterAsyncHandler<TestEvent>(Handler, Logger.None);

            manager.DeregisterAsyncHandler<TestEvent>(Handler, Logger.None);

            var evt = new TestEvent();
            manager.Raise(evt, Logger.None);

            Assert.NotEqual(100, evt.Value);
        }

        [Fact]
        public void DeregisterAsyncHandler_NotRegistered()
        {
            var manager = new OrionEventManager();

            manager.DeregisterAsyncHandler<TestEvent>(async evt => await Task.Delay(100), Logger.None);
        }

        [Event("test")]
        private sealed class TestEvent : Event
        {
            public int Value { get; set; }
        }
    }
}
