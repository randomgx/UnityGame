using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFaceCamera : MonoBehaviour
{
    void Update()
    {
        this.transform.LookAt(Camera.main.transform);
    }
}
