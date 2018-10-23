using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISearchable
{
    /// <summary>
    /// Get tags by which this item can be searched.
    /// </summary>
    string[] GetSearchTags();
}
