#region FileInfo
// 
// File: AppSettingsNode.cs
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
using System.Linq;
using System.Reflection;
using LibLSLCC.Compilers;
using LibLSLCC.Settings;
using LSLCCEditor.EditControl;
using LSLCCEditor.Utility.Xml;

namespace LSLCCEditor.Settings
{
    /// <summary>
    /// The settings node that gets serialized by <see cref="AppSettings.Load"/>
    /// All members should have a publicly accessible constructor with no parameters, and be friendly to XmlSerializer
    /// </summary>
    public class AppSettingsNode : SettingsBaseClass<AppSettingsNode>
    {
        private XmlDictionary<string, CompilerConfigurationNode> _compilerConfigurations;
        private XmlDictionary<string, EditorControlSettingsNode> _editorControlConfigurations;
        private string _currentEditorControlConfigurationName;
        private string _currentCompilerConfigurationName;
        private XmlDictionary<string, FormatterSettingsNode> _formatterConfigurations;
        private string _currentFormatterConfigurationName;
        private XmlDictionary<string, EditorControlThemeNode> _editorControlThemes;
        private string _currentEditorControlThemeName;
        private bool _showSpaces;
        private bool _showTabs;
        private bool _showEndOfLine;

        public bool ShowSpaces
        {
            get { return _showSpaces; }
            set { SetField(ref _showSpaces,value, "ShowSpaces"); }
        }

        public bool ShowTabs
        {
            get { return _showTabs; }
            set { SetField(ref _showTabs,value, "ShowTabs"); }
        }

        public bool ShowEndOfLine
        {
            get { return _showEndOfLine; }
            set { SetField(ref _showEndOfLine,value, "ShowEndOfLine"); }
        }


        private const string ClientSideScriptCompilerHeader =
            @"//c#
/** 
*  Do not remove //c# from the first line of this script.
*
*  This is OpenSim CSharp code, CSharp scripting must be enabled on the server to run.
*
*  Please note this script does not support being reset, because a constructor was not generated.
*  Compile using the server side script option to generate a script constructor.
*
*  This code will run on an unmodified OpenSim server, however script resets will not reset global variables,
*  and OpenSim will be unable to save the state of this script as its global variables are created in an object container.
*
*/ 
";


        private const string ServerSideScriptCompilerHeader =
            @"//c#-raw
/** 
*  Do not remove //c#-raw from the first line of this script.
*
*  This is OpenSim CSharp code, CSharp scripting must be enabled on the server to run.
*
*  This is a server side script.  It constitutes a fully generated script class that
*  will be sent to the CSharp compiler in OpenSim.  This code supports script resets.
*
*  This script is meant to upload compatible with the LibLSLCC OpenSim fork.
*
*  If you are running a version of OpenSim with the LibLSLCC compiler enabled, you must add 'csraw'
*  to the allowed list of compiler languages under [XEngine] for this script to successfully upload.
*
*  Adding 'csraw' to your allowed language list when using the old OpenSim compiler will have no effect
*  besides an error being written to your log file.  OpenSim will run but you will not actually be able
*  to use the 'csraw' upload type.
*
*  Note that you can also set 'CreateClassWrapperForCSharpScripts' to 'false' under the [LibLCLCC]
*  OpenSim.ini config section in order to enable 'csraw' mode uploads for every CSharp script sent to the 
*  LibLSLCC compiler;  Including those marked with '//c#' if you have 'cs' in your list of allowed languages.
*
*/ 
";


        private class EditorControlConfigurationsDefaultFactory : IDefaultSettingsValueFactory
        {
            public bool CheckForNecessaryResets(MemberInfo member, object settingsNode, object propertyValues)
            {
                if (settingsNode == null) return true;

                var dict = (XmlDictionary<string, EditorControlSettingsNode>) propertyValues;


                if (dict == null || dict.Count == 0)
                {
                    return true;
                }

                foreach (var kvp in dict.ToList())
                {
                    if (kvp.Value != null && kvp.Value.EditorControlSettings != null)
                    {
                        DefaultValueInitializer.DoNeccessaryResets(kvp.Value);
                        continue;
                    }

                    var initNode = new EditorControlSettingsNode();
                    dict[kvp.Key] = initNode;
                    DefaultValueInitializer.Init(initNode);
                }

                return false;
            }

            public object GetDefaultValue(MemberInfo member, object settingsNode)
            {
                var d = new XmlDictionary<string, EditorControlSettingsNode>
                {
                    {
                        "Default",
                        new EditorControlSettingsNode() {EditorControlSettings = new LSLEditorControlSettings()}
                    }
                };
                return d;
            }
        }


        [DefaultValueFactory(typeof (EditorControlConfigurationsDefaultFactory), initOrder: 0)]
        public XmlDictionary<string, EditorControlSettingsNode> EditorControlConfigurations
        {
            get { return _editorControlConfigurations; }
            set { SetField(ref _editorControlConfigurations, value, "EditorControlConfigurations"); }
        }


        private class CurrentEditorConfigurationDefaultFactory : IDefaultSettingsValueFactory
        {
            public bool CheckForNecessaryResets(MemberInfo member, object settingsNode, object propertyValue)
            {
                if (propertyValue == null) return true;

                var val = propertyValue.ToString();


                var settingsNodeInstance = (AppSettingsNode) settingsNode;

                if (!settingsNodeInstance.EditorControlConfigurations.ContainsKey(val))
                {
                    return true;
                }

                if (string.IsNullOrWhiteSpace(val))
                {
                    return true;
                }

                return false;
            }

            public object GetDefaultValue(MemberInfo member, object settingsNode)
            {
                var settingsNodeInstance = (AppSettingsNode) settingsNode;

                if (settingsNodeInstance.EditorControlConfigurations.ContainsKey("Default"))
                {
                    return "Default";
                }
                return settingsNodeInstance.EditorControlConfigurations.First().Key;
            }
        }


        [DefaultValueFactory(typeof (CurrentEditorConfigurationDefaultFactory), initOrder: 1)]
        public string CurrentEditorControlConfigurationName
        {
            get { return _currentEditorControlConfigurationName; }
            set { SetField(ref _currentEditorControlConfigurationName, value, "CurrentEditorControlConfigurationName"); }
        }


        public EditorControlSettingsNode CurrentEditorControlConfiguration
        {
            get { return EditorControlConfigurations[CurrentEditorControlConfigurationName]; }
        }


        private class CompilerConfigurationsDefaultFactory : IDefaultSettingsValueFactory
        {
            public bool CheckForNecessaryResets(MemberInfo member, object settingsNode, object propertyValue)
            {
                if (propertyValue == null) return true;

                var dict = (XmlDictionary<string, CompilerConfigurationNode>) propertyValue;

                if (dict.Count == 0)
                {
                    return true;
                }

                foreach (var kvp in dict.ToList())
                {
                    if (kvp.Value != null && kvp.Value.OpenSimCompilerSettings != null)
                    {
                        DefaultValueInitializer.DoNeccessaryResets(kvp.Value);
                        continue;
                    }

                    var initNode = new CompilerConfigurationNode();
                    dict[kvp.Key] = initNode;
                    DefaultValueInitializer.Init(initNode);
                }

                return false;
            }

            public object GetDefaultValue(MemberInfo member, object settingsNode)
            {
                var d = new XmlDictionary<string, CompilerConfigurationNode>();


                var clientCode = new CompilerConfigurationNode
                {
                    OpenSimCompilerSettings = LSLOpenSimCompilerSettings.CreateOpenSimClientSide()
                };

                clientCode.OpenSimCompilerSettings.ScriptHeader = ClientSideScriptCompilerHeader;
                d.Add("OpenSim Client Code", clientCode);


                var clientCodeCoOp = new CompilerConfigurationNode
                {
                    OpenSimCompilerSettings = LSLOpenSimCompilerSettings.CreateOpenSimClientSide()
                };

                clientCodeCoOp.OpenSimCompilerSettings.ScriptHeader = ClientSideScriptCompilerHeader;
                clientCodeCoOp.OpenSimCompilerSettings.InsertCoOpTerminationCalls = true;


                d.Add("OpenSim Client Code (co-op Stop)", clientCodeCoOp);


                var serverCode = new CompilerConfigurationNode
                {
                    OpenSimCompilerSettings = LSLOpenSimCompilerSettings.CreateOpenSimServerSide()
                };

                serverCode.OpenSimCompilerSettings.ScriptHeader = ServerSideScriptCompilerHeader;
                d.Add("OpenSim Server Code", serverCode);


                var serverCodeCoOp = new CompilerConfigurationNode
                {
                    OpenSimCompilerSettings = LSLOpenSimCompilerSettings.CreateOpenSimServerSide()
                };

                serverCodeCoOp.OpenSimCompilerSettings.ScriptHeader = ServerSideScriptCompilerHeader;
                serverCodeCoOp.OpenSimCompilerSettings.InsertCoOpTerminationCalls = true;


                d.Add("OpenSim Server Code (co-op Stop)", serverCodeCoOp);


                return d;
            }
        }


        [DefaultValueFactory(typeof (CompilerConfigurationsDefaultFactory), initOrder: 2)]
        public XmlDictionary<string, CompilerConfigurationNode> CompilerConfigurations
        {
            get { return _compilerConfigurations; }
            set { SetField(ref _compilerConfigurations, value, "CompilerConfigurations"); }
        }


        private class CurrentCompilerConfigurationDefaultFactory : IDefaultSettingsValueFactory
        {
            public bool CheckForNecessaryResets(MemberInfo member, object settingsNode, object propertyValue)
            {
                if (propertyValue == null) return true;

                var val = propertyValue.ToString();

                var settingsNodeInstance = (AppSettingsNode) settingsNode;

                if (!settingsNodeInstance.CompilerConfigurations.ContainsKey(val))
                {
                    return true;
                }

                if (string.IsNullOrWhiteSpace(val))
                {
                    return true;
                }

                return false;
            }

            public object GetDefaultValue(MemberInfo member, object settingsNode)
            {
                var settingsNodeInstance = (AppSettingsNode) settingsNode;

                if (settingsNodeInstance.EditorControlConfigurations.ContainsKey("OpenSim Client Code"))
                {
                    return "OpenSim Client Code";
                }
                return settingsNodeInstance.CompilerConfigurations.First().Key;
            }
        }


        [DefaultValueFactory(typeof (CurrentCompilerConfigurationDefaultFactory), initOrder: 3)]
        public string CurrentCompilerConfigurationName
        {
            get { return _currentCompilerConfigurationName; }
            set { SetField(ref _currentCompilerConfigurationName, value, "CurrentCompilerConfigurationName"); }
        }

        public CompilerConfigurationNode CurrentCompilerConfiguration
        {
            get { return CompilerConfigurations[CurrentCompilerConfigurationName]; }
        }


        private class FormatterConfigurationsDefaultFactory : IDefaultSettingsValueFactory
        {
            public bool CheckForNecessaryResets(MemberInfo member, object objectInstance, object settingValue)
            {
                if (settingValue == null) return true;

                var dict = (XmlDictionary<string, FormatterSettingsNode>) settingValue;

                if (dict.Count == 0)
                {
                    return true;
                }

                foreach (var kvp in dict.ToList())
                {
                    if (kvp.Value != null && kvp.Value.FormatterSettings != null)
                    {
                        DefaultValueInitializer.DoNeccessaryResets(kvp.Value);
                        continue;
                    }

                    var initNode = new FormatterSettingsNode();
                    dict[kvp.Key] = initNode;
                    DefaultValueInitializer.Init(initNode);
                }

                return false;
            }

            public object GetDefaultValue(MemberInfo member, object objectInstance)
            {
                var d = new XmlDictionary<string, FormatterSettingsNode>();


                d.Add("Default", DefaultValueInitializer.Init(new FormatterSettingsNode()));

                return d;
            }
        }


        [DefaultValueFactory(typeof (FormatterConfigurationsDefaultFactory), initOrder: 4)]
        public XmlDictionary<string, FormatterSettingsNode> FormatterConfigurations
        {
            get { return _formatterConfigurations; }
            set { SetField(ref _formatterConfigurations, value, "FormatterConfigurations"); }
        }


        private class CurrentFormatterConfigurationDefaultFactory : IDefaultSettingsValueFactory
        {
            public bool CheckForNecessaryResets(MemberInfo member, object settingsNode, object propertyValue)
            {
                if (propertyValue == null) return true;

                var val = propertyValue.ToString();

                var settingsNodeInstance = (AppSettingsNode) settingsNode;

                if (!settingsNodeInstance.FormatterConfigurations.ContainsKey(val))
                {
                    return true;
                }

                if (string.IsNullOrWhiteSpace(val))
                {
                    return true;
                }

                return false;
            }

            public object GetDefaultValue(MemberInfo member, object settingsNode)
            {
                var settingsNodeInstance = (AppSettingsNode) settingsNode;

                if (settingsNodeInstance.FormatterConfigurations.ContainsKey("Default"))
                {
                    return "Default";
                }
                return settingsNodeInstance.FormatterConfigurations.First().Key;
            }
        }


        [DefaultValueFactory(typeof (CurrentFormatterConfigurationDefaultFactory), initOrder: 5)]
        public string CurrentFormatterConfigurationName
        {
            get { return _currentFormatterConfigurationName; }
            set { SetField(ref _currentFormatterConfigurationName, value, "CurrentFormatterConfigurationName"); }
        }


        public FormatterSettingsNode CurrentFormatterConfiguration
        {
            get { return FormatterConfigurations[CurrentFormatterConfigurationName]; }
        }


        private class EditorThemesDefaultFactory : IDefaultSettingsValueFactory
        {
            public bool CheckForNecessaryResets(MemberInfo member, object objectInstance, object settingValue)
            {
                if (settingValue == null) return true;

                var dict = (XmlDictionary<string, EditorControlThemeNode>) settingValue;

                if (dict.Count == 0)
                {
                    return true;
                }

                foreach (var kvp in dict.ToList())
                {
                    if (kvp.Value != null && kvp.Value.Theme != null)
                    {
                        DefaultValueInitializer.DoNeccessaryResets(kvp.Value);
                        continue;
                    }

                    var initNode = new EditorControlThemeNode();
                    dict[kvp.Key] = initNode;
                    DefaultValueInitializer.Init(initNode);
                }

                return false;
            }

            public object GetDefaultValue(MemberInfo member, object objectInstance)
            {
                var d = new XmlDictionary<string, EditorControlThemeNode>();


                d.Add("Default", DefaultValueInitializer.Init(new EditorControlThemeNode()));

                return d;
            }
        }

        [DefaultValueFactory(typeof (EditorThemesDefaultFactory), initOrder: 6)]
        public XmlDictionary<string, EditorControlThemeNode> EditorControlThemes
        {
            get { return _editorControlThemes; }
            set { SetField(ref _editorControlThemes ,value, "EditorControlThemes"); }
        }


        private class CurrentEditorControlThemeDefaultFactory : IDefaultSettingsValueFactory
        {
            public bool CheckForNecessaryResets(MemberInfo member, object settingsNode, object propertyValue)
            {
                if (propertyValue == null) return true;

                var val = propertyValue.ToString();


                var settingsNodeInstance = (AppSettingsNode)settingsNode;

                if (!settingsNodeInstance.EditorControlThemes.ContainsKey(val))
                {
                    return true;
                }

                if (string.IsNullOrWhiteSpace(val))
                {
                    return true;
                }

                return false;
            }

            public object GetDefaultValue(MemberInfo member, object settingsNode)
            {
                var settingsNodeInstance = (AppSettingsNode)settingsNode;

                if (settingsNodeInstance.EditorControlThemes.ContainsKey("Default"))
                {
                    return "Default";
                }
                return settingsNodeInstance.EditorControlThemes.First().Key;
            }
        }


        [DefaultValueFactory(typeof (CurrentEditorControlThemeDefaultFactory), initOrder: 7)]
        public string CurrentEditorControlThemeName
        {
            get { return _currentEditorControlThemeName; }
            set { SetField(ref _currentEditorControlThemeName, value, "CurrentEditorControlThemeName"); }
        }


        public EditorControlThemeNode CurrentEditorControlTheme
        {
            get { return EditorControlThemes[CurrentEditorControlThemeName]; }
        }

        public void AddCompilerConfiguration(string configurationName)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
            {
                throw new ArgumentException("Compiler configuration name cannot be null or whitespace.",
                    "configurationName");
            }

            if (CompilerConfigurations.ContainsKey(configurationName))
            {
                throw new ArgumentException(
                    string.Format("Compiler configuration named {0} already exist.", configurationName),
                    "configurationName");
            }


            CompilerConfigurations.Add(configurationName, DefaultValueInitializer.Init(new CompilerConfigurationNode()));
        }



        public void AddEditorControlTheme(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
            {
                throw new ArgumentException("Editor theme name cannot be null or whitespace.",
                    "themeName");
            }

            if (EditorControlThemes.ContainsKey(themeName))
            {
                throw new ArgumentException(
                    string.Format("Editor theme named {0} already exist.", themeName),
                    "themeName");
            }


            EditorControlThemes.Add(themeName,
                DefaultValueInitializer.Init(new EditorControlThemeNode()));
        }



        public void AddEditorControlConfiguration(string configurationName)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
            {
                throw new ArgumentException("Editor configuration name cannot be null or whitespace.",
                    "configurationName");
            }

            if (EditorControlConfigurations.ContainsKey(configurationName))
            {
                throw new ArgumentException(
                    string.Format("Editor configuration named {0} already exist.", configurationName),
                    "configurationName");
            }


            EditorControlConfigurations.Add(configurationName,
                DefaultValueInitializer.Init(new EditorControlSettingsNode()));
        }

        public void AddFormatterConfiguration(string configurationName)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
            {
                throw new ArgumentException("Formatter configuration name cannot be null or whitespace.",
                    "configurationName");
            }

            if (FormatterConfigurations.ContainsKey(configurationName))
            {
                throw new ArgumentException(
                    string.Format("Formatter configuration named {0} already exist.", configurationName),
                    "configurationName");
            }


            FormatterConfigurations.Add(configurationName,
                DefaultValueInitializer.Init(new FormatterSettingsNode()));
        }



        public void RenameEditorControlTheme(string themeName, string newName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
            {
                throw new ArgumentException("Editor theme name cannot be null or whitespace.",
                    "themeName");
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Editor theme name cannot be null or whitespace.",
                    "newName");
            }


            if (!EditorControlThemes.ContainsKey(themeName))
            {
                throw new ArgumentException(
                    string.Format("Editor theme named {0} does not exist.", themeName),
                    "themeName");
            }

            bool resetCurrentlySelected = AppSettings.Settings.CurrentEditorControlThemeName == themeName;

            var old = EditorControlThemes[themeName];
            EditorControlThemes.Remove(themeName);
            EditorControlThemes.Add(newName, old);

            if (resetCurrentlySelected)
            {
                AppSettings.Settings.CurrentEditorControlThemeName = newName;
            }
        }



        public void RenameCompilerConfiguration(string configurationName, string newName)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
            {
                throw new ArgumentException("Compiler configuration name cannot be null or whitespace.",
                    "configurationName");
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Compiler configuration name cannot be null or whitespace.",
                    "newName");
            }


            if (!CompilerConfigurations.ContainsKey(configurationName))
            {
                throw new ArgumentException(
                    string.Format("Compiler configuration named {0} does not exist.", configurationName),
                    "configurationName");
            }

            bool resetCurrentlySelected = AppSettings.Settings.CurrentCompilerConfigurationName == configurationName;

            var old = CompilerConfigurations[configurationName];
            CompilerConfigurations.Remove(configurationName);
            CompilerConfigurations.Add(newName, old);

            if (resetCurrentlySelected)
            {
                AppSettings.Settings.CurrentCompilerConfigurationName = newName;
            }
        }



        public void RemoveCompilerConfiguration(string configurationName, string newCurrentConfiguration)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
            {
                throw new ArgumentException("Compiler configuration name cannot be null or whitespace.",
                    "configurationName");
            }

            if (string.IsNullOrWhiteSpace(newCurrentConfiguration))
            {
                throw new ArgumentException("Compiler configuration name cannot be null or whitespace.",
                    "newCurrentConfiguration");
            }


            if (!CompilerConfigurations.ContainsKey(configurationName))
            {
                throw new ArgumentException(
                    string.Format("Compiler configuration named {0} does not exist.", configurationName),
                    "configurationName");
            }

            if (!CompilerConfigurations.ContainsKey(newCurrentConfiguration))
            {
                throw new ArgumentException(
                    string.Format("Compiler configuration named {0} does not exist.", newCurrentConfiguration),
                    "newCurrentConfiguration");
            }

            if (CompilerConfigurations.Count == 1)
            {
                throw new InvalidOperationException(
                    "There must be at least one compiler configuration present in the " +
                    "application settings, cannot remove the configuration as it is the only one present.");
            }


            CompilerConfigurations.Remove(configurationName);

            CurrentCompilerConfigurationName = newCurrentConfiguration;
        }




        public void RemoveFormatterConfiguration(string configurationName, string newCurrentConfiguration)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
            {
                throw new ArgumentException("Formatter configuration name cannot be null or whitespace.",
                    "configurationName");
            }

            if (string.IsNullOrWhiteSpace(newCurrentConfiguration))
            {
                throw new ArgumentException("Formatter configuration name cannot be null or whitespace.",
                    "newCurrentConfiguration");
            }


            if (!FormatterConfigurations.ContainsKey(configurationName))
            {
                throw new ArgumentException(
                    string.Format("Formatter configuration named {0} does not exist.", configurationName),
                    "configurationName");
            }

            if (!FormatterConfigurations.ContainsKey(newCurrentConfiguration))
            {
                throw new ArgumentException(
                    string.Format("Formatter configuration named {0} does not exist.", newCurrentConfiguration),
                    "newCurrentConfiguration");
            }

            if (FormatterConfigurations.Count == 1)
            {
                throw new InvalidOperationException(
                    "There must be at least one formatter configuration present in the " +
                    "application settings, cannot remove the configuration as it is the only one present.");
            }


            FormatterConfigurations.Remove(configurationName);

            CurrentFormatterConfigurationName = newCurrentConfiguration;
        }


        public void RenameFormatterConfiguration(string configurationName, string newName)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
            {
                throw new ArgumentException("Formatter configuration name cannot be null or whitespace.",
                    "configurationName");
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Formatter configuration name cannot be null or whitespace.",
                    "newName");
            }


            if (!FormatterConfigurations.ContainsKey(configurationName))
            {
                throw new ArgumentException(
                    string.Format("Formatter configuration named {0} does not exist.", configurationName),
                    "configurationName");
            }

            bool resetCurrentlySelected = AppSettings.Settings.CurrentFormatterConfigurationName == configurationName;

            var old = FormatterConfigurations[configurationName];
            FormatterConfigurations.Remove(configurationName);
            FormatterConfigurations.Add(newName, old);

            if (resetCurrentlySelected)
            {
                AppSettings.Settings.CurrentFormatterConfigurationName = newName;
            }
        }



        public void RenameEditorControlConfiguration(string configurationName, string newName)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
            {
                throw new ArgumentException("Editor configuration name cannot be null or whitespace.",
                    "configurationName");
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Editor configuration name cannot be null or whitespace.",
                    "newName");
            }


            if (!EditorControlConfigurations.ContainsKey(configurationName))
            {
                throw new ArgumentException(
                    string.Format("Editor configuration named {0} does not exist.", configurationName),
                    "configurationName");
            }

            bool resetCurrentlySelected = AppSettings.Settings.CurrentCompilerConfigurationName == configurationName;

            var old = EditorControlConfigurations[configurationName];
            EditorControlConfigurations.Remove(configurationName);
            EditorControlConfigurations.Add(newName, old);

            if (resetCurrentlySelected)
            {
                AppSettings.Settings.CurrentEditorControlConfigurationName = newName;
            }
        }



        public void RemoveEditorControlConfiguration(string configurationName, string newCurrentConfiguration)
        {
            if (string.IsNullOrWhiteSpace(configurationName))
            {
                throw new ArgumentException("Editor configuration name cannot be null or whitespace.",
                    "configurationName");
            }

            if (string.IsNullOrWhiteSpace(newCurrentConfiguration))
            {
                throw new ArgumentException("Editor configuration name cannot be null or whitespace.",
                    "newCurrentConfiguration");
            }


            if (!EditorControlConfigurations.ContainsKey(configurationName))
            {
                throw new ArgumentException(
                    string.Format("Editor configuration named {0} does not exist.", configurationName),
                    "configurationName");
            }

            if (!EditorControlConfigurations.ContainsKey(newCurrentConfiguration))
            {
                throw new ArgumentException(
                    string.Format("Editor configuration named {0} does not exist.", newCurrentConfiguration),
                    "newCurrentConfiguration");
            }

            if (EditorControlConfigurations.Count == 1)
            {
                throw new InvalidOperationException(
                    "There must be at least one editor configuration present in the " +
                    "application settings, cannot remove the configuration as it is the only one present.");
            }


            EditorControlConfigurations.Remove(configurationName);

            CurrentEditorControlConfigurationName = newCurrentConfiguration;
        }




        public void RemoveEditorControlTheme(string themeName, string newCurrentThemeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
            {
                throw new ArgumentException("Editor theme name cannot be null or whitespace.",
                    "themeName");
            }

            if (string.IsNullOrWhiteSpace(newCurrentThemeName))
            {
                throw new ArgumentException("Editor theme name cannot be null or whitespace.",
                    "newCurrentThemeName");
            }


            if (!EditorControlThemes.ContainsKey(themeName))
            {
                throw new ArgumentException(
                    string.Format("Editor configuration named {0} does not exist.", themeName),
                    "themeName");
            }

            if (!EditorControlThemes.ContainsKey(newCurrentThemeName))
            {
                throw new ArgumentException(
                    string.Format("Editor theme named {0} does not exist.", newCurrentThemeName),
                    "newCurrentThemeName");
            }

            if (EditorControlThemes.Count == 1)
            {
                throw new InvalidOperationException(
                    "There must be at least one editor theme present in the " +
                    "application settings, cannot remove the theme as it is the only one present.");
            }


            EditorControlThemes.Remove(themeName);

            CurrentEditorControlThemeName = newCurrentThemeName;
        }
    }
}