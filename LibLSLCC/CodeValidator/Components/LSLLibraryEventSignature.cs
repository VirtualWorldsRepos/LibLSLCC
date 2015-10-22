#region FileInfo
// 
// File: LSLLibraryEventSignature.cs
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
using System.Security;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using LibLSLCC.CodeValidator.Components.Interfaces;
using LibLSLCC.CodeValidator.Enums;
using LibLSLCC.CodeValidator.Exceptions;
using LibLSLCC.CodeValidator.Primitives;
using LibLSLCC.Collections;

#endregion

namespace LibLSLCC.CodeValidator.Components
{
    /// <summary>
    /// Represents a library event handler returned from an ILSLLibraryDataProvider implementation.
    /// </summary>
    [XmlRoot("EventHandler")]
    public sealed class LSLLibraryEventSignature : LSLEventSignature, IXmlSerializable, ILSLLibrarySignature
    {
        private HashMap<string, string> _properties = new HashMap<string, string>();
        private HashedSet<string> _subsets = new HashedSet<string>();

        private LSLLibraryDataSubsetsAttributeRegex _subsetsRegex = new
            LSLLibraryDataSubsetsAttributeRegex();

        private LSLLibraryEventSignature()
        {
        }

        /// <summary>
        /// Construct an LSLLibraryEventSignature by copying the signature details from an LSLEventSignature object.
        /// </summary>
        /// <param name="sig">The LSLEventSignature object to copy signatures details from.</param>
        public LSLLibraryEventSignature(LSLEventSignature sig)
            : base(sig)
        {
            DocumentationString = "";
        }


        /// <summary>
        /// Construct an LSLLibraryEventSignature by cloning another LSLLibraryEventSignature object.
        /// </summary>
        /// <param name="other">The LSLLibraryEventSignature to copy construct from.</param>
        public LSLLibraryEventSignature(LSLLibraryEventSignature other)
            : base(other)
        {
            DocumentationString = other.DocumentationString;
            _subsets = new HashedSet<string>(other._subsets);
            _properties = other._properties.ToHashMap(x => x.Key, y => y.Value);
        }


        /// <summary>
        /// Construct an LSLLibraryEventSignature by providing a Name and a list of LSLParameter's that belong to the signature.
        /// </summary>
        /// <param name="name">The name of the event handler.</param>
        /// <param name="parameters">The list of parameters that belong to this signature.</param>
        public LSLLibraryEventSignature(string name, IEnumerable<LSLParameter> parameters)
            : base(name, parameters)
        {
            DocumentationString = "";
        }


        /// <summary>
        /// Construct an LSLLibraryEventSignature with no parameters by providing an event Name only.
        /// </summary>
        /// <param name="name">The name of the event Handler.</param>
        public LSLLibraryEventSignature(string name)
            : base(name)
        {
            DocumentationString = "";
        }

        /// <summary>
        /// The library subsets that this LSLLibraryEventSignature belongs to.
        /// </summary>
        public IReadOnlyHashedSet<string> Subsets
        {
            get { return _subsets; }
        }


        /// <summary>
        /// Additional dynamic property values that can be attached to the constant signature and parsed from XML
        /// </summary>
        public IDictionary<string, string> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Returns the documentation string attached to this library signature.
        /// </summary>
        public string DocumentationString { get; set; }


        /// <summary>
        /// Returns a formated string containing the signature and documentation for this library signature.
        /// It consists of the SignatureString followed by a semi-colon, and then followed by a new-line and DocumentationString
        /// if the documentation string is not null.
        /// </summary>
        public string SignatureAndDocumentation
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DocumentationString))
                {
                    return SignatureString + ";";
                }
                return SignatureString + ";" +
                       Environment.NewLine +
                       DocumentationString;
            }
        }

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return
        ///     null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the
        ///     <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is
        ///     produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        ///     method.
        /// </returns>
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }


        /// <summary>
        /// Fills an event signature object from an XML fragment.
        /// </summary>
        /// <param name="reader">The XML reader containing the fragment to read.</param>
        /// <exception cref="LSLInvalidSymbolNameException">Thrown if the event signatures name or any of its parameters names do not abide by LSL symbol naming conventions.</exception>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            var parameterNames = new HashSet<string>();


            reader.MoveToContent();

            var hasSubsets = false;
            var hasName = false;

            var lineNumberInfo = (IXmlLineInfo) reader;

            while (reader.MoveToNextAttribute())
            {
                if (reader.Name == "Subsets")
                {
                    SetSubsets(reader.Value);
                    hasSubsets = true;
                }
                else if (reader.Name == "Name")
                {
                    if (string.IsNullOrWhiteSpace(reader.Value))
                    {
                        throw new XmlSyntaxException(lineNumberInfo.LineNumber,
                            GetType().Name + ", Name attribute invalid");
                    }
                    hasName = true;
                    Name = reader.Value;
                }
                else
                {
                    throw new XmlSyntaxException(lineNumberInfo.LineNumber,
                        GetType().Name + ", unknown attribute " + reader.Name);
                }
            }


            if (!hasName)
            {
                throw new XmlSyntaxException(lineNumberInfo.LineNumber,
                    "Missing Name attribute");
            }


            if (!hasSubsets)
            {
                throw new XmlSyntaxException(lineNumberInfo.LineNumber,
                    "Missing Subsets attribute");
            }


            var canRead = reader.Read();
            while (canRead)
            {
                if ((reader.Name == "Parameter") && reader.IsStartElement())
                {
                    LSLType pType;

                    if (!Enum.TryParse(reader.GetAttribute("Type"), out pType))
                    {
                        throw new XmlSyntaxException(lineNumberInfo.LineNumber,
                            GetType().Name + ", Parameter Type attribute invalid");
                    }

                    if (pType == LSLType.Void)
                    {
                        throw new XmlSyntaxException(lineNumberInfo.LineNumber,
                            GetType().Name + ", Parameter Type invalid, event handler parameters cannot be Void");
                    }

                    var pName = reader.GetAttribute("Name");
                    if (string.IsNullOrWhiteSpace(pName))
                    {
                        throw new XmlSyntaxException(lineNumberInfo.LineNumber,
                            GetType().Name + ", Parameter Name attribute invalid");
                    }

                    if (parameterNames.Contains(pName))
                    {
                        throw new XmlSyntaxException(lineNumberInfo.LineNumber,
                            GetType().Name + ", Parameter Name already used");
                    }

                    parameterNames.Add(pName);
                    AddParameter(new LSLParameter(pType, pName, false));
     
                    canRead = reader.Read();
                }
                else if ((reader.Name == "DocumentationString") && reader.IsStartElement())
                {
                    DocumentationString = reader.ReadElementContentAsString();
                    canRead = reader.Read();
                }
                else if ((reader.Name == "Property") && reader.IsStartElement())
                {
                    var name = reader.GetAttribute("Name");

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        throw new XmlSyntaxException(lineNumberInfo.LineNumber,
                            string.Format(
                                "{0}, Event {1}: Property element's Name attribute cannot be empty",
                                GetType().Name, Name));
                    }

                    var value = reader.GetAttribute("Value");

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new XmlSyntaxException(lineNumberInfo.LineNumber,
                            string.Format(
                                "{0}, Event {1}: Property element's Value attribute cannot be empty",
                                GetType().Name, Name));
                    }

                    if (_properties.ContainsKey(name))
                    {
                        throw new XmlSyntaxException(lineNumberInfo.LineNumber,
                            string.Format(
                                "{0}, Event {1}: Property name {2} has already been used",
                                GetType().Name, Name, name));
                    }

                    _properties.Add(name, value);

                    canRead = reader.Read();
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "EventHandler")
                {
                    break;
                }
                else
                {
                    canRead = reader.Read();
                }
            }
        }

        /// <summary>
        ///     Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is serialized. </param>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("Subsets", string.Join(",", _subsets));

            foreach (var param in Parameters)
            {
                writer.WriteStartElement("Parameter");
                writer.WriteAttributeString("Name", param.Name);
                writer.WriteAttributeString("Type", param.Type.ToString());
                writer.WriteEndElement();
            }

            foreach (var prop in Properties)
            {
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("Name", prop.Key);
                writer.WriteAttributeString("Value", prop.Value);
                writer.WriteEndElement();
            }

            writer.WriteStartElement("DocumentationString");
            writer.WriteString(DocumentationString);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Attempts to parse the signature from a formated string.
        /// Such as:  listen( integer channel, string name, key id, string message )
        /// Trailing semi-colon is optional.
        /// </summary>
        /// <param name="str"></param>
        /// <exception cref="ArgumentException">Thrown if the string could not be parsed.</exception>
        /// <returns>The Parsed LSLLibraryEventSignature</returns>
        public new static LSLLibraryEventSignature Parse(string str)
        {
            return new LSLLibraryEventSignature(LSLEventSignature.Parse(str));
        }

        /// <summary>
        /// Adds to the current library subsets this signature belongs to by parsing them out of a comma separated string of names.
        /// </summary>
        /// <param name="subsets">A comma separated list of subset names in a string to add.</param>
        public void AddSubsets(string subsets)
        {
            _subsets.UnionWith(_subsetsRegex.ParseSubsets(subsets));
        }

        /// <summary>
        /// Sets the library subsets this signature belongs to.
        /// </summary>
        /// <param name="subsets">An enumerable of subset name strings</param>
        public void AddSubsets(IEnumerable<string> subsets)
        {
            _subsets.UnionWith(subsets);
        }

        /// <summary>
        /// Sets the library subsets this signature belongs to.
        /// </summary>
        /// <param name="subsets">An enumerable of subset name strings</param>
        public void SetSubsets(IEnumerable<string> subsets)
        {
            _subsets = new HashedSet<string>(subsets);
        }

        /// <summary>
        /// Sets the library subsets this signature belongs to by parsing them out of a comma separated string of names.
        /// </summary>
        /// <param name="subsets">A comma separated list of subset names in a string.</param>
        public void SetSubsets(string subsets)
        {
            _subsets = new HashedSet<string>(_subsetsRegex.ParseSubsets(subsets));
        }

        /// <summary>
        /// Reads an event signature object from an XML fragment.
        /// </summary>
        /// <param name="fragment">The XML reader containing the fragment to read.</param>
        /// <returns>The parsed LSLLibraryEventSignature object.</returns>
        /// <exception cref="LSLInvalidSymbolNameException">Thrown if the event signatures name or any of its parameters names do not abide by LSL symbol naming conventions.</exception>
        public static LSLLibraryEventSignature FromXmlFragment(XmlReader fragment)
        {
            var ev = new LSLLibraryEventSignature();
            IXmlSerializable x = ev;
            x.ReadXml(fragment);
            return ev;
        }

        /// <summary>
        /// Whether or not this library signature is marked as deprecated or not.
        /// </summary>
        public bool Deprecated
        {
            get
            {
                string deprecatedStatus;
                return (Properties.TryGetValue("Deprecated", out deprecatedStatus) &&
                        deprecatedStatus.ToLower() == "true");
            }
            set
            {
                if (value == false)
                {
                    if (Properties.ContainsKey("Deprecated"))
                    {
                        Properties.Remove("Deprecated");
                    }
                }
                else
                {
                    Properties["Deprecated"] = "true";
                }
            }
        }
    }
}