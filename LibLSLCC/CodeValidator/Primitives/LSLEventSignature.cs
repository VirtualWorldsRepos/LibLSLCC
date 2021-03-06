#region FileInfo

// 
// File: LSLEventSignature.cs
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
using System.Linq;
using LibLSLCC.Collections;
using LibLSLCC.Utility;

#endregion

namespace LibLSLCC.CodeValidator
{
    /// <summary>
    ///     Represents a basic event handler call signature.
    /// </summary>
    public class LSLEventSignature : ILSLEventSignature
    {
        private readonly GenericArray<LSLParameterSignature> _parameters;
        private string _name;


        /// <summary>
        ///     Construct an event signature by cloning another <see cref="ILSLEventSignature" /> object.
        /// </summary>
        /// <param name="other">The  <see cref="ILSLEventSignature" /> to copy construct from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
        public LSLEventSignature(ILSLEventSignature other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            Name = other.Name;
            _parameters = new GenericArray<LSLParameterSignature>(other.Parameters);
        }


        /// <summary>
        ///     Construct and empty event signature, can only be used by derived classes.
        /// </summary>
        protected LSLEventSignature()
        {
            _parameters = new GenericArray<LSLParameterSignature>();
        }


        /// <summary>
        ///     Construct and event signature with the provide parameters by providing a name and an enumerable containing
        ///     <see cref="LSLParameterSignature" /> objects.
        /// </summary>
        /// <param name="name">The name of the event signature.</param>
        /// <param name="parameters">The parameters to include in the signature.</param>
        /// <exception cref="LSLInvalidSymbolNameException">
        ///     Thrown if the event handler name does not follow LSL symbol naming conventions.
        /// </exception>
        public LSLEventSignature(string name, IEnumerable<LSLParameterSignature> parameters)
        {
            Name = name;

            if (parameters == null)
            {
                _parameters = new GenericArray<LSLParameterSignature>();
            }
            else
            {
                _parameters = new GenericArray<LSLParameterSignature>();
                foreach (var lslParameter in parameters)
                {
                    AddParameter(lslParameter);
                }
            }
        }


        /// <summary>
        ///     Construct an event signature that has no parameters by providing a name only.
        /// </summary>
        /// <param name="name">The name of the event signature.</param>
        /// <exception cref="LSLInvalidSymbolNameException">
        ///     Thrown if the event handler name does not follow LSL symbol naming conventions.
        /// </exception>
        protected LSLEventSignature(string name)
        {
            Name = name;
            _parameters = new GenericArray<LSLParameterSignature>();
        }


        /// <summary>
        ///     The number of parameters the event handler signature has
        /// </summary>
        public int ParameterCount
        {
            get { return _parameters.Count; }
        }

        /// <summary>
        ///     The event handlers name, must follow LSL symbol naming conventions
        /// </summary>
        /// <exception cref="LSLInvalidSymbolNameException" accessor="set">
        ///     Thrown if the event handler name does not follow LSL symbol naming conventions.
        /// </exception>
        public string Name
        {
            get { return _name; }
            protected set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new LSLInvalidSymbolNameException(GetType().FullName + ": Event name was null or whitespace.");
                }

                if (!LSLTokenTools.IDRegexAnchored.IsMatch(value))
                {
                    throw new LSLInvalidSymbolNameException(
                        string.Format(
                            GetType().FullName + ": Event name '{0}' contained invalid characters or formatting.", value));
                }
                _name = value;
            }
        }


        /// <summary>
        ///     Indexable list of objects describing the event handlers parameters
        /// </summary>
        public IReadOnlyGenericArray<LSLParameterSignature> Parameters
        {
            get { return _parameters; }
        }


        /// <summary>
        ///     Returns a formated signature string for the <see cref="ILSLEventSignature" />.  This does not include a trailing
        ///     semi-colon.
        ///     An example would be: listen(integer channel, string name, key id, string message)
        /// </summary>
        public string SignatureString
        {
            get
            {
                return Name + "(" +
                       string.Join(", ", Parameters.Select(x => x.Type.ToLSLTypeName() + " " + x.Name)) +
                       ")";
            }
        }


        /// <summary>
        ///     Delegates to SignatureString
        /// </summary>
        /// <returns>SignatureString</returns>
        public override string ToString()
        {
            return SignatureString;
        }


        /// <summary>
        ///     Determines if two event handler signatures match exactly, parameter names do not matter but parameter
        ///     types do.
        /// </summary>
        /// <param name="otherSignature">The other event handler signature to compare to.</param>
        /// <returns>True if the two signatures are identical.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="otherSignature"/> is <c>null</c>.</exception>
        public bool SignatureMatches(ILSLEventSignature otherSignature)
        {
            if (otherSignature == null) throw new ArgumentNullException("otherSignature");

            if (Name != otherSignature.Name)
            {
                return false;
            }
            if (ParameterCount != otherSignature.ParameterCount)
            {
                return false;
            }
            for (var i = 0; i < ParameterCount; i++)
            {
                if (Parameters[i].Type != otherSignature.Parameters[i].Type)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        ///     <see cref="GetHashCode"/> uses the name of the <see cref="ILSLEventSignature" /> and the LSL Types of the parameters. <para/>
        ///     This means the Hash Code is determined by the event name, and the Types of all its parameters. <para/>
        ///     Inherently, uniqueness is also determined by the number of parameters.
        /// </summary>
        /// <returns>Hash code for this <see cref="ILSLEventSignature" /></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash*31 + Name.GetHashCode();

                return Parameters.Aggregate(hash,
                    (current, lslParameter) => current*31 + lslParameter.Type.GetHashCode());
            }
        }


        /// <summary>
        ///     <see cref="Equals(object)"/> delegates to <see cref="ILSLEventSignature.SignatureMatches" />
        /// </summary>
        /// <param name="obj">The other event signature</param>
        /// <returns>Equality.</returns>
        public override bool Equals(object obj)
        {
            var o = obj as ILSLEventSignature;
            if (o == null)
            {
                return false;
            }

            return SignatureMatches(o);
        }


        /// <summary>
        ///     Add an <see cref="LSLParameterSignature" /> object to this event signatures
        /// </summary>
        /// <param name="parameterSignature">The <see cref="LSLParameterSignature" /> object to add.</param>
        /// <exception cref="ArgumentException">Thrown if the added parameter is a variadic parameter.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="parameterSignature"/> is <c>null</c>.</exception>
        public void AddParameter(LSLParameterSignature parameterSignature)
        {
            if (parameterSignature == null) throw new ArgumentNullException("parameterSignature");

            if (parameterSignature.Variadic)
            {
                throw new ArgumentException(
                    GetType().FullName + ": Cannot add variadic parameters to an event signature", "parameterSignature");
            }

            _parameters.Add(new LSLParameterSignature(parameterSignature, _parameters.Count));
        }


        /// <summary>
        ///     Attempts to parse the signature from a formated string. <para/>
        ///     Such as:  "listen( integer channel, string name, key id, string message )" <para/>
        ///     Trailing semi-colon is optional.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <exception cref="ArgumentException">Thrown if the string could not be parsed.</exception>
        /// <returns>The Parsed <see cref="LSLEventSignature" /></returns>
        public static LSLEventSignature Parse(string str)
        {
            var regex = new LSLEventSignatureRegex("", ";*");
            var m = regex.GetSignature(str);
            if (m == null)
            {
                throw new ArgumentException("Syntax error parsing event signature", "str");
            }
            return m;
        }
    }
}