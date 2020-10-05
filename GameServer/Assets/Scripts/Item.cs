using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemInfo info;
    bool equipped;
    bool drawn;

    public bool IsEquipped()
    {
        return equipped;
    }

    public bool IsDrawn()
    {
        return drawn;
    }

    public void SetDraw(int _id, bool _drawn)
    {
        drawn = _drawn;
        ServerSend.DrawedItem(_id, info.itemName, _drawn);
    }

    public void SetEquipped(int _id, bool _state)
    {
        equipped = _state;
        if(equipped)
        {
            ServerSend.EquippedItem(_id, info.itemName, _state);
        }
        else
        {
            Debug.Log("unequipped weapon on server sucessfully");
        }
    }
}
