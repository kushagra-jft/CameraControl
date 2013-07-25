using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
//using System.Threading;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Vision.Motion;
using CameraControl.Classes;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Core.Translation;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Timer = System.Timers.Timer;

namespace CameraControl.windows
{
    /// <summary>
    /// Interaction logic for LiveViewWnd.xaml
    /// </summary>
    public partial class LiveViewSimpleWnd : IWindow, INotifyPropertyChanged
    {
        private const int DesiredFrameRate = 20;

        private int _retries = 0;
        private ICameraDevice selectedPortableDevice;
        //private Rectangle _focusrect = new Rectangle();
        private BackgroundWorker _worker = new BackgroundWorker();
        private int _totalframes = 0;
        private DateTime _framestart;
        private MotionDetector _detector;
        private DateTime _photoCapturedTime;
        

        public LiveViewData LiveViewData { get; set; }

        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                NotifyPropertyChanged("IsBusy");
                NotifyPropertyChanged("IsFree");
            }
        }

        private Timer _timer = new Timer(1000 / DesiredFrameRate);
        private Timer _freezeTimer = new Timer();

        private bool oper_in_progress = false;

        public ICameraDevice SelectedPortableDevice
        {
            get { return this.selectedPortableDevice; }
            set
            {
                if (this.selectedPortableDevice != value)
                {
                    this.selectedPortableDevice = value;
                    NotifyPropertyChanged("SelectedPortableDevice");
                }
            }
        }

        public LiveViewSimpleWnd()
        {
            SelectedPortableDevice = ServiceProvider.DeviceManager.SelectedCameraDevice;
            Init();
        }

        private void SelectedBitmap_BitmapLoaded(object sender)
        {
            if (ServiceProvider.Settings.PreviewLiveViewImage && IsVisible)
            {
                Dispatcher.Invoke(new Action(delegate
                                               {
                                                   ServiceProvider.Settings.SelectedBitmap.DisplayImage.Freeze();
                                                   image1.Source = ServiceProvider.Settings.SelectedBitmap.DisplayImage;
                                               }));
            }
        }

        public LiveViewSimpleWnd(ICameraDevice device)
        {
            SelectedPortableDevice = device;
            Init();
        }

        public void Init()
        {
            InitializeComponent();
            //ThemeManager.ChangeTheme(Application.Current, ThemeManager.DefaultAccents.First(a => a.Name == "Blue"), Theme.Dark);
            _timer.Stop();
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            //_focusrect.Stroke = new SolidColorBrush(Colors.Green);
            //canvas.Children.Add(_focusrect);
            _worker.DoWork += delegate
                                {
                                        GetLiveImage();
                                };
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_retries > 100)
            {
                _timer.Stop();

                Dispatcher.Invoke(new ThreadStart(delegate
                                                    {
                                                        image1.Visibility = Visibility.Hidden;
                                                        //chk_grid.IsChecked = false;
                                                    }));
                return;
            }
            if (!_worker.IsBusy)
                _worker.RunWorkerAsync();
        }

        private void GetLiveImage()
        {
            if (oper_in_progress)
                return;
            oper_in_progress = true;
            _totalframes++;
            try
            {
                LiveViewData = LiveViewManager.GetLiveViewImage(SelectedPortableDevice);
            }
            catch (Exception)
            {
                _retries++;
                oper_in_progress = false;
                return;
            }

            if (LiveViewData == null || LiveViewData.ImageData == null)
            {
                _retries++;
                oper_in_progress = false;
                return;
            }

            Dispatcher.Invoke(new Action(delegate
                                           {
                                               try
                                               {
                                                   WriteableBitmap preview;
                                                   if (LiveViewData != null && LiveViewData.ImageData != null)
                                                   {

                                                       MemoryStream stream = new MemoryStream(LiveViewData.ImageData,
                                                                                              LiveViewData.ImagePosition,
                                                                                              LiveViewData.ImageData.Length -
                                                                                              LiveViewData.ImagePosition);

                                                       using (var bmp = new Bitmap(stream))
                                                       {
                                                           
                                                           preview =
                                                               BitmapFactory.ConvertToPbgra32Format(
                                                                   BitmapSourceConvert.ToBitmapSource(bmp));

                                                           Bitmap newbmp = bmp;
                                                           
                                                           WriteableBitmap writeableBitmap;

                                                           writeableBitmap = 
                                                               BitmapFactory.ConvertToPbgra32Format(
                                                                   BitmapSourceConvert.ToBitmapSource(newbmp));
                                                           
                                                           //DrawGrid(writeableBitmap);
                                                           writeableBitmap.Freeze();
                                                           image1.BeginInit();
                                                           image1.Source = writeableBitmap;
                                                           image1.EndInit();
                                                       }
                                                       stream.Close();
                                                   }
                                               }
                                               catch (Exception exception)
                                               {
                                                   Log.Error(exception);
                                                   _retries++;
                                                   oper_in_progress = false;
                                               }

                                           }));
            _retries = 0;
            oper_in_progress = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //SelectedPortableDevice.StoptLiveView();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("LiveView: Capture started");
            _timer.Stop();
            Thread.Sleep(300);
            try
            {
                //selectedPortableDevice.StopLiveView();
                SelectedPortableDevice.CapturePhotoNoAf();
                Log.Debug("LiveView: Capture Initialization Done");
            }
            catch (DeviceException exception)
            {
                StaticHelper.Instance.SystemMessage = exception.Message;
                Log.Error("Unable to take picture with no af", exception);
            }
            //_timer.Start();
        }

        private void StartLiveView()
        {
            if (!IsVisible)
                return;
            string resp = SelectedPortableDevice.GetProhibitionCondition(OperationEnum.LiveView);
            if (string.IsNullOrEmpty(resp))
            {
                Thread thread = new Thread(StartLiveViewThread);
                thread.Start();
                thread.Join();
            }
            else
            {
                Log.Error("Error starting live view " + resp);
                MessageBox.Show(TranslationStrings.LabelLiveViewError + "\n" + TranslationManager.GetTranslation(resp));
                return;
            }
        }

        private void StartLiveViewThread()
        {
            try
            {
                _totalframes = 0;
                _framestart = DateTime.Now;
                bool retry = false;
                int retryNum = 0;
                Log.Debug("LiveView: Liveview started");
                do
                {
                    try
                    {
                        LiveViewManager.StartLiveView(SelectedPortableDevice);
                    }
                    catch (DeviceException deviceException)
                    {
                        Dispatcher.Invoke(new Action(delegate { grid_wait.Visibility = Visibility.Visible; }));
                        if (deviceException.ErrorCode == ErrorCodes.ERROR_BUSY ||
                            deviceException.ErrorCode == ErrorCodes.MTP_Device_Busy)
                        {
                            Thread.Sleep(200);
                            if (!IsVisible)
                                break;
                            Log.Debug("Retry live view :" + deviceException.ErrorCode.ToString("X"));
                            retry = true;
                            retryNum++;
                        }
                        else
                        {
                            throw;
                        }
                    }

                } while (retry && retryNum < 35);
                if (IsVisible)
                {
                    _timer.Start();
                    oper_in_progress = false;
                    _retries = 0;
                    Log.Debug("LiveView: Liveview start done");
                }
            }
            catch (Exception exception)
            {
                Log.Error("Unable to start liveview !", exception);
                StaticHelper.Instance.SystemMessage = "Unable to start liveview ! " + exception.Message;
                //MessageBox.Show("Unable to start liveview !");
                //ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.LiveViewWnd_Hide);
            }
            Dispatcher.BeginInvoke(new Action(delegate { grid_wait.Visibility = Visibility.Hidden; }));
            Dispatcher.BeginInvoke(new Action(delegate { image1.Visibility = Visibility.Visible; }));
        }

        private void StopLiveView()
        {
            Thread thread = new Thread(StopLiveViewThread);
            thread.Start();
        }

        private void StopLiveViewThread()
        {
            try
            {
                _totalframes = 0;
                _framestart = DateTime.Now;
                bool retry = false;
                int retryNum = 0;
                Log.Debug("LiveView: Liveview stopping");
                do
                {
                    try
                    {
                        LiveViewManager.StopLiveView(SelectedPortableDevice);
                    }
                    catch (DeviceException deviceException)
                    {
                        Dispatcher.Invoke(new Action(delegate { grid_wait.Visibility = Visibility.Visible; }));
                        if (deviceException.ErrorCode == ErrorCodes.ERROR_BUSY ||
                            deviceException.ErrorCode == ErrorCodes.MTP_Device_Busy)
                        {
                            Thread.Sleep(500);
                            Log.Debug("Retry live view stop:" + deviceException.ErrorCode.ToString("X"));
                            retry = true;
                            retryNum++;
                        }
                        else
                        {
                            throw;
                        }
                    }

                } while (retry && retryNum < 35);
            }
            catch (Exception exception)
            {
                Log.Error("Unable to stop liveview !", exception);
                StaticHelper.Instance.SystemMessage = "Unable to stop liveview ! " + exception.Message;
            }
            Dispatcher.Invoke(new Action(delegate { grid_wait.Visibility = Visibility.Hidden; }));
            Dispatcher.Invoke(new Action(delegate { image1.Visibility = Visibility.Hidden; }));
        }

        private void image1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left && LiveViewData != null &&
                LiveViewData.HaveFocusData && selectedPortableDevice.LiveViewImageZoomRatio.Value == "All")
            {
                try
                {
                    Point initialPoint = e.MouseDevice.GetPosition(image1);
                    double xt = LiveViewData.ImageWidth / image1.ActualWidth;
                    double yt = LiveViewData.ImageHeight / image1.ActualHeight;
                    int posx = (int)(initialPoint.X * xt);
                    int posy = (int)(initialPoint.Y * yt);
                    selectedPortableDevice.Focus(posx, posy);
                }
                catch (Exception exception)
                {
                    Log.Error("Focus Error", exception);
                    StaticHelper.Instance.SystemMessage = "Focus error: " + exception.Message;
                }
            }
        }

        #region Implementation of IWindow

        public void ExecuteCommand(string cmd, object param)
        {
            switch (cmd)
            {
                case WindowsCmdConsts.LiveViewSimpleWnd_Show:
                    Dispatcher.Invoke(new Action(delegate
                                                     {
                                                         try
                                                         {

                                                             ICameraDevice cameraparam = param as ICameraDevice;
                                                             if (cameraparam == SelectedPortableDevice && IsVisible)
                                                             {
                                                                 Activate();
                                                                 Focus();
                                                                 return;
                                                             }
                                                             _freezeTimer.Interval =
                                                                 ServiceProvider.Settings.LiveViewFreezeTimeOut*1000;
                                                             ServiceProvider.Settings.SelectedBitmap.BitmapLoaded +=
                                                                 SelectedBitmap_BitmapLoaded;
                                                             SelectedPortableDevice = cameraparam;
                                                             SelectedPortableDevice.CameraDisconnected +=
                                                                 selectedPortableDevice_CameraDisconnected;
                                                             CameraProperty property =
                                                                 ServiceProvider.Settings.CameraProperties.Get(
                                                                     SelectedPortableDevice);
                                                             Title = TranslationStrings.LiveViewWindowTitle + " - " +
                                                                     property.DeviceName;
                                                             Show();
                                                             Activate();
                                                             //Topmost = true;
                                                             //Topmost = false;
                                                             Focus();
                                                             //ServiceProvider.Settings.Manager.PhotoTakenDone += Manager_PhotoTaked;
                                                             //Thread.Sleep(500);
                                                             StartLiveView();
                                                             //Thread.Sleep(500);
                                                             SelectedPortableDevice.CaptureCompleted +=
                                                                 selectedPortableDevice_CaptureCompleted;

                                                             if (ServiceProvider.Settings.DetectionType == 0)
                                                             {
                                                                 _detector = new MotionDetector(
                                                                     new TwoFramesDifferenceDetector(true),
                                                                     new BlobCountingObjectsProcessing(
                                                                         ServiceProvider.Settings.MotionBlockSize,
                                                                         ServiceProvider.Settings.MotionBlockSize, true));
                                                             }
                                                             else
                                                             {
                                                                 _detector = new MotionDetector(
                                                                     new SimpleBackgroundModelingDetector(true, true),
                                                                     new BlobCountingObjectsProcessing(
                                                                         ServiceProvider.Settings.MotionBlockSize,
                                                                         ServiceProvider.Settings.MotionBlockSize, true));
                                                             }
                                                             _photoCapturedTime = DateTime.Now;
                                                             _timer.Start();
                                                         }
                                                         catch (Exception exception)
                                                         {
                                                             Log.Error("Error initialize live view window ", exception);
                                                         }
                                                     }
                                          ));
                    break;
                case WindowsCmdConsts.LiveViewWnd_Hide:
                    Dispatcher.Invoke(new Action(delegate
                                                   {
                                                       Hide();
                                                       try
                                                       {
                                                           _timer.Stop();
                                                           selectedPortableDevice.CameraDisconnected -=
                                                             selectedPortableDevice_CameraDisconnected;
                                                           selectedPortableDevice.CaptureCompleted -=
                                                             selectedPortableDevice_CaptureCompleted;
                                                           ServiceProvider.Settings.SelectedBitmap.BitmapLoaded -=
                                                             SelectedBitmap_BitmapLoaded;
                                                           Thread.Sleep(100);
                                                           StopLiveView();
                                                           LiveViewData = null;
                                                       }
                                                       catch (Exception exception)
                                                       {
                                                           Log.Error("Unable to stop live view", exception);
                                                       }
                                                       //ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.FocusStackingWnd_Hide);
                                                   }));
                    break;
                case CmdConsts.All_Close:
                    Dispatcher.Invoke(new Action(delegate
                                                   {
                                                       Hide();
                                                       Close();
                                                   }));
                    break;
            }
        }
        private void selectedPortableDevice_CameraDisconnected(object sender, DisconnectCameraEventArgs eventArgs)
        {
            ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.LiveViewWnd_Hide, SelectedPortableDevice);
        }

        private void selectedPortableDevice_CaptureCompleted(object sender, EventArgs e)
        {
            if (!IsVisible)
                return;
            _detector.Reset();
            _photoCapturedTime = DateTime.Now;
                IsBusy = false;
                _timer.Start();
                StartLiveView();
        }

        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (IsVisible)
            {
                e.Cancel = true;
                ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.LiveViewWnd_Hide, SelectedPortableDevice);
            }
        }


        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion


        private void TakePhoto()
        {
            try
            {
                //if (IsBusy)
                //{
                    Log.Debug("LiveView: Stack photo capture started");
                    StartLiveView();
                    //if (PhotoCount > 0)
                    //{
                    //    SetFocus(FocusStep);
                    //}
                    //PhotoCount++;
                    GetLiveImage();
                    //Thread.Sleep(WaitTime * 1000);
                    //if (PhotoCount <= PhotoNo)
                    //{
                        //if (!_preview)
                        //{
                            //Recording = false;
                            SelectedPortableDevice.CapturePhotoNoAf();
                        //}
                        //else
                        //{
                        //    TakePhoto();
                        //}
                    //}
                    //else
                    //{
                    //    StartLiveView();
                        //FreezeImage = false;
                        //IsBusy = false;
                        //PhotoCount = 0;
                        //_timer.Start();
                    //}
                //}
                //else
                //{
                //    ServiceProvider.DeviceManager.SelectedCameraDevice.StartLiveView();
                    //FreezeImage = false;
                //}
            }
            catch (DeviceException exception)
            {
                StaticHelper.Instance.SystemMessage = exception.Message;
                Log.Error("Live view. Unable to take photo", exception);
            }
        }

        private void btn_takephoto_Click(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(500);
            GetLiveImage();
            IsBusy = true;
            Thread thread = new Thread(TakePhoto);
            thread.Start();
        }

        private void btn_help_Click(object sender, RoutedEventArgs e)
        {
            HelpProvider.Run(HelpSections.LiveView);
        }

        
    }
}
