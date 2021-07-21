using System;
using Discord;

namespace Noodle.Extensions
{
    public static class IGuildExtensions
    {
        public static (int Normal, int Animated) GetEmoteCap(this IGuild guild)
        {
            // 0 - 50/50
            // 1 - 100/100
            // 2 - 150/150
            // 3 - 250/250

            return guild.PremiumTier switch
            {
                PremiumTier.None => (50, 50),
                PremiumTier.Tier1 => (100, 100),
                PremiumTier.Tier2 => (150, 150),
                PremiumTier.Tier3 => (250, 250),
                _ => throw new ArgumentOutOfRangeException(nameof(guild))
            };
        }
    }
}