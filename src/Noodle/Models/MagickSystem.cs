using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using ImageMagick;
using Noodle.Extensions;
using Serilog;

namespace Noodle.Models
{
    public class MagickSystem<T> : IDisposable, IAsyncDisposable where T : class
    {
        public MagickImage Image { get; private set; }
        public MagickImageCollection Collection { get; private set; }

        private readonly bool _isImage;

        public MagickSystem(string url)
        {
            using var client = new HttpClient();
            using var stream = client.GetStreamAsync(url.SanitizeUrl()).GetAwaiter().GetResult();
            
            if (typeof(T) == typeof(MagickImage))
            {
                _isImage = true;
                Image = new MagickImage(stream);
            }

            if (typeof(T) == typeof(MagickImageCollection))
            {
                _isImage = false;
                Collection = new MagickImageCollection(stream);
            }
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

            var fileSize = ToByteArray().Length;
            if (fileSize >= 256000)
            {
                var size = ((long) fileSize).FormatSize();
                throw new Exception($"File size too large ({size})");
            }

            try
            {
                var stream = ToStream();
                return new Image(stream);
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Unknown error");
                return new Image();
            }
        }

        public Image ToHacked(string name, int width, int height)
        {
            var path = Path.Combine("emotes", $"{name}.gif");
            using var image = new MagickImage(Collection[0]);
            Collection.Add(image);
            Resize(width, height);
            Collection.WriteAsync(path).GetAwaiter().GetResult();
            
            var fileSize = ToByteArray().Length;
            if (fileSize >= 256000)
            {
                var size = ((long) fileSize).FormatSize();
                throw new Exception($"File size too large ({size})");
            }

            try
            {
                return new Image(path);
            }
            catch (Exception exception)
            {
                Log.Warning(exception, "Unknown error");
                return new Image();
            }
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