using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoo.Systems.Construction
{
    public class PathObject : BuildableObject
    {
        
        public PathObject()
        {
            objectFootprintType = FootprintType.PathObject;
            type = "PathObject";

        }

    }
}
