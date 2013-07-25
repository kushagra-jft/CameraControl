using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core.Classes;
using CameraControl.Devices;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    public class Capture:BaseScript
    {
        public override bool Execute(ScriptObject scriptObject)
        {
            try
            {
                ServiceProvider.ScriptManager.OutPut("Capture started");
                CameraHelper.Capture(scriptObject.CameraDevice); 
            }
            catch (Exception exception)
            {
                ServiceProvider.ScriptManager.OutPut("Capture error " + exception.Message);
                Log.Debug("Script capture error", exception);
            }
            return true;
        }

        public Capture()
        {
            Name = "capture";
            Description = "Trigger capture command on camera";
            DefaultValue = "capture";
        }
    }
}
