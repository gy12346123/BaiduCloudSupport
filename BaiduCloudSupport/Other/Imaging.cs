using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BaiduCloudSupport.Other
{
    /// <summary>
    /// Image converter
    /// </summary>
    public static class Imaging
    {

        /// <summary>
        /// Bitmap to BitmapSource
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource CreateBitmapSourceFromBitmap(ref Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            if (Application.Current.Dispatcher == null)
                return null; // Is it possible?

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // You need to specify the image format to fill the stream. 
                // I'm assuming it is PNG
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Make sure to create the bitmap in the UI thread
                if (InvokeRequired)
                    return (BitmapSource)Application.Current.Dispatcher.Invoke(
                        new Func<Stream, BitmapSource>(CreateBitmapSourceFromBitmap),
                        DispatcherPriority.Normal,
                        memoryStream);
                return CreateBitmapSourceFromBitmap(memoryStream);
            }
        }

        /// <summary>
        /// BitmapSource to Bitmap
        /// </summary>
        /// <param name="s">BitmapSource</param>
        /// <returns>Bitmap</returns>
        public static System.Drawing.Bitmap WpfBitmapSourceToBitmap(BitmapSource s)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(s.PixelWidth, s.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            s.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        /// <summary>
        /// Cut bitmap and return new size bitmap resource
        /// </summary>
        /// <param name="b">raw bitmap</param>
        /// <param name="StartX">start X point</param>
        /// <param name="StartY">start Y point</param>
        /// <param name="iWidth">cut width</param>
        /// <param name="iHeight">cut height</param>
        /// <returns></returns>
        public static Bitmap CutBitmap(Bitmap b, int StartX, int StartY, int iWidth, int iHeight)
        {
            if (b == null)
            {
                return null;
            }

            int w = b.Width;
            int h = b.Height;

            if (StartX >= w || StartY >= h)
            {
                return null;
            }

            if (StartX + iWidth > w)
            {
                iWidth = w - StartX;
            }

            if (StartY + iHeight > h)
            {
                iHeight = h - StartY;
            }

            Bitmap bmpOut = new Bitmap(iWidth, iHeight, PixelFormat.Format24bppRgb);

            Graphics g = Graphics.FromImage(bmpOut);
            g.DrawImage(b, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(StartX, StartY, iWidth, iHeight), GraphicsUnit.Pixel);
            g.Dispose();

            return bmpOut;
        }

        private static bool InvokeRequired
        {
            get { return Dispatcher.CurrentDispatcher != Application.Current.Dispatcher; }
        }

        private static BitmapSource CreateBitmapSourceFromBitmap(Stream stream)
        {
            BitmapDecoder bitmapDecoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            // This will disconnect the stream from the image completely...
            WriteableBitmap writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
            writable.Freeze();

            return writable;
        }

        /// <summary>
        /// Save image as file from WriteableBitmap
        /// </summary>
        /// <param name="wtbBmp">WriteableBitmap</param>
        /// <param name="path">full path with file name</param>
        public static void SaveWriteableBitmap(WriteableBitmap wtbBmp, string path)
        {
            if (wtbBmp == null)
            {
                return;
            }
            RenderTargetBitmap rtbitmap = new RenderTargetBitmap(wtbBmp.PixelWidth, wtbBmp.PixelHeight, wtbBmp.DpiX, wtbBmp.DpiY, System.Windows.Media.PixelFormats.Default);
            System.Windows.Media.DrawingVisual drawingVisual = new System.Windows.Media.DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawImage(wtbBmp, new Rect(0, 0, wtbBmp.Width, wtbBmp.Height));
            }
            rtbitmap.Render(drawingVisual);
            JpegBitmapEncoder bitmapEncoder = new JpegBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(rtbitmap));
            //string strDir = @"Images\";
            //string strpath = strDir + DateTime.Now.ToString("yyyyMMddfff") + ".jpg";
            if (!File.Exists(path))
            {
                MemoryStream ms = new MemoryStream();
                //bitmapEncoder.Save(File.OpenWrite(path));
                bitmapEncoder.Save(ms);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(ms);
                bitmap.Save(path);
            }
        }
    }
}
