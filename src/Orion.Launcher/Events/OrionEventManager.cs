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
using System.Collections.Generic;
using System.Threading.Tasks;
using Orion.Core.Events;
using Serilog;

namespace Orion.Launcher.Events
{
    internal sealed class OrionEventManager : IEventManager
    {
        private readonly IDictionary<Type, object> _eventHandlerCollections = new Dictionary<Type, object>();

        public void RegisterHandler<TEvent>(Action<TEvent> handler, ILogger log) where TEvent : Event
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var collection = GetEventHandlerCollection<TEvent>();
            collection.RegisterHandler(handler, log);
        }

        public void RegisterAsyncHandler<TEvent>(Func<TEvent, Task> handler, ILogger log) where TEvent : Event
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var collection = GetEventHandlerCollection<TEvent>();
            collection.RegisterAsyncHandler(handler, log);
        }

        public void DeregisterHandler<TEvent>(Action<TEvent> handler, ILogger log) where TEvent : Event
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var collection = GetEventHandlerCollection<TEvent>();
            collection.DeregisterHandler(handler, log);
        }

        public void DeregisterAsyncHandler<TEvent>(Func<TEvent, Task> handler, ILogger log) where TEvent : Event
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var collection = GetEventHandlerCollection<TEvent>();
            collection.DeregisterAsyncHandler(handler, log);
        }

        public void Raise<TEvent>(TEvent evt, ILogger log) where TEvent : Event
        {
            if (evt is null)
            {
                throw new ArgumentNullException(nameof(evt));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            var collection = GetEventHandlerCollection<TEvent>();
            collection.Raise(evt, log);
        }

        private EventHandlerCollection<TEvent> GetEventHandlerCollection<TEvent>() where TEvent : Event
        {
            var type = typeof(TEvent);
            if (!_eventHandlerCollections.TryGetValue(type, out var collection))
            {
                collection = new EventHandlerCollection<TEvent>();
                _eventHandlerCollections[type] = collection;
            }

            return (EventHandlerCollection<TEvent>)collection;
        }
    }
}
