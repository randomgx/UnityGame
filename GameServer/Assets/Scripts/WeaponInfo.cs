using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Weapon")]
public class WeaponInfo : ItemInfo
{
    public float range;
    public float damage;
    public Animator fpAnimator;
    public Animator tpAnimator;
}
