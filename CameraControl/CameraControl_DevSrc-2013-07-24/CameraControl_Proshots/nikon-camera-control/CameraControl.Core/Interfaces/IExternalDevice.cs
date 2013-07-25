using System.Windows.Controls;
using CameraControl.Core.Classes;

namespace CameraControl.Core.Interfaces
{
    public interface IExternalDevice
    {
        string Name { get; set; }
        bool Execute(CustomConfig config);
        bool CanExecute(CustomConfig config);
        UserControl GetConfig(CustomConfig config);
        SourceEnum DeviceType { get; set; }
        bool Start(CustomConfig config);
        bool Stop(CustomConfig config);
    }
}