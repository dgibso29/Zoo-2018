using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Zoo.Events
{
    class EventManager : MonoBehaviour
    {

        // Declare component references
        public Systems.Construction.ConstructionTools constructionTools;

        private void Start()
        {
            InitializeListeners();
        }

        void InitializeListeners()
        {
            InitializeConstructionToolsListeners();
            InitializeErrorListeners();
        }

        void InitializeConstructionToolsListeners()
        {
            constructionTools.ObjectBuilt += new Systems.Construction.ConstructionTools.ObjectBuiltEventHandler(HandleObjectBuilt);
            constructionTools.ObjectDeleted += new Systems.Construction.ConstructionTools.ObjectDeletedEventHandler(HandleObjectDeleted);
        }

        void InitializeErrorListeners()
        {
            ErrorHelper.ErrorOccurred += new ErrorHelper.ErrorOccurredEventHandler(HandleErrorOccurred);
        }

        void HandleObjectBuilt(object sender, Systems.Construction.ObjectEventArgs args)
        {
            Debug.Log($"Built a {args.ObjectBuilt.objectID} with a UOID of {args.ObjectBuilt.UniqueObjectID}.");
        }

        void HandleObjectDeleted(object sender, Systems.Construction.ObjectEventArgs args)
        {
            Debug.Log($"Deleted a {args.ObjectBuilt.objectID} with a UOID of {args.ObjectBuilt.UniqueObjectID}.");
        }

        void HandleErrorOccurred(object sender, ErrorOccurredEventArgs args)
        {
            Debug.Log(ErrorHelper.ErrorDictionary[args.type]);
        }
    }
}
