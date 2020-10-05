using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFaceCamera : MonoBehaviour
{
    void Update()
    {
        if (transform != null && Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
        }
        else if(Camera.current != null)
        {
            transform.LookAt(Camera.current.transform);
        }
        else
        {
            return;
        }
    }
}
