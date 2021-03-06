﻿using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kontur.ImageTransformer
{
    /// <summary>
    /// Filtering Bitmap
    /// </summary>
    public static class ImageFilters
    {
        public delegate int Filter(uint pix, byte level = 0);

        /// <summary>
        /// Precalculated values for grayscale filter in ARGB format. Default alpha channel is 0 
        /// </summary>
        public static readonly uint[] GrayUInt = (from r in Enumerable.Range(0x00, 0x100)
                from g in Enumerable.Range(0x00, 0x100)
                from b in Enumerable.Range(0x00, 0x100)
                select (uint) ((((GrayPix(r, g, b) << 8) + GrayPix(r, g, b)) << 8) + GrayPix(r, g, b))
            ).ToArray();

        /// <summary>
        /// Precalculated values for sepia filter in ARGB format. Default alpha channel is 0 
        /// </summary>
        public static readonly uint[] SepiaUInt = (from r in Enumerable.Range(0x00, 0x100)
                from g in Enumerable.Range(0x00, 0x100)
                from b in Enumerable.Range(0x00, 0x100)
                select (uint) ((((SepiaR(r, g, b) << 8) + SepiaG(r, g, b)) << 8) + SepiaB(r, g, b))
            ).ToArray();


        /// <summary>
        /// Implementation of grayscale filter with save source alpha. It's not grayscale, it's AVERAGE filter:
        /// R = G = B = (oldR + oldG + oldB) / 3 
        /// </summary>
        /// <param name="pixel">Pixel in ARGB format</param>
        /// <param name="level">Unused argument</param>
        /// <returns>Filtered pixel with saved source alpha channel</returns>
        public static int GrayscaleFilter(uint pixel, byte level = 0) =>
            (int) (GrayUInt[pixel & 0x00FFFFFF] | pixel & 0xFF000000);

        /// <summary>
        /// Implementation of sepia filter with save source alpha channel. Give precalculated value from static array.
        /// newtRed = (oldRed * .393) + (oldGreen *.769) + (oldBlue * .189)
        /// newGreen = (oldRed * .349) + (oldGreen *.686) + (oldBlue * .168)
        /// newBlue = (oldRed * .272) + (oldGreen *.534) + (oldBlue * .131)
        /// </summary>
        /// <param name="pixel">Pixel in ARGB format</param>
        /// <param name="level">Unused argument</param>
        /// <returns>Filtered pixel with saved source alpha channel</returns>
        public static int SepiaFilter(uint pixel, byte level = 0) =>
            (int) (SepiaUInt[pixel & 0x00FFFFFF] | pixel & 0xFF000000);

        /// <summary>
        /// Implementation of threshold filter. Get a last byte from precalculated average filter values.
        /// If (AVG[pixel] >= level)
        ///     R = G = B = 0xFF
        /// else
        ///     R = G = B = 0x0
        /// </summary>
        /// <param name="pixel">Pixel in ARGB format</param>
        /// <param name="level">Intensity of threshold filter [0..255]</param>
        /// <returns>Filtered pixel with saved source alpha channel</returns>
        public static int ThresholdFilter(uint pixel, byte level = 0)
        {
            var k = (byte) (GrayUInt[pixel & 0x00FFFFFF] & 0x000000FF);

            return (int) ((k < level ? 0x0u : 0x00FFFFFFu) | pixel & 0xFF000000);
        }

        /// <summary>
        /// Return Filter delegate with  
        /// </summary>
        /// <param name="flt">String implementation of Filter</param>
        /// <param name="level">Variable for threshold filter level</param>
        /// <returns>Filter delegate</returns>
        /// <exception cref="ArgumentException"></exception>
        public static Filter FromString(string flt, out byte level)
        {
            level = 0;
            if (flt.Equals("grayscale"))
            {
                return GrayscaleFilter;
            }

            if (flt.Equals("sepia"))
            {
                return SepiaFilter;
            }

            if (Regex.IsMatch(flt, @"^threshold\(([0-9]|[1-9][0-9]|100)\)$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline))
            {
                byte.TryParse(Regex.Match(flt, @"\d+").Value, out level);
                return ThresholdFilter;
            }

            throw new ArgumentException();
        }

        private static int GrayPix(int r, int g, int b) => (r + g + b) / 3;

        private static int SepiaR(int r, int g, int b) => Math.Min((int) (r * .393 + g * .769 + b * .189), 255);

        private static int SepiaG(int r, int g, int b) => Math.Min((int) (r * .349 + g * .686 + b * .168), 255);

        private static int SepiaB(int r, int g, int b) => Math.Min((int) (r * .272 + g * .534 + b * .131), 255);
    }
}