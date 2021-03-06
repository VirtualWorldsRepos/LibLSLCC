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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using LibLSLCC.CodeValidator;

#endregion

namespace LibLSLCC.LibraryData
{
    /// <summary>
    ///     Represents a library event handler returned from an <see cref="ILSLLibraryDataProvider" /> implementation.
    /// </summary>
    [XmlRoot("EventHandler")]
    public sealed class LSLLibraryEventSignature : LSLEventSignature, IXmlSerializable, ILSLLibrarySignature
    {
        private Dictionary<string, string> _properties = new Dictionary<string, string>();
        private LSLLibraryDataSubsetCollection _subsets = new LSLLibraryDataSubsetCollection();


        private LSLLibraryEventSignature()
        {
            DocumentationString = "";
        }


        /// <summary>
        ///     Construct an LSLLibraryEventSignature by copying the signature details from an <see cref="ILSLEventSignature" />
        ///     object.
        /// </summary>
        /// <param name="sig">The <see cref="ILSLEventSignature" /> object to copy signatures details from.</param>
        public LSLLibraryEventSignature(ILSLEventSignature sig)
            : base(sig)
        {
            DocumentationString = "";
        }


        /// <summary>
        ///     Construct an LSLLibraryEventSignature by cloning another LSLLibraryEventSignature object.
        /// </summary>
        /// <param name="other">The LSLLibraryEventSignature to copy construct from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other" /> is <c>null</c>.</exception>
        public LSLLibraryEventSignature(LSLLibraryEventSignature other)
            : base(other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }


            DocumentationString = other.DocumentationString;
            _subsets = new LSLLibraryDataSubsetCollection(other._subsets);
            _properties = other._properties.ToDictionary(x => x.Key, y => y.Value);
        }


        /// <summary>
        ///     Construct an LSLLibraryEventSignature by providing a Name and a list of <see cref="LSLParameterSignature" /> objects that
        ///     belong to the signature.
        /// </summary>
        /// <param name="name">The name of the event handler.</param>
        /// <param name="parameters">The list of parameters that belong to this signature.</param>
        public LSLLibraryEventSignature(string name, IEnumerable<LSLParameterSignature> parameters)
            : base(name, parameters)
        {
            DocumentationString = "";
        }


        /// <summary>
        ///     Construct an LSLLibraryEventSignature with no parameters by providing an event Name only.
        /// </summary>
        /// <param name="name">The name of the event Handler.</param>
        public LSLLibraryEventSignature(string name)
            : base(name)
        {
            DocumentationString = "";
        }


        /// <summary>
        ///     The library subsets that this LSLLibraryEventSignature belongs to.
        /// </summary>
        public LSLLibraryDataSubsetCollection Subsets
        {
            get { return _subsets; }
        }

        /// <summary>
        ///     Additional dynamic property values that can be attached to the constant signature and parsed from XML
        /// </summary>
        public IDictionary<string, string> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        ///     Returns the documentation string attached to this library signature.
        /// </summary>
        public string DocumentationString { get; set; }

        /// <summary>
        ///     Returns a formated string containing the signature and documentation for this library signature.
        ///     It consists of the SignatureString followed by a semi-colon, and then followed by a new-line and
        ///     DocumentationString
        ///     if the documentation string is not null.
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
        ///     Whether or not this library signature is marked as deprecated or not.
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
        ///     Fills an event signature object from an XML fragment.
        /// </summary>
        /// <param name="reader">The XML reader containing the fragment to read.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="reader" /> is <c>null</c>.</exception>
        /// <exception cref="LSLLibraryDataXmlSyntaxException">
        ///     On missing or unknown attributes.
        ///     If a parameter 'Name' is used more than once.
        ///     If a parameter 'Name' is whitespace.
        ///     If a parameter 'Type' is <see cref="LSLType.Void" />.
        ///     If a parameter 'Type' does not correspond to an <see cref="LSLType" /> enumeration member.
        ///     If a 'Properties' node 'Name' is <c>null</c> or whitespace.
        ///     If a 'Properties' node 'Name' is used more than once.
        ///     If a 'Properties' node 'Value' is <c>null</c> or whitespace.
        /// </exception>
        /// <exception cref="LSLInvalidSymbolNameException">
        ///     Thrown if the event signatures name or any of its parameters names do
        ///     not abide by LSL symbol naming conventions.
        /// </exception>
        /// <exception cref="XmlException">Incorrect XML encountered in the input stream. </exception>
        /// <exception cref="LSLInvalidSubsetNameException">
        ///     Thrown if any of the given subset names in the 'Subsets' CSV string do
        ///     not match the pattern ([a-zA-Z]+[a-zA-Z_0-9\\-]*).
        /// </exception>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            var parameterNames = new HashSet<string>();


            reader.MoveToContent();

            var hasSubsets = false;
            var hasName = false;

            var lineNumberInfo = (IXmlLineInfo) reader;

            while (reader.MoveToNextAttribute())
            {
                if (reader.Name == "Subsets")
                {
                    Subsets.SetSubsets(reader.Value);
                    hasSubsets = true;
                }
                else if (reader.Name == "Name")
                {
                    hasName = true;
                    Name = reader.Value;
                }
                else
                {
                    throw new LSLLibraryDataXmlSyntaxException(lineNumberInfo.LineNumber,
                        string.Format("EventHandler{0}: Unknown attribute '{1}'.",
                            hasName ? (" '" + Name + "'") : "", reader.Name));
                }
            }


            if (!hasName)
            {
                throw new LSLLibraryDataXmlSyntaxException(lineNumberInfo.LineNumber,
                    "EventHandler: Missing Name attribute.");
            }


            if (!hasSubsets)
            {
                throw new LSLLibraryDataXmlSyntaxException(lineNumberInfo.LineNumber,
                    string.Format("EventHandler '{0}': Missing Subsets attribute.", Name));
            }


            var canRead = reader.Read();
            while (canRead)
            {
                if ((reader.Name == "Parameter") && reader.IsStartElement())
                {
                    var pName = reader.GetAttribute("Name");
                    if (string.IsNullOrWhiteSpace(pName))
                    {
                        throw new LSLLibraryDataXmlSyntaxException(lineNumberInfo.LineNumber,
                            string.Format(
                                "EventHandler '{0}': Parameter Name attribute invalid, cannot be empty or whitespace.",
                                Name));
                    }

                    LSLType pType;

                    if (!Enum.TryParse(reader.GetAttribute("Type"), out pType))
                    {
                        throw new LSLLibraryDataXmlSyntaxException(lineNumberInfo.LineNumber,
                            string.Format("EventHandler '{0}': Parameter named '{1}' has an invalid Type attribute.",
                                pName, Name));
                    }

                    if (pType == LSLType.Void)
                    {
                        throw new LSLLibraryDataXmlSyntaxException(lineNumberInfo.LineNumber,
                            string.Format(
                                "EventHandler '{0}': Parameter named '{1}' has an invalid Type, event parameters cannot be Void.",
                                Name, pName));
                    }

                    if (parameterNames.Contains(pName))
                    {
                        throw new LSLLibraryDataXmlSyntaxException(lineNumberInfo.LineNumber,
                            string.Format("EventHandler '{0}': Parameter Name '{1}' already used.", Name, pName));
                    }

                    parameterNames.Add(pName);
                    AddParameter(new LSLParameterSignature(pType, pName, false));

                    canRead = reader.Read();
                }
                else if ((reader.Name == "DocumentationString") && reader.IsStartElement())
                {
                    DocumentationString = reader.ReadElementContentAsString();
                    canRead = reader.Read();
                }
                else if ((reader.Name == "Property") && reader.IsStartElement())
                {
                    var pName = reader.GetAttribute("Name");

                    if (string.IsNullOrWhiteSpace(pName))
                    {
                        throw new LSLLibraryDataXmlSyntaxException(lineNumberInfo.LineNumber,
                            string.Format("EventHandler '{0}': Property element's Name attribute cannot be empty.",
                                pName));
                    }

                    var value = reader.GetAttribute("Value");

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new LSLLibraryDataXmlSyntaxException(lineNumberInfo.LineNumber,
                            string.Format("EventHandler '{0}': Property element's Value attribute cannot be empty.",
                                pName));
                    }

                    if (_properties.ContainsKey(pName))
                    {
                        throw new LSLLibraryDataXmlSyntaxException(lineNumberInfo.LineNumber,
                            string.Format(
                                "EventHandler '{0}': Property name '{1}' has already been used.", Name, pName));
                    }

                    _properties.Add(pName, value);

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
        ///     Converts this <see cref="LSLLibraryEventSignature" /> into its XML representation.
        ///     The root element name is not written, this is due to <see cref="IXmlSerializable" /> implementation requirements.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter" /> stream to which the object is serialized. </param>
        /// <exception cref="ArgumentNullException"><paramref name="writer" /> is <c>null</c>.</exception>
        public void WriteXml(XmlWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");

            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("Subsets", string.Join(",", _subsets));

            foreach (var param in Parameters)
            {
                writer.WriteStartElement("Parameter");
                writer.WriteAttributeString("Name", param.Name);
                writer.WriteAttributeString("Type", param.Type.ToString());
                writer.WriteEndElement();
            }

            writer.WriteStartElement("DocumentationString");
            writer.WriteString(DocumentationString);
            writer.WriteEndElement();

            foreach (var prop in Properties)
            {
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("Name", prop.Key);
                writer.WriteAttributeString("Value", prop.Value);
                writer.WriteEndElement();
            }
        }


        /// <summary>
        ///     Attempts to parse the signature from a formated string.
        ///     Such as:  listen( integer channel, string name, key id, string message )
        ///     Trailing semi-colon is optional.
        /// </summary>
        /// <param name="str"></param>
        /// <exception cref="ArgumentException">Thrown if the string could not be parsed.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>The Parsed LSLLibraryEventSignature</returns>
        public new static LSLLibraryEventSignature Parse(string str)
        {
            if (str == null) throw new ArgumentNullException("str");

            return new LSLLibraryEventSignature(LSLEventSignature.Parse(str));
        }


        /// <summary>
        ///     Reads an event signature object from an XML fragment.
        /// </summary>
        /// <param name="reader">The XML reader containing the fragment to read.</param>
        /// <exception cref="LSLLibraryDataXmlSyntaxException">
        ///     On missing or unknown attributes.
        ///     If a parameter 'Name' is used more than once.
        ///     If a parameter 'Name' is whitespace.
        ///     If a parameter 'Type' is <see cref="LSLType.Void" />.
        ///     If a parameter 'Type' does not correspond to an <see cref="LSLType" /> enumeration member.
        ///     If a 'Properties' node 'Name' is <c>null</c> or whitespace.
        ///     If a 'Properties' node 'Name' is used more than once.
        ///     If a 'Properties' node 'Value' is <c>null</c> or whitespace.
        /// </exception>
        /// <exception cref="LSLInvalidSymbolNameException">
        ///     Thrown if the event signatures name or any of its parameters names do
        ///     not abide by LSL symbol naming conventions.
        /// </exception>
        /// <exception cref="XmlException">Incorrect XML encountered in the input stream. </exception>
        /// <exception cref="ArgumentNullException"><paramref name="reader" /> is <c>null</c>.</exception>
        /// <exception cref="LSLInvalidSubsetNameException">
        ///     Thrown if any of the given subset names in the 'Subsets' CSV string do
        ///     not match the pattern ([a-zA-Z]+[a-zA-Z_0-9\\-]*).
        /// </exception>
        /// <returns>The parsed <see cref="LSLLibraryEventSignature" /> object.</returns>
        public static LSLLibraryEventSignature FromXmlFragment(XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            var ev = new LSLLibraryEventSignature();
            IXmlSerializable x = ev;
            x.ReadXml(reader);
            return ev;
        }
    }
}