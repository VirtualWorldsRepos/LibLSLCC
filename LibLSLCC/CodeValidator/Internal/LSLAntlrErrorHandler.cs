﻿#region FileInfo

// 
// File: LSLAntlrErrorHandler.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;

#endregion

namespace LibLSLCC.CodeValidator
{
    internal sealed class LSLAntlrErrorHandler : IAntlrErrorListener<IToken>
    {
        private static readonly Regex NonLValueAssignmentError =
            new Regex("mismatched input '[*+-/%]?=' expecting {(.*?)}");

        private readonly ILSLSyntaxErrorListener _errorListener;


        public LSLAntlrErrorHandler(ILSLSyntaxErrorListener errorListener)
        {
            _errorListener = errorListener;
        }


        public bool HasErrors { get; private set; }


        public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            HasErrors = true;

            var m = NonLValueAssignmentError.Match(msg);

            var expected = new HashSet<string>(m.Groups[1].ToString().Split(',').Select(x => x.Trim('\'', ' ')));

            if (m.Success && expected.Contains("*") && !(
                expected.Contains("TYPE")
                || expected.Contains("ID")
                || expected.Contains("INT")
                || expected.Contains("FLOAT")
                || expected.Contains("HEX_LITERAL")
                || expected.Contains("QUOTED_STRING")
                ))
            {
                _errorListener.AssignmentToNonassignableExpression(new LSLSourceCodeRange(offendingSymbol),
                    offendingSymbol.Text);
            }
            else
            {
                _errorListener.GrammarLevelParserSyntaxError(line, charPositionInLine,
                    new LSLSourceCodeRange(offendingSymbol), offendingSymbol.Text, msg);
            }
        }
    }
}