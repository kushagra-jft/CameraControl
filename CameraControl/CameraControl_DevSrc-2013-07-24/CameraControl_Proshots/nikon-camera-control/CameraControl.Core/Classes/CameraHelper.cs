using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Devices;

namespace CameraControl.Core.Classes
{
    public class CameraHelper
    {
        /// <summary>
        /// Captures the specified camera device.
        /// </summary>
        /// <param name="o">ICameraDevice</param>
        public static void Capture(object o)
        {
            if (o != null)
            {
                var camera = o as ICameraDevice;
                if (camera != null)
                {
                    CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(camera);
                    if (property.UseExternalShutter && property.SelectedConfig!=null)
                    {
                        ServiceProvider.ExternalDeviceManager.Start(property.SelectedConfig);
                        return;
                    }
                    camera.CapturePhoto();
                }
            }
        }


    }
}
