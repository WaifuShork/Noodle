using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using ImageMagick;
using Noodle.Extensions;

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

            foreach (var image in Collection)
            {
                image.Flip();
            }
        }
        
        public void Flop()
        {
            if (_isImage)
            {
                Image.Flop();
                return;
            }

            foreach (var image in Collection)
            {
                image.Flop();
            }
        }

        public void Negate()
        {
            if (_isImage)
            {
                Image.Negate();
                return;
            }

            foreach (var image in Collection)
            {
                image.Negate();
            }
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

            foreach (var image in Collection)
            {
                image.Resize(size);
            }
        }

        public void Rotate(double degrees)
        {
            if (_isImage)
            {
                Image.Rotate(degrees);
                return;
            }

            foreach (var image in Collection)
            {
                image.Rotate(degrees);
            }
        }

        public void Foreach(Action<IMagickImage> action)
        {
            foreach (var image in Collection)
            {
                action(image);
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

        public Image ToImage()
        {
            if (_isImage)
            {
                using var imageStream = new MemoryStream(Image.ToByteArray());
                return new Image(imageStream);
            }
            
            using var collectionStream = new MemoryStream(Collection.ToByteArray());
            return new Image(collectionStream);
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