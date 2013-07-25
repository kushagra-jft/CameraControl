using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    class SetCamera:BaseScript
    {
        public override bool Execute(ScriptObject scriptObject)
        {
            string property = scriptObject.ParseString(LoadedParams["property"]);
            string val = scriptObject.ParseString(LoadedParams["value"]).Trim().ToLower();
            switch (property)
            {
                case "iso":
                    {
                        scriptObject.CameraDevice.IsoNumber.SetValue(val);
                    }
                    break;
                case "aperture":
                    {
                        val = val.Replace("f", "ƒ");
                        if(!val.Contains("/"))
                        {
                            val = "ƒ/" + val;
                        }
                        scriptObject.CameraDevice.FNumber.SetValue(val);
                    }
                    break;
                case "shutter" :
                    {
                        if(!val.Contains("/") && !val.EndsWith("s") && !val.Equals("bulb"))
                        {
                            val += "s";
                        }
                        if (val.Equals("bulb"))
                        {
                            val = "Bulb";
                        }
                        scriptObject.CameraDevice.ShutterSpeed.SetValue(val);
                    }
                    break;
                case "ec":
                    {
                        scriptObject.CameraDevice.ExposureCompensation.SetValue(val);
                    }
                    break;
                case "wb":
                    {
                        scriptObject.CameraDevice.WhiteBalance.SetValue(val);
                    }
                    break;
                case "cs":
                    {
                        scriptObject.CameraDevice.CompressionSetting.SetValue(val);
                    }
                    break;
                default:
                    {
                        ServiceProvider.ScriptManager.OutPut("Wrong property :" + property);
                    }
                    break;
            }
            return true;
        }

        public SetCamera()
        {
            Name = "setcamera";
            Description = "Set a camera property";
            DefaultValue = "setcamera";
        }
    }
}
