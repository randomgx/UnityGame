using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Default Item")]
public class ItemInfo : ScriptableObject
{
    public string itemName;
    public ItemType itemType;

    public float range;
    public float damage;
    public Animator fpAnimator;
    public Animator tpAnimator;

    public enum ItemType
    {
        Weapon,
        Gameplay
    }
}
