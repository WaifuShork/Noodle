using System;
using System.Diagnostics.CodeAnalysis;
using Discord;
using System.IO;
using System.Net;
using ImageMagick;
using System.Net.Http;
using Noodle.Extensions;
using System.Threading.Tasks;

namespace Noodle.Common.Models
{
    /// <summary>
    /// Represents MagickImage and MagickImageCollection wrapper. MagickImageCollection doesn't support some functions that MagickImage does, because it
    /// requires you to iterate over each image and modify them individually, this serves to unionize them into a single type 
    /// </summary>
    // Suppress because this is internal and not a library so I want to control what's private and public 
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InvertIf.Global")]
    public sealed class MagickSystem : IDisposable
    {
        private MagickImage _image;
        private MagickImageCollection _collection;
        private bool _isImage;
        
        private static string _filePath;
        private static Stream _stream; 

        /// <summary>
        /// Data is saved to a file so this allows you to retrieve it's location since the file name will always be a random GUID
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public string FilePath => _filePath;

        /// <summary>
        /// Represents the file size in Bytes 
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public long FileSize
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_filePath))
                {
                    throw new NullReferenceException("The underlying file path was null so information about size cannot be returned.");
                }
                
                return new FileInfo(_filePath).Length;
            }
        }

        public int Width
        {
            get
            {
                if (_isImage)
                {
                    return _image.Width;
                }

                return _collection[0].Width;
            }
        }

        public int Height
        {
            get
            {
                if (_isImage)
                {
                    return _image.Height;
                }

                return _collection[0].Height;
            }
        }

        public static async Task<MagickSystem> CreateAsync<TType>(HttpClient client, string url, string name)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "Client cannot be null or uninitialized");
            }

            // Sometimes when we download a file from somewhere like tenor.com (commonly where Discord gets gifs from)
            // the actual link doesn't hold the file data itself, just a tenor page which will throw when attempting to 
            // download a png, gif, or any other file type. I'm not actually sure if there's a better way to do this,
            // so I just use a get request and check before ever downloading 
            var response = await client.GetAsync(url.SanitizeUrl());
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException("Status did not return OK");
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var magick = new MagickSystem();

            _stream = stream;
            if (typeof(TType) == typeof(MagickImage))
            {
                magick._isImage = true;
                magick._image = new MagickImage(stream);
                magick._collection = null;
                
                // Noodle/assets/emotes/name.png
                _filePath = Path.Combine("assets", "emotes", $"{name}-{Guid.NewGuid().ToString()}.png");
                return magick;
            }

            if (typeof(TType) == typeof(MagickImageCollection))
            {
                magick._isImage = false;
                magick._collection = new MagickImageCollection(stream);
                magick._image = null;
                
                // Noodle/assets/emotes/name.png
                _filePath = Path.Combine("assets", "emotes", $"{name}-{Guid.NewGuid().ToString()}.gif");                
                return magick;
            }

            throw new ArgumentException("TType can only be MagickImage or MagickImageCollection", nameof(TType));
        }
        
        public void Flip()
        {
            if (_isImage)
            {
                _image.Flip();
                return;
            }

            _collection.Flip();
        }
        
        public void Flop()
        {
            if (_isImage)
            {
                _image.Flop();
                return;
            }

            _collection.Flop();
        }

        public void Negate()
        {
            if (_isImage)
            {
                _image.Negate(Channels.RGB);
                return;
            }

            _collection.Negate(Channels.RGB);
        }

        public void Resize(int width, int height, bool ignoreRatio = true)
        {
            var size = new MagickGeometry
            {
                Width = width,
                Height = height,
                IgnoreAspectRatio = ignoreRatio
            };
            
            if (_isImage)
            {
                _image.Resize(size);
                return;
            }

            _collection.Resize(size);
        }

        public void Rotate(double degrees)
        {
            if (_isImage)
            {
                _image.Rotate(degrees);
                return;
            }

            _collection.Rotate(degrees);
        }

        public void Reverse()
        {
            if (_isImage)
            {
                throw new Exception("Reverse is only available on Collections");
            }
            
            _collection.Reverse();
        }

        public void Colorize(MagickColor color, Percentage alpha)
        {
            if (_isImage)
            {
                _image.Colorize(color, alpha);
                return;
            }

            foreach (var image in _collection)
            {
                image.Colorize(color, alpha);
            }
        }

        public void Sharpen(double radius, double sigma, Channels channels)
        {
            if (_isImage)
            {
                _image.Sharpen(radius, sigma, channels);
                return;
            }

            foreach (var image in _collection)
            {
                image.Sharpen(radius, sigma, channels);
            }
        }

        public void AddNoise(NoiseType noiseType, Channels channels)
        {
            if (_isImage)
            {
                _image.AddNoise(noiseType, channels);
                return;
            }

            foreach (var image in _collection)
            {
                image.AddNoise(noiseType, channels);
            }
        }

        public void RotationalBlur(double angle)
        {
            if (_isImage)
            {
                _image.RotationalBlur(angle);
                return;
            }

            foreach (var image in _collection)
            {
                image.RotationalBlur(angle);
            }
        }

        public void Blur(double radius, double sigma, Channels channels)
        {
            if (_isImage)
            {
                _image.Blur(radius, sigma, channels);
                return;
            }

            foreach (var image in _collection)
            {
                image.Blur(radius, sigma, channels);
            }
        }
        
        public void RotationalBlur(double angle, Channels channels)
        {
            if (_isImage)
            {
                _image.RotationalBlur(angle, channels);
                return;
            }

            foreach (var image in _collection)
            {
                image.RotationalBlur(angle, channels);
            }
        }

        public void Contrast(bool enhance)
        {
            if (_isImage)
            {
                _image.Contrast(enhance);
                return;
            }

            foreach (var image in _collection)
            {
                image.Contrast(enhance);
            }
        }

        public void SetFormat(MagickFormat format)
        {
            
            if (_isImage)
            {
                _image.Settings.Format = format;
                return;
            }

            throw new ArgumentException("Type must be MagickImage");
        }

        public void SetAntialiasing(bool antialiasing)
        {
            if (_isImage)
            {
                _image.Settings.AntiAlias = antialiasing;
                return;
            }

            foreach (var image in _collection)
            {
                image.Settings.AntiAlias = antialiasing;
            }
        }

        public void SetQuality(int quality)
        {
            if (_isImage)
            {
                _image.Quality = quality;
                return;
            }

            foreach (var image in _collection)
            {
                image.Quality = quality;
            }
        }

        public void SetColorFuzz(Percentage fuzziness)
        {
            if (_isImage)
            {
                _image.ColorFuzz = fuzziness;
                return;
            }

            foreach (var image in _collection)
            {
                image.ColorFuzz = fuzziness;
            }
        }

        public void SetTransparency(MagickColor color)
        {
            if (_isImage)
            {
                _image.Transparent(color);
                return;
            }

            foreach (var image in _collection)
            {
                image.Transparent(color);
            }
        }

        public void SetSpeed(int speedPercentile)
        {
            if (_isImage)
            {
                throw new ArgumentException("Type must be MagickImageCollection");
            }

            foreach (var image in _collection)
            {
                image.AnimationDelay = speedPercentile;
            }
        }

        public void Coalesce()
        {
            if (_isImage)
            {
                throw new ArgumentException("Type must be MagickImageCollection");
            }
            
            _collection.Coalesce();
        }
        
        public void BrightnessContrast( Percentage brightness, Percentage contrast, Channels channels)
        {
            if (_isImage)
            {
                _image.BrightnessContrast(brightness, contrast, channels);
                return;
            }

            foreach (var image in _collection)
            {
                image.BrightnessContrast(brightness, contrast, channels);
            }
        }

        public void DrawText(string text, 
                             string fillColorName, 
                             string strokeColorName, 
                             int fontSize, 
                             int xCoord,
                             int yCoord,
                             bool ignoreAspect,
                             TextAlignment alignment,
                             FontStyleType fontStyleType,
                             FontWeight fontWeight,
                             FontStretch fontStretch)
        {
            if (string.IsNullOrWhiteSpace(fillColorName))
            {
                throw new ArgumentNullException(nameof(fillColorName), "Fill color cannot be null");
            }
            
            if (string.IsNullOrWhiteSpace(strokeColorName))
            {
                throw new ArgumentNullException(nameof(fillColorName), "Stroke color cannot be null");
            }

            if (fontSize <= 0)
            {
                throw new ArgumentException("Font size must be greater than 0", nameof(fontSize));
            }
            
            Resize(256, 256, ignoreAspect);
            var drawable = new Drawables()
                .FontPointSize(fontSize);
         
            var colorName = System.Drawing.Color.FromName(fillColorName);
            var fillColor = MagickColor.FromRgba(colorName.R, colorName.G, colorName.B, colorName.A);
            drawable.FillColor(fillColor);
            
            colorName = System.Drawing.Color.FromName(strokeColorName);
            var strokeColor = MagickColor.FromRgba(colorName.R, colorName.G, colorName.B, colorName.A);
            drawable.StrokeColor(strokeColor)
                    .Font("Comic Sans", fontStyleType, fontWeight, fontStretch)
                    .Text(xCoord, yCoord, text)
                    .TextAlignment(alignment);

            if (_isImage)
            {
                drawable.Draw(_image);
                return;
            }

            foreach (var image in _collection)
            {
                drawable.Draw(image);
            }
        }

        public async Task WriteAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(path);
            }

            if (_isImage)
            {
                await _image.WriteAsync(path);
                return;
            }

            await _collection.WriteAsync(path);
        }

        public Stream ToStream()
        {
            if (_isImage)
            {
                return new MemoryStream(_image.ToByteArray());
            }
            
            return new MemoryStream(_collection.ToByteArray());
        }

        public byte[] ToByteArray()
        {
            if (_isImage)
            {
                return _image.ToByteArray();
            }

            return _collection.ToByteArray();
        }

        public bool CanUpload(bool isEmote = true)
        {
            if (isEmote)
            {
                // 256KB 
                if (FileSize > 256000)
                {
                    return false;
                }

                return true;
            }
            
            // 8MB 
            if (FileSize > 8000000)
            {
                return false;
            }

            return true;
        }
        
        public async Task<Image> ToEmoteAsync(int width, int height)
        {
            Resize(width, height);

            var fileSize = ToByteArray().LongLength;
            if (fileSize >= 256000)
            {
                var size = fileSize.FormatSize();
                throw new Exception($"File size too large ({size})");
            }

            await WriteAsync(_filePath);
            return new Image(_filePath);
        }

        public async Task<Image> ToHackedAsync(int width, int height)
        {
            using var image = new MagickImage(_collection[0]);
            _collection.Add(image);
            Resize(width, height);
            
            var fileSize = ToByteArray().LongLength;
            if (fileSize >= 256000)
            {
                var size = fileSize.FormatSize();
                throw new Exception($"File size too large ({size})");
            }

            await WriteAsync(_filePath);
            return new Image(_filePath);
        }

        ~MagickSystem()
        {
            Dispose();
        }
        
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.Dispose();
                _collection?.Dispose();
                _image?.Dispose();
            }

            _stream = null;
            _collection = null;
            _image = null;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}