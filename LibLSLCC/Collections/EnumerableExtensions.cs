﻿#region FileInfo

// 
// File: EnumerableExtensions.cs
// 
// 
// ============================================================
// ============================================================
// 
// 
// Copyright (c) 2015, Eric A. Blundell
// 
// All rights reserved.
// 
// 
// This file is part of LibLSLCC.
// 
// LibLSLCC is distributed under the following BSD 3-Clause License
// 
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
//     in the documentation and/or other materials provided with the distribution.
// 
// 3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived
//     from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// 
// ============================================================
// ============================================================
// 
// 

#endregion

#region Imports

using System;
using System.Collections.Generic;

#endregion

namespace LibLSLCC.Collections
{
    /// <summary>
    ///     <see cref="IEnumerable{T}" /> extensions.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Find the minimum object in an <see cref="IEnumerable{T}" /> using a selection key.
        /// </summary>
        /// <param name="source">The <see cref="IEnumerable{T}" /> source.</param>
        /// <param name="selector">The key selector function.</param>
        /// <typeparam name="TSource">The type <see cref="IEnumerable{T}" /> enumerates over.</typeparam>
        /// <typeparam name="TKey">The key type used in the MinBy comparison.</typeparam>
        /// <returns>The smallest element found.</returns>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            return source.MinBy(selector, Comparer<TKey>.Default);
        }


        /// <summary>
        ///     Find the minimum object in an <see cref="IEnumerable{T}" /> using a selection key.
        /// </summary>
        /// <param name="source">The <see cref="IEnumerable{T}" /> source.</param>
        /// <param name="selector">The key selector function.</param>
        /// <param name="comparer">The comparer used to compare keys.</param>
        /// <typeparam name="TSource">The type <see cref="IEnumerable{T}" /> enumerates over.</typeparam>
        /// <typeparam name="TKey">The key type used in the MinBy comparison.</typeparam>
        /// <returns>The smallest element found.</returns>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            if (comparer == null) throw new ArgumentNullException("comparer");

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence was empty");
                }

                var min = sourceIterator.Current;
                var minKey = selector(min);

                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);

                    if (comparer.Compare(candidateProjected, minKey) >= 0) continue;

                    min = candidate;
                    minKey = candidateProjected;
                }
                return min;
            }
        }


        /// <summary>
        ///     Find the maximum object in an <see cref="IEnumerable{T}" /> using a selection key.
        /// </summary>
        /// <param name="source">The <see cref="IEnumerable{T}" /> source.</param>
        /// <param name="selector">The key selector function.</param>
        /// <typeparam name="TSource">The type <see cref="IEnumerable{T}" /> enumerates over.</typeparam>
        /// <typeparam name="TKey">The key type used in the MaxBy comparison.</typeparam>
        /// <returns>The largest element found.</returns>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, Comparer<TKey>.Default);
        }


        /// <summary>
        ///     Find the maximum object in an <see cref="IEnumerable{T}" /> using a selection key.
        /// </summary>
        /// <param name="source">The <see cref="IEnumerable{T}" /> source.</param>
        /// <param name="selector">The key selector function.</param>
        /// <param name="comparer">The comparer used to compare keys.</param>
        /// <typeparam name="TSource">The type <see cref="IEnumerable{T}" /> enumerates over.</typeparam>
        /// <typeparam name="TKey">The key type used in the MaxBy comparison.</typeparam>
        /// <returns>The largest element found.</returns>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            if (comparer == null) throw new ArgumentNullException("comparer");
            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }

                var max = sourceIterator.Current;
                var maxKey = selector(max);

                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);

                    if (comparer.Compare(candidateProjected, maxKey) <= 0) continue;

                    max = candidate;
                    maxKey = candidateProjected;
                }
                return max;
            }
        }
    }
}