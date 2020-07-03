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
using Moq;
using Orion.Core.Events;
using Serilog;
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
            var log = Mock.Of<ILogger>();

            Assert.Throws<ArgumentNullException>(() => manager.RegisterHandler<TestEvent>(null!, log));
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
            var log = Mock.Of<ILogger>();

            Assert.Throws<ArgumentNullException>(() => manager.RegisterAsyncHandler<TestEvent>(null!, log));
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
            var log = Mock.Of<ILogger>();

            Assert.Throws<ArgumentNullException>(() => manager.DeregisterHandler<TestEvent>(null!, log));
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
            var log = Mock.Of<ILogger>();

            Assert.Throws<ArgumentNullException>(() => manager.DeregisterAsyncHandler<TestEvent>(null!, log));
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
            var log = Mock.Of<ILogger>();

            Assert.Throws<ArgumentNullException>(() => manager.Raise<TestEvent>(null!, log));
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
            var log = Mock.Of<ILogger>();
            manager.RegisterHandler<TestEvent>(evt => evt.Value = 100, log);
            var evt = new TestEvent();

            manager.Raise(evt, Logger.None);

            Assert.Equal(100, evt.Value);
        }

        [Fact]
        public void RegisterHandler_Raise_EventCanceled()
        {
            var manager = new OrionEventManager();
            var log = Mock.Of<ILogger>();
            manager.RegisterHandler<TestEvent2>(TestEvent2Handler, log);
            manager.RegisterHandler<TestEvent2>(TestEvent2Handler2, log);
            var evt = new TestEvent2();

            manager.Raise(evt, Logger.None);

            Assert.Equal(0, evt.Value);
        }

        [Fact]
        public void RegisterHandler_Raise_ThrowsException()
        {
            var manager = new OrionEventManager();
            var log = Mock.Of<ILogger>();
            manager.RegisterHandler<TestEvent>(evt => throw new InvalidOperationException(), log);
            var evt = new TestEvent();

            manager.Raise(evt, Logger.None);
        }

        [Fact]
        public void RegisterAsyncHandler_Raise()
        {
            var manager = new OrionEventManager();
            var log = Mock.Of<ILogger>();
            manager.RegisterAsyncHandler<TestEvent>(async evt =>
            {
                await Task.Delay(100);

                evt.Value = 100;
            }, log);
            var evt = new TestEvent();

            manager.Raise(evt, Logger.None);

            // HACK: sleep here to ensure that the handler is finished.
            Thread.Sleep(1000);

            Assert.Equal(100, evt.Value);
        }

        [Fact]
        public void RegisterAsyncHandler_Raise_EventCanceled()
        {
            var manager = new OrionEventManager();
            var log = Mock.Of<ILogger>();
            manager.RegisterHandler<TestEvent2>(TestEvent2Handler, log);
            manager.RegisterAsyncHandler<TestEvent2>(TestEvent2Handler2Async, log);
            var evt = new TestEvent2();

            manager.Raise(evt, Logger.None);

            // HACK: sleep here to ensure that the handler is finished.
            Thread.Sleep(1000);

            Assert.Equal(0, evt.Value);
        }

        [Fact]
        public void RegisterAsyncHandler_Raise_ThrowsException()
        {
            var manager = new OrionEventManager();
            var log = Mock.Of<ILogger>();
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            manager.RegisterAsyncHandler<TestEvent>(async evt => throw new InvalidOperationException(), log);
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            var evt = new TestEvent();

            manager.Raise(evt, log);
        }

        [Fact]
        public void RegisterAsyncHandler_Raise_ThrowsExceptionAsynchronously()
        {
            var manager = new OrionEventManager();
            var log = Mock.Of<ILogger>();
            manager.RegisterAsyncHandler<TestEvent>(async evt =>
            {
                await Task.Delay(100);

                throw new InvalidOperationException();
            }, log);
            var evt = new TestEvent();

            manager.Raise(evt, log);

            // HACK: sleep here to ensure that the handler is finished.
            Thread.Sleep(1000);
        }

        [Fact]
        public void DeregisterHandler()
        {
            static void Handler(TestEvent evt) => evt.Value = 100;

            var manager = new OrionEventManager();
            var log = Mock.Of<ILogger>();
            manager.RegisterHandler<TestEvent>(Handler, log);

            manager.DeregisterHandler<TestEvent>(Handler, log);

            var evt = new TestEvent();
            manager.Raise(evt, Logger.None);

            Assert.NotEqual(100, evt.Value);
        }

        [Fact]
        public void DeregisterHandler_NotRegistered()
        {
            var manager = new OrionEventManager();
            var log = Mock.Of<ILogger>();

            manager.DeregisterHandler<TestEvent>(evt => { }, log);
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
            var log = Mock.Of<ILogger>();
            manager.RegisterAsyncHandler<TestEvent>(Handler, log);

            manager.DeregisterAsyncHandler<TestEvent>(Handler, log);

            var evt = new TestEvent();
            manager.Raise(evt, Logger.None);

            Assert.NotEqual(100, evt.Value);
        }

        [Fact]
        public void DeregisterAsyncHandler_NotRegistered()
        {
            var manager = new OrionEventManager();
            var log = Mock.Of<ILogger>();

            manager.DeregisterAsyncHandler<TestEvent>(async evt => await Task.Delay(100), log);
        }

        [EventHandler("test", Priority = EventPriority.Highest)]
        private void TestEvent2Handler(TestEvent2 evt)
        {
            evt.Cancel();
        }

        [EventHandler("test-2", Priority = EventPriority.Normal)]
        private void TestEvent2Handler2(TestEvent2 evt)
        {
            evt.Value = 100;
        }

        [EventHandler("test-2")]
        private async Task TestEvent2Handler2Async(TestEvent2 evt)
        {
            await Task.Delay(100);

            evt.Value = 100;
        }

        [Event("test")]
        private sealed class TestEvent : Event
        {
            public int Value { get; set; }
        }

        private sealed class TestEvent2 : Event
        {
            public int Value { get; set; }
        }
    }
}
