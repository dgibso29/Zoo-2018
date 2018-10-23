using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMouseSelectable
{
    bool CanBeSelected();

    void OnClicked();
}
