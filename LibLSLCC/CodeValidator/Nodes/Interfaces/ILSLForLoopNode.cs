﻿#region FileInfo

// 
// File: ILSLForLoopNode.cs
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

#endregion

namespace LibLSLCC.CodeValidator
{
    /// <summary>
    ///     AST node interface for for-loop statements.
    /// </summary>
    public interface ILSLForLoopNode : ILSLReadOnlyCodeStatement, ILSLLoopNode
    {
        /// <summary>
        ///     The source code range of the 'for' keyword in the statement.
        /// </summary>
        /// <remarks>
        ///     If <see cref="ILSLReadOnlySyntaxTreeNode.SourceRangesAvailable" /> is <c>false</c> this property will be
        ///     <c>null</c>.
        /// </remarks>
        LSLSourceCodeRange SourceRangeForKeyword { get; }

        /// <summary>
        ///     The source code range of the opening parenthesis that starts the for-loop clauses area.
        /// </summary>
        /// <remarks>
        ///     If <see cref="ILSLReadOnlySyntaxTreeNode.SourceRangesAvailable" /> is <c>false</c> this property will be
        ///     <c>null</c>.
        /// </remarks>
        LSLSourceCodeRange SourceRangeOpenParenth { get; }

        /// <summary>
        ///     The expression list node that contains the expressions used in the initialization clause of the for-loop.
        ///     This property should never be null unless the for loop node is an erroneous node.
        ///     Ideally you should not be handling a syntax tree containing syntax errors.
        /// </summary>
        ILSLExpressionListNode InitExpressionList { get; }

        /// <summary>
        ///     The source code range of the semi-colon that separates the initialization clause from the condition clause of the
        ///     for-loop;
        /// </summary>
        /// <remarks>
        ///     If <see cref="ILSLReadOnlySyntaxTreeNode.SourceRangesAvailable" /> is <c>false</c> this property will be
        ///     <c>null</c>.
        /// </remarks>
        LSLSourceCodeRange SourceRangeFirstSemicolon { get; }

        /// <summary>
        ///     The source code range of the semi-colon that separates the condition clause from the afterthought expressions of
        ///     the for-loop;
        /// </summary>
        /// <remarks>
        ///     If <see cref="ILSLReadOnlySyntaxTreeNode.SourceRangesAvailable" /> is <c>false</c> this property will be
        ///     <c>null</c>.
        /// </remarks>
        LSLSourceCodeRange SourceRangeSecondSemicolon { get; }

        /// <summary>
        ///     The source code range of the closing parenthesis that ends the for-loop clause section.
        /// </summary>
        /// <remarks>
        ///     If <see cref="ILSLReadOnlySyntaxTreeNode.SourceRangesAvailable" /> is <c>false</c> this property will be
        ///     <c>null</c>.
        /// </remarks>
        LSLSourceCodeRange SourceRangeCloseParenth { get; }

        /// <summary>
        ///     The expression list node that contains the expressions used in the afterthought area of the for-loop's clauses.
        ///     This property should never be null unless the for loop node is an erroneous node.
        ///     Ideally you should not be handling a syntax tree containing syntax errors.
        /// </summary>
        ILSLExpressionListNode AfterthoughtExpressionList { get; }

        /// <summary>
        ///     Returns true if the for-loop statement contains any initialization expressions, otherwise False.
        /// </summary>
        bool HasInitExpressions { get; }

        /// <summary>
        ///     Returns true if the for-loop statement contains a condition expression, otherwise False.
        /// </summary>
        bool HasConditionExpression { get; }

        /// <summary>
        ///     Returns true if the for-loop statement contains any afterthought expressions, otherwise False.
        /// </summary>
        bool HasAfterthoughtExpressions { get; }
    }
}