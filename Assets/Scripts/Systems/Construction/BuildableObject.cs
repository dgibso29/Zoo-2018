using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoo.Systems.Construction
{

    /// <summary>
    /// Differentiates between different object footprint types, such as a 'Full' building, or an 'EdgeOfTile' wall, etc.
    /// </summary>
    public enum FootprintType { Full, EdgeOfTile, CornerOfTile, PartialTile, Path, PathObject }

    [RequireComponent(typeof(BoxCollider))]
    [System.Serializable]
    public class BuildableObject : MonoBehaviour, ISearchable
    {

        /// <summary>
        /// Type of object (Building, Scenery, Fence, Path, etc) as a string.
        /// </summary>
        public string type;

        /// <summary>
        /// Determines what type of footprint the object has, whether it's 'Full' for a building or 'EdgeOfTile' for a wall, etc.
        /// This dictates how, and if, the object is rotated, and how it is placed on the grid during construction.
        /// </summary>
        public FootprintType objectFootprintType;

        /// <summary>
        /// Unique ID to identify object type.
        /// </summary>
        public string objectID;

        /// <summary>
        /// Unique object ID number, used to find this specific object.
        /// </summary>
        public int UniqueObjectID { get; set; }

        /// <summary>
        /// Date, in game time, that the object was built.
        /// </summary>
        public DateTime BuildDate { get; set; }

        /// <summary>
        /// Object name in game UI.
        /// </summary>
        public string objectName;

        /// <summary>
        /// Tags by which this item can be searched.
        /// </summary>
        public string[] searchTags;

        /// <summary>
        /// Name of the scenery groups this item belongs to.
        /// </summary>
        public string[] SceneryGroupNames { get; }

        /// <summary>
        /// Object position in world coodinates.
        /// </summary>
        public Vector3 worldCoordinates;

        /// <summary>
        /// Object rotation.
        /// </summary>
        public Quaternion objectRotation;

        /// <summary>
        /// Footprint of object in X/Y/Z coordinates, where 1x1 is a quarter tile-sized object. 
        /// Can also be used as reference for object clearances.
        /// </summary>
        public Vector3 objectFootprint;

        /// <summary>
        /// How much does this object cost?
        /// </summary>
        public float buildCost = 0;

        /// <summary>
        /// How much does this object cost to delete?
        /// </summary>
        public float deleteCost = 0;
        
        /// <summary>
        /// Used to calculate refund when object is deleted. Defaults to a full refund.
        /// </summary>
        public float refundPercentage = 100;

        /// <summary>
        /// Has the player unlocked this object?
        /// </summary>
        public bool unlocked;

        /// <summary>
        /// Does the object qualify for clearance checks?
        /// </summary>
        public bool hasClearances = true;

        /// <summary>
        /// Can the object be automatically deleted when building over it? Will also disqualify it from clearance checks.
        /// Note: When building autoDeletable objects, other autoDeletable objects will be treated as normal objects.
        /// </summary>
        public bool autoDeletable = true;

        /// <summary>
        /// Can the object be deleted? Useful to designate 'historical' objects, or any other potential uses. Defaults to true.
        /// </summary>
        public bool canBeDeleted = true;

        /// <summary>
        /// Does construction stop after placing this object?
        /// </summary>
        public bool multiBuildable = true;

        /// <summary>
        /// Can the object's height be changed?
        /// </summary>
        public bool canHaveHeightModified = true;

        /// <summary>
        /// Can the object be built free of the grid?
        /// </summary>
        public bool canBeBuiltFreeform = true;

        /// <summary>
        /// Can the object be rotated in fixed increments?
        /// </summary>
        public bool isFixedRotateable = false;

        /// <summary>
        /// Can the object be free-form rotated?
        /// </summary>
        public bool isFullyRotateable = false;

        /// <summary>
        /// Can the object be dragged when building? Used for paths, fences, etc.
        /// </summary>
        public bool isDraggable = false;

        public UnlockRequirements unlockRequirements;

        /// <summary>
        /// Requirements to unlock this object if not in sandbox mode.
        /// </summary>
        public struct UnlockRequirements
        {
            // Renown, Reputation, Other factors.            
        }

        public UnityEngine.Events.UnityAction ObjectBuiltUA;


        /// <summary>
        /// Perform necessary tasks when object is built.
        /// </summary>
        public void InitializeObject()
        {
            // Add object to directory of objects
            Administration.ZooManager.ObjectsInZoo.Add(this);
            
            GenerateUniqueObjectID();
        }

        void GenerateUniqueObjectID()
        {
            UniqueObjectID = Administration.ZooManager.ZooInfo.LastUniqueObjectID;
            Administration.ZooManager.ZooInfo.LastUniqueObjectID++;
        }

        void Start()
        {
            GenerateObjectFootprint();
            gameObject.layer = 12;

        }

        /// <summary>
        /// When called, delete this BuildableObject.
        /// </summary>
        public virtual void Delete()
        {
            //Debug.Log($"Deleting object of type {objectID} with UOID of {UniqueObjectID}.");
            // Remove the object from the list of built objects.
            Administration.ZooManager.ObjectsInZoo.Remove(this);

            // Refund Build Cost/Charge Delete Cost if applicable -- Do in construction tools..? Use events??

            // Destroy the game object.
            Destroy(gameObject);

        }

        /// <summary>
        /// Set object footprint based on model size.
        /// </summary>
        public void GenerateObjectFootprint()
        {
            float footX;
            float footY;
            float footZ;

            float colX;
            float colY;
            float colZ;

            // Grab footprint before adjusting to meet game logic requirements.
            Mesh objectMesh = GetComponent<MeshFilter>().sharedMesh;
            footX = objectMesh.bounds.size.x / 100;
            footY = objectMesh.bounds.size.y / 100;
            footZ = objectMesh.bounds.size.z / 100;

            colX = footX;
            colY = footY;
            colZ = footZ;

            // Adjust footprint to meet logic requirements based on object footprint type.
            switch (objectFootprintType)
            {
                case FootprintType.Full:
                    {
                        // Do nothing!
                        break;
                    }
                case FootprintType.EdgeOfTile:
                    {
                        // Do nothing!
                        break;
                    }
                case FootprintType.CornerOfTile:
                    {
                        // Round to nearest quarter to accommodate smallest possible objects.
                        footX = World.World.RoundToNearestQuarter(footX);
                        footY = World.World.RoundToNearestQuarter(footY);
                        footZ = World.World.RoundToNearestQuarter(footZ);
                        break;
                    }
                case FootprintType.PartialTile:
                    {
                        // Do nothing!
                        break;
                    }
                case FootprintType.Path:
                    {
                        // All paths have a clearance of 2 meters.
                        colY = 2;
                        // Set center of collider to start at bottom of path object.
                        GetComponent<BoxCollider>().center = new Vector3(0, 100 - (objectMesh.bounds.size.y / 2), 0);
                        break;
                    }
                case FootprintType.PathObject:
                    {
                        // Do stuff later!
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            // Set footprint
            objectFootprint = new Vector3(footX, footY, footZ);
            // Set object collider to match game logic requirements (primarily for clearances).
            GetComponent<BoxCollider>().size = new Vector3(colX * 100, colY * 100, colZ * 100);
        }

        public string[] GetSearchTags()
        {
            return searchTags;
        }
    }
}
