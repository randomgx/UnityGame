using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFaceCamera : MonoBehaviour
{
    void Update()
    {
        if(this.transform != null)
        this.transform.LookAt(Camera.main.transform);
    }
}
