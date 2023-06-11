using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Create Hand Pose")]
public class HVRHandPose : ScriptableObject
{
    public string handPoseName;
    
    public HVRHandPoseData LeftHand;
    public HVRHandPoseData RightHand;
    
}

public enum HVRHandSide
{
    Left, Right
}
