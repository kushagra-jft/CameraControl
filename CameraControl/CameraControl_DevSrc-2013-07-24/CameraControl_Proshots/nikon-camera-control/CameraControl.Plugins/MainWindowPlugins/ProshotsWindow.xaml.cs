using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using MessageBox = System.Windows.MessageBox;
using System.Xml;
using System.Text.RegularExpressions;

namespace CameraControl.Plugins.MainWindowPlugins
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class ProshotsWindow : IMainWindowPlugin, INotifyPropertyChanged
  { 
    public string DisplayName { get; set; }

    System.Windows.Threading.DispatcherTimer _timer;
    public bool BarcodeVerified;
    public bool BarcodeUsed = true;
    public string Barcode
    {
        get { return ServiceProvider.Settings.DefaultSession.LastBarcode; }
        set
        {
            ServiceProvider.Settings.DefaultSession.LastBarcode = value;
            NotifyPropertyChanged("Barcode");
        }
    }
    public string QuickTag
    {
        get { return ServiceProvider.Settings.DefaultSession.QuickTag; }
        set
        {
            ServiceProvider.Settings.DefaultSession.QuickTag = value;
            NotifyPropertyChanged("QuickTag");
        }
    }

    public ProshotsWindow()
    {
        DisplayName = "Proshots Custom";
        InitializeComponent();
    }

    private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ServiceProvider.DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
        ServiceProvider.DeviceManager.CameraDisconnected += DeviceManager_CameraDisconnected;

        _timer = new System.Windows.Threading.DispatcherTimer();
        _timer.Tick += on_timer_Tick;
        _timer.Interval = new TimeSpan(0, 0, 0, 0, 250);

        brushRegexDef = txt_Barcode.Background;
        flashBackgroundDefault = MainGrid.Background;
        
        if(!ServiceProvider.Settings.DefaultsWereLoaded)
            ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.EditSessionWnd_Firstrun);
        
        CheckBarcode();
    }

    public void ShowWarning(string msg)
    {
        Dispatcher.BeginInvoke(new Action(delegate
        { 
            FlashScreen(-1);
            var prompt = new CameraControl.Plugins.windows.DialogPrompt(msg); 
            prompt.ShowDialog();
            FlashScreenStop();
        }));
    }
    
    private Brush flashBackgroundDefault = null;
    private Brush flashBackgroundWarning = new SolidColorBrush(Color.FromRgb(0xAF, 0x00, 0x00));    
    private int timer_TicksRemain;
    private void on_timer_Tick(object sender, EventArgs e)
    {   
        if (timer_TicksRemain == 0)
        {
            MainGrid.Background = flashBackgroundDefault;
            _timer.Stop();
            return;
        }
        if (timer_TicksRemain > 0)
            timer_TicksRemain--;
        else
            timer_TicksRemain = (timer_TicksRemain == -1) ? -2 : -1;
        
        Dispatcher.BeginInvoke(new Action(delegate
        {   
            MainGrid.Background = (timer_TicksRemain % 2 == 0) ? flashBackgroundWarning : flashBackgroundDefault;
        }));
    }
    public void FlashScreen(int flashes)
    {
        Dispatcher.BeginInvoke(new Action(delegate
        {
            this.Activate();
        }));
        timer_TicksRemain = flashes * 2;
        _timer.Start();
    }
    public void FlashScreenStop()
    {
        timer_TicksRemain = 0;
    }


    void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
    {
      PhotoCaptured(eventArgs);
      //Thread thread = new Thread(PhotoCaptured);
      //thread.Start(eventArgs);
    }

    void DeviceManager_CameraDisconnected(ICameraDevice target)
    {
        ShowWarning("Camera Disconnected!");
    }

    private void MetroWindow_Closed(object sender, EventArgs e)
    {
      ServiceProvider.WindowsManager.ExecuteCommand(CmdConsts.All_Close);
    }

    #region Implementation of INotifyPropertyChanged

    public virtual event PropertyChangedEventHandler PropertyChanged;

    public virtual void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }

    #endregion

    private void btn_capture_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhoto();
      }
      catch (DeviceException exception)
      {
        MessageBox.Show("Error occurred :" + exception.Message);
      }
    }

    private void PhotoCaptured(object o)
    {
        PhotoSession session = ServiceProvider.Settings.DefaultSession;
        
        // Save recent barcode and prefix.
        ServiceProvider.Settings.Save(ServiceProvider.Settings.DefaultSession);
        ServiceProvider.Settings.Save();

        ServiceProvider.Settings.DefaultSession.Files.Clear();
        GC.Collect();

        if (!BarcodeVerified)
        {
            ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.Select_Image, new FileItem(""));
            ShowWarning("Invalid Barcode; Image not transfered!");
            return;
        }

        PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;
        if (eventArgs == null) return;

        try
        {
            Log.Debug("Photo transfer begin.");
            eventArgs.CameraDevice.IsBusy = true;
            CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(eventArgs.CameraDevice);
            
            if (!eventArgs.CameraDevice.CaptureInSdRam)
            {
                if (property.NoDownload)
                {
                    eventArgs.CameraDevice.IsBusy = false;
                    return;
                }
                var extension = Path.GetExtension(eventArgs.FileName);
                if (extension != null && (session.DownloadOnlyJpg && extension.ToLower() != ".jpg"))
                {
                    eventArgs.CameraDevice.IsBusy = false;
                    return;
                }
            }

            string fileName = "";
            if (!session.UseOriginalFilename || eventArgs.CameraDevice.CaptureInSdRam)
            {
                fileName =
                  session.GetNextFileName(Path.GetExtension(eventArgs.FileName),
                                          eventArgs.CameraDevice);
            }
            else
            {
                fileName = Path.Combine(session.Folder, eventArgs.FileName);
            }
            
            if (File.Exists(fileName))
                fileName = StaticHelper.GetUniqueFilename(
                        Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                        Path.GetExtension(fileName));
            

            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }

            var tmpFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Settings.AppName,
                                               "Cache", Path.GetFileName(fileName));

            Log.Debug("Transfer started :" + tmpFileName);
            DateTime startTIme = DateTime.Now;
            eventArgs.CameraDevice.TransferFile(eventArgs.Handle, tmpFileName);
            Log.Debug("Transfer done :" + tmpFileName);
            Log.Debug("[BENCHMARK]Speed :" +
                      (new System.IO.FileInfo(tmpFileName).Length / (DateTime.Now - startTIme).TotalSeconds / 1024 / 1024).ToString("0000.00"));
            Log.Debug("[BENCHMARK]Transfer time :" + ((DateTime.Now - startTIme).TotalSeconds).ToString("0000.000"));

            var fileItem = session.AddFile(tmpFileName);
            fileItem.DestinationFilename = fileName;
            ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.Select_Image, fileItem);
            
            if (!session.RetainCameraCopy && BarcodeVerified) {
                var Cam = ServiceProvider.DeviceManager.SelectedCameraDevice;
                var imgFile = Cam.GetObjects(null).Where(t => (uint)(t.Handle) == (uint)(eventArgs.Handle)).SingleOrDefault();
                if (imgFile != null)
                {
                    FileItem delFileItem = new FileItem(imgFile, Cam);
                    fileItem.Device.DeleteObject(delFileItem.DeviceObject);
                }
            }

            BarcodeUsed = true;
            eventArgs.CameraDevice.IsBusy = false;
           
            if (ServiceProvider.Settings.PlaySound)
            {
                PhotoUtils.PlayCaptureSound();
            }

        }
        catch (Exception exception)
        {
            eventArgs.CameraDevice.IsBusy = false;
            //MessageBox.Show("Error downloading photo from camera :\n" + exception.ToString());
            Log.Error("Error downloading photo from camera :\n" + exception.ToString());
        }

        Log.Debug("Photo transfer done.");
        GC.Collect();
    }

    private void btn_top_Click(object sender, RoutedEventArgs e)
    {
      Topmost = !Topmost;
    }

    private void on_btn_Preview(object sender, RoutedEventArgs e)
    {
        var t = ServiceProvider.Settings.SelectedBitmap;
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.FullScreenWnd_ShowMin);
    }

    private void on_btn_Session(object sender, RoutedEventArgs e)
    {
        if(String.IsNullOrEmpty(ServiceProvider.Settings.SettingsPassword))
            ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.EditSessionWnd_Show);
        else
        {
            var dialog = new CameraControl.Plugins.windows.PasswordPrompt("A password is required to edit these settings.");
            var res = dialog.ShowDialog() ?? false;
            if (res && dialog.ResponseString==ServiceProvider.Settings.SettingsPassword)
                ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.EditSessionWnd_Show);
        }
    }

    private Brush brushRegexDef = null;
    private Brush brushRegexOkay = new SolidColorBrush(Color.FromRgb(0x00, 0x6F, 0x00));
    private Brush brushRegexBad = new SolidColorBrush(Color.FromRgb(0x6F, 0x00, 0x00));
    
      private void CheckBarcode() {
        var Session = ServiceProvider.Settings.DefaultSession;

        bool PassedRegex = false;
        try
        {
            Regex BarcodeCheck = new Regex(Session.BarcodeRegex);
            if (BarcodeCheck.IsMatch(Barcode))
                PassedRegex = true;
        }
        catch (Exception e)
        {
            Log.Debug(e.ToString());
        }

        bool PassedLength = ( Barcode.Length <= Session.BarcodeLengthMax && Barcode.Length >= Session.BarcodeLengthMin );

        if (PassedLength && PassedRegex)
        {
            txt_Barcode.Background = brushRegexOkay;
            BarcodeVerified = true;
        }
        else
        {
            txt_Barcode.Background = brushRegexBad;
            if (String.IsNullOrEmpty(Barcode))
                txt_Barcode.Background = brushRegexDef;
            BarcodeVerified = false;
        }
    }

    private void on_txt_Prefix_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        var QTRegex = @"[^a-zA-Z]{1}";
        QuickTag = txt_Prefix.Text;    // Expedite update
        var pos = txt_Prefix.CaretIndex;
        var matchesBefore = Regex.Matches(QuickTag.Substring(0,pos), QTRegex).Count;
        QuickTag = Regex.Replace(QuickTag, QTRegex, "");
        txt_Prefix.Text = QuickTag;
        txt_Prefix.CaretIndex = pos - matchesBefore;
    }

    private void on_txt_Barcode_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (ServiceProvider.DeviceManager.SelectedCameraDevice != null && ServiceProvider.DeviceManager.SelectedCameraDevice.IsBusy)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                var prompt = new CameraControl.Plugins.windows.DialogPrompt("Should not clear barcode: transfers still pending.");
                prompt.ShowDialog();
            }));
            return;
        }

        Barcode = txt_Barcode.Text;    // Expedite update
        BarcodeUsed = false;
        CheckBarcode();
    }

    private void on_txt_Barcode_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        //if (BarcodeUsed)
        //    txt_Barcode.Text = "";
    }

    private void on_btn_LiveView(object sender, RoutedEventArgs e)
    {
        var canView = ServiceProvider.DeviceManager.SelectedCameraDevice.GetCapability(CapabilityEnum.LiveView);
        //ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.LiveViewWnd_Show, ServiceProvider.DeviceManager.SelectedCameraDevice);
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.LiveViewSimpleWnd_Show, ServiceProvider.DeviceManager.SelectedCameraDevice);
    }

    private void btn_ClearBarcode_Click(object sender, RoutedEventArgs e)
    {
        if (ServiceProvider.DeviceManager.SelectedCameraDevice != null && ServiceProvider.DeviceManager.SelectedCameraDevice.IsBusy)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                var prompt = new CameraControl.Plugins.windows.DialogPrompt("Should not clear barcode: transfers still pending.");
                prompt.ShowDialog();
            }));
            return;
        }

        Barcode = "";
        // Should already be in WPF thread, no?
        Dispatcher.BeginInvoke(new Action(delegate
        {
            txt_Barcode.Text = "";
            CheckBarcode();
            txt_Barcode.Focus();
        }));
    }
    

  }
}
