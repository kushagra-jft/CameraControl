﻿#pragma checksum "..\..\..\..\windows\ScriptWnd.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F31BB7D1BD0950E1585B01DFAD118C2C"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1008
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace CameraControl.windows {
    
    
    /// <summary>
    /// ScriptWnd
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
    public partial class ScriptWnd : MahApps.Metro.Controls.MetroWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 29 "..\..\..\..\windows\ScriptWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Menu menu2;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\..\windows\ScriptWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem mnu_new;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\..\windows\ScriptWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem mnu_save;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\..\windows\ScriptWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem mnu_save_as;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\..\windows\ScriptWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem mnu_verify;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\..\windows\ScriptWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem mnu_run;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\windows\ScriptWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem mnu_stop;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\..\windows\ScriptWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal ICSharpCode.AvalonEdit.TextEditor textEditor;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\windows\ScriptWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox lst_output;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\..\..\windows\ScriptWnd.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.GridSplitter gridSplitter1;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/CameraControl;component/windows/scriptwnd.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\windows\ScriptWnd.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 5 "..\..\..\..\windows\ScriptWnd.xaml"
            ((CameraControl.windows.ScriptWnd)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.MetroWindow_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.menu2 = ((System.Windows.Controls.Menu)(target));
            return;
            case 3:
            this.mnu_new = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 4:
            
            #line 32 "..\..\..\..\windows\ScriptWnd.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItem_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.mnu_save = ((System.Windows.Controls.MenuItem)(target));
            
            #line 33 "..\..\..\..\windows\ScriptWnd.xaml"
            this.mnu_save.Click += new System.Windows.RoutedEventHandler(this.mnu_save_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.mnu_save_as = ((System.Windows.Controls.MenuItem)(target));
            
            #line 34 "..\..\..\..\windows\ScriptWnd.xaml"
            this.mnu_save_as.Click += new System.Windows.RoutedEventHandler(this.mnu_save_as_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.mnu_verify = ((System.Windows.Controls.MenuItem)(target));
            
            #line 36 "..\..\..\..\windows\ScriptWnd.xaml"
            this.mnu_verify.Click += new System.Windows.RoutedEventHandler(this.mnu_verify_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.mnu_run = ((System.Windows.Controls.MenuItem)(target));
            
            #line 37 "..\..\..\..\windows\ScriptWnd.xaml"
            this.mnu_run.Click += new System.Windows.RoutedEventHandler(this.mnu_run_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.mnu_stop = ((System.Windows.Controls.MenuItem)(target));
            
            #line 38 "..\..\..\..\windows\ScriptWnd.xaml"
            this.mnu_stop.Click += new System.Windows.RoutedEventHandler(this.mnu_stop_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.textEditor = ((ICSharpCode.AvalonEdit.TextEditor)(target));
            return;
            case 11:
            this.lst_output = ((System.Windows.Controls.ListBox)(target));
            return;
            case 12:
            this.gridSplitter1 = ((System.Windows.Controls.GridSplitter)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

