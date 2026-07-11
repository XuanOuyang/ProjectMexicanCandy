using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class MoveCamera : MonoBehaviour 
{
    public Transform cameraPos;

    private void Update()
    {
        // Move the camera to the specified position
        transform.position = cameraPos.position;
    }
}
