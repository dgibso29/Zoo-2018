using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Zoo.Systems.Construction
{
    public class ObjectEventArgs : EventArgs
    {
        /// <summary>
        /// Object that was built.
        /// </summary>
        public BuildableObject ObjectBuilt { get; }


        public ObjectEventArgs(BuildableObject objectBuilt)
        {
            ObjectBuilt = objectBuilt;
        }
    }
}
