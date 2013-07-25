using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CameraControl.Devices;

namespace CameraControl.Core.Classes
{
    public class BitmapLoader
    {
        private const int LargeThumbSize = 1920;
        private const int SmallThumbSize = 255;

        private static BitmapLoader _instance;
        public static BitmapLoader Instance
        {
            get { return _instance ?? (_instance = new BitmapLoader()); }
            set { _instance = value; }
        }

        private BitmapImage _defaultThumbnail;
        public BitmapImage DefaultThumbnail
        {
            get
            {
                if (_defaultThumbnail == null)
                {
                    if (!string.IsNullOrEmpty(ServiceProvider.Branding.DefaultThumbImage) &&
                        File.Exists(ServiceProvider.Branding.DefaultThumbImage))
                    {
                        BitmapImage bi = new BitmapImage();
                        // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                        bi.BeginInit();
                        bi.UriSource = new Uri(ServiceProvider.Branding.DefaultThumbImage);
                        bi.EndInit();
                        _defaultThumbnail = bi;
                    }
                    else
                    {
                        _defaultThumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/logo.png"));
                    }
                }
                return _defaultThumbnail;
            }
            set { _defaultThumbnail = value; }
        }


        private BitmapImage _noImageThumbnail;

        public BitmapImage NoImageThumbnail
        {
            get
            {
                if (_noImageThumbnail == null)
                {
                    if (!string.IsNullOrEmpty(ServiceProvider.Branding.DefaultMissingThumbImage) &&
                        File.Exists(ServiceProvider.Branding.DefaultMissingThumbImage))
                    {
                        BitmapImage bi = new BitmapImage();
                        // BitmapImage.UriSource must be in a BeginInit/EndInit block.
                        bi.BeginInit();
                        bi.UriSource = new Uri(ServiceProvider.Branding.DefaultMissingThumbImage);
                        bi.EndInit();
                        _noImageThumbnail = bi;
                    }
                    else
                    {
                        _noImageThumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/NoImage.png"));
                    }
                }
                return _noImageThumbnail;
            }
            set { _noImageThumbnail = value; }
        }

        public void GenerateCache(FileItem fileItem)
        {
            if (fileItem == null)
                return;
            if (!File.Exists(fileItem.FileName) || fileItem.IsLoaded)
                return;
            GetMetadata(fileItem);
            try
            {
                BitmapDecoder bmpDec = BitmapDecoder.Create(new Uri(fileItem.FileName),
                                                            BitmapCreateOptions.None,
                                                            BitmapCacheOption.Default);
                WriteableBitmap writeableBitmap = BitmapFactory.ConvertToPbgra32Format(bmpDec.Frames[0]);

                fileItem.Width = writeableBitmap.PixelWidth;
                fileItem.Height = writeableBitmap.PixelHeight;

                double dw = (double) LargeThumbSize/writeableBitmap.PixelWidth;
                writeableBitmap = writeableBitmap.Resize((int) (writeableBitmap.PixelWidth*dw),
                                                         (int) (writeableBitmap.PixelHeight*dw),
                                                         WriteableBitmapExtensions.Interpolation.Bilinear);
                LoadHistogram(fileItem, writeableBitmap);
                Save2Jpg(writeableBitmap, fileItem.LargeThumb);
                dw = (double) SmallThumbSize/writeableBitmap.PixelWidth;
                writeableBitmap = writeableBitmap.Resize((int) (writeableBitmap.PixelWidth*dw),
                                                         (int) (writeableBitmap.PixelHeight*dw),
                                                         WriteableBitmapExtensions.Interpolation.Bilinear);
                Save2Jpg(writeableBitmap, fileItem.SmallThumb);
                fileItem.IsLoaded = true;
            }
            catch (Exception exception)
            {
                Log.Error("Error generating cache", exception);
            }
        }

        public static void Save2Jpg(BitmapSource source, string filename)
        {
            string dir = Path.GetDirectoryName(filename);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);
                stream.Close();
            }
        }

        private unsafe void LoadHistogram(FileItem fileItem, WriteableBitmap bitmap)
        {
            fileItem.FileInfo.HistogramBlue = new int[256];
            fileItem.FileInfo.HistogramGreen = new int[256];
            fileItem.FileInfo.HistogramRed = new int[256];
            fileItem.FileInfo.HistogramLuminance = new int[256];
            using (BitmapContext bitmapContext = bitmap.GetBitmapContext())
            {
                for (var i = 0; i < bitmapContext.Width*bitmapContext.Height; i++)
                {

                    int num1 = bitmapContext.Pixels[i];
                    byte a = (byte) (num1 >> 24);
                    int num2 = (int) a;
                    if (num2 == 0)
                        num2 = 1;
                    int num3 = 65280/num2;
                    byte R = (byte) ((num1 >> 16 & (int) byte.MaxValue)*num3 >> 8);
                    byte G = (byte) ((num1 >> 8 & (int) byte.MaxValue)*num3 >> 8);
                    byte B = (byte) ((num1 & (int) byte.MaxValue)*num3 >> 8);

                    fileItem.FileInfo.HistogramBlue[B]++;
                    fileItem.FileInfo.HistogramGreen[G]++;
                    fileItem.FileInfo.HistogramRed[R]++;
                    int lum = (R + R + R + B + G + G + G + G) >> 3;
                    fileItem.FileInfo.HistogramLuminance[lum]++;
                }
            }

        }

        public unsafe void Highlight(BitmapFile file, bool under , bool over)
        {
            if (!under && !over)
                return;
            if (file == null || file.DisplayImage == null)
                return;
            WriteableBitmap bitmap = file.DisplayImage.Clone();
            int color1 = ConvertColor(Colors.Blue);
            int color2 = ConvertColor(Colors.Red);
            int treshold = 3;
            using (BitmapContext bitmapContext = bitmap.GetBitmapContext())
            {
                for (var i = 0; i < bitmapContext.Width * bitmapContext.Height; i++)
                {

                    int num1 = bitmapContext.Pixels[i];
                    byte a = (byte)(num1 >> 24);
                    int num2 = (int)a;
                    if (num2 == 0)
                        num2 = 1;
                    int num3 = 65280 / num2;
                    //Color col = Color.FromArgb(a, (byte)((num1 >> 16 & (int)byte.MaxValue) * num3 >> 8),
                    //                           (byte)((num1 >> 8 & (int)byte.MaxValue) * num3 >> 8),
                    //                           (byte)((num1 & (int)byte.MaxValue) * num3 >> 8));
                    byte R = (byte)((num1 >> 16 & byte.MaxValue) * num3 >> 8);
                    byte G = (byte)((num1 >> 8 & byte.MaxValue) * num3 >> 8);
                    byte B = (byte)((num1 & byte.MaxValue) * num3 >> 8);

                    if ( under && R < treshold && G < treshold && B < treshold)
                        bitmapContext.Pixels[i] = color1;
                    if (over && R > 255 - treshold && G > 255 - treshold && B > 255 - treshold)
                        bitmapContext.Pixels[i] = color2;
                }
            }
            bitmap.Freeze();
            file.DisplayImage = bitmap;
        }

        public void SetData(BitmapFile file,FileItem fileItem)
        {
            if (fileItem == null)
                return;
            file.Metadata.Clear();
            foreach (ValuePair item in fileItem.FileInfo.ExifTags.Items)
            {
                file.Metadata.Add(new DictionaryItem(){Name = item.Name,Value = item.Value});
            }
            file.BlueColorHistogramPoints = ConvertToPointCollection(fileItem.FileInfo.HistogramBlue);
            file.RedColorHistogramPoints = ConvertToPointCollection(fileItem.FileInfo.HistogramRed);
            file.GreenColorHistogramPoints = ConvertToPointCollection(fileItem.FileInfo.HistogramGreen);
            file.LuminanceHistogramPoints = ConvertToPointCollection(fileItem.FileInfo.HistogramLuminance);

            file.InfoLabel = Path.GetFileName(file.FileItem.FileName);
            file.InfoLabel += String.Format(" | {0}x{1}", fileItem.Width, fileItem.Height);
            if (fileItem.FileInfo.ExifTags.ContainName("Exif.Photo.ExposureTime"))
                file.InfoLabel += " | E " + fileItem.FileInfo.ExifTags["Exif.Photo.ExposureTime"];
            if (fileItem.FileInfo.ExifTags.ContainName("Exif.Photo.FNumber"))
                file.InfoLabel += " | " + fileItem.FileInfo.ExifTags["Exif.Photo.FNumber"];
            if (fileItem.FileInfo.ExifTags.ContainName("Exif.Photo.ISOSpeedRatings"))
                file.InfoLabel += " | ISO " + fileItem.FileInfo.ExifTags["Exif.Photo.ISOSpeedRatings"];
            if (fileItem.FileInfo.ExifTags.ContainName("Exif.Photo.ExposureBiasValue"))
                file.InfoLabel += " | " + fileItem.FileInfo.ExifTags["Exif.Photo.ExposureBiasValue"];
            if (fileItem.FileInfo.ExifTags.ContainName("Exif.Photo.FocalLength"))
                file.InfoLabel += " | " + fileItem.FileInfo.ExifTags["Exif.Photo.FocalLength"];

        }

        public WriteableBitmap LoadImage(FileItem fileItem, bool fullres)
        {
            if (fileItem == null)
                return null;
            if (!File.Exists(fileItem.LargeThumb) && !fullres)
                return null;
            try
            {
                BitmapDecoder bmpDec = BitmapDecoder.Create(new Uri(fullres ? fileItem.FileName : fileItem.LargeThumb),
                                                            BitmapCreateOptions.None,
                                                            BitmapCacheOption.Default);

                WriteableBitmap bitmap = BitmapFactory.ConvertToPbgra32Format(bmpDec.Frames[0]);
                
                if (ServiceProvider.Settings.ShowFocusPoints)
                    DrawFocusPoints(fileItem, bitmap);
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception exception)
            {
                Log.Error("Error loading image", exception);
            }
            return null;
        }

        public WriteableBitmap LoadSmallImage(FileItem fileItem)
        {
            if (!File.Exists(fileItem.SmallThumb))
                return null;
            try
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.UriSource = new Uri(fileItem.SmallThumb);
                bi.EndInit();
                WriteableBitmap bitmap = BitmapFactory.ConvertToPbgra32Format(bi);
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception exception)
            {
                Log.Error("Error loading image", exception);
            }
            return null;
        }

        public void GetMetadata(FileItem fileItem)
        {
            Exiv2Helper exiv2Helper = new Exiv2Helper();
            try
            {
                exiv2Helper.Load(fileItem);
            }
            catch (Exception exception)
            {
                Log.Error("Error loading metadata ", exception);
            }
        }

        private void DrawFocusPoints(FileItem fileItem, WriteableBitmap bitmap)
        {
            bitmap.Lock();
            double dw = (double)bitmap.PixelWidth / fileItem.Width;
            double dh = (double)bitmap.PixelHeight / fileItem.Height;

            foreach (Rect focuspoint in fileItem.FileInfo.FocusPoints)
            {
                DrawRect(bitmap, (int) (focuspoint.X*dw), (int) (focuspoint.Y*dh),
                         (int) ((focuspoint.X + focuspoint.Width)*dw),
                         (int) ((focuspoint.Y + focuspoint.Height)*dh), Colors.Aqua, fileItem.Width/1000*2);
            }
            bitmap.Unlock();
        }

        void DrawRect(WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color, int line)
        {
            for (int i = 0; i < line; i++)
            {
                bmp.DrawRectangle(x1 - i, y1 - i, x2 - i, y2 - i, color);
            }
        }

        private PointCollection ConvertToPointCollection(int[] values)
        {
            PointCollection points = new PointCollection();
            if (values == null)
            {
                points.Freeze();
                return points;
            }

            //values = SmoothHistogram(values);

            int max = values.Max();


            // first point (lower-left corner)
            points.Add(new System.Windows.Point(0, max));
            // middle points
            for (int i = 0; i < values.Length; i++)
            {
                points.Add(new System.Windows.Point(i, max - values[i]));
            }
            // last point (lower-right corner)
            points.Add(new System.Windows.Point(values.Length - 1, max));
            points.Freeze();
            return points;
        }

        private int[] SmoothHistogram(int[] originalValues)
        {
            int[] smoothedValues = new int[originalValues.Length];

            double[] mask = new double[] { 0.25, 0.5, 0.25 };

            for (int bin = 1; bin < originalValues.Length - 1; bin++)
            {
                double smoothedValue = 0;
                for (int i = 0; i < mask.Length; i++)
                {
                    smoothedValue += originalValues[bin - 1 + i] * mask[i];
                }
                smoothedValues[bin] = (int)smoothedValue;
            }

            return smoothedValues;
        }

        private static int ConvertColor(Color color)
        {
            int num = (int)color.A + 1;
            return (int)color.A << 24 | (int)(byte)((int)color.R * num >> 8) << 16 | (int)(byte)((int)color.G * num >> 8) << 8 | (int)(byte)((int)color.B * num >> 8);
        }
    }
}
