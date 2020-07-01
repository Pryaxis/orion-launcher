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
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Destructurama.Attributed;
using Orion.Core.Events;
using Serilog;
using Serilog.Events;

namespace Orion.Launcher.Events
{
    internal sealed partial class OrionEventManager
    {
        public sealed class Collection<TEvent> where TEvent : Event
        {
            private readonly string _eventName;
            private readonly LogEventLevel _eventLoggingLevel;

            private readonly ISet<Registration> _registrations =
                new SortedSet<Registration>(
                    Comparer<Registration>.Create((r1, r2) => r1.Priority.CompareTo(r2.Priority)));
            private readonly ISet<AsyncRegistration> _asyncRegistrations = new HashSet<AsyncRegistration>();
            private readonly IDictionary<Action<TEvent>, Registration> _handlerToRegistration =
                new Dictionary<Action<TEvent>, Registration>();
            private readonly IDictionary<Func<TEvent, Task>, AsyncRegistration> _handlerToAsyncRegistration =
                new Dictionary<Func<TEvent, Task>, AsyncRegistration>();

            public Collection()
            {
                var attribute = typeof(TEvent).GetCustomAttribute<EventAttribute>();
                _eventName = attribute?.Name ?? typeof(TEvent).Name;
                _eventLoggingLevel = attribute?.LoggingLevel ?? LogEventLevel.Debug;
            }

            public void RegisterHandler(Action<TEvent> handler, ILogger log)
            {
                Debug.Assert(handler != null);
                Debug.Assert(log != null);

                var registration = new Registration(handler);
                _registrations.Add(registration);
                _handlerToRegistration[handler] = registration;

                log.Debug("Registering {@Registration} to {EventName}", _eventName, registration);
            }

            public void RegisterAsyncHandler(Func<TEvent, Task> handler, ILogger log)
            {
                Debug.Assert(handler != null);
                Debug.Assert(log != null);

                var registration = new AsyncRegistration(handler);
                _asyncRegistrations.Add(registration);
                _handlerToAsyncRegistration[handler] = registration;

                log.Debug("Registering {@AsyncRegistration} to {EventName}", _eventName, registration);
            }

            public void DeregisterHandler(Action<TEvent> handler, ILogger log)
            {
                Debug.Assert(handler != null);
                Debug.Assert(log != null);

                if (_handlerToRegistration.TryGetValue(handler, out var registration))
                {
                    _registrations.Remove(registration);
                    _handlerToRegistration.Remove(handler);

                    log.Debug("Deregistering {@Registration} from {EventName}", _eventName, registration);
                }
            }

            public void DeregisterAsyncHandler(Func<TEvent, Task> handler, ILogger log)
            {
                Debug.Assert(handler != null);
                Debug.Assert(log != null);

                if (_handlerToAsyncRegistration.TryGetValue(handler, out var registration))
                {
                    _asyncRegistrations.Remove(registration);
                    _handlerToAsyncRegistration.Remove(handler);

                    log.Debug("Deregistering {@AsyncRegistration} from {EventName}", _eventName, registration);
                }
            }

            public void Raise(TEvent evt, ILogger log)
            {
                Debug.Assert(evt != null);
                Debug.Assert(log != null);

                log.Write(_eventLoggingLevel, "Raising {@Event} on {EventName}", _eventName, evt);

                RaiseHandlers();
                RaiseAsyncHandlers();

                if (evt.IsCanceled)
                {
                    log.Write(
                        _eventLoggingLevel, "Canceled {@Event} for {CancellationReason}", _eventName,
                        evt.CancellationReason);
                }

                void RaiseHandlers()
                {
                    foreach (var registration in _registrations)
                    {
                        if (evt.IsCanceled && registration.IgnoreCanceled)
                        {
                            continue;
                        }

                        try
                        {
                            registration.Handler(evt);
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Unhandled exception in {@Registration}", _eventName, registration);
                        }
                    }
                }

                void RaiseAsyncHandlers()
                {
                    foreach (var registration in _asyncRegistrations)
                    {
                        if (evt.IsCanceled && registration.IgnoreCanceled)
                        {
                            continue;
                        }

                        // Launch the task, but don't bother tracking it.
                        var task = registration.Handler(evt).ContinueWith(t =>
                        {
                            var ex = t.Exception;
                            if (ex != null)
                            {
                                log.Error(ex, "Unhandled exception in {@AsyncRegistration}", _eventName, registration);
                            }
                        });
                    }
                }
            }

            private sealed class Registration
            {
                [NotLogged] public Action<TEvent> Handler { get; }
                public string Name { get; }
                public EventPriority Priority { get; }
                public bool IgnoreCanceled { get; }

                public Registration(Action<TEvent> handler)
                {
                    Handler = handler;

                    var attribute = handler.Method.GetCustomAttribute<EventHandlerAttribute>();
                    Name = attribute?.Name ?? handler.Method.Name;
                    Priority = attribute?.Priority ?? EventPriority.Normal;
                    IgnoreCanceled = attribute?.IgnoreCanceled ?? true;
                }
            }

            private sealed class AsyncRegistration
            {
                [NotLogged] public Func<TEvent, Task> Handler { get; }
                public string Name { get; }
                public bool IgnoreCanceled { get; }

                public AsyncRegistration(Func<TEvent, Task> handler)
                {
                    Handler = handler;

                    var attribute = handler.Method.GetCustomAttribute<EventHandlerAttribute>();
                    Name = attribute?.Name ?? handler.Method.Name;
                    IgnoreCanceled = attribute?.IgnoreCanceled ?? true;
                }
            }
        }
    }
}
