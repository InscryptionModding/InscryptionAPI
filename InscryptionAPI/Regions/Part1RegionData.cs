using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Regions
{
    public class Part1RegionData
    {
        private int tier;
        private RegionData region;

        public int Tier { get => tier; private set => tier = value; }
        public RegionData Region { get => region; private set => region = value; }

        public Part1RegionData (RegionData region, int tier)
        {
            this.region = region;
            this.tier = tier;
        }
    }
}
