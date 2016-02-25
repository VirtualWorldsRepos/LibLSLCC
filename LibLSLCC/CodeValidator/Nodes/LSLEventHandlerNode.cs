﻿#region FileInfo

// 
// File: LSLEventHandlerNode.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using LibLSLCC.AntlrParser;

#endregion

namespace LibLSLCC.CodeValidator
{
    /// <summary>
    ///     Default <see cref="ILSLEventHandlerNode" /> implementation used by <see cref="LSLCodeValidator" />
    /// </summary>
    public sealed class LSLEventHandlerNode : ILSLEventHandlerNode, ILSLSyntaxTreeNode
    {
// ReSharper disable UnusedParameter.Local
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "err")]
        private LSLEventHandlerNode(LSLSourceCodeRange sourceRange, Err err)
// ReSharper restore UnusedParameter.Local
        {
            SourceRange = sourceRange;
            HasErrors = true;
        }



        /// <summary>
        /// Construct an <see cref="LSLEventHandlerNode"/> with the given parameter list and code body.
        /// </summary>
        /// <param name="parameterList">The parameter list.</param>
        /// <param name="code">The code body.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="parameterList"/> or <paramref name="code"/> is <c>null</c>.</exception>
        public LSLEventHandlerNode(LSLParameterListNode parameterList, LSLCodeScopeNode code)
        {
            if(parameterList == null) throw new ArgumentNullException("parameterList");
            if(code == null) throw new ArgumentNullException("code");

            ParameterList = parameterList;
            ParameterList.Parent = this;

            Code = code;
            Code.Parent = this;
        }



        /// <summary>
        /// Construct an <see cref="LSLEventHandlerNode"/> with the given code body and no parameters.
        /// </summary>
        /// <param name="code">The code body.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="code"/> is <c>null</c>.</exception>
        public LSLEventHandlerNode(LSLCodeScopeNode code)
        {
            if (code == null) throw new ArgumentNullException("code");

            ParameterList = new LSLParameterListNode();
            ParameterList.Parent = this;

            Code = code;
            Code.Parent = this;
        }



        /// <exception cref="ArgumentNullException">
        ///     <paramref name="parameterList" /> or <paramref name="code" /> is
        ///     <c>null</c>.
        /// </exception>
        internal LSLEventHandlerNode(LSLParser.EventHandlerContext context, LSLParameterListNode parameterList,
            LSLCodeScopeNode code)
        {
            if (parameterList == null)
            {
                throw new ArgumentNullException("parameterList");
            }

            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            Name = context.handler_name.Text;

            Code = code;
            Code.Parent = this;

            ParameterList = parameterList;
            ParameterList.Parent = this;


            SourceRange = new LSLSourceCodeRange(context);
            SourceRangeName = new LSLSourceCodeRange(context.handler_name);

            SourceRangesAvailable = true;
        }


        /// <summary>
        ///     The code scope node that represents the code body of the event handler.
        /// </summary>
        public LSLCodeScopeNode Code { get; private set; }

        /// <summary>
        ///     The parameter list node for the parameters of the event handler.  This is non null even when no parameters exist.
        /// </summary>
        public LSLParameterListNode ParameterList { get; private set; }

        /// <summary>
        ///     The source code range of the event handler name.
        /// </summary>
        /// <remarks>
        ///     If <see cref="ILSLReadOnlySyntaxTreeNode.SourceRangesAvailable" /> is <c>false</c> this property will be
        ///     <c>null</c>.
        /// </remarks>
        public LSLSourceCodeRange SourceRangeName { get; private set; }

        ILSLReadOnlySyntaxTreeNode ILSLReadOnlySyntaxTreeNode.Parent
        {
            get { return Parent; }
        }

        /// <summary>
        ///     The name of the event handler.
        /// </summary>
        public string Name { get; private set; }

        ILSLCodeScopeNode ILSLEventHandlerNode.Code
        {
            get { return Code; }
        }

        ILSLParameterListNode ILSLEventHandlerNode.ParameterList
        {
            get { return ParameterList; }
        }


        /// <summary>
        ///     Returns a version of this node type that represents its error state;  in case of a syntax error
        ///     in the node that prevents the node from being even partially built.
        /// </summary>
        /// <param name="sourceRange">The source code range of the error.</param>
        /// <returns>A version of this node type in its undefined/error state.</returns>
        public static
            LSLEventHandlerNode GetError(LSLSourceCodeRange sourceRange)
        {
            return new LSLEventHandlerNode(sourceRange, Err.Err);
        }

        #region Nested type: Err

        private enum Err
        {
            Err
        }

        #endregion

        #region ILSLTreeNode Members

        /// <summary>
        ///     The source code range that this syntax tree node occupies.
        /// </summary>
        /// <remarks>
        ///     If <see cref="ILSLReadOnlySyntaxTreeNode.SourceRangesAvailable" /> is <c>false</c> this property will be
        ///     <c>null</c>.
        /// </remarks>
        public LSLSourceCodeRange SourceRange { get; private set; }


        /// <summary>
        ///     Should return true if source code ranges are available/set to meaningful values for this node.
        /// </summary>
        public bool SourceRangesAvailable { get; private set; }


        /// <summary>
        ///     True if this syntax tree node contains syntax errors.
        /// </summary>
        public bool HasErrors { get; internal set; }


        /// <summary>
        ///     Accept a visit from an implementor of <see cref="ILSLValidatorNodeVisitor{T}" />
        /// </summary>
        /// <typeparam name="T">The visitors return type.</typeparam>
        /// <param name="visitor">The visitor instance.</param>
        /// <returns>The value returned from this method in the visitor used to visit this node.</returns>
        public T AcceptVisitor<T>(ILSLValidatorNodeVisitor<T> visitor)
        {
            return visitor.VisitEventHandler(this);
        }


        /// <summary>
        ///     The parent node of this syntax tree node.
        /// </summary>
        public ILSLSyntaxTreeNode Parent { get; set; }

        #endregion


        /// <summary>
        /// Build a <see cref="LSLEventSignature"/> object based off the signature of this function declaration node.
        /// </summary>
        /// <returns>The created <see cref="LSLEventSignature"/>.</returns>
        public LSLEventSignature CreateSignature()
        {
            return new LSLEventSignature(Name, ParameterList.Parameters.Select(x => new LSLParameter(x.Type, x.Name, false)));
        }
    }
}