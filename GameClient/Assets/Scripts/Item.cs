using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public GameObject itemGameObject;
    public ItemInfo info;
    public bool equipped;
    public bool drawn;

    public bool IsEquipped()
    {
        return equipped;
    }

    public bool IsDrawn()
    {
        return drawn;
    }

    public void SetDraw(bool _drawn)
    {
        drawn = _drawn;
        if(_drawn)
        {
            itemGameObject.SetActive(true);
        }
        else
        {
            itemGameObject.SetActive(false);
        }
    }

    public void SetEquipped(bool _state)
    {
        equipped = _state;
    }
}
