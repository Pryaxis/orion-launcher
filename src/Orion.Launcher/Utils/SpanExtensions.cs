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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Orion.Core.Utils
{
    /// <summary>
    /// Provides extensions for the <see cref="Span{T}"/> structure.
    /// </summary>
    internal static class SpanExtensions
    {
        /// <summary>
        /// Returns a reference to the element at the given <paramref name="index"/>. <i>Performs no bounds
        /// checking!</i>
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="span">The span.</param>
        /// <param name="index">The index.</param>
        /// <returns>A reference to the element at the given <paramref name="index"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T At<T>(this Span<T> span, int index)
        {
            Debug.Assert(index >= 0 && index < span.Length);

            return ref Unsafe.Add(ref MemoryMarshal.GetReference(span), index);
        }
    }
}
