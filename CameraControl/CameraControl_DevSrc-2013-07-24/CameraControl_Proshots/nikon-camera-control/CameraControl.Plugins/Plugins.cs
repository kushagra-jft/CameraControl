using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using CameraControl.Plugins.ExportPlugins;
using CameraControl.Plugins.ExternalDevices;
using CameraControl.Plugins.MainWindowPlugins;
using CameraControl.Plugins.ToolPlugin;

namespace CameraControl.Plugins
{
    public class Plugins : IPlugin
    {
        #region Implementation of IPlugin

        public bool Register()
        {
            try
            {
                ServiceProvider.PluginManager.ExportPlugins.Add(new ExportToZip());
                ServiceProvider.PluginManager.ExportPlugins.Add(new ExportToFolder());
                ServiceProvider.PluginManager.MainWindowPlugins.Add(new SimpleMainWindow());
                ServiceProvider.PluginManager.MainWindowPlugins.Add(new ProshotsWindow());
                ServiceProvider.PluginManager.ToolPlugins.Add(new PhdPlugin());
                ServiceProvider.PluginManager.ToolPlugins.Add(new WaterDropWnd());
                ServiceProvider.ExternalDeviceManager.ExternalDevices.Add(new SerialPortShutterRelease());
            }
            catch (Exception exception)
            {
                Log.Error("Error loadings plugins ", exception);
            }
            return true;
        }

        #endregion
    }
}
