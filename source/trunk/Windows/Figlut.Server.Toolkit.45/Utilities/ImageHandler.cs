﻿namespace Figlut.Server.Toolkit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Drawing.Drawing2D;

    #endregion //Using Directives

    public class ImageHandler
    {
        #region Methods

        public static void RotateImageFile(
    string filePath,
    RotateFlipType rotateFlipType)
        {
            FileSystemHelper.ValidateFileExists(filePath);
            using (Image image = Image.FromFile(filePath))
            {
                image.RotateFlip(rotateFlipType);
                image.Save(filePath, ImageFormat.Png);
            }
        }

        /// <summary>
        /// Method to resize, convert and save the image.
        /// </summary>
        /// <param name="image">Bitmap image.</param>
        /// <param name="maxWidth">resize width.</param>
        /// <param name="maxHeight">resize height.</param>
        /// <param name="quality">quality setting value.</param>
        /// <param name="filePath">file path.</param>      
        public static void ResizeImage(Bitmap image, int maxWidth, int maxHeight, int quality, string filePath)
        {
            // Get the image's original width and height
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            // To preserve the aspect ratio
            float ratioX = (float)maxWidth / (float)originalWidth;
            float ratioY = (float)maxHeight / (float)originalHeight;
            float ratio = Math.Min(ratioX, ratioY);

            // New width and height based on aspect ratio
            int newWidth = (int)(originalWidth * ratio);
            int newHeight = (int)(originalHeight * ratio);

            // Convert other formats (including CMYK) to RGB.
            using (Bitmap newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb))
            {
                // Draws the image in the specified size with quality mode set to HighQuality
                using (Graphics graphics = Graphics.FromImage(newImage))
                {
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                }
                // Get an ImageCodecInfo object that represents the JPEG codec.
                ImageCodecInfo imageCodecInfo = GetEncoderInfo(ImageFormat.Jpeg);

                // Create an Encoder object for the Quality parameter.
                System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;

                // Create an EncoderParameters object. 
                using (EncoderParameters encoderParameters = new EncoderParameters(1))
                {
                    // Save the image as a JPEG file with quality level.
                    using (EncoderParameter encoderParameter = new EncoderParameter(encoder, quality))
                    {
                        encoderParameters.Param[0] = encoderParameter;
                        //newImage.Save(filePath, imageCodecInfo, encoderParameters);
                        using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
                        {
                            newImage.Save(fs, imageCodecInfo, encoderParameters);
                            fs.Close();
                        }
                    }
                }
                newImage.Dispose();
            }
        }

        public static void SaveImage(string filePath, byte[] imageBytes)
        {
            EncoderParameter param = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 1);
            ImageCodecInfo jpegCodec = GetEncoderInfo(@"image/bmp");
            EncoderParameters encoderParams = new EncoderParameters(100);
            encoderParams.Param[0] = param;
            using (MemoryStream ms = new MemoryStream())
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Write(imageBytes, 0, imageBytes.Length);
                }
            }
        }

        public static Image GetImageFromBytes(byte[] imageBytes)
        {
            return GetImageFromBytes(imageBytes, null);
        }

        public static Image GetImageFromBytes(byte[] imageBytes, string mimeType)
        {
            EncoderParameter param = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 1);
            if (string.IsNullOrEmpty(mimeType))
            {
                mimeType = @"image/bmp";
            }
            ImageCodecInfo jpegCodec = GetEncoderInfo(mimeType);
            EncoderParameters encoderParams = new EncoderParameters(100);
            encoderParams.Param[0] = param;
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                return Image.FromStream(ms);
            }
        }

        /// <summary>
        /// Method to get encoder infor for given image format.
        /// </summary>
        /// <param name="format">Image format</param>
        /// <returns>image codec info.</returns>
        public static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().SingleOrDefault(c => c.FormatID == format.Guid);
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        /// <summary>
        /// Static readonly aray of valid file extensions for images.
        /// </summary>
        public static readonly string[] ValidExtensions = { ".jpg", ".bmp", ".gif", ".png" };

        /// <summary>
        /// Performs validations and checks whether a file is an image.
        /// </summary>
        /// <param name="filePath">The file path to the image file.</param>
        /// <param name="validFileExtensions">An array of valid file extensions to use to validate the file extension of the file.</param>
        /// <param name="validateFileExists">Whether or not to validate that the file exists. Throws FileNotFoundException if validation fails.</param>
        /// <param name="loadImageToValidate">Whether or not to try and load the file into an image object. If an exception is thrown due to an invalid image file, the exception is swallowed and this method returns false.</param>
        /// <param name="errorMessage">The error message that gets set if the file is not an image.</param>
        /// <returns></returns>
        public static bool IsFileAnImage(
            string filePath, 
            string[] validFileExtensions, 
            bool validateFileExists, 
            bool loadImageToValidate, 
            out string errorMessage)
        {
            validFileExtensions = validFileExtensions ?? ValidExtensions;
            if (validateFileExists && !File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("Could not find {0}.", filePath));
            }
            string extension = Path.GetExtension(filePath);
            if (!ValidExtensions.Contains(extension))
            {
                errorMessage = string.Format("File {0} does not have a valid file extension for an image file.", Path.GetFileName(filePath));
                return false;
            }
            if (loadImageToValidate)
            {
                try
                {
                    using (Image image = Image.FromFile(filePath))
                    {
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = string.Format("{0} is not a valid image file.", Path.GetFileName(filePath));
                    return false;
                }
            }
            errorMessage = null;
            return true;
        }

        #endregion //Methods
    }
}