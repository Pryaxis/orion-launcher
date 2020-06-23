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
using System.Collections.Concurrent;
using System.Threading;

namespace Orion.Launcher
{
    internal sealed class TerrariaSynchronizationContext : SynchronizationContext, IDisposable
    {
        private readonly BlockingCollection<(SendOrPostCallback callback, object? state)> _queue =
            new BlockingCollection<(SendOrPostCallback, object?)>();

        public override void Post(SendOrPostCallback callback, object? state)
        {
            _queue.Add((callback, state));
        }

        public void Dispose()
        {
            _queue.Dispose();
        }

        public void TryExecute()
        {
            while (_queue.TryTake(out var tuple))
            {
                var (callback, state) = tuple;
                callback(state);
            }
        }
    }
}
