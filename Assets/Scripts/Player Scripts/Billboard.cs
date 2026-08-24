using UnityEngine;

public class Billboard : MonoBehaviour
{
private Quaternion startingRotation;

void Start()
{
    startingRotation = transform.rotation;
}

void LateUpdate()
{
    transform.rotation = startingRotation;
}
}