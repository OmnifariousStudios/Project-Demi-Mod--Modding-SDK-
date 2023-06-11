using UnityEngine;
using System.Collections;

public class RotateOverTime : MonoBehaviour 
{
    public Vector3 DegreesPerFrame;
	
    void FixedUpdate() 
    { 
        transform.Rotate (DegreesPerFrame); 
    }
}
