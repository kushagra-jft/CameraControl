using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CameraControl.Core.Classes;
using CameraControl.Devices;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    public class CaptureAll:BaseScript
    {
        public override bool Execute(ScriptObject scriptObject)
        {
            foreach (ICameraDevice cameraDevice in ServiceProvider.DeviceManager.ConnectedDevices)
            {
                new Thread(CameraHelper.Capture).Start(cameraDevice);
            }
            return true;
        }

        public CaptureAll()
        {
            Name = "captureall";
            Description = "Trigger capture command on all connected cameras";
            DefaultValue = "captureall";
        }
    }
}
