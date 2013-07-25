using System;
using System.Collections.Generic;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PhotoBooth
{
    /// <summary>
    /// Interaction logic for PhotoBoothControlWindow.xaml
    /// </summary>
    public partial class PhotoBoothControlWindow : Window
    {
        private PhotoBoothCamera camera;
        private PrintTicket printerSetupTicket;
        private bool initializing = false;
        
        public string SaveFileFolder
        {
            get { return (string)GetValue(SaveFileFolderProperty); }
            set { SetValue(SaveFileFolderProperty, value); }
        }

        public bool KioskMode
        {
            get { return (bool)GetValue(KioskModeProperty); }
            set { SetValue(KioskModeProperty, value); }
        }

        public bool OneButtonOperation
        {
            get { return (bool)GetValue(OneButtonOperationProperty); }
            set { SetValue(OneButtonOperationProperty, value); }
        }

        public string CardBannerText
        {
            get { return (string)GetValue(CardBannerTextProperty); }
            set { SetValue(CardBannerTextProperty, value); }
        }

        public string CardBottomVerticalText
        {
            get { return (string)GetValue(CardBottomVerticalTextProperty); }
            set { SetValue(CardBottomVerticalTextProperty, value); }
        }

        public string CardTopVerticalText
        {
            get { return (string)GetValue(CardTopVerticalTextProperty); }
            set { SetValue(CardTopVerticalTextProperty, value); }
        }

        public PhotoBoothControlWindow()
        {
            if (Properties.Settings.Default.PrintTicket != null)
            {
                this.printerSetupTicket = Properties.Settings.Default.PrintTicket;
            }

            this.SaveFileFolder = Properties.Settings.Default.SaveFileFolder;
            this.CardBannerText = Properties.Settings.Default.CardBannerText;
            this.CardTopVerticalText = Properties.Settings.Default.CardTopVerticalText;
            this.CardBottomVerticalText = Properties.Settings.Default.CardBottomVerticalText;
            this.OneButtonOperation = Properties.Settings.Default.OneButtonOperation;
            this.KioskMode = Properties.Settings.Default.KioskMode;

            InitializeComponent();
            this.DataContext = this;

            this.LoadPreviousImages();
        }

        private void LoadPreviousImages()
        {
            List<string> imageFiles = new List<string>();
            AddToImageList(imageFiles, Properties.Settings.Default.ImagePath1);
            AddToImageList(imageFiles, Properties.Settings.Default.ImagePath2);
            AddToImageList(imageFiles, Properties.Settings.Default.ImagePath3);
            AddToImageList(imageFiles, Properties.Settings.Default.ImagePath4);

            if (imageFiles.Count > 0)
            {
                this.UpdateImageDisplay(imageFiles);
            }
        }

        private static void AddToImageList(List<string> imageFiles, string filename)
        {
            if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                imageFiles.Add(filename);
            }
        }

        private void ShowPhotoBoothWindow()
        {
            PhotoBoothWindow window = new PhotoBoothWindow()
            {
                BottomVerticalText = this.CardBottomVerticalText,
                TopVerticalText = this.CardTopVerticalText,
                BannerText = this.CardBannerText,
                Camera = this.camera,
                PrinterSetupTicket = this.printerSetupTicket,
                OneButtonOperation = this.OneButtonOperation
            };

            if(this.KioskMode)
            {
                window.WindowState = System.Windows.WindowState.Maximized;
                window.WindowStyle = System.Windows.WindowStyle.None;
                window.Topmost = true;
            }

            if (!string.IsNullOrEmpty(this.SaveFileFolder))
            {
                DirectoryInfo saveFolderInfo = new DirectoryInfo(this.SaveFileFolder);
                if (saveFolderInfo.Exists)
                {
                    window.SaveCards = true;
                    window.SaveFileLocation = saveFolderInfo;
                }
            }

            window.Image1File = Properties.Settings.Default.ImagePath1;
            window.Image2File = Properties.Settings.Default.ImagePath2;
            window.Image3File = Properties.Settings.Default.ImagePath3;
            window.Image4File = Properties.Settings.Default.ImagePath4;
            
            window.ShowDialog();

            if (window.ClosedAbnormally)
            {
                //MessageBox.Show("Photobooth closed abnormally");
                this.CloseCamera();
            }
        }

        private void PhotoBooth_Executed(object target, ExecutedRoutedEventArgs e)
        {
            this.ShowPhotoBoothWindow();
        }

        private void PhotoBooth_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.initializing && this.camera != null && this.camera.CameraReady;
        }

        private void PrinterSetup_Executed(object target, ExecutedRoutedEventArgs e)
        {
            PrintDialog dlg = new PrintDialog();
            if (this.printerSetupTicket != null)
            {
                dlg.PrintTicket = this.printerSetupTicket;
            }
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                this.printerSetupTicket = dlg.PrintTicket;
                Properties.Settings.Default.PrintTicket = this.printerSetupTicket;
            }
        }

        private void PrinterSetup_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.initializing;
        }

        private void InitializeCamera_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.initializing;
        }

        private void InitializeCamera_Executed(object target, ExecutedRoutedEventArgs e)
        {
            this.InitializeCamera();
        }

        private void ShowCardView_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !this.initializing;
        }

        private void ShowCardView_Executed(object target, ExecutedRoutedEventArgs e)
        {
            this.ShowCardView();
        }

        private void InitializeCamera()
        {
            try
            {
                this.initializing = true;
                CommandManager.InvalidateRequerySuggested();
                this.CloseCamera();

                this.camera = new PhotoBoothCamera();
                if (this.camera.Initialize())
                {
                    this.cameraSettingsGrid.DataContext = this.camera.Camera;
                }
                else
                {
                    MessageBox.Show(this.camera.InitializationLog);
                    this.CloseCamera();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Caught an exception during camera initialization: " + ex.Message);
            }
            finally
            {
                this.initializing = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void UpdateImageDisplay(List<string> imageFilenames)
        {
            int count = 0;

            foreach (string filename in imageFilenames)
            {
                if (!File.Exists(filename))
                {
                    continue;
                }

                Uri imageLocation = new Uri(filename, UriKind.Absolute);
                BitmapImage image = new BitmapImage(imageLocation);

                switch (count)
                {
                    case 0:
                        this.image1.Source = image;
                        this.image1.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case 1:
                        this.image2.Source = image;
                        this.image2.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case 2:
                        this.image3.Source = image;
                        this.image3.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case 3:
                        this.image4.Source = image;
                        this.image4.Visibility = System.Windows.Visibility.Visible;
                        break;
                    default:
                        break;
                }
                if (count >= 3)
                {
                    break;
                }
                count++;
            }
        }

        private void CloseCamera()
        {
            if (this.camera != null)
            {
                this.camera.Dispose();
                this.camera = null;
            }
        }

        private void ShowCardView()
        {
            PhotoCardInformation info = new PhotoCardInformation()
            {
                TopLeftImage = this.image1.Source,
                TopRightImage = this.image2.Source,
                BottomLeftImage = this.image3.Source,
                BottomRightImage = this.image4.Source,
                BottomVerticalText = this.CardBottomVerticalText,
                TopVerticalText = this.CardTopVerticalText,
                BannerText = this.CardBannerText
            };

            CardView cardView = new CardView();
            cardView.SizeToContent = SizeToContent.Manual;
            cardView.DataContext = info;
            cardView.ShowDialog();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.CardBannerText = this.CardBannerText;
            Properties.Settings.Default.CardBottomVerticalText = this.CardBottomVerticalText;
            Properties.Settings.Default.CardTopVerticalText = this.CardTopVerticalText;
            Properties.Settings.Default.SaveFileFolder = this.SaveFileFolder;
            Properties.Settings.Default.OneButtonOperation  = this.OneButtonOperation;
            Properties.Settings.Default.KioskMode = this.KioskMode;

            Properties.Settings.Default.Save();

            base.OnClosing(e);
            this.CloseCamera();
        }

        private void saveFileBrowsePB_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            
            if (!string.IsNullOrEmpty(this.SaveFileFolder))
            {
                dlg.SelectedPath = this.SaveFileFolder;
            }

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.SaveFileFolder = dlg.SelectedPath;
            }
        }

        // Using a DependencyProperty as the backing store for OneButtonOperation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OneButtonOperationProperty =
            DependencyProperty.Register("OneButtonOperation", typeof(bool), typeof(PhotoBoothControlWindow), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for KioskMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KioskModeProperty =
            DependencyProperty.Register("KioskMode", typeof(bool), typeof(PhotoBoothControlWindow), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for SaveFileFolder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SaveFileFolderProperty =
            DependencyProperty.Register("SaveFileFolder", typeof(string), typeof(PhotoBoothControlWindow), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for CardBottomVerticalText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardBottomVerticalTextProperty =
            DependencyProperty.Register("CardBottomVerticalText", typeof(string), typeof(PhotoBoothControlWindow), new PropertyMetadata("Photo Booth"));

        // Using a DependencyProperty as the backing store for CardTopVerticalText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardTopVerticalTextProperty =
            DependencyProperty.Register("CardTopVerticalText", typeof(string), typeof(PhotoBoothControlWindow), new PropertyMetadata("Photo Booth"));

        // Using a DependencyProperty as the backing store for CardBannerText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CardBannerTextProperty =
            DependencyProperty.Register("CardBannerText", typeof(string), typeof(PhotoBoothControlWindow), new PropertyMetadata("Congrats!"));
    }
}
