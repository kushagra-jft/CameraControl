using System;
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
    public partial class DialogPrompt// : Window
    {
        public DialogPrompt(String prompt)
        {
            InitializeComponent();
            txtPromptMessage.Text = prompt;
        }

        public DialogPrompt()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate { Jarloo.WindowExtensions.FlashWindow(this); }));
        }

        private void on_Okay(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
                DialogResult = true;
            else
                this.Close();
        }
    }
}
