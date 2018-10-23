using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoo.Systems.Construction
{
    public class Scenery : BuildableObject
    {
        public Scenery()
        {
            objectFootprintType = FootprintType.Full;
            isFixedRotateable = true;
            isFullyRotateable = true;
            type = "Scenery";

        }
    }
}
