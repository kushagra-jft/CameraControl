using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using CameraControl.Core.Classes.Queue;
using CameraControl.Core.Exif.EXIF;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using FreeImageAPI;

namespace CameraControl.Core.Classes
{

    public enum FileItemType
    {
        File,
        CameraObject,
        Missing

    }

    public class FileItem : BaseFieldClass
    {

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                if (File.Exists(_fileName))
                {
                    FileDate = File.GetLastWriteTime(_fileName);
                }
                NotifyPropertyChanged("FileName");
            }
        }

        public string DestinationFilename { get; set; }

        public bool IsRaw
        {
            get { return !string.IsNullOrEmpty(FileName) && Path.GetExtension(FileName).ToLower() == ".nef"; }
        }

        public DateTime FileDate { get; set; }

        public string Name { get; set; }

        [XmlIgnore]
        public string ToolTip
        {
            get { return string.Format("File name: {0}\nFile date :{1}", FileName, FileDate.ToString()); }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                NotifyPropertyChanged("IsChecked");
            }
        }

        private BitmapImage _bitmapImage;
        [XmlIgnore]
        public BitmapImage BitmapImage
        {
            get
            {
                return _bitmapImage;
            }
            set
            {
                if (value == null)
                {

                }
                _bitmapImage = value;
                NotifyPropertyChanged("BitmapImage");
            }
        }

        private DeviceObject _deviceObject;
        [XmlIgnore]
        public DeviceObject DeviceObject
        {
            get { return _deviceObject; }
            set
            {
                _deviceObject = value;
                NotifyPropertyChanged("DeviceObject");
            }
        }

        private ICameraDevice _device;
        [XmlIgnore]
        public ICameraDevice Device
        {
            get { return _device; }
            set
            {
                _device = value;
                NotifyPropertyChanged("Device");
            }
        }

        private FileItemType _itemType;
        [XmlIgnore]
        public FileItemType ItemType
        {
            get { return _itemType; }
            set
            {
                _itemType = value;
                NotifyPropertyChanged("ItemType");
            }
        }

        public FileItem()
        {
            IsLoaded = false;
            FileInfo = new FileInfo();
        }

        public FileItem(DeviceObject deviceObject, ICameraDevice device)
        {
            Device = device;
            DeviceObject = deviceObject;
            ItemType = FileItemType.CameraObject;
            FileName = deviceObject.FileName;
            FileDate = deviceObject.FileDate;
            IsChecked = true;
            if (deviceObject.ThumbData != null && deviceObject.ThumbData.Length > 4)
            {
                try
                {
                    var stream = new MemoryStream(deviceObject.ThumbData, 0, deviceObject.ThumbData.Length);

                    using (var bmp = new Bitmap(stream))
                    {
                        Thumbnail = BitmapSourceConvert.ToBitmapSource(bmp);
                    }
                    stream.Close();
                }
                catch (Exception exception)
                {
                    Log.Debug("Error loading device thumb ", exception);
                }
            }
        }


        public FileItem(string file)
        {
            IsLoaded = false;
            FileName = file;
            Name = Path.GetFileName(file);
            ItemType = FileItemType.File;
            FileInfo = new FileInfo();
        }

        public FileItem(ICameraDevice device, DateTime time)
        {
            Device = device;
            ItemType = FileItemType.CameraObject;
            IsChecked = true;
            ItemType = FileItemType.Missing;
            FileName = "Missing";
            FileDate = time;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            if(!string.IsNullOrEmpty(FileName) && File.Exists(FileName))
            {
                long hash = FileName.GetHashCode() +File.GetCreationTime(FileName).GetHashCode();
                return hash.GetHashCode();
            }
            return base.GetHashCode();
        }


        public int Orientation { get; set; }
        public bool IsLoaded { get; set; }
        public string InfoLabel { get; set; }
        public FileInfo FileInfo { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public string SmallThumb
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                    Settings.AppName, "Cache\\Small", GetHashCode() + ".jpg");
            }
        }

        public string LargeThumb
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                    Settings.AppName, "Cache\\Large", GetHashCode() + ".jpg");
            }
        }

        private BitmapSource _thumbnail;
        [XmlIgnore]
        public BitmapSource Thumbnail
        {
            get
            {
                if (_thumbnail == null)
                {
                    if (File.Exists(SmallThumb))
                    {
                        _thumbnail = BitmapLoader.Instance.LoadSmallImage(this);
                    }
                    else
                    {
                        _thumbnail = ItemType == FileItemType.Missing
                                         ? BitmapLoader.Instance.NoImageThumbnail
                                         : BitmapLoader.Instance.DefaultThumbnail;
                        if (!ServiceProvider.Settings.DontLoadThumbnails)
                            ServiceProvider.QueueManager.Add(new QueueItemFileItem {FileItem = this});
                    }
                }
                return _thumbnail;
            }
            set
            {
                _thumbnail = value;
                NotifyPropertyChanged("Thumbnail");
            }
        }

        public void GetExtendedThumb()
        {
            if (ItemType != FileItemType.File)
                return;
            try
            {
                if (IsRaw)
                {
                    try
                    {
                        BitmapDecoder bmpDec = BitmapDecoder.Create(new Uri(FileName),
                                                                    BitmapCreateOptions.None,
                                                                    BitmapCacheOption.OnLoad);
                        if (bmpDec.Thumbnail != null)
                        {
                            WriteableBitmap bitmap = new WriteableBitmap(bmpDec.Thumbnail);
                            bitmap.Freeze();
                            Thumbnail = bitmap;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    Image.GetThumbnailImageAbort myCallback = ThumbnailCallback;
                    Stream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // or any stream
                    Image tempImage = Image.FromStream(fs, true, false);
                    fs.Close();

                    Thumbnail =
                      BitmapSourceConvert.ToBitmapSource(
                        (Bitmap)tempImage.GetThumbnailImage(160, 120, myCallback, IntPtr.Zero));
                    tempImage.Dispose();
                    
                }
            }
            catch (Exception exception)
            {
                Log.Debug("Unable load thumbnail: " + FileName, exception);
            }
        }

        private bool ThumbnailCallback()
        {
            return false;
        }
    }
}
