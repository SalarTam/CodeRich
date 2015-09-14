using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing.Drawing2D;

namespace Code.Common.Image
{
    /// <summary>
    /// 裁切图片的功能类
    /// </summary>
    public class Thumbnail
    {
        #region 裁剪图片

        public static void MakeSuitThumNailbyStream(Stream originalImageStream, string thumNailPath, int width, int height)
        {
            System.Drawing.Image originalImage = System.Drawing.Image.FromStream(originalImageStream);

            try
            {
                if (originalImage == null)
                {
                    return;
                }
                if (originalImage.Width < width && originalImage.Height < height)
                {
                    //MakeThumNail(originalImage, thumNailPath, width, height, "BL");
                    MakeThumNail(originalImage, thumNailPath, width, height, ThumbnailType.AutoMaxPercent, false);
                }
                else
                {
                    MakeThumNail(originalImage, thumNailPath, width, height, ThumbnailType.Cut, false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static void MakeSuitThumNail(System.Drawing.Image originalImage, string thumNailPath, int width, int height)
        {

            try
            {
                if (originalImage == null)
                {
                    return;
                }
                if (originalImage.Width < width && originalImage.Height < height)
                {
                    //MakeThumNail(originalImage, thumNailPath, width, height, "BL");
                    MakeThumNail(originalImage, thumNailPath, width, height, ThumbnailType.AutoMaxPercent, false);
                }
                else
                {
                    MakeThumNail(originalImage, thumNailPath, width, height, ThumbnailType.Cut, false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion

        #region 生成缩略图

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImageStream">原图数据流</param>
        /// <param name="thumNailPath">缩略图存储路径</param>
        /// <param name="width">缩略图宽</param>
        /// <param name="height">缩略图高</param>
        /// <param name="model">HW（指定宽高，会变形）；W(指定宽度,高度按照比例缩放)；H（指定高度,宽度按照等比例缩放）；BL(按要缩的宽高比例);AUP(判断原图和缩图比例)</param>
        public static void MakeThumNail(System.Drawing.Image originalImage, string thumNailPath, int width, int height, string model)
        {
            int thumWidth = width;      //缩略图的宽度
            int thumHeight = height;    //缩略图的高度

            int x = 0;
            int y = 0;

            int originalWidth = originalImage.Width;    //原始图片的宽度
            int originalHeight = originalImage.Height;  //原始图片的高度

            switch (model)
            {
                case "HW":      //指定高宽缩放,可能变形
                    break;
                case "W":       //指定宽度,高度按照比例缩放
                    thumHeight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H":       //指定高度,宽度按照等比例缩放
                    thumWidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut":
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)thumWidth / (double)thumHeight)
                    {
                        originalHeight = originalImage.Height;
                        originalWidth = originalImage.Height * thumWidth / thumHeight;
                        y = 0;
                        x = (originalImage.Width - originalWidth) / 2;
                    }
                    else
                    {
                        originalWidth = originalImage.Width;
                        originalHeight = originalWidth * height / thumWidth;
                        x = 0;
                        y = (originalImage.Height - originalHeight) / 2;
                    }
                    break;
                case "BL":
                    if (originalWidth > width || originalHeight > height)
                    {
                        if (originalWidth >= originalHeight)
                        {
                            thumHeight = originalHeight * width / originalWidth;
                            thumWidth = width;
                        }
                        else
                        {
                            thumWidth = originalWidth * height / originalHeight;
                            thumHeight = height;
                        }
                    }
                    else
                    {
                        thumWidth = originalWidth;
                        thumHeight = originalHeight;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(thumWidth, thumHeight);

            //新建一个画板
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量查值法
            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            //设置高质量，低速度呈现平滑程度
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            graphic.Clear(System.Drawing.Color.White);

            //在指定位置并且按指定大小绘制原图片的指定部分
            graphic.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, thumWidth, thumHeight), new System.Drawing.Rectangle(x, y, originalWidth, originalHeight), System.Drawing.GraphicsUnit.Pixel);

            long[] quality = new long[1];
            quality[0] = 100;

            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();//获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象。
            ImageCodecInfo jpegICI = null;
            for (int i = 0; i < arrayICI.Length; i++)
            {
                if (arrayICI[i].FormatDescription.Equals("JPEG"))
                {
                    jpegICI = arrayICI[i];//设置JPEG编码
                    break;
                }
            }


            try
            {
                if (jpegICI != null)
                {
                    bitmap.Save(thumNailPath, jpegICI, encoderParams);
                    //using (FileStream fs = new FileStream(thumNailPath, FileMode.Create))
                    //{
                    //    bitmap.Save(fs, jpegICI, encoderParams);
                    //}
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                bitmap.Dispose();
                graphic.Dispose();
            }

        }


        /// <summary>
        /// 强制按比例生成缩略图(现用)
        /// </summary>
        /// <param name="originalImageStream">原图数据流</param>
        /// <param name="thumNailPath">缩略图存储路径</param>
        /// <param name="width">缩略图宽</param>
        /// <param name="height">缩略图高</param>
        /// <param name="model">HW（指定宽高，会变形）；W(指定宽度,高度按照比例缩放)；H（指定高度,宽度按照等比例缩放）；BL(按要缩的宽高比例)</param>
        public static void MakeConvertThumNail(Stream originalImageStream, string thumNailPath, int width, int height, string model)
        {
            System.Drawing.Image originalImage = System.Drawing.Image.FromStream(originalImageStream);

            int thumWidth = width;      //缩略图的宽度
            int thumHeight = height;    //缩略图的高度

            int x = 0;
            int y = 0;

            int originalWidth = originalImage.Width;    //原始图片的宽度
            int originalHeight = originalImage.Height;  //原始图片的高度

            switch (model)
            {


                case "HW":      //指定高宽缩放,可能变形
                    break;
                case "W":       //指定宽度,高度按照比例缩放
                    thumHeight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H":       //指定高度,宽度按照等比例缩放
                    thumWidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut":
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)thumWidth / (double)thumHeight)
                    {
                        originalHeight = originalImage.Height;
                        originalWidth = originalImage.Height * thumWidth / thumHeight;
                        y = 0;
                        x = (originalImage.Width - originalWidth) / 2;
                    }
                    else
                    {
                        originalWidth = originalImage.Width;
                        originalHeight = originalWidth * height / thumWidth;
                        x = 0;
                        y = (originalImage.Height - originalHeight) / 2;
                    }
                    break;
                case "BL":
                    if (originalWidth >= originalHeight)
                    {
                        thumHeight = originalHeight * width / originalWidth;
                        thumWidth = width;
                    }
                    else
                    {
                        thumWidth = originalWidth * height / originalHeight;
                        thumHeight = height;
                    }
                    break;
                case "AUP":
                    if (originalWidth > width || originalHeight > height)
                    {
                        if (originalWidth >= originalHeight)
                        {
                            thumHeight = originalHeight * width / originalWidth;
                            thumWidth = width;
                        }
                        else
                        {
                            thumWidth = originalWidth * height / originalHeight;
                            thumHeight = height;
                        }
                    }
                    else
                    {
                        thumWidth = originalWidth;
                        thumHeight = originalHeight;
                    }
                    break;
                case "AUTOW":
                    if (originalWidth <= width)
                    {
                        thumWidth = originalWidth;
                        thumHeight = originalHeight;
                    }
                    else
                    {
                        thumWidth = width;
                        thumHeight = originalImage.Height * width / originalImage.Width;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(thumWidth, thumHeight);

            //新建一个画板
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量查值法
            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            //设置高质量，低速度呈现平滑程度
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            graphic.Clear(System.Drawing.Color.White);

            //在指定位置并且按指定大小绘制原图片的指定部分
            graphic.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, thumWidth, thumHeight), new System.Drawing.Rectangle(x, y, originalWidth, originalHeight), System.Drawing.GraphicsUnit.Pixel);

            long[] quality = new long[1];
            quality[0] = 100;

            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();//获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象。
            ImageCodecInfo jpegICI = null;
            for (int i = 0; i < arrayICI.Length; i++)
            {
                if (arrayICI[i].FormatDescription.Equals("JPEG"))
                {
                    jpegICI = arrayICI[i];//设置JPEG编码
                    break;
                }
            }


            try
            {
                if (jpegICI != null)
                {
                    bitmap.Save(thumNailPath, jpegICI, encoderParams);
                    //using (FileStream fs = new FileStream(thumNailPath, FileMode.Create))
                    //{
                    //    bitmap.Save(fs, jpegICI, encoderParams);
                    //}
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                graphic.Dispose();
            }

        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImageStream">原图数据流</param>
        /// <param name="thumNailPath">缩略图存储路径</param>
        /// <param name="width">缩略图宽</param>
        /// <param name="height">缩略图高</param>
        /// <param name="model">HW（指定宽高，会变形）；W(指定宽度,高度按照比例缩放)；H（指定高度,宽度按照等比例缩放）；BL(按要缩的宽高比例)</param>
        public static void MakeThumNail(Stream originalImageStream, string thumNailPath, int width, int height, string model)
        {
            System.Drawing.Image originalImage = System.Drawing.Image.FromStream(originalImageStream);

            int thumWidth = width;      //缩略图的宽度
            int thumHeight = height;    //缩略图的高度

            int x = 0;
            int y = 0;

            int originalWidth = originalImage.Width;    //原始图片的宽度
            int originalHeight = originalImage.Height;  //原始图片的高度

            switch (model)
            {
                case "HW":      //指定高宽缩放,可能变形
                    break;
                case "W":       //指定宽度,高度按照比例缩放
                    thumHeight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H":       //指定高度,宽度按照等比例缩放
                    thumWidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut":
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)thumWidth / (double)thumHeight)
                    {
                        originalHeight = originalImage.Height;
                        originalWidth = originalImage.Height * thumWidth / thumHeight;
                        y = 0;
                        x = (originalImage.Width - originalWidth) / 2;
                    }
                    else
                    {
                        originalWidth = originalImage.Width;
                        originalHeight = originalWidth * height / thumWidth;
                        x = 0;
                        y = (originalImage.Height - originalHeight) / 2;
                    }
                    break;
                case "BL":
                    if (originalWidth > width || originalHeight > height)
                    {
                        if (originalWidth >= originalHeight)
                        {
                            thumHeight = originalHeight * width / originalWidth;
                            thumWidth = width;
                        }
                        else
                        {
                            thumWidth = originalWidth * height / originalHeight;
                            thumHeight = height;
                        }
                    }
                    else
                    {
                        thumWidth = originalWidth;
                        thumHeight = originalHeight;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(thumWidth, thumHeight);

            //新建一个画板
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量查值法
            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            //设置高质量，低速度呈现平滑程度
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            graphic.Clear(System.Drawing.Color.White);

            //在指定位置并且按指定大小绘制原图片的指定部分
            graphic.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, thumWidth, thumHeight), new System.Drawing.Rectangle(x, y, originalWidth, originalHeight), System.Drawing.GraphicsUnit.Pixel);

            long[] quality = new long[1];
            quality[0] = 100;

            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();//获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象。
            ImageCodecInfo jpegICI = null;
            for (int i = 0; i < arrayICI.Length; i++)
            {
                if (arrayICI[i].FormatDescription.Equals("JPEG"))
                {
                    jpegICI = arrayICI[i];//设置JPEG编码
                    break;
                }
            }


            try
            {
                if (jpegICI != null)
                {
                    bitmap.Save(thumNailPath, jpegICI, encoderParams);
                    //using (FileStream fs = new FileStream(thumNailPath, FileMode.Create))
                    //{
                    //    bitmap.Save(fs, jpegICI, encoderParams);
                    //}
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                graphic.Dispose();
            }

        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImageStream">原图数据流</param>
        /// <param name="thumNailPath">缩略图存储路径</param>
        /// <param name="width">缩略图宽</param>
        /// <param name="height">缩略图高</param>
        /// <param name="model">HW（指定宽高，会变形）；W(指定宽度,高度按照比例缩放)；H（指定高度,宽度按照等比例缩放）；BL(按要缩的宽高比例)</param>
        public static void MakeThumNailNoDispose(Stream originalImageStream, string thumNailPath, int width, int height, ThumbnailType model)
        {
            System.Drawing.Image originalImage = System.Drawing.Image.FromStream(originalImageStream);
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

            int thumWidth = width;      //缩略图的宽度
            int thumHeight = height;    //缩略图的高度

            int x = 0;
            int y = 0;

            int originalWidth = originalImage.Width;    //原始图片的宽度
            int originalHeight = originalImage.Height;  //原始图片的高度

            switch (model)
            {
                case ThumbnailType.AssignWidthAndHeight:      //指定高宽缩放,可能变形
                    break;
                case ThumbnailType.AssignWidth:       //指定宽度,高度按照比例缩放
                    thumHeight = originalImage.Height * width / originalImage.Width;
                    break;
                case ThumbnailType.AssignHeight:       //指定高度,宽度按照等比例缩放
                    thumWidth = originalImage.Width * height / originalImage.Height;
                    break;
                case ThumbnailType.Cut:
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)thumWidth / (double)thumHeight)
                    {
                        originalHeight = originalImage.Height;
                        originalWidth = originalImage.Height * thumWidth / thumHeight;
                        y = 0;
                        x = (originalImage.Width - originalWidth) / 2;
                    }
                    else
                    {
                        originalWidth = originalImage.Width;
                        originalHeight = originalWidth * height / thumWidth;
                        x = 0;
                        y = (originalImage.Height - originalHeight) / 2;
                    }
                    break;
                case ThumbnailType.AutoMaxPercent:
                    if (originalWidth > width || originalHeight > height)
                    {
                        if (originalWidth >= originalHeight)
                        {
                            thumHeight = originalHeight * width / originalWidth;
                            thumWidth = width;
                        }
                        else
                        {
                            thumWidth = originalWidth * height / originalHeight;
                            thumHeight = height;
                        }
                    }
                    else
                    {
                        thumWidth = originalWidth;
                        thumHeight = originalHeight;
                    }
                    break;
                case ThumbnailType.AutoMinPercent:
                    if (originalWidth > width || originalHeight > height)
                    {
                        var perw = width / (double)originalWidth;
                        var perh = height / (double)originalHeight;
                        var per = Math.Min(perw, perh);
                        thumWidth = (int)Math.Ceiling(originalWidth * per);
                        thumHeight = (int)Math.Ceiling(originalHeight * per);
                    }
                    else
                    {
                        thumWidth = originalWidth;
                        thumHeight = originalHeight;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(thumWidth, thumHeight);

            //新建一个画板
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量查值法
            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;

            //设置高质量，低速度呈现平滑程度
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            graphic.Clear(System.Drawing.Color.White);

            //在指定位置并且按指定大小绘制原图片的指定部分
            //graphic.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, thumWidth, thumHeight), new System.Drawing.Rectangle(x, y, originalWidth, originalHeight), System.Drawing.GraphicsUnit.Pixel,);
            graphic.DrawImage(originalImage, new Rectangle(0, 0, thumWidth, thumHeight), x, y,
                              originalWidth, originalHeight, GraphicsUnit.Pixel, imageAttributes);

            long[] quality = new long[1];
            quality[0] = 100;

            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();//获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象。
            ImageCodecInfo jpegICI = null;
            for (int i = 0; i < arrayICI.Length; i++)
            {
                if (arrayICI[i].FormatDescription.Equals("JPEG"))
                {
                    jpegICI = arrayICI[i];//设置JPEG编码
                    break;
                }
            }


            try
            {
                if (jpegICI != null)
                {
                    bitmap.Save(thumNailPath, jpegICI, encoderParams);
                    //using (FileStream fs = new FileStream(thumNailPath, FileMode.Create))
                    //{
                    //    bitmap.Save(fs, jpegICI, encoderParams);
                    //}
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            //finally
            //{
            //    originalImage.Dispose();
            //    bitmap.Dispose();
            //    graphic.Dispose();
            //}
        }


        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImage">原图</param>
        /// <param name="thumNailPath">缩略图存储路径</param>
        /// <param name="width">缩略图宽</param>
        /// <param name="height">缩略图高</param>
        /// <param name="model">HW（指定宽高，会变形）；W(指定宽度,高度按照比例缩放)；H（指定高度,宽度按照等比例缩放）；BL(按要缩的宽高比例)</param>
        /// <param name="disposeImg">是否注销图片</param>
        public static void MakeThumNail(System.Drawing.Image originalImage, string thumNailPath, int width, int height, ThumbnailType model, bool disposeImg)
        {
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

            int thumWidth = width;      //缩略图的宽度
            int thumHeight = height;    //缩略图的高度

            int x = 0;
            int y = 0;

            int originalWidth = originalImage.Width;    //原始图片的宽度
            int originalHeight = originalImage.Height;  //原始图片的高度

            switch (model)
            {
                case ThumbnailType.AssignWidthAndHeight:      //指定高宽缩放,可能变形
                    break;
                case ThumbnailType.AssignWidth:       //指定宽度,高度按照比例缩放
                    thumHeight = originalImage.Height * width / originalImage.Width;
                    break;
                case ThumbnailType.AssignHeight:       //指定高度,宽度按照等比例缩放
                    thumWidth = originalImage.Width * height / originalImage.Height;
                    break;
                case ThumbnailType.Cut:
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)thumWidth / (double)thumHeight)
                    {
                        originalHeight = originalImage.Height;
                        originalWidth = originalImage.Height * thumWidth / thumHeight;
                        y = 0;
                        x = (originalImage.Width - originalWidth) / 2;
                    }
                    else
                    {
                        originalWidth = originalImage.Width;
                        originalHeight = originalWidth * height / thumWidth;
                        x = 0;
                        y = (originalImage.Height - originalHeight) / 2;
                    }
                    break;
                case ThumbnailType.AutoMaxPercent:
                    if (originalWidth > width || originalHeight > height)
                    {
                        if (originalWidth >= originalHeight)
                        {
                            thumHeight = originalHeight * width / originalWidth;
                            thumWidth = width;
                        }
                        else
                        {
                            thumWidth = originalWidth * height / originalHeight;
                            thumHeight = height;
                        }
                    }
                    else
                    {
                        thumWidth = originalWidth;
                        thumHeight = originalHeight;
                    }
                    break;
                case ThumbnailType.AutoMinPercent:
                    if (originalWidth > width || originalHeight > height)
                    {
                        var perw = width / (double)originalWidth;
                        var perh = height / (double)originalHeight;
                        var per = Math.Min(perw, perh);
                        thumWidth = (int)Math.Ceiling(originalWidth * per);
                        thumHeight = (int)Math.Ceiling(originalHeight * per);
                    }
                    else
                    {
                        thumWidth = originalWidth;
                        thumHeight = originalHeight;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(thumWidth, thumHeight);

            //新建一个画板
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量查值法
            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;
            //设置高质量，低速度呈现平滑程度
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            graphic.Clear(System.Drawing.Color.White);

            //在指定位置并且按指定大小绘制原图片的指定部分
            graphic.DrawImage(originalImage, new Rectangle(0, 0, thumWidth, thumHeight), x, y,
                              originalWidth, originalHeight, GraphicsUnit.Pixel, imageAttributes);

            long[] quality = new long[1];
            quality[0] = 100;

            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();//获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象。
            ImageCodecInfo jpegICI = null;
            for (int i = 0; i < arrayICI.Length; i++)
            {
                if (arrayICI[i].FormatDescription.Equals("JPEG"))
                {
                    jpegICI = arrayICI[i];//设置JPEG编码
                    break;
                }
            }


            try
            {
                if (jpegICI != null)
                {
                    var dirpath = Path.GetDirectoryName(thumNailPath);
                    if (!Directory.Exists(dirpath))
                        Directory.CreateDirectory(dirpath);

                    bitmap.Save(thumNailPath, jpegICI, encoderParams);
                    //using (FileStream fs = new FileStream(thumNailPath, FileMode.Create))
                    //{
                    //    bitmap.Save(fs, jpegICI, encoderParams);
                    //}
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                if (disposeImg) originalImage.Dispose();
                bitmap.Dispose();
                graphic.Dispose();
            }

        }
        /// <summary>
        /// 指定坐标和比例裁剪图片
        /// </summary>
        /// <param name="ImagePath">原图地址</param>
        /// <param name="thumNailPath">缩略图存放地址</param>
        /// <param name="X">裁剪原点X坐标</param>
        /// <param name="Y">裁剪原点Y坐标</param>
        /// <param name="width">裁剪区域宽</param>
        /// <param name="height">裁剪区域高</param>
        /// <param name="scale">裁剪模板图相对原图缩放比例</param>
        public static void MakeThumNailandCut(string ImagePath, string thumNailPath, int X, int Y, int width, int height, double scale)
        {
            #region 计算
            System.Drawing.Image originalImage = System.Drawing.Image.FromFile(ImagePath);
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

            int originalWidth = originalImage.Width;
            int originalHeight = originalImage.Height;

            int thumWidth = Convert.ToInt32(originalWidth * scale);
            int thumHeight = Convert.ToInt32(originalHeight * scale);
            #endregion

            #region 图片先压缩

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(thumWidth, thumHeight);

            //新建一个画板
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量查值法
            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;
            //设置高质量，低速度呈现平滑程度
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            graphic.Clear(System.Drawing.Color.White);

            graphic.DrawImage(originalImage, new Rectangle(0, 0, thumWidth, thumHeight), 0, 0,
                              originalWidth, originalHeight, GraphicsUnit.Pixel, imageAttributes);
            #endregion

            #region 裁剪
            var bm = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(bm))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                g.Clear(System.Drawing.Color.White);
                g.DrawImage(bitmap, new Rectangle(0, 0, width, height), new Rectangle(X, Y, width, height), GraphicsUnit.Pixel);
            }
            /* * 内存不足版 2014/4/11
            var bm = new Bitmap(bitmap);
            var cloneRect = new Rectangle(X, Y, width, height);
            PixelFormat format = bm.PixelFormat;
            Bitmap cloneBitmap = bm.Clone(cloneRect, format);
             * */
            #endregion

            #region 高质量保存
            var quality = new long[1];
            quality[0] = 100;

            var encoderParams = new System.Drawing.Imaging.EncoderParameters();
            var encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            var arrayICI = ImageCodecInfo.GetImageEncoders();//获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象。
            var jpegICI = arrayICI.FirstOrDefault(t => t.FormatDescription.Equals("JPEG"));

            try
            {
                if (jpegICI != null)
                {
                    bm.Save(thumNailPath, jpegICI, encoderParams);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                bm.Dispose();
                graphic.Dispose();
            }
            #endregion

            #region Test模糊版
            /*
            System.Drawing.Image thumNailImage = originalImage.GetThumbnailImage(thumWidth, thumHeight,
                                                                                  new System.Drawing.Image.
                                                                                      GetThumbnailImageAbort(
                                                                                      ThumbnailCallback), IntPtr.Zero);
            var bm = new Bitmap(thumNailImage);
            var cloneRect = new Rectangle(X, Y, width, height);
            PixelFormat format = bm.PixelFormat;
            Bitmap cloneBitmap = bm.Clone(cloneRect, format);
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo ici = null;
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == "image/jpeg")
                    ici = codec;
            }
            var ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100);

            try
            {
                if (ici != null)
                {
                    cloneBitmap.Save(thumNailPath, ImageFormat.Jpeg);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                originalImage.Dispose();
                cloneBitmap.Dispose();
                bm.Dispose();
            }
            */
            #endregion

        }

        private static bool ThumbnailCallback()
        {
            return false;
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="orginalImagePat">原图片地址</param>
        /// <param name="thumNailPath">缩略图地址</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="model">生成缩略的模式</param>
        public static void MakeThumNail(string originalImagePath, string thumNailPath, int width, int height, string model)
        {
            System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath);
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

            int thumWidth = width;      //缩略图的宽度
            int thumHeight = height;    //缩略图的高度

            int x = 0;
            int y = 0;

            int originalWidth = originalImage.Width;    //原始图片的宽度
            int originalHeight = originalImage.Height;  //原始图片的高度

            switch (model)
            {
                case "HW":      //指定高宽缩放,可能变形
                    break;
                case "W":       //指定宽度,高度按照比例缩放
                    thumHeight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H":       //指定高度,宽度按照等比例缩放
                    thumWidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut":
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)thumWidth / (double)thumHeight)
                    {
                        originalHeight = originalImage.Height;
                        originalWidth = originalImage.Height * thumWidth / thumHeight;
                        y = 0;
                        x = (originalImage.Width - originalWidth) / 2;
                    }
                    else
                    {
                        originalWidth = originalImage.Width;
                        originalHeight = originalWidth * height / thumWidth;
                        x = 0;
                        y = (originalImage.Height - originalHeight) / 2;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(thumWidth, thumHeight);

            //新建一个画板
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量查值法
            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;
            //设置高质量，低速度呈现平滑程度
            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            graphic.Clear(System.Drawing.Color.White);

            //在指定位置并且按指定大小绘制原图片的指定部分
            graphic.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, thumWidth, thumHeight), x, y,
                              originalWidth, originalHeight, GraphicsUnit.Pixel, imageAttributes);


            long[] quality = new long[1];
            quality[0] = 100;

            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();//获得包含有关内置图像编码解码器的信息的ImageCodecInfo 对象。
            ImageCodecInfo jpegICI = null;
            for (int i = 0; i < arrayICI.Length; i++)
            {
                if (arrayICI[i].FormatDescription.Equals("JPEG"))
                {
                    jpegICI = arrayICI[i];//设置JPEG编码
                    break;
                }
            }


            try
            {
                if (jpegICI != null)
                {
                    bitmap.Save(thumNailPath, jpegICI, encoderParams);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                graphic.Dispose();
            }

        }

        #endregion

        #region 制作小正方形

        /// <summary>
        /// 制作小正方形
        /// </summary>
        /// <param name="fileName">原图的文件路径</param>
        /// <param name="newFileName">新地址</param>
        /// <param name="newSize">长度或宽度</param>
        public static void MakeSquareImage(string fileName, string newFileName, int newSize, long compression)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(fileName);

            int i = 0;
            int width = image.Width;
            int height = image.Height;
            if (width > height)
                i = height;
            else
                i = width;

            Bitmap b = new Bitmap(newSize, newSize);

            try
            {
                Graphics g = Graphics.FromImage(b);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;

                //清除整个绘图面并以透明背景色填充
                g.Clear(Color.Transparent);
                if (width < height)
                    g.DrawImage(image, new Rectangle(0, 0, newSize, newSize), new Rectangle(0, (height - width) / 2, width, width), GraphicsUnit.Pixel);
                else
                    g.DrawImage(image, new Rectangle(0, 0, newSize, newSize), new Rectangle((width - height) / 2, 0, height, height), GraphicsUnit.Pixel);

                SaveImage(b, newFileName, GetCodecInfo("image/" + GetFormat(fileName).ToString().ToLower()), compression);
            }
            finally
            {
                image.Dispose();
                b.Dispose();
            }
        }
        public static void MakeSquareImage(Stream fileStream, string newFileName, int newSize, long compression)
        {
            System.Drawing.Image image = System.Drawing.Image.FromStream(fileStream);

            int i = 0;
            int width = image.Width;
            int height = image.Height;
            if (width > height)
                i = height;
            else
                i = width;

            Bitmap b = new Bitmap(newSize, newSize);

            try
            {
                Graphics g = Graphics.FromImage(b);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;

                //清除整个绘图面并以透明背景色填充
                g.Clear(Color.Transparent);
                if (width < height)
                    g.DrawImage(image, new Rectangle(0, 0, newSize, newSize), new Rectangle(0, (height - width) / 2, width, width), GraphicsUnit.Pixel);
                else
                    g.DrawImage(image, new Rectangle(0, 0, newSize, newSize), new Rectangle((width - height) / 2, 0, height, height), GraphicsUnit.Pixel);

                SaveImage(b, newFileName, GetCodecInfo("image/" + GetFormat(newFileName).ToString().ToLower()), compression);
            }
            finally
            {
                image.Dispose();
                b.Dispose();
            }
        }


        #endregion

        #region 添加水印方法

        /// <summary>
        /// 在图片上添加文字水印
        /// </summary>
        /// <param name="path">要添加水印的图片路径</param>
        /// <param name="syPath">生成的水印图片存放的位置</param>
        /// <param name="syPath">要添加水印文字</param>
        public static void AddWaterWord(string path, string syPath, string syWord)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(path);

            //新建一个画板
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(image);
            graphic.DrawImage(image, 0, 0, image.Width, image.Height);

            //设置字体
            System.Drawing.Font f = new System.Drawing.Font("Verdana", 60);

            //设置字体颜色
            System.Drawing.Brush b = new System.Drawing.SolidBrush(System.Drawing.Color.Green);

            graphic.DrawString(syWord, f, b, 35, 35);
            graphic.Dispose();

            //保存文字水印图片
            image.Save(syPath);
            image.Dispose();

        }

        /// <summary>
        /// 在图片上添加文字水印
        /// </summary>
        /// <param name="path">要添加水印的图片路径</param>
        /// <param name="syPath">生成的水印图片存放的位置</param>
        /// <param name="syPath">要添加水印文字</param>
        public static void AddWaterWord(Stream imageStream, string syPath, string syWord)
        {
            System.Drawing.Image image = System.Drawing.Image.FromStream(imageStream);

            //新建一个画板
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(image);
            graphic.DrawImage(image, 0, 0, image.Width, image.Height);

            //设置字体
            System.Drawing.Font f = new System.Drawing.Font("Verdana", 60);

            //设置字体颜色
            System.Drawing.Brush b = new System.Drawing.SolidBrush(System.Drawing.Color.Green);

            graphic.DrawString(syWord, f, b, 35, 35);
            graphic.Dispose();

            //保存文字水印图片
            image.Save(syPath);
            image.Dispose();

        }

        /// <summary>
        /// 在图片上添加文字水印(生成后不保存图片，不释放画板在内存的资源)
        /// </summary>
        /// <param name="graphic">图片画板</param>
        /// <param name="fontSize">水印字体大小</param>
        /// <param name="oColor">水印字体颜色</param>
        /// <param name="syWord">要添加的水印文字</param>
        /// <param name="x">生成水印的X坐标</param>
        /// <param name="y">生成水印的Y坐标</param>
        public static void AddWaterWord(System.Drawing.Graphics graphic, int fontSize, System.Drawing.Color oColor, string syWord, int x, int y)
        {
            //设置字体
            System.Drawing.Font f = new System.Drawing.Font("Verdana", fontSize);

            //设置字体颜色
            System.Drawing.Brush b = new System.Drawing.SolidBrush(oColor);

            graphic.DrawString(syWord, f, b, x, y);

        }

        /// <summary>
        /// 在图片上添加图片水印
        /// </summary>
        /// <param name="path">原服务器上的图片路径</param>
        /// <param name="syPicPath">水印图片的路径</param>
        /// <param name="waterPicPath">生成的水印图片存放路径</param>
        public static void AddWaterPic(string path, string syPicPath, string waterPicPath)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(path);
            System.Drawing.Image waterImage = System.Drawing.Image.FromFile(syPicPath);
            System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(image);
            graphic.DrawImage(waterImage, new System.Drawing.Rectangle(image.Width - waterImage.Width, image.Height - waterImage.Height, waterImage.Width, waterImage.Height), 0, 0, waterImage.Width, waterImage.Height, System.Drawing.GraphicsUnit.Pixel);
            graphic.Dispose();

            image.Save(waterPicPath);
            image.Dispose();
        }

        #endregion

        #region Helper

        public static bool IsPicture(Stream fs)//filePath是文件的完整路径
        {
            try
            {
                BinaryReader reader = new BinaryReader(fs);
                string fileClass;
                byte buffer;
                byte[] b = new byte[2];
                buffer = reader.ReadByte();
                b[0] = buffer;
                fileClass = buffer.ToString();
                buffer = reader.ReadByte();
                b[1] = buffer;
                fileClass += buffer.ToString();


                //reader.Close();
                if (fileClass == "255216" || fileClass == "7173" || fileClass == "6677" || fileClass == "13780")//255216是jpg;7173是gif;6677是BMP,13780是PNG;7790是exe,8297是rar
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取图像编码解码器的所有相关信息
        /// </summary>
        /// <param name="mimeType">包含编码解码器的多用途网际邮件扩充协议 (MIME) 类型的字符串</param>
        /// <returns>返回图像编码解码器的所有相关信息</returns>
        private static ImageCodecInfo GetCodecInfo(string mimeType)
        {
            ImageCodecInfo[] codecInfo = ImageCodecInfo.GetImageEncoders();

            foreach (ImageCodecInfo ici in codecInfo)
            {
                if (ici.MimeType == mimeType)
                    return ici;
            }
            return null;
        }


        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="image">Image 对象</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="ici">指定格式的编解码参数</param>
        private static void SaveImage(System.Drawing.Image image, string savePath, ImageCodecInfo ici, long compression)
        {
            //设置 原图片 对象的 EncoderParameters 对象
            EncoderParameters parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, ((long)compression));
            image.Save(savePath, ici, parameters);
            parameters.Dispose();
        }

        /// <summary>
        /// 得到图片格式
        /// </summary>
        /// <param name="name">文件名称</param>
        /// <returns></returns>
        public static ImageFormat GetFormat(string name)
        {
            string ext = name.Substring(name.LastIndexOf(".") + 1);
            switch (ext.ToLower())
            {
                case "jpg":
                case "jpeg":
                    return ImageFormat.Jpeg;
                case "bmp":
                    return ImageFormat.Bmp;
                case "png":
                    return ImageFormat.Png;
                case "gif":
                    return ImageFormat.Gif;
                default:
                    return ImageFormat.Jpeg;
            }
        }


        #endregion
    }

    public enum ThumbnailType
    {
        [Description("指定宽和高，会变形")]
        AssignWidthAndHeight,
        [Description("指定宽，高按比例")]
        AssignWidth,
        [Description("指定高，宽按比例")]
        AssignHeight,
        [Description("居中裁剪出指定宽高")]
        Cut,
        [Description("以较小比例按比例缩小（缩小后宽高都不会超过指定值）")]
        AutoMinPercent,
        [Description("以较大比例按比例缩小")]
        AutoMaxPercent
    }
}
