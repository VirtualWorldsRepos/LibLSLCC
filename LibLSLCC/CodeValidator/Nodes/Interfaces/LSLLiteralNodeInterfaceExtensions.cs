﻿#region FileInfo
// 
// File: LSLLiteralNodeInterfaceExtensions.cs
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
using System;

namespace LibLSLCC.CodeValidator.Nodes.Interfaces
{
    public static class LSLLiteralNodeInterfaceExtensions
    {
        /// <summary>
        /// Determines whether the integer literal node is a literal value that overflows/underflows a 32 bit integer.
        /// </summary>
        /// <param name="node">The integer literal node to test.</param>
        /// <returns>True if the integer literal overflows/underflows a 32 bit integer.</returns>
        public static bool IsIntegerLiteralOverflowed(this ILSLIntegerLiteralNode node)
        {
            try
            {
                Convert.ToInt32(node.RawText);
                return false;
            }
            catch (OverflowException)
            {
                return true;
            }
        }

        /// <summary>
        /// Determines whether the hex literal node is a literal value that overflows/underflows a 32 bit integer.
        /// </summary>
        /// <param name="node">The integer hex node to test.</param>
        /// <returns>True if the hex literal overflows/underflows a 32 bit integer.</returns>
        public static bool IsHexLiteralOverflowed(this ILSLHexLiteralNode node)
        {
            try
            {
                Convert.ToInt32(node.RawText, 16);
                return false;
            }
            catch (OverflowException)
            {
                return true;
            }
        }
    }
}
