using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoo.Systems.Construction
{
    public class Building : BuildableObject
    {

        /// <summary>
        /// Age of the building in in-game days.
        /// </summary>
        int ageInDays = 0;


        public Building()
        {
            objectFootprintType = FootprintType.Full;
            isFixedRotateable = true;
            isFullyRotateable = true;
            multiBuildable = false;
            autoDeletable = false;
            type = "Building";            
        }

    }
}
