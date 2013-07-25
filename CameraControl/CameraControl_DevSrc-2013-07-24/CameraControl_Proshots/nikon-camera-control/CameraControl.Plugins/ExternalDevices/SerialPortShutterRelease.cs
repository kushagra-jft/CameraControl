using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;

namespace CameraControl.Plugins.ExternalDevices
{
    public class SerialPortShutterRelease : IExternalDevice
    {

        #region Implementation of IExternalShutterReleaseSource

        public string Name { get; set; }
        
        public bool Execute(CustomConfig config)
        {
            return true;
        }

        public bool CanExecute(CustomConfig config)
        {
            return true;
        }

        public UserControl GetConfig(CustomConfig config)
        {
            return new SerialPortShutterReleaseConfig(config);
        }

        public SourceEnum DeviceType { get; set; }
        public bool Start(CustomConfig config)
        {
            if (config.AttachedObject != null)
                Stop(config);
            SerialPort serialPort = new SerialPort(config.Get("Port"));
            serialPort.Open();
            serialPort.RtsEnable = true;
            config.AttachedObject = serialPort;
            return true;
        }

        public bool Stop(CustomConfig config)
        {
            if (config.AttachedObject == null)
                return false;
            SerialPort serialPort = config.AttachedObject as SerialPort;
            if (serialPort == null) throw new ArgumentNullException("serialPort");
            serialPort.RtsEnable = false;
            serialPort.Close();
            config.AttachedObject = null;
            return true;
        }

        #endregion

        public SerialPortShutterRelease()
        {
            Name = "Serial Port Shutter Release";
            DeviceType=SourceEnum.ExternaExternalShutterRelease;
        }
    }
}
