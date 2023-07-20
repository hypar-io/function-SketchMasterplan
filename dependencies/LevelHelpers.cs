using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements
{
    public static class LevelHelpers
    {

        public static double LevelHeightAtLevelNumber(this List<double> levels, int levelNumber)
        {
            var index = Math.Max(0, levelNumber - 1);

            if (index >= levels.Count())
            {
                index = levels.Count() - 1;
            }
            return levels[index];
        }

        public static double LevelElevationAtLevelNumber(this List<double> levels, int levelNumber)
        {
            var height = 0.0;
            for (int i = 1; i < levelNumber; i++)
            {
                height += levels.LevelHeightAtLevelNumber(i);
            }
            return height;
        }

    }

}