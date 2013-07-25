﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using CameraControl.Classes;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using CameraControl.Devices.Classes;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for CameraPropertyWnd.xaml
  /// </summary>
  public partial class CameraPropertyWnd : IWindow, INotifyPropertyChanged
  {
    private ICameraDevice _cameraDevice;

    private CameraProperty _cameraProperty;
    public CameraProperty CameraProperty
    {
      get { return _cameraProperty; }
      set
      {
        _cameraProperty = value;
        NotifyPropertyChanged("CameraProperty");
      }
    }

    private AsyncObservableCollection<string> _photoSessionNames;
    public AsyncObservableCollection<string> PhotoSessionNames
    {
      get { return _photoSessionNames; }
      set
      {
        _photoSessionNames = value;
        NotifyPropertyChanged("PhotoSessionNames");
      }
    }

    public CameraPropertyWnd()
    {
      InitializeComponent();
      PhotoSessionNames = new AsyncObservableCollection<string>();
    }

    #region Implementation of IWindow

    public void ExecuteCommand(string cmd, object param)
    {
      switch (cmd)
      {
        case WindowsCmdConsts.CameraPropertyWnd_Show:
          PhotoSessionNames.Clear();
          PhotoSessionNames.Add("(None)");
          foreach (PhotoSession photoSession in ServiceProvider.Settings.PhotoSessions)
          {
            PhotoSessionNames.Add(photoSession.Name);
          }
          _cameraDevice = param as ICameraDevice;
          if (_cameraDevice == null)
            return;
          CameraProperty = ServiceProvider.Settings.CameraProperties.Get(_cameraDevice);
          CameraProperty.BeginEdit();
          Dispatcher.Invoke(new Action(delegate
          {
            Show();
            Activate();
            Topmost = true;
            //Topmost = false;
            Focus();
          }));
          break;
        case WindowsCmdConsts.CameraPropertyWnd_Hide:
          Hide();
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

    #endregion

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      if (IsVisible)
      {
        e.Cancel = true;
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.CameraPropertyWnd_Hide);
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

    private void btn_save_Click(object sender, RoutedEventArgs e)
    {
      CameraProperty.EndEdit();
      ServiceProvider.Settings.Save();
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.CameraPropertyWnd_Hide);
    }

    private void btn_cancel_Click(object sender, RoutedEventArgs e)
    {
      CameraProperty.CancelEdit();
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.CameraPropertyWnd_Hide);
    }

    private void btn_identify_Click(object sender, RoutedEventArgs e)
    {
      Thread thread = new Thread(new ThreadStart(delegate
                                                   {
                                                     for (int i = 0; i < 5; i++)
                                                     {
                                                       _cameraDevice.LockCamera();
                                                       Thread.Sleep(800);
                                                       _cameraDevice.UnLockCamera();
                                                       Thread.Sleep(800);
                                                     }
                                                   }));
      thread.Start();
    }


  }
}
