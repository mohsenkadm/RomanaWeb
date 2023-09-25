                      
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using RestSharp;
using RomanaWeb.Classes;
using RomanaWeb.Helper;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;         

namespace RomanaWeb.UploadService
{
    public interface IStorageServices
    {
        Task<ResObj> UploadImageAsync(IFormFile keys, string WebRootPath);     
    }
    public class StorageServices : IStorageServices, IRegisterSingleton
    {                        

        #region UploadImageAsync
        public async Task<ResObj> UploadImageAsync(IFormFile keys, string WebRootPath)
        {
            try
            {
                //get file ext
                var FileExt = Path.GetExtension(keys.FileName);

                //check file ext
                if (FileExt != ".PNG" && FileExt != ".png" && FileExt != ".jpg" && FileExt != ".JPG")
                    return Result.Return(false, "رجاءا اختيار صيفة ملف كصورة");
                var objname = $"{Guid.NewGuid()}{FileExt}";

                var imgsave = WebRootPath + $@"\Uplouds\image-" + objname;
                var stream = new FileStream(imgsave, FileMode.Create);
                await keys.CopyToAsync(stream);
                //CompressAndResizeImage(keys, imgsave, 900, 900, 70L);
                return Result.Return(true, "تم رفع الملف بنجاح", objname);
            }
            catch (Exception ex)
            {
                return Result.Return(true, "حدث خطا" + ex.Message);
            }
        }
        #endregion  

        #region       CompressAndResizeImage
        public static void CompressAndResizeImage(IFormFile sourceImageFile, string destinationImagePath, int maxWidth, int maxHeight, long quality)
        {
            using (var sourceImage = Image.FromStream(sourceImageFile.OpenReadStream()))
            {
                // Calculate the new dimensions while maintaining the aspect ratio
                int newWidth, newHeight;
                if (sourceImage.Width > sourceImage.Height)
                {
                    newWidth = maxWidth;
                    newHeight = (int)((float)sourceImage.Height / sourceImage.Width * maxWidth);
                }
                else
                {
                    newWidth = (int)((float)sourceImage.Width / sourceImage.Height * maxHeight);
                    newHeight = maxHeight;
                }

                // Create a new bitmap with the desired dimensions
                using (var resizedImage = new Bitmap(newWidth, newHeight))
                {
                    using (var graphics = Graphics.FromImage(resizedImage))
                    {
                        // Configure the graphics settings for better quality
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                        // Draw the resized image
                        graphics.DrawImage(sourceImage, 0, 0, newWidth, newHeight);
                    }

                    // Save the resized image with the desired quality
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                    var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    resizedImage.Save(destinationImagePath, codec, encoderParameters);
                }
            }
        }
        #endregion
                 
                       

    }
}
          