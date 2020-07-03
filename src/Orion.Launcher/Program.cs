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
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Destructurama;
using Orion.Core.Events.Server;
using Orion.Launcher.Properties;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Orion.Launcher
{
    internal static class Program
    {
#if DEBUG
        private const LogEventLevel MinimumLogLevel = LogEventLevel.Debug;
#else
        private const LogEventLevel MinimumLogLevel = LogEventLevel.Information;
#endif

        internal static void Main(string[] args)
        {
            SetUpTerrariaLanguage();

            var log = SetUpLog();
            using var server = SetUpServer(log);
            using var context = SetUpSynchronizationContext();

            server.Events.Raise(new ServerArgsEvent(args), log);

            using var game = new Terraria.Main();
            game.DedServ();

            // Sets up the Terraria language.
            static void SetUpTerrariaLanguage()
            {
                var previousCulture = Thread.CurrentThread.CurrentCulture;
                var previousUICulture = Thread.CurrentThread.CurrentUICulture;
                Terraria.Localization.LanguageManager.Instance.SetLanguage(previousUICulture.Name);
                Terraria.Lang.InitializeLegacyLocalization();
                Thread.CurrentThread.CurrentCulture = previousCulture;
                Thread.CurrentThread.CurrentUICulture = previousUICulture;
            }

            // Sets up a log which outputs to the console and to the logs/ directory.
            static ILogger SetUpLog()
            {
                Directory.CreateDirectory("logs");

                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;

                var log = new LoggerConfiguration()
                    .Destructure.UsingAttributes()
                    .MinimumLevel.Is(MinimumLogLevel)
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Name}: {Message:l}{NewLine}{Exception}",
                        theme: AnsiConsoleTheme.Code)
                    .WriteTo.File(Path.Combine("logs", "log-.txt"),
                        outputTemplate:
                            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Name}: {Message:l}{NewLine}{Exception}",
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        fileSizeLimitBytes: 2 << 20)
                    .CreateLogger()
                    .ForContext("Name", "orion-launcher");

                AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                {
                    log.Fatal(eventArgs.ExceptionObject as Exception, Resources.UnhandledExceptionMessage);
                };

                return log;
            }

            // Sets up a server which loads plugins from the plugins/ directory.
            static OrionServer SetUpServer(ILogger log)
            {
                Directory.CreateDirectory("plugins");

                var server = new OrionServer(log);

                foreach (var path in Directory.EnumerateFiles("plugins", "*.dll"))
                {
                    try
                    {
                        var assembly = Assembly.LoadFile(path);
                        server.Load(assembly);
                    }
                    catch (BadImageFormatException)
                    {
                    }
                }

                server.Initialize();

                return server;
            }

            // Sets up a synchronization context which ensures that continuations run on the main Terraria thread.
            static TerrariaSynchronizationContext SetUpSynchronizationContext()
            {
                var context = new TerrariaSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(context);

                // We use `Terraria.Main.OnTickForThirdPartySoftwareOnly` to try executing continuations each game tick.
                // We could instead register a `ServerTickEvent` handler, but that would mean that continuations can
                // only run if there are players present on the server.
                Terraria.Main.OnTickForThirdPartySoftwareOnly += () => context.TryExecute();

                return context;
            }
        }
    }
}
