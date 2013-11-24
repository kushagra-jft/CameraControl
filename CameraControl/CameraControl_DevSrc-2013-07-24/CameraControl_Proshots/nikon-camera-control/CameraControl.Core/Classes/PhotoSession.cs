using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using CameraControl.Devices;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Classes
{
    public class PhotoSession : BaseFieldClass
    {
        private object _locker = new object();
        private string _lastFilename = null;

        [XmlIgnore]
        public List<string> SupportedExtensions = new List<string> { ".jpg", ".nef", ".tif", ".png", ".cr2" };
        [XmlIgnore]
        public List<string> RawExtensions = new List<string> { ".cr2", ".nef" };

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private bool _alowFolderChange;
        public bool AlowFolderChange
        {
            get { return _alowFolderChange; }
            set
            {
                _alowFolderChange = value;
                NotifyPropertyChanged("AlowFolderChange");
            }
        }


        private string _folder;
        public string Folder
        {
            get { return _folder; }
            set
            {
                if (_folder != value)
                {
                    if (!Directory.Exists(value))
                    {
                        try
                        {
                            Directory.CreateDirectory(value);
                        }
                        catch (Exception exception)
                        {
                            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), Name);
                            if (value != folder)
                                value = folder;
                            Log.Error("Error creating session folder", exception);
                        }
                    }
                    _systemWatcher.Path = value;
                    _systemWatcher.EnableRaisingEvents = true;
                    _systemWatcher.IncludeSubdirectories = true;
                }
                _folder = value;
                NotifyPropertyChanged("Folder");
            }
        }

        private string _fileNameTemplate;

        public string FileNameTemplate
        {
            get { return _fileNameTemplate; }
            set
            {
                _fileNameTemplate = value;
                NotifyPropertyChanged("FileNameTemplate");
            }
        }

        private int _counter;
        public int Counter
        {
            get { return _counter; }
            set
            {
                _counter = value;
                NotifyPropertyChanged("Counter");
            }
        }

        private bool _useOriginalFilename;
        public bool UseOriginalFilename
        {
            get { return _useOriginalFilename; }
            set
            {
                _useOriginalFilename = value;
                NotifyPropertyChanged("UseOriginalFilename");
            }
        }


        private AsyncObservableCollection<FileItem> _files;

        public AsyncObservableCollection<FileItem> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                NotifyPropertyChanged("Files");
            }
        }

        private TimeLapseClass _timeLapse;
        public TimeLapseClass TimeLapse
        {
            get { return _timeLapse; }
            set
            {
                _timeLapse = value;
                NotifyPropertyChanged("TimeLapse");
            }
        }

        private BraketingClass _braketing;
        public BraketingClass Braketing
        {
            get { return _braketing; }
            set
            {
                _braketing = value;
                NotifyPropertyChanged("Braketing");
            }
        }

        private AsyncObservableCollection<TagItem> _tags;
        public AsyncObservableCollection<TagItem> Tags
        {
            get { return _tags; }
            set
            {
                _tags = value;
                NotifyPropertyChanged("Tags");
            }
        }

        private bool _retainCameraCopy;
        public bool RetainCameraCopy
        {
            get { return _retainCameraCopy; }
            set
            {
                _retainCameraCopy = value;
                NotifyPropertyChanged("RetainCameraCopy");
            }
        }

        private int _barcodeClearDelay;
        public int BarcodeClearDelay
        {
            get { return _barcodeClearDelay; }
            set
            {
                _barcodeClearDelay = value;
                NotifyPropertyChanged("BarcodeClearDelay");
            }
        }

        private string _lastBarcode = "";
        public string LastBarcode
        {
            get { return _lastBarcode; }
            set
            {
                _lastBarcode = value;
                NotifyPropertyChanged("LastBarcode");
            }
        }

        private string _barcodeRegex = "";
        public string BarcodeRegex
        {
            get { return _barcodeRegex; }
            set
            {
                _barcodeRegex = value;
                NotifyPropertyChanged("BarcodeRegex");
            }
        }

        private string _barcodeDelimiter = "-";
        public string BarcodeDelimiter
        {
            get { return _barcodeDelimiter; }
            set
            {
                _barcodeDelimiter = (value + "-").Substring(0, 1);
                NotifyPropertyChanged("BarcodeDelimiter");
            }
        }

        private int _barcodeLengthMax;
        public int BarcodeLengthMax
        {
            get { return _barcodeLengthMax; }
            set
            {
                _barcodeLengthMax = value;
                NotifyPropertyChanged("BarcodeLengthMax");
            }
        }

        private int _barcodeLengthMin;
        public int BarcodeLengthMin
        {
            get { return _barcodeLengthMin; }
            set
            {
                _barcodeLengthMin = value;
                NotifyPropertyChanged("BarcodeLengthMin");
            }
        }

        private QuickTagOptions _quickTagOption;
        public QuickTagOptions QuickTagOption
        {
            get { return _quickTagOption; }
            set
            {   
                _quickTagOption = value;
                QuickTagMainEditable = _quickTagOption==QuickTagOptions.Set_From_Main;
                NotifyPropertyChanged("QuickTagOption");
            }
        }
        [XmlIgnore]
        public bool QuickTagMainEditable
        {
            get { return (_quickTagOption==QuickTagOptions.Set_From_Main); }
            set 
            {
                NotifyPropertyChanged("QuickTagMainEditable");
            }
        }

        private string _quickTag = "";
        public string QuickTag
        {
            get { return _quickTag; }
            set
            {
                _quickTag = value;
                NotifyPropertyChanged("QuickTag");
            }
        }

        private TagItem _selectedTag1;
        public TagItem SelectedTag1
        {
            get { return _selectedTag1; }
            set
            {
                _selectedTag1 = value;
                NotifyPropertyChanged("SelectedTag1");
            }
        }

        private TagItem _selectedTag2;
        public TagItem SelectedTag2
        {
            get { return _selectedTag2; }
            set
            {
                _selectedTag2 = value;
                NotifyPropertyChanged("SelectedTag2");
            }
        }

        private TagItem _selectedTag3;
        public TagItem SelectedTag3
        {
            get { return _selectedTag3; }
            set
            {
                _selectedTag3 = value;
                NotifyPropertyChanged("SelectedTag3");
            }
        }

        private TagItem _selectedTag4;
        public TagItem SelectedTag4
        {
            get { return _selectedTag4; }
            set
            {
                _selectedTag4 = value;
                NotifyPropertyChanged("SelectedTag4");
            }
        }


        private bool _useCameraCounter;
        public bool UseCameraCounter
        {
            get { return _useCameraCounter; }
            set
            {
                _useCameraCounter = value;
                NotifyPropertyChanged("UseCameraCounter");
            }
        }

        private bool _downloadOnlyJpg;
        public bool DownloadOnlyJpg
        {
            get { return _downloadOnlyJpg; }
            set
            {
                _downloadOnlyJpg = value;
                NotifyPropertyChanged("DownloadOnlyJpg");
            }
        }

        private int _leadingZeros;
        public int LeadingZeros
        {
            get { return _leadingZeros; }
            set
            {
                _leadingZeros = value;
                NotifyPropertyChanged("LeadingZeros");
            }
        }

        public string ConfigFile { get; set; }
        private FileSystemWatcher _systemWatcher;

        public PhotoSession()
        {
            _systemWatcher = new FileSystemWatcher();
            _systemWatcher.EnableRaisingEvents = false;
            _systemWatcher.Deleted += _systemWatcher_Deleted;
            _systemWatcher.Created += new FileSystemEventHandler(_systemWatcher_Created);

            Name = "Default";
            Braketing = new BraketingClass();
            Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), Name);
            Files = new AsyncObservableCollection<FileItem>();
            FileNameTemplate = "DSC_$C";
            TimeLapse = new TimeLapseClass();
            if (ServiceProvider.Settings != null && ServiceProvider.Settings.VideoTypes.Count > 0)
                TimeLapse.VideoType = ServiceProvider.Settings.VideoTypes[0];
            TimeLapse.OutputFIleName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                                                    Name + ".avi");
            UseOriginalFilename = false;
            AlowFolderChange = false;
            Tags = new AsyncObservableCollection<TagItem>();
            UseCameraCounter = false;
            DownloadOnlyJpg = false;
            LeadingZeros = 4;
        }

        void _systemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                //AddFile(e.FullPath);
            }
            catch (Exception exception)
            {
                Log.Error("Add file error", exception);
            }
        }

        void _systemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            FileItem deletedItem = null;
            lock (this)
            {
                foreach (FileItem fileItem in Files)
                {
                    if (fileItem.FileName == e.FullPath)
                        deletedItem = fileItem;
                }
            }
            try
            {
                if (deletedItem != null)
                    Files.Remove(deletedItem);
                //Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Files.Remove(
                //  deletedItem)));
            }
            catch (Exception)
            {


            }
        }

        public string GetNextFileName(string ext, ICameraDevice device)
        {
            lock (_locker)
            {
                if (string.IsNullOrEmpty(ext))
                    ext = ".nef";
                if (!string.IsNullOrEmpty(_lastFilename) && RawExtensions.Contains(ext.ToLower()) && !RawExtensions.Contains(Path.GetExtension(_lastFilename).ToLower()))
                {
                    string rawfile = Path.Combine(Folder,
                                                  FormatFileName(device, ext,false) + (!ext.StartsWith(".") ? "." : "") + ext);
                    if (!File.Exists(rawfile))
                        return rawfile;
                }
                    
                string fileName = Path.Combine(Folder,
                                               FormatFileName(device, ext) + (!ext.StartsWith(".") ? "." : "") + ext);
                if (File.Exists(fileName))
                    return GetNextFileName(ext, device);
                _lastFilename = fileName;
                return fileName;
            }
        }

        private string FormatFileName(ICameraDevice device, string ext, bool incremetCounter=true)
        {
            CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(device);
            string res = FileNameTemplate;
            if (!res.Contains("$C"))
                res += "$C";

            if (UseCameraCounter)
            {
                if (incremetCounter)
                    property.Counter = property.Counter + property.CounterInc;
                res = res.Replace("$C", property.Counter.ToString(new string('0', LeadingZeros)));
            }
            else
            {
                if (incremetCounter)
                    Counter++;
                res = res.Replace("$C", Counter.ToString(new string('0', LeadingZeros)));
            }
            res = res.Replace("$N", Name.Trim());
            if (device.ExposureCompensation != null)
                res = res.Replace("$E", device.ExposureCompensation.Value != "0" ? device.ExposureCompensation.Value : "");
            res = res.Replace("$D", DateTime.Now.ToString("yyyy-MM-dd"));

            res = res.Replace("$Type", GetType(ext));

            res = res.Replace("$UTime", ((long)(DateTime.UtcNow-new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds).ToString() );

            res = res.Replace("$Qt", (String.IsNullOrWhiteSpace(QuickTag) || ServiceProvider.Settings.DefaultSession.QuickTagOption==QuickTagOptions.None) ? "" : QuickTag+"_" );
            res = res.Replace("$B", LastBarcode);
            res = res.Replace("$-", BarcodeDelimiter.ToString());

            res = res.Replace("$X", property.DeviceName.Replace(":", "_").Replace("?", "_").Replace("*", "_"));
            res = res.Replace("$Tag1", SelectedTag1 != null ? SelectedTag1.Value.Trim() : "");
            res = res.Replace("$Tag2", SelectedTag1 != null ? SelectedTag2.Value.Trim() : "");
            res = res.Replace("$Tag3", SelectedTag1 != null ? SelectedTag3.Value.Trim() : "");
            res = res.Replace("$Tag4", SelectedTag1 != null ? SelectedTag4.Value.Trim() : "");
            //prevent multiple \ if a tag is empty 
            while (res.Contains(@"\\"))
            {
                res = res.Replace(@"\\", @"\");
            }
            // if the file name start with \ the Path.Combine isn't work right 
            if (res.StartsWith("\\"))
                res = res.Substring(1);
            return res;
        }

        private string GetType(string ext)
        {
            if (ext.StartsWith("."))
                ext = ext.Substring(1);
            switch (ext.ToLower())
            {
                case "jpg":
                    return "Jpg";
                case "nef":
                    return "Raw";
                case "cr2":
                    return "Raw";
                case "tif":
                    return "Tif";
            }
            return ext;
        }

        public FileItem AddFile(string fileName)
        {
            FileItem oitem = GetFile(fileName);
            if (oitem != null)
                return oitem;
            FileItem item = new FileItem(fileName);
            Files.Add(item);
            return item;
        }

        public bool ContainFile(string fileName)
        {
            foreach (FileItem fileItem in Files)
            {
                if (fileItem.FileName.ToUpper() == fileName.ToUpper())
                    return true;
            }
            return false;
        }

        public FileItem GetFile(string fileName)
        {
            foreach (FileItem fileItem in Files)
            {
                if (fileItem.FileName.ToUpper() == fileName.ToUpper())
                    return fileItem;
            }
            return null;
        }

        public override string ToString()
        {
            return Name;
        }

        public AsyncObservableCollection<FileItem> GetSelectedFiles()
        {
            AsyncObservableCollection<FileItem> list = new AsyncObservableCollection<FileItem>();
            foreach (FileItem fileItem in Files)
            {
                if (fileItem.IsChecked)
                    list.Add(fileItem);
            }
            return list;
        }

        public void SelectAll()
        {
            foreach (FileItem fileItem in Files)
            {
                fileItem.IsChecked = true;
            }
        }

        public void SelectNone()
        {
            foreach (FileItem fileItem in Files)
            {
                fileItem.IsChecked = false;
            }
        }

        public void PurgeOldFiles(TimeSpan retentionPeriod)
        {
            // default: keep files from last 15 minutes.
            if (retentionPeriod == null) retentionPeriod = new TimeSpan(0, 0, 15, 0);
            DateTime retentionCutoff = DateTime.Now - retentionPeriod;
            
            // Remove session entries
            var files = Files;
            for (int i = files.Count - 1; i >= 0; i--)
                if (files[i].FileDate < retentionCutoff)
                    files.RemoveAt(i);
            ServiceProvider.Settings.Save(this);

            // Purge files
            var cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Settings.AppName, "Cache");
            var cachePathLarge = Path.Combine(cachePath, "Large");
            var cachePathSmall = Path.Combine(cachePath, "Small");
            if (!Directory.Exists(cachePath) || !Directory.Exists(cachePathLarge) || !Directory.Exists(cachePathSmall)) return;

            foreach (var file in Directory.GetFiles(cachePathLarge))
            {
                try
                {
                    if (File.Exists(file) && File.GetCreationTime(file) < retentionCutoff)
                        File.Delete(file);
                }
                catch (Exception e) { Log.Error(e.ToString()); }
            }
            foreach (var file in Directory.GetFiles(cachePathSmall))
            {
                try
                {
                    if (File.Exists(file) && File.GetCreationTime(file) < retentionCutoff)
                        File.Delete(file);
                }
                catch (Exception e) { Log.Error(e.ToString()); }
            }
        }

    }


    public enum QuickTagOptions
    {
        None = 0,
        Set_From_Session = 1,
        Set_From_Main = 2
    }

}
