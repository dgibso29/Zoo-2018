using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoo.Systems.Construction
{
    public class Path : BuildableObject
    {
        public Path()
        {
            objectFootprintType = FootprintType.Path;
            autoDeletable = false;
            isDraggable = true;
            canBeBuiltFreeform = false;
            type = "Path";
        }
    }
}
