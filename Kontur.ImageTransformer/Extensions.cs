﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Kontur.ImageTransformer
{
    public static class Extensions
    {
        /// <summary>
        ///  Convert Request content to Bitmap
        /// </summary>
        /// <param name="request"></param>
        /// <param name="img"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryToBitmap(this HttpRequestMessage request, out Bitmap img)
        {
            try
            {
                img = new Bitmap(request.Content.ReadAsStreamAsync().Result);
            }
            catch
            {
                img = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copy part of 32bppArgb Bitmap to int array
        /// </summary>
        /// <param name="img">Source Bitmap</param>
        /// <param name="area">Rectangle for copied to array</param>
        /// <returns>Array with ARGB pixels</returns>
        /// <exception cref="ArgumentException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] ToArray(this Bitmap img, Rectangle area)
        {
            if (img.PixelFormat != PixelFormat.Format32bppArgb || area.IsEmpty)
            {
                throw new ArgumentException();
            }

            var bytes = area.Width * area.Height;
            var imgArray = new int[bytes];
            var bmpData = img.LockBits(area, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bmpData.Scan0, imgArray, 0, bytes);
            img.UnlockBits(bmpData);
            return imgArray;
        }

        /// <summary>
        /// Partially implementing Image.RotateFlip (RotateFlipType) from System.Drawing for Rectangle.
        /// Incorrect for default usage, only Kontur.ImageTransformer
        /// </summary>
        /// <param name="rect">Rectangle for transformation</param>
        /// <param name="rotateFlipType">COmbination of rotate and flip</param>
        /// <exception cref="NotFiniteNumberException">I implemented only needed consts</exception>
        public static void RotateFlip(this Rectangle rect, RotateFlipType rotateFlipType, int srcW, int srcH)
        {
            int t;
            switch (rotateFlipType)
            {
                case RotateFlipType.RotateNoneFlipNone: // == Rotate180FlipXY
                    break;
                case RotateFlipType.Rotate90FlipNone: // == Rotate270FlipXY cw
                    rect.X = rect.Y;
                    rect.Y = rect.Y;
                    t = rect.Width;
                    rect.Width = -rect.Width;
                    rect.Height = rect.Width;
                    break;
                case RotateFlipType.Rotate270FlipNone: // == Rotate90FlipXY ccw
                    rect.X += rect.Width;
                    rect.Y = rect.Y ;
                    t = rect.Width;
                    rect.Width = -rect.Height;
                    rect.Height = t;
                    break;
                case RotateFlipType.RotateNoneFlipX: // == Rotate180FlipY
                    rect.X += rect.Width;
                    rect.Y = rect.Y;
                    rect.Width = -rect.Width;
                    rect.Height = rect.Height;
                    break;
                case RotateFlipType.RotateNoneFlipY: // == Rotate180FlipX
                    rect.X = rect.X;
                    rect.Y += rect.Height;
                    rect.Width = rect.Width;
                    rect.Height = -rect.Height;
                    break;
                default:
                    throw new NotFiniteNumberException();
            }
        }

        /// <summary>
        /// Convert int array to 32bppArgb Bitmap
        /// </summary>
        /// <param name="imgArray">Array with ARGB pixel data</param>
        /// <param name="width">Width of new Bitmap</param>
        /// <param name="height">Height of new Bitmap</param>
        /// <returns>Bitmap with imgArray pixel data</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bitmap ToBitmap(this int[] imgArray, int width, int height)
        {
            if (width * height != imgArray.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            var img = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var bmpData = img.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            Marshal.Copy(imgArray, 0, bmpData.Scan0, width * height);
            img.UnlockBits(bmpData);
            return img;
        }
    }
}