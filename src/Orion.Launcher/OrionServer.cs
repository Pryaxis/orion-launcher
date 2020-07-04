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
using System.Linq;
using System.Reflection;
using Ninject;
using Orion.Core;
using Orion.Core.Events;
using Orion.Core.Events.Server;
using Orion.Core.Items;
using Orion.Core.Npcs;
using Orion.Core.Players;
using Orion.Core.Projectiles;
using Orion.Core.World;
using Orion.Core.World.Chests;
using Orion.Core.World.Signs;
using Orion.Launcher.Properties;
using Serilog;

namespace Orion.Launcher
{
    internal sealed class OrionServer : IServer, IDisposable
    {
        private readonly ILogger _log;

        private readonly Lazy<IEventManager> _events;
        private readonly Lazy<IItemService> _items;
        private readonly Lazy<INpcService> _npcs;
        private readonly Lazy<IPlayerService> _players;
        private readonly Lazy<IProjectileService> _projectiles;
        private readonly Lazy<IChestService> _chests;
        private readonly Lazy<ISignService> _signs;
        private readonly Lazy<IWorld> _world;

        private readonly StandardKernel _kernel = new StandardKernel();
        private readonly HashSet<Type> _serviceInterfaceTypes = new HashSet<Type>();
        private readonly Dictionary<Type, HashSet<Type>> _serviceBindingTypes = new Dictionary<Type, HashSet<Type>>();
        private readonly HashSet<Type> _pluginTypes = new HashSet<Type>();

        private readonly Dictionary<string, OrionPlugin> _plugins = new Dictionary<string, OrionPlugin>();

        public OrionServer(ILogger log)
        {
            Debug.Assert(log != null);

            _log = log.ForContext("Name", "orion-server");

            _kernel.Bind<IServer>().ToConstant(this).InTransientScope();

            // Bind `ILogger` to allow plugin/binding-specific logs.
            _kernel
                .Bind<ILogger>()
                .ToMethod(ctx =>
                {
                    var type = ctx.Request.Target.Member.ReflectedType;
                    Debug.Assert(type != null);

                    var name =
                        type.GetCustomAttribute<BindingAttribute>()?.Name ??
                        type.GetCustomAttribute<PluginAttribute>()!.Name;
                    return log.ForContext("Name", name);
                })
                .InTransientScope();

            // Load the Orion.Core and Orion.Launcher assemblies so that the Orion interfaces and bindings are
            // loaded.
            Load(typeof(IServer).Assembly);
            Load(typeof(OrionServer).Assembly);

            _events = new Lazy<IEventManager>(() => _kernel.Get<IEventManager>());
            _items = new Lazy<IItemService>(() => _kernel.Get<IItemService>());
            _npcs = new Lazy<INpcService>(() => _kernel.Get<INpcService>());
            _players = new Lazy<IPlayerService>(() => _kernel.Get<IPlayerService>());
            _projectiles = new Lazy<IProjectileService>(() => _kernel.Get<IProjectileService>());
            _chests = new Lazy<IChestService>(() => _kernel.Get<IChestService>());
            _signs = new Lazy<ISignService>(() => _kernel.Get<ISignService>());
            _world = new Lazy<IWorld>(() => _kernel.Get<IWorld>());

            OTAPI.Hooks.Game.PreInitialize = PreInitializeHandler;
            OTAPI.Hooks.Game.Started = StartedHandler;
            OTAPI.Hooks.Game.PreUpdate = PreUpdateHandler;
            OTAPI.Hooks.Command.Process = ProcessHandler;
        }

        public IEventManager Events => _events.Value;

        public IItemService Items => _items.Value;

        public INpcService Npcs => _npcs.Value;

        public IPlayerService Players => _players.Value;

        public IProjectileService Projectiles => _projectiles.Value;

        public IChestService Chests => _chests.Value;

        public ISignService Signs => _signs.Value;

        public IWorld World => _world.Value;

        public void Dispose()
        {
            _kernel.Dispose();

            OTAPI.Hooks.Game.PreInitialize = null;
            OTAPI.Hooks.Game.Started = null;
            OTAPI.Hooks.Game.PreUpdate = null;
            OTAPI.Hooks.Command.Process = null;
        }

        // =============================================================================================================
        // Framework support
        //

        public void Load(Assembly assembly)
        {
            Debug.Assert(assembly != null);

            LoadServiceInterfaceTypes();
            LoadServiceBindingTypes();
            LoadPluginTypes();

            void LoadServiceInterfaceTypes()
            {
                _serviceInterfaceTypes.UnionWith(
                    assembly.ExportedTypes
                        .Where(t => t.IsInterface)
                        .Where(t => t.GetCustomAttribute<ServiceAttribute>() != null));
            }

            void LoadServiceBindingTypes()
            {
                foreach (var bindingType in assembly.DefinedTypes
                    .Where(t => !t.IsAbstract)
                    .Where(t => t.GetCustomAttribute<BindingAttribute>() != null))
                {
                    foreach (var interfaceType in bindingType
                        .GetInterfaces()
                        .Where(_serviceInterfaceTypes.Contains))
                    {
                        if (!_serviceBindingTypes.TryGetValue(interfaceType, out var types))
                        {
                            types = new HashSet<Type>();
                            _serviceBindingTypes[interfaceType] = types;
                        }

                        types.Add(bindingType);
                    }
                }
            }

            void LoadPluginTypes()
            {
                foreach (var pluginType in assembly.ExportedTypes
                    .Where(t => !t.IsAbstract)
                    .Where(t => t.GetCustomAttribute<PluginAttribute>() != null))
                {
                    _pluginTypes.Add(pluginType);

                    var pluginName = pluginType.GetCustomAttribute<PluginAttribute>()!.Name;
                    _log.Information(Resources.LoadedPlugin, pluginName);
                }
            }
        }

        public void Initialize()
        {
            InitializeServiceBindings();
            InitializePlugins();

            void InitializeServiceBindings()
            {
                // Initialize the service bindings.
                foreach (var (interfaceType, bindingTypes) in _serviceBindingTypes)
                {
                    var bindingType = bindingTypes
                        .OrderByDescending(t => t.GetCustomAttribute<BindingAttribute>()!.Priority)
                        .FirstOrDefault();
                    if (bindingType is null)
                    {
                        continue;
                    }

                    var binding = _kernel.Bind(interfaceType).To(bindingType);
                    _ = interfaceType.GetCustomAttribute<ServiceAttribute>()!.Scope switch
                    {
                        ServiceScope.Singleton => binding.InSingletonScope(),
                        ServiceScope.Transient => binding.InTransientScope(),

                        _ => throw new InvalidOperationException("Invalid service scope")
                    };
                }

                // Initialize the singleton services so that an instance always exists. This must be done in a separate
                // stage since not all of the bindings are available.
                foreach (var interfaceType in _serviceInterfaceTypes)
                {
                    if (interfaceType.GetCustomAttribute<ServiceAttribute>()!.Scope == ServiceScope.Singleton)
                    {
                        _ = _kernel.Get(interfaceType);
                    }
                }
            }

            void InitializePlugins()
            {
                // Initialize the plugin bindings to allow plugin dependencies.
                foreach (var pluginType in _pluginTypes)
                {
                    _kernel.Bind(pluginType).ToSelf().InSingletonScope();
                }

                // Initialize the plugins.
                foreach (var pluginType in _pluginTypes)
                {
                    var attribute = pluginType.GetCustomAttribute<PluginAttribute>()!;
                    var pluginName = attribute.Name;
                    var pluginVersion = pluginType.Assembly.GetName().Version;
                    var pluginAuthor = attribute.Author;
                    _log.Information(Resources.InitializedPlugin, pluginName, pluginVersion, pluginAuthor);

                    var plugin = (OrionPlugin)_kernel.Get(pluginType);
                    _plugins[pluginName] = plugin;
                }
            }
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
            var evt = ServerTickEvent.Instance;
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
