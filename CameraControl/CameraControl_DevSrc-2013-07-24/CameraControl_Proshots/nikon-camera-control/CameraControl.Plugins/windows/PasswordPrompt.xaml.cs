﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CameraControl.Plugins.windows
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PasswordPrompt// : Window
    {
        public PasswordPrompt(String prompt)
        {
            InitializeComponent();
            txtPromptMessage.Text = prompt;
        }

        public PasswordPrompt()
        {
            InitializeComponent();
        }

        public String ResponseString
        {
            get { return txtPromptResult.Password; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void on_Okay(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
