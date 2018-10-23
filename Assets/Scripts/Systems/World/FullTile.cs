using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoo.Systems.World
{
    /// <summary>
    /// Virtual representation of 4 quarter tiles.
    /// </summary>
    public class FullTile
    {
        public QuarterTile southWest;
        public QuarterTile northWest;
        public QuarterTile northEast;
        public QuarterTile southEast;

        /// <summary>
        /// The 4 quarter tiles making up the full tile.
        /// </summary>
        public List<QuarterTile> tiles;

        public FullTile()
        {

        }
        public FullTile(QuarterTile SW, QuarterTile NW, QuarterTile NE, QuarterTile SE)
        {
            southWest = SW;
            northWest = NW;
            northEast = NE;
            southEast = SE;
            tiles = new List<QuarterTile>(4) { SW, NW, NE, SE };
        }

        public void GenerateTileList()
        {
            tiles = new List<QuarterTile>(4);
            if (southWest != null)
                tiles.Add(southWest);
            if (northWest != null)
                tiles.Add(northWest);
            if (northEast != null)
                tiles.Add(northEast);
            if (southEast != null)
                tiles.Add(southEast);
            tiles.TrimExcess();
        }

        public float HeightOfHighestQuarterTile()
        {
            float height = -10;
            foreach (QuarterTile t in tiles)
            {
                if (t.height > height)
                {
                    height = t.height;
                }
            }
            return height;
        }
    }
}
