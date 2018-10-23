using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Zoo
{
    public class ErrorManager : MonoBehaviour
    {
        void Start()
        {
            ErrorHelper.InitializeErrorDictionary();
        }
    }
}
