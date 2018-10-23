using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoo.Systems.Construction
{
    public class Wall : BuildableObject
    {
        public Wall()
        {
            objectFootprintType = FootprintType.EdgeOfTile;
            isFixedRotateable = true;
            isFullyRotateable = true;
            isDraggable = true;
            type = "Wall";
        }
    }
}
