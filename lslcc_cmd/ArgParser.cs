#region FileInfo
// 
// File: ArgParser.cs
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
using System.Collections.Generic;
using System.IO;

namespace lslcc
{
    internal sealed class ArgParser
    {
        private string _helpDescriptionIndent = "    ";
        private string _helpDescriptionSeperator = Environment.NewLine+"======================================"+Environment.NewLine;
        private Dictionary<string, ArgSwitchDesc> Switches { get; set; }  

        public ArgParser()
        {
            Switches = new Dictionary<string, ArgSwitchDesc>();
        }


        public ArgSwitchDesc AddSwitch(ArgSwitchDesc desc)
        {
            Switches.Add(desc.Name, desc);
            return desc;
        }


        public IEnumerable<ArgSwitch> ParseArgs(string[] args)
        {
            var current = new ArgSwitch();

            bool inProgramArgs = true;

            var used = new HashSet<string>();

            int index = 0;
            int end = args.Length - 1;
            foreach (var argument in args)
            {
                ArgSwitchDesc desc;

                if (Switches.TryGetValue(argument, out desc))
                {
                    if (!inProgramArgs)
                    {
                        var last = Switches[current.Name];

                        if (last.MustBeUsedAlone)
                        {
                            throw new ArgParseException(
                                string.Format("Option '{0}' cannot be used with other arguments.",
                                    desc.Name));
                        }

                        if (current.Arguments.Count < last.MinArgs)
                        {
                            throw new ArgParseException(string.Format("Option '{0}' requires at least {1} argument{2}.",
                                last.Name, last.MinArgs, last.MinArgs > 1 ? "s" : ""));
                        }

                    }

                    if (used.Overlaps(desc.CantBeUsedWith))
                    {
                        throw new ArgParseException(string.Format("Option '{0}' cannot be used with: '{1}'.", desc.Name,
                            string.Join(" or ", desc.CantBeUsedWith)));
                    }

                    if (desc.MustAppearOnlyOnce && used.Contains(desc.Name))
                    {
                        throw new ArgParseException(string.Format("Option '{0}' may only be used once.", desc.Name));
                    }


                    used.Add(argument);

                    yield return current;

                    inProgramArgs = false;
                    current = new ArgSwitch(argument);

                    if (index == end)
                    {

                        if (desc.MinArgs > 0)
                        {
                            throw new ArgParseException(
                                string.Format("Option '{0}' requires at least {1} argument{2}.",
                                    desc.Name, desc.MinArgs, desc.MinArgs > 1 ? "s" : ""));
                        }

                        yield return current;
                    }
                }
                else if(index == end)
                {
                    if (!inProgramArgs)
                    {
                        var last = Switches[current.Name];

                        if (last.MaxArgs == 0)
                        {
                            throw new ArgParseException(string.Format("Option '{0}' does not allow for arguments.",
                                last.Name));
                        }

                        if (last.MaxArgs != -1 && current.Arguments.Count == last.MaxArgs)
                        {
                            throw new ArgParseException(string.Format("Option '{0}' only allows {1} argument{2}.",
                                last.Name, last.MaxArgs, last.MaxArgs > 1 ? "s" : ""));
                        }

                        current.Arguments.Add(argument);
                    }
                    else
                    {

                        current.Arguments.Add(argument);
                    }

                    yield return current;
                }
                else
                {
                    if (!inProgramArgs)
                    {
                        var last = Switches[current.Name];

                        if (last.MaxArgs == 0)
                        {
                            throw new ArgParseException(string.Format("Option '{0}' does not allow for arguments.",
                                last.Name));
                        }

                        if (last.MaxArgs != -1 && current.Arguments.Count == last.MaxArgs)
                        {
                            throw new ArgParseException(string.Format("Option '{0}' only allows {1} argument{2}.",
                                last.Name, last.MaxArgs, last.MaxArgs > 1 ? "s" : ""));
                        }
                    }

                    current.Arguments.Add(argument);
                }
                index++;
            }
        }


        public string HelpDescriptionIndent
        {
            get { return _helpDescriptionIndent; }
            set
            {
                _helpDescriptionIndent = value;
            }
        }

        public string HelpDescriptionSeperator
        {
            get { return _helpDescriptionSeperator; }
            set { _helpDescriptionSeperator = value; }
        }


        public void WriteHelp(TextWriter outWriter)
        {
            foreach (var sw in Switches)
            {
                var e = new ArgSwitchHelpWriteEvent(outWriter);
                if (!string.IsNullOrWhiteSpace(sw.Value.HelpLine))
                {
                    sw.Value.OnWriteBeforeShortHelp(e);
                    outWriter.WriteLine(sw.Key + ": " + sw.Value.HelpLine);
                    sw.Value.OnWriteAfterShortHelp(e);
                }
                else
                {
                    sw.Value.OnWriteBeforeShortHelp(e);
                    outWriter.WriteLine(sw.Key + ":");
                    sw.Value.OnWriteAfterShortHelp(e);
                }

                bool longHelp = sw.Value.DescriptionLines.Count > 0;

                if (longHelp)
                {
                    outWriter.WriteLine();
                    sw.Value.OnWriteBeforeDescription(e);
                   
                }

                foreach (var desc in sw.Value.DescriptionLines)
                {
                    if (string.IsNullOrWhiteSpace(desc))
                    {
                        outWriter.WriteLine();
                        continue;
                    }

                    string indent = HelpDescriptionIndent ?? "";

                    outWriter.WriteLine(indent + desc);
                }

                if (!longHelp) continue;

                sw.Value.OnWriteAfterDescription(e);

                if (!string.IsNullOrWhiteSpace(HelpDescriptionSeperator))
                {
                    outWriter.WriteLine(HelpDescriptionSeperator);
                }
            }
        }
    }
}