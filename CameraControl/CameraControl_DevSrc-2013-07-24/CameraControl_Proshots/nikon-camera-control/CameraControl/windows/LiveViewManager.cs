using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using CameraControl.Classes;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Nikon;

namespace CameraControl.windows
{
    public class LiveViewManager : IWindow
    {
        private static object _locker = new object();

        #region Implementation of IWindow

        private Dictionary<object, IWindow> _register;
        private static Dictionary<ICameraDevice, bool> _recordtoRam;

        public LiveViewManager()
        {
            _register = new Dictionary<object, IWindow>();
            _recordtoRam = new Dictionary<ICameraDevice, bool>();
        }

        public void ExecuteCommand(string cmd, object param)
        {
            if (param == null)
                param = ServiceProvider.DeviceManager.SelectedCameraDevice;
            switch (cmd)
            {
                case WindowsCmdConsts.LiveViewWnd_Show:
                    if (!_register.ContainsKey(param))
                    {
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                                                                           {
                                                                               LiveViewWnd wnd = new LiveViewWnd();
                                                                               ServiceProvider.Settings.ApplyTheme(wnd);
                                                                               _register.Add(param, wnd);
                                                                           }));
                    }
                    _register[param].ExecuteCommand(cmd, param);
                    break;
                case WindowsCmdConsts.LiveViewSimpleWnd_Show:
                    if (!_register.ContainsKey(param))
                    {
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                                                                            {
                                                                                var wnd = new LiveViewSimpleWnd();
                                                                                ServiceProvider.Settings.ApplyTheme(wnd);
                                                                                _register.Add(param, wnd);
                                                                            }));
                    }
                    _register[param].ExecuteCommand(cmd, param);
                    break;
                case WindowsCmdConsts.LiveViewWnd_Hide:
                    if (_register.ContainsKey(param))
                        _register[param].ExecuteCommand(cmd, param);
                    break;
                case CmdConsts.All_Close:
                    foreach (var liveViewWnd in _register)
                    {
                        liveViewWnd.Value.ExecuteCommand(cmd, param);
                    }
                    break;
                default:
                    foreach (var liveViewWnd in _register)
                    {
                        if (cmd.StartsWith("LiveView"))
                            liveViewWnd.Value.ExecuteCommand(cmd, param);
                    }
                    break;
            }

        }

        public bool IsVisible { get; private set; }

        #endregion

        public static void StartLiveView(ICameraDevice device)
        {
            lock (_locker)
            {
                // some nikon cameras can set af to manual
                //force to capture in ram
                if (device is NikonBase && device.GetCapability(CapabilityEnum.CaptureNoAf))
                {
                    if (!_recordtoRam.ContainsKey(device))
                        _recordtoRam.Add(device, device.CaptureInSdRam);
                    else
                        _recordtoRam[device] = device.CaptureInSdRam;
                    device.CaptureInSdRam = true;
                }
                device.StartLiveView();
            }
        }

        public static void StopLiveView(ICameraDevice device)
        {
            lock (_locker)
            {
                device.StopLiveView();
                if (device is NikonBase && device.GetCapability(CapabilityEnum.CaptureNoAf))
                {
                    if (_recordtoRam.ContainsKey(device))
                        device.CaptureInSdRam = _recordtoRam[device];

                }

            }
        }

        public static LiveViewData GetLiveViewImage(ICameraDevice device)
        {
            lock (_locker)
            {
                return device.GetLiveViewImage();
            }
        }

    }
}
