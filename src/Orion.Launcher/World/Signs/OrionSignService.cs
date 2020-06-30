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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Orion.Core;
using Orion.Core.Events;
using Orion.Core.Events.Packets;
using Orion.Core.Events.World.Signs;
using Orion.Core.Packets.World.Signs;
using Orion.Core.World.Signs;
using Orion.Launcher.Utils;
using Serilog;

namespace Orion.Launcher.World.Signs
{
    [Binding("orion-signs", Author = "Pryaxis", Priority = BindingPriority.Lowest)]
    internal sealed class OrionSignService : ISignService, IDisposable
    {
        private readonly IEventManager _events;
        private readonly ILogger _log;
        private readonly IReadOnlyList<ISign> _signs;

        public OrionSignService(IEventManager events, ILogger log)
        {
            Debug.Assert(events != null);
            Debug.Assert(log != null);

            _events = events;
            _log = log;

            _signs = new WrappedReadOnlyList<OrionSign, Terraria.Sign?>(
                Terraria.Main.sign, (signIndex, terrariaSign) => new OrionSign(signIndex, terrariaSign));

            _events.RegisterHandlers(this, _log);
        }

        public ISign this[int index] => _signs[index];

        public int Count => _signs.Count;

        public IEnumerator<ISign> GetEnumerator() => _signs.GetEnumerator();

        public void Dispose()
        {
            _events.DeregisterHandlers(this, _log);
        }

        private ISign? FindSign(int x, int y) => this.FirstOrDefault(s => s.IsActive && s.X == x && s.Y == y);

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // =============================================================================================================
        // Sign event publishers
        //

        [EventHandler("orion-signs", Priority = EventPriority.Lowest)]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Implicitly used")]
        private void OnSignRead(PacketReceiveEvent<SignRead> evt)
        {
            var packet = evt.Packet;
            var sign = FindSign(packet.X, packet.Y);
            if (sign is null)
            {
                return;
            }

            _events.Forward(evt, new SignReadEvent(sign, evt.Sender), _log);
        }
    }
}
