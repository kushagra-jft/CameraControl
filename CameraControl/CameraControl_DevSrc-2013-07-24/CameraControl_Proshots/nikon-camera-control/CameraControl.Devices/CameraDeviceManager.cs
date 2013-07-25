using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CameraControl.Devices.Canon;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Nikon;
using CameraControl.Devices.Others;
using Canon.Eos.Framework;
using PortableDeviceLib;
using WIA;

namespace CameraControl.Devices
{
    public class CameraDeviceManager : BaseFieldClass
    {
        private const string AppName = "CameraControl";
        private const int AppMajorVersionNumber = 1;
        private const int AppMinorVersionNumber = 0;
        private DeviceDescriptorEnumerator _deviceEnumerator;
        private EosFramework _framework;


        /// <summary>
        /// Gets or sets a value indicating whether use experimental drivers.
        /// Experimental drivers isn't tested and may not implement all camera possibilities 
        /// </summary>
        /// <value>
        /// <c>true</c> if [use experimental drivers]; otherwise, <c>false</c>.
        /// </value>
        public bool UseExperimentalDrivers { get; set; }

        /// <summary>
        /// Gets or sets the natively supported model dictionary.
        /// This property is used to fine the right driver for connected camera, 
        /// if model not found in this dictionary generic wia driver will used.
        /// </summary>
        /// <value>
        /// The device class.
        /// </value>
        public Dictionary<string, Type> DeviceClass { get; set; }

        private ICameraDevice _selectedCameraDevice;
        /// <summary>
        /// Gets or sets the default selected camera device. When new camera connected this property set automatically to new device
        /// </summary>
        /// <value>
        /// The selected camera device.
        /// </value>
        public ICameraDevice SelectedCameraDevice
        {
            get { return _selectedCameraDevice; }
            set
            {
                ICameraDevice device = _selectedCameraDevice;
                _selectedCameraDevice = value;
                if (CameraSelected != null)
                {
                    CameraSelected(device, _selectedCameraDevice);
                }

                NotifyPropertyChanged("SelectedCameraDevice");
            }
        }

        private AsyncObservableCollection<ICameraDevice> _connectedDevices;
        public AsyncObservableCollection<ICameraDevice> ConnectedDevices
        {
            get { return _connectedDevices; }
            set
            {
                _connectedDevices = value;
                NotifyPropertyChanged("ConnectedDevices");
            }
        }

        private void PopulateDeviceClass()
        {
            DeviceClass = new Dictionary<string, Type>
                      {
                        {"D200", typeof (NikonD40)},
                        {"D3S", typeof (NikonD90)},
                        {"D3X", typeof (NikonD3X)},
                        {"D300", typeof (NikonD300)},
                        {"D300s", typeof (NikonD300)},
                        {"D300S", typeof (NikonD300)},
                        {"D3200", typeof (NikonD3200)},
                        {"D4", typeof (NikonD4)},
                        {"D40", typeof (NikonD40)},
                        {"D50", typeof (NikonD40)},
                        {"D5200", typeof (NikonD5200)},
                        {"D5100", typeof (NikonD5100)},
                        {"D5000", typeof (NikonD90)},
                        {"D60", typeof (NikonD60)},
                        {"D600", typeof (NikonD600)},
                        {"D70", typeof (NikonD40)},
                        {"D70s", typeof (NikonD40)},
                        {"D700", typeof (NikonD700)},
                        {"D7000", typeof (NikonD7000)},
                        {"D7100", typeof (NikonD7100)},
                        {"D80", typeof (NikonD80)},
                        {"D800", typeof (NikonD800)},
                        {"D800E", typeof (NikonD800)},
                        {"D800e", typeof (NikonD800)},
                        {"D90", typeof (NikonD90)},
                        //{"Canon EOS 5D Mark II", typeof (CanonSDKBase)},
                        {"MTP Sim", typeof (BaseMTPCamera)},
                        
                        //{"D.*", typeof (NikonBase)},
                        // for mtp simulator
                       //{"Test Camera ", typeof (NikonBase)},
                      };
            //if(UseExperimentalDrivers)
            //{
            //  DeviceClass.Add("Canon EOS.*", typeof(CanonSDKBase));
            //}

        }

        public CameraDeviceManager()
        {
            UseExperimentalDrivers = true;
            SelectedCameraDevice = new NotConnectedCameraDevice();
            ConnectedDevices = new AsyncObservableCollection<ICameraDevice>();
            _deviceEnumerator = new DeviceDescriptorEnumerator();
            WiaDeviceManager = new DeviceManager();
            WiaDeviceManager.RegisterEvent(Conts.wiaEventDeviceConnected, "*");
            WiaDeviceManager.RegisterEvent(Conts.wiaEventDeviceDisconnected, "*");
            WiaDeviceManager.OnEvent += DeviceManager_OnEvent;
        }

        private void InitCanon()
        {
            try
            {
                _framework = new EosFramework();
                AddCanonCameras();
                _framework.CameraAdded += _framework_CameraAdded;
            }
            catch (Exception exception)
            {
                Log.Error("Unable init canon driver", exception);
            }   
        }

        private void _framework_CameraAdded(object sender, EventArgs e)
        {
            AddCanonCameras();
        }

        private void AddCanonCameras()
        {
            foreach (EosCamera eosCamera in _framework.GetCameraCollection())
            {
                bool shouldbeadded = true;
                foreach (ICameraDevice device in ConnectedDevices)
                {
                    CanonSDKBase camera = device as CanonSDKBase;
                    if(camera!=null )
                    {
                        if (camera.Camera == eosCamera)
                        {
                            shouldbeadded = false;
                            break;
                        }
                    }
                }

                if(shouldbeadded)
                {
                    Log.Debug("New canon camera found !");
                    CanonSDKBase camera = new CanonSDKBase();
                    camera.Init(eosCamera);
                    ConnectedDevices.Add(camera);
                    NewCameraConnected(camera);
                }
           }
            Thread.Sleep(2500);
        }

        private ICameraDevice GetWiaIDevice(IDeviceInfo devInfo)
        {
            // if camera already is connected do nothing
            if (_deviceEnumerator.GetByWiaId(devInfo.DeviceID) != null)
                return _deviceEnumerator.GetByWiaId(devInfo.DeviceID).CameraDevice;
            _deviceEnumerator.RemoveDisconnected();
            DeviceDescriptor descriptor = new DeviceDescriptor { WiaDeviceInfo = devInfo, WiaId = devInfo.DeviceID };

            ICameraDevice cameraDevice = new WiaCameraDevice();
            bool isConnected = cameraDevice.Init(descriptor);

            descriptor.CameraDevice = cameraDevice;
            _deviceEnumerator.Add(descriptor);
            ConnectedDevices.Add(cameraDevice);

            if (isConnected)
            {
                NewCameraConnected(cameraDevice);
            }
            //ServiceProvider.DeviceManager.SelectedCameraDevice.ReadDeviceProperties(0);

            return SelectedCameraDevice;
        }

        void cameraDevice_CameraDisconnected(object sender, DisconnectCameraEventArgs e)
        {
            if (e.StillImageDevice != null)
            {
                DisconnectCamera(e.StillImageDevice);
            }
            OnCameraDisconnected((ICameraDevice)sender);
        }

        /// <summary>
        /// Raise CameraDisconnected event.
        /// </summary>
        /// <param name="cameraDevice">The camera device.</param>
        public void OnCameraDisconnected(ICameraDevice cameraDevice)
        {
            if (CameraDisconnected != null)
                CameraDisconnected(cameraDevice);
        }

        void cameraDevice_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
        {
            if (PhotoCaptured != null)
                PhotoCaptured(sender, eventArgs);
        }

        private void ConnectDevices()
        {
 
            if (PortableDeviceCollection.Instance == null)
            {
                PortableDeviceCollection.CreateInstance(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
                PortableDeviceCollection.Instance.AutoConnectToPortableDevice = false;
            }
            _deviceEnumerator.RemoveDisconnected();

            if (UseExperimentalDrivers)
                InitCanon();

            foreach (PortableDevice portableDevice in PortableDeviceCollection.Instance.Devices)
            {
                Log.Debug("Connection device " + portableDevice.DeviceId);
                //TODO: avoid to load some mass storage in my computer need to find a general solution
                if (!portableDevice.DeviceId.StartsWith("\\\\?\\usb") && !portableDevice.DeviceId.StartsWith("\\\\?\\comp"))
                    continue;
                // ignore Canon cameras
                if (portableDevice.DeviceId.Contains("vid_04a9"))
                    continue;
                portableDevice.ConnectToDevice(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
                if (_deviceEnumerator.GetByWpdId(portableDevice.DeviceId) == null && GetNativeDriver(portableDevice.Model) != null)
                {
                    ICameraDevice cameraDevice;
                    DeviceDescriptor descriptor = new DeviceDescriptor { WpdId = portableDevice.DeviceId };
                    cameraDevice = (ICameraDevice)Activator.CreateInstance(GetNativeDriver(portableDevice.Model));
                    cameraDevice.SerialNumber = StaticHelper.GetSerial(portableDevice.DeviceId);
                    cameraDevice.Init(descriptor);
                    descriptor.CameraDevice = cameraDevice;
                    _deviceEnumerator.Add(descriptor);
                    ConnectedDevices.Add(cameraDevice);

                    NewCameraConnected(cameraDevice);
                }
            }
        }

        private void NewCameraConnected(ICameraDevice cameraDevice)
        {
            StaticHelper.Instance.SystemMessage = "New Camera is connected ! Driver :" + cameraDevice.DeviceName;
            Log.Debug("===========Camera is connected==============");
            Log.Debug("Driver :" + cameraDevice.GetType().Name);
            Log.Debug("Name :" + cameraDevice.DeviceName);
            Log.Debug("Manufacturer :" + cameraDevice.Manufacturer);
            if (CameraConnected != null)
                CameraConnected(cameraDevice);
            SelectedCameraDevice = cameraDevice;
            cameraDevice.PhotoCaptured += cameraDevice_PhotoCaptured;
            cameraDevice.CameraDisconnected += cameraDevice_CameraDisconnected;
        }

        /// <summary>
        /// Gets the native driver based on camera model.
        /// </summary>
        /// <param name="model">The model name.</param>
        /// <returns>If the model not supported return null else the driver type</returns>
        private Type GetNativeDriver(string model)
        {
            if (String.IsNullOrEmpty(model))
                return null;
            // first check if driver exist with same driver name
            if (DeviceClass.ContainsKey(model))
                return DeviceClass[model];
            // in driver not found will check with regex name
            return (from keyValuePair in DeviceClass
                    let regex = new Regex(keyValuePair.Key)
                    where regex.IsMatch(model)
                    select keyValuePair.Value).FirstOrDefault();
        }

        private void DisconnectCamera(string wiaId)
        {
            DeviceDescriptor descriptor = _deviceEnumerator.GetByWiaId(wiaId);
            if (descriptor != null)
            {
                descriptor.CameraDevice.PhotoCaptured -= cameraDevice_PhotoCaptured;
                descriptor.CameraDevice.CameraDisconnected -= cameraDevice_CameraDisconnected;
                ConnectedDevices.Remove(descriptor.CameraDevice);
                StaticHelper.Instance.SystemMessage = "Camera disconnected :" + descriptor.CameraDevice.DeviceName;
                Log.Debug("===========Camera disconnected==============");
                Log.Debug("Name :" + descriptor.CameraDevice.DeviceName);
                if (SelectedCameraDevice == descriptor.CameraDevice)
                {
                    if (ConnectedDevices.Count > 0)
                        SelectedCameraDevice = ConnectedDevices[0];
                    else
                    {
                        SelectedCameraDevice = new NotConnectedCameraDevice();
                    }
                }
                _deviceEnumerator.Remove(descriptor);
                descriptor.CameraDevice.Close();
                var wiaCameraDevice = descriptor.CameraDevice as WiaCameraDevice;
                if (wiaCameraDevice != null)
                {
                    OnCameraDisconnected(wiaCameraDevice);
                }
                PortableDeviceCollection.Instance.RefreshDevices();
            }
        }

        private void DisconnectCamera(StillImageDevice device)
        {
            DeviceDescriptor descriptor = _deviceEnumerator.GetByWpdId(device.DeviceId);
            if (descriptor != null)
            {
                descriptor.CameraDevice.PhotoCaptured -= cameraDevice_PhotoCaptured;
                descriptor.CameraDevice.CameraDisconnected -= cameraDevice_CameraDisconnected;
                ConnectedDevices.Remove(descriptor.CameraDevice);
                StaticHelper.Instance.SystemMessage = "Camera disconnected :" + descriptor.CameraDevice.DeviceName;
                Log.Debug("===========Camera disconnected==============");
                Log.Debug("Name :" + descriptor.CameraDevice.DeviceName);
                PortableDeviceCollection.Instance.RemoveDevice(device.DeviceId);
                device.IsConnected = false;
                if (SelectedCameraDevice == descriptor.CameraDevice)
                {
                    SelectedCameraDevice = ConnectedDevices.Count > 0 ? ConnectedDevices[0] : new NotConnectedCameraDevice();
                }
                descriptor.CameraDevice.Close();
                _deviceEnumerator.Remove(descriptor);
                _deviceEnumerator.RemoveDisconnected();
            }
            RemoveDisconnected();
        }

        private void RemoveDisconnected()
        {
            List<ICameraDevice> removedCameras = ConnectedDevices.Where(device => !device.IsConnected).ToList();
            foreach (ICameraDevice device in removedCameras)
            {
                ConnectedDevices.Remove(device);
            }
        }

        private DeviceManager WiaDeviceManager { get; set; }

        /// <summary>
        /// Gets or sets a value for disabling native drivers.
        /// </summary>
        /// <value>
        /// <c>true</c> all devices are loaded like WIA devices <c>false</c> If native driver are available for connected model the will be loaded that driver else will be loaded WIA driver.
        /// </value>
        public bool DisableNativeDrivers { get; set; }


        private void DeviceManager_OnEvent(string eventId, string deviceId, string itemId)
        {
            if (eventId == Conts.wiaEventDeviceConnected)
            {
                ConnectToCamera();
            }
            else if (eventId == Conts.wiaEventDeviceDisconnected)
            {
                DisconnectCamera(deviceId);
            }
        }


        /// <summary>
        /// Populate the ConnectedDevices list with connected cameras. This method will be called automatically every time new devices will be connected 
        /// </summary>
        /// <returns></returns>
        public bool ConnectToCamera()
        {
            return ConnectToCamera(true);
        }

        public bool ConnectToCamera(bool retry)
        {
            if (DeviceClass == null || DeviceClass.Count == 0)
                PopulateDeviceClass();

            if (!DisableNativeDrivers)
            {
                ConnectDevices();
            }
            else
            {
                Log.Debug("Native drivers are disabled !!!!");
            }
            // if canon camera is connected don't use wia driver
            if (UseExperimentalDrivers && _framework!=null && _framework.GetCameraCollection().Count > 0)
                return true;

            bool ret = false;
            bool noDriversDetected = ConnectedDevices.Count == 0;
            int retries = 0;
            foreach (IDeviceInfo devInfo in new DeviceManager().DeviceInfos)
            {
                // Look for CameraDeviceType devices
                string model = devInfo.Properties["Name"].get_Value();
                // skip canon cameras 
                if (!string.IsNullOrEmpty(model) && model.Contains("Canon"))
                    continue;
                if (devInfo.Type == WiaDeviceType.CameraDeviceType && (GetNativeDriver(model) == null || DisableNativeDrivers || noDriversDetected))
                {
                    do
                    {
                        Log.Debug("Wia Camera Found: " + model);
                        try
                        {
                            GetWiaIDevice(devInfo);
                            retries = 4;
                        }
                        catch (Exception exception)
                        {
                            Log.Error("Unable to connect to the camera", exception);
                            retries++;
                            if (retries < 3)
                            {
                                Log.Debug("Retrying");
                                StaticHelper.Instance.SystemMessage = "Unable to connect to the camera. Retrying";
                            }
                            else
                            {
                                StaticHelper.Instance.SystemMessage =
                                   "Unable to connect to the camera. Please reconnect your camera !";
                            }
                            Thread.Sleep(1000);
                        }
                    } while (retries < 3);
                    ret = true;
                }
            }
            return ret;
        }

        public void CloseAll()
        {
            foreach (ICameraDevice connectedDevice in ConnectedDevices.Where(connectedDevice => connectedDevice.IsConnected))
            {
                connectedDevice.Close();
            }
        }

        public event PhotoCapturedEventHandler PhotoCaptured;

        public delegate void CameraConnectedEventHandler(ICameraDevice cameraDevice);
        /// <summary>
        /// Occurs when a new camera is connected.
        /// </summary>
        public event CameraConnectedEventHandler CameraConnected;

        /// <summary>
        /// Occurs when a camera disconnected.
        /// </summary>
        public event CameraConnectedEventHandler CameraDisconnected;

        public delegate void CameraSelectedEventHandler(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice);

        /// <summary>
        /// Occurs when SelectedCameraDevice property changed.
        /// </summary>
        public event CameraSelectedEventHandler CameraSelected;
    }
}

