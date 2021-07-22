using ImageMagick;

namespace Noodle.Extensions
{
    public static class MagickImageExtensions
    {
        /// <summary>
        /// Negate colors in collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="channels"></param>
        public static void Negate(this MagickImageCollection collection, Channels channels)
        {
            foreach (var image in collection)
            {
                image.Negate(channels);
            }
        }
        
        /// <summary>
        /// Resize collection to specified size.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="ignoreAspectRatio"></param>
        public static void Resize(this MagickImageCollection collection, int width, int height, bool ignoreAspectRatio)
        {
            var size = new MagickGeometry
            {
                Width = width,
                Height = height,
                IgnoreAspectRatio = ignoreAspectRatio
            };
            
            foreach (var image in collection)
            {
                image.Resize(size);
                image.Format = MagickFormat.Png;
            }
        }

        /// <summary>
        /// Resize collection to specified geometry.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="geometry"></param>
        public static void Resize(this MagickImageCollection collection, MagickGeometry geometry)
        {
            foreach (var image in collection)
            {
                image.Resize(geometry);
            }
        }

        /// <summary>
        /// Flip collection (reflect each scanline in the vertical direction).
        /// </summary>
        /// <param name="collection"></param>
        public static void Flip(this MagickImageCollection collection)
        {
            foreach (var image in collection)
            {
                image.Flip();
            }
        }

        /// <summary>
        /// Flop collection (reflect each scanline in the horizontal direction).
        /// </summary>
        /// <param name="collection"></param>
        public static void Flop(this MagickImageCollection collection)
        {
            foreach (var image in collection)
            {
                image.Flop();
            }
        }

        /// <summary>
        /// Rotate collection clockwise by specified number of degrees.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="degrees"></param>
        public static void Rotate(this MagickImageCollection collection, double degrees)
        {
            foreach (var image in collection)
            {
                image.Rotate(degrees);
            }
        }
    }
}