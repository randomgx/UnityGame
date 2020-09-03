using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformUpdate
{
    public static TransformUpdate zero = new TransformUpdate(0, Vector3.zero);

    public int tick;
    public Vector3 position;
    public Quaternion rotation;

    public TransformUpdate(int _tick, Vector3 _position)
    {
        tick = _tick;
        position = _position;
        rotation = Quaternion.identity;
    }
}