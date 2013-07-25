using System.Windows.Input;

namespace PhotoBooth
{
    public class RoutedCommands
    {
        public static RoutedUICommand BrowseForLocation = new RoutedUICommand("Browse", "Browse", typeof(RoutedCommands));
        public static RoutedUICommand InitializeCamera = new RoutedUICommand("_Initialize Camera", "InitializeCamera", typeof(RoutedCommands));
        public static RoutedUICommand OpenPhotoBooth = new RoutedUICommand("Photo_Booth", "PhotoBooth", typeof(RoutedCommands));
        public static RoutedUICommand PrinterSetup = new RoutedUICommand("Printer _Setup", "PrinterSetup", typeof(RoutedCommands));
        public static RoutedUICommand ShowCardView = new RoutedUICommand("Card View", "CardView", typeof(RoutedCommands));
        public static RoutedUICommand TakePicture = new RoutedUICommand("_Take Picture", "TakePicture", typeof(RoutedCommands));
        public static RoutedUICommand TakePictureSet = new RoutedUICommand("Take Picture _Set", "TakePictureSet", typeof(RoutedCommands));
    }
}
