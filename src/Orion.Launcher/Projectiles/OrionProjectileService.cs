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
using Orion.Core.DataStructures;
using Orion.Core.Events;
using Orion.Core.Events.Projectiles;
using Orion.Core.Framework;
using Orion.Core.Projectiles;
using Orion.Launcher.Collections;
using Serilog;

namespace Orion.Launcher.Projectiles
{
    [Binding("orion-projs", Author = "Pryaxis", Priority = BindingPriority.Lowest)]
    internal sealed class OrionProjectileService : IProjectileService, IDisposable
    {
        private readonly IEventManager _events;
        private readonly ILogger _log;
        private readonly IReadOnlyList<IProjectile> _projectiles;

        private readonly object _lock = new object();

        public OrionProjectileService(IEventManager events, ILogger log)
        {
            Debug.Assert(events != null);
            Debug.Assert(log != null);

            _events = events;
            _log = log;

            // Note that the last projectile should be ignored, as it is not a real projectile.
            _projectiles = new WrappedReadOnlyList<OrionProjectile, Terraria.Projectile>(
                Terraria.Main.projectile.AsMemory(..^1),
                (projectileIndex, terrariaProjectile) => new OrionProjectile(projectileIndex, terrariaProjectile));

            OTAPI.Hooks.Projectile.PreSetDefaultsById = PreSetDefaultsByIdHandler;
            OTAPI.Hooks.Projectile.PreUpdate = PreUpdateHandler;
        }

        public IProjectile this[int index] => _projectiles[index];

        public int Count => _projectiles.Count;

        public IEnumerator<IProjectile> GetEnumerator() => _projectiles.GetEnumerator();

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IProjectile Spawn(ProjectileId id, Vector2f position, Vector2f velocity, int damage, float knockback)
        {
            // Not localized because this string is developer-facing.
            Log.Debug("Spawning {ProjectileId} at {Position}", id, position);

            lock (_lock)
            {
                var projectileIndex = Terraria.Projectile.NewProjectile(
                    position.X, position.Y, velocity.X, velocity.Y, (int)id, damage, knockback);
                Debug.Assert(projectileIndex >= 0 && projectileIndex < Count);

                return this[projectileIndex];
            }
        }

        public void Dispose()
        {
            OTAPI.Hooks.Projectile.PreSetDefaultsById = null;
            OTAPI.Hooks.Projectile.PreUpdate = null;
        }

        // =============================================================================================================
        // OTAPI hooks
        //

        private OTAPI.HookResult PreSetDefaultsByIdHandler(Terraria.Projectile terrariaProjectile, ref int projectileId)
        {
            Debug.Assert(terrariaProjectile != null);

            var projectile = GetProjectile(terrariaProjectile);
            var evt = new ProjectileDefaultsEvent(projectile) { Id = (ProjectileId)projectileId };
            _events.Raise(evt, _log);
            if (evt.IsCanceled)
            {
                return OTAPI.HookResult.Cancel;
            }

            projectileId = (int)evt.Id;
            return OTAPI.HookResult.Continue;
        }

        private OTAPI.HookResult PreUpdateHandler(Terraria.Projectile terrariaProjectile, ref int projectileIndex)
        {
            Debug.Assert(projectileIndex >= 0 && projectileIndex < Count);

            var evt = new ProjectileTickEvent(this[projectileIndex]);
            _events.Raise(evt, _log);
            return evt.IsCanceled ? OTAPI.HookResult.Cancel : OTAPI.HookResult.Continue;
        }

        // Gets an `IProjectile` which corresponds to the given Terraria projectile. Retrieves the `IProjectile` from
        // the `Projectiles` array, if possible.
        private IProjectile GetProjectile(Terraria.Projectile terrariaProjectile)
        {
            Debug.Assert(terrariaProjectile != null);

            var projectileIndex = terrariaProjectile.whoAmI;
            Debug.Assert(projectileIndex >= 0 && projectileIndex < Count);

            var isConcrete = terrariaProjectile == Terraria.Main.projectile[projectileIndex];
            return isConcrete ? this[projectileIndex] : new OrionProjectile(terrariaProjectile);
        }
    }
}
