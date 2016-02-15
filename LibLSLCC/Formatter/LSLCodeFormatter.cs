﻿#region FileInfo
// 
// File: LSLCodeFormatter.cs
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
using System.IO;
using LibLSLCC.CodeValidator.Nodes.Interfaces;
using LibLSLCC.Formatter.Visitor;

namespace LibLSLCC.Formatter
{
    public sealed class LSLCodeFormatter
    {
        private LSLCodeFormatterSettings _settings;

        public LSLCodeFormatter(LSLCodeFormatterSettings settings)
        {
            _settings = settings;
        }

        public LSLCodeFormatter()
        {
            _settings = new LSLCodeFormatterSettings();
        }

        public LSLCodeFormatterSettings Settings
        {
            get { return _settings; }
            set
            {
                if (_settings == null)
                {
                    throw new ArgumentNullException("value", typeof(LSLCodeFormatter).Name+".Settings cannot be null!.");
                }
                _settings = value;
            }
        }

        public void Format(string sourceReference, ILSLCompilationUnitNode node, TextWriter writer,
            bool closeStream = true)
        {
            var formatter = new LSLCodeFormatterVisitor(Settings);
            formatter.WriteAndFlush(sourceReference, node, writer, closeStream);
        }
    }
}
