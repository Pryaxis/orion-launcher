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
using System.Diagnostics;
using Orion.Core;
using Orion.Core.Events;
using Orion.Core.Events.Server;
using Orion.Core.Framework;
using Orion.Launcher.Impl.Events;
using Orion.Launcher.Impl.Extensions;
using Serilog;

namespace Orion.Launcher.Impl
{
    internal sealed class OrionServer : IServer, IDisposable
    {
        private readonly OrionExtensionManager _extensions;
        private readonly ILogger _log;

        public OrionServer(ILogger log)
        {
            Debug.Assert(log != null);

            _extensions = new OrionExtensionManager(this, log);
            _log = log.ForContext("Name", "orion-server");

            OTAPI.Hooks.Game.PreInitialize = PreInitializeHandler;
            OTAPI.Hooks.Game.Started = StartedHandler;
            OTAPI.Hooks.Game.PreUpdate = PreUpdateHandler;
            OTAPI.Hooks.Command.Process = ProcessHandler;
        }

        public IExtensionManager Extensions => _extensions;

        public IEventManager Events { get; } = new OrionEventManager();

        public void Dispose()
        {
            _extensions.Dispose();

            OTAPI.Hooks.Game.PreInitialize = null;
            OTAPI.Hooks.Game.Started = null;
            OTAPI.Hooks.Game.PreUpdate = null;
            OTAPI.Hooks.Command.Process = null;
        }

        // =============================================================================================================
        // OTAPI hooks
        //

        private void PreInitializeHandler()
        {
            var evt = new ServerInitializeEvent();
            Events.Raise(evt, _log);
        }

        private void StartedHandler()
        {
            var evt = new ServerStartEvent();
            Events.Raise(evt, _log);
        }

        private void PreUpdateHandler(ref Microsoft.Xna.Framework.GameTime gameTime)
        {
            var evt = new ServerTickEvent();
            Events.Raise(evt, _log);
        }

        private OTAPI.HookResult ProcessHandler(string lowered, string input)
        {
            var evt = new ServerCommandEvent(input);
            Events.Raise(evt, _log);
            return evt.IsCanceled ? OTAPI.HookResult.Cancel : OTAPI.HookResult.Continue;
        }
    }
}
