﻿#region FileInfo
// 
// File: SettingsWindow.xaml.cs
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
using System.Collections.ObjectModel;
using System.Windows;
using LSLCCEditor.Settings;
using LSLCCEditor.Styles;

namespace LSLCCEditor.SettingsUI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly ObservableCollection<ISettingsPane> _settingPanes = new ObservableCollection<ISettingsPane>();

        public SettingsWindow()
        {

            InitializeComponent();

            MetroWindowStyleInit.Init(this);

            var compilerPane = new CompilerPane {OwnerSettingsWindow = this};
            _settingPanes.Add(compilerPane);

            var formatterPane = new FormatterPane { OwnerSettingsWindow = this };
            _settingPanes.Add(formatterPane);


            var editorPane = new EditorThemePane { OwnerSettingsWindow = this };
            _settingPanes.Add(editorPane);



            SettingsPagesList.SelectedItem = compilerPane; 
        }

        public ObservableCollection<ISettingsPane> SettingPanes
        {
            get { return _settingPanes; }
        }


        private void SettingsWindow_OnClosed(object sender, EventArgs e)
        {
            AppSettings.Save();
        }
    }
}
