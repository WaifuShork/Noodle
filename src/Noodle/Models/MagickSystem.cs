using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using ImageMagick;
using Noodle.Extensions;
using Serilog;

namespace Noodle.Models
{
    public class MagickSystem : IDisposable, IAsyncDisposable
    {
        private MagickImage _image;

        private MagickImageCollection _collection;
        
        private bool _isImage;

        public static async Task<MagickSystem> CreateAsync<T>(HttpClient client, string url)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "Client was not properly injected");
            }

            var response = await client.GetAsync(url.SanitizeUrl());
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException("Status did not return OK");
            }

            using var stream = await response.Content.ReadAsStreamAsync();

            var magick = new MagickSystem();
            
            if (typeof(T) == typeof(MagickImage))
            {
                magick._isImage = true;
                magick._image = new MagickImage(stream);
                magick._collection = null;
                return magick;
            }

            if (typeof(T) == typeof(MagickImageCollection))
            {
                magick._isImage = false;
                magick._collection = new MagickImageCollection(stream);
                magick._image = null;
                return magick;
            }

            throw new ArgumentException("TType can only be MagickImage or MagickImageCollection", nameof(T));
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

        public Image ToEmote(int width, int height)
        {
            Resize(width, height);

            var fileSize = ToByteArray().LongLength;
            if (fileSize >= 256000)
            {
                var size = fileSize.FormatSize();
                throw new Exception($"File size too large ({size})");
            }
            
            var stream = ToStream();
            return new Image(stream);
        }

        public Image ToHacked(string name, int width, int height)
        {
            var path = Path.Combine("assets", "emotes", $"{name}.gif");
            using var image = new MagickImage(_collection[0]);
            _collection.Add(image);
            Resize(width, height);
            _collection.WriteAsync(path).GetAwaiter().GetResult();
            
            var fileSize = ToByteArray().LongLength;
            if (fileSize >= 256000)
            {
                var size = fileSize.FormatSize();
                throw new Exception($"File size too large ({size})");
            }

            return new Image(path);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _collection?.Dispose();
                _image?.Dispose();
            }

            _collection = null;
            _image = null;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            await Task.Run(() =>
            {
                _collection?.Dispose();
                _image?.Dispose();

                _collection = null;
                _image = null;
            });
        }
        
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
            GC.SuppressFinalize(this);
        }
    }
}