using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HVRPosableBoneData
{
    public Vector3 Position;
    public Quaternion Rotation;
    
    public HVRPosableBoneData DeepCopy()
    {
        return new HVRPosableBoneData()
        {
            Position = Position,
            Rotation = Rotation
        };
    }
}
