using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoo.Systems.Administration
{
    [System.Serializable]
    public class ZooInfo
    {
        /// <summary>
        /// Name of the zoo.
        /// </summary>
        public string ZooName { get; set; }

        /// <summary>
        /// Last used Unique Object ID. 
        /// </summary>
        public int LastUniqueObjectID { get; set; }
    }
}
