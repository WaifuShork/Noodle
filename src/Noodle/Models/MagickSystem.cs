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
    public class MagickSystem<T> : IDisposable, IAsyncDisposable
    {
        public MagickImage Image { get; private set; }

        public MagickImageCollection Collection { get; private set; }
        
        private readonly bool _isImage;

        public MagickSystem(HttpClient client, string url)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "Client was not properly injected");
            }

            var response = client.GetAsync(url.SanitizeUrl()).GetAwaiter().GetResult();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException("Status did not return OK");
            }
            
            using var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            
            if (typeof(T) == typeof(MagickImage))
            {
                _isImage = true;
                Image = new MagickImage(stream);
                return;
            }

            if (typeof(T) == typeof(MagickImageCollection))
            {
                _isImage = false;
                Collection = new MagickImageCollection(stream);
                return;
            }

            Log.Information("Unable to resolve type of T");
        }

        public void Flip()
        {
            if (_isImage)
            {
                Image.Flip();
                return;
            }

            Collection.Flip();
        }
        
        public void Flop()
        {
            if (_isImage)
            {
                Image.Flop();
                return;
            }

            Collection.Flop();
        }

        public void Negate()
        {
            if (_isImage)
            {
                Image.Negate(Channels.RGB);
                return;
            }

            Collection.Negate(Channels.RGB);
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
                Image.Resize(size);
                return;
            }

            Collection.Resize(size);
        }

        public void Rotate(double degrees)
        {
            if (_isImage)
            {
                Image.Rotate(degrees);
                return;
            }

            Collection.Rotate(degrees);
        }

        public void Reverse()
        {
            if (_isImage)
            {
                throw new Exception("Reverse is only available on Collections");
            }
            
            Collection.Reverse();
        }

        public void Colorize(MagickColor color, Percentage alpha)
        {
            if (_isImage)
            {
                Image.Colorize(color, alpha);
                return;
            }

            foreach (var image in Collection)
            {
                image.Colorize(color, alpha);
            }
        }

        public void Sharpen(double radius, double sigma, Channels channels)
        {
            if (_isImage)
            {
                Image.Sharpen(radius, sigma, channels);
                return;
            }

            foreach (var image in Collection)
            {
                image.Sharpen(radius, sigma, channels);
            }
        }

        public void AddNoise(NoiseType noiseType, Channels channels)
        {
            if (_isImage)
            {
                Image.AddNoise(noiseType, channels);
                return;
            }

            foreach (var image in Collection)
            {
                image.AddNoise(noiseType, channels);
            }
        }

        public void RotationalBlur(double angle)
        {
            if (_isImage)
            {
                Image.RotationalBlur(angle);
                return;
            }

            foreach (var image in Collection)
            {
                image.RotationalBlur(angle);
            }
        }
        
        public void RotationalBlur(double angle, Channels channels)
        {
            if (_isImage)
            {
                Image.RotationalBlur(angle, channels);
                return;
            }

            foreach (var image in Collection)
            {
                image.RotationalBlur(angle, channels);
            }
        }

        public void Contrast(bool enhance)
        {
            if (_isImage)
            {
                Image.Contrast(enhance);
                return;
            }

            foreach (var image in Collection)
            {
                image.Contrast(enhance);
            }
        }

        public void SetFormat(MagickFormat format)
        {
            if (_isImage)
            {
                Image.Settings.Format = format;
                return;
            }

            throw new ArgumentException("Type must be MagickImage", nameof(T));
        }

        public void SetAntialiasing(bool antialiasing)
        {
            if (_isImage)
            {
                Image.Settings.AntiAlias = antialiasing;
                return;
            }

            foreach (var image in Collection)
            {
                image.Settings.AntiAlias = antialiasing;
            }
        }

        public void SetQuality(int quality)
        {
            if (_isImage)
            {
                Image.Quality = quality;
                return;
            }

            foreach (var image in Collection)
            {
                image.Quality = quality;
            }
        }

        public void SetColorFuzz(Percentage fuzziness)
        {
            if (_isImage)
            {
                Image.ColorFuzz = fuzziness;
                return;
            }

            foreach (var image in Collection)
            {
                image.ColorFuzz = fuzziness;
            }
        }

        public void SetTransparency(MagickColor color)
        {
            if (_isImage)
            {
                Image.Transparent(color);
                return;
            }

            foreach (var image in Collection)
            {
                image.Transparent(color);
            }
        }
        
        public void BrightnessContrast( Percentage brightness, Percentage contrast, Channels channels)
        {
            if (_isImage)
            {
                Image.BrightnessContrast(brightness, contrast, channels);
                return;
            }

            foreach (var image in Collection)
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
                await Image.WriteAsync(path);
                return;
            }

            await Collection.WriteAsync(path);
        }

        public Stream ToStream()
        {
            if (_isImage)
            {
                return new MemoryStream(Image.ToByteArray());
            }
            
            return new MemoryStream(Collection.ToByteArray());
        }

        public byte[] ToByteArray()
        {
            if (_isImage)
            {
                return Image.ToByteArray();
            }

            return Collection.ToByteArray();
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
            using var image = new MagickImage(Collection[0]);
            Collection.Add(image);
            Resize(width, height);
            Collection.WriteAsync(path).GetAwaiter().GetResult();
            
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
                Collection?.Dispose();
                Image?.Dispose();
            }

            Collection = null;
            Image = null;
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
                Collection?.Dispose();
                Image?.Dispose();

                Collection = null;
                Image = null;
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