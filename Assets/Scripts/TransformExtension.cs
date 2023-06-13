using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension
{
    public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
    {
        return transform.position + transform.rotation * position;
    }
 
    public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
    {
        position -= transform.position;
        return Quaternion.Inverse(transform.rotation) * position;
    }
}
