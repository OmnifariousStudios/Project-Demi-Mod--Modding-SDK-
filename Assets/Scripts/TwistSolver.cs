using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwistSolver : MonoBehaviour
{
    [Tooltip("If 0.5, this Transform will be twisted half way from parent to child. If 1, the twist angle will be locked to the child and will rotate with along with it.")]
    [Range(0f, 1f)] public float parentChildCrossfade = 0.5f;

    [Tooltip("Rotation offset around the twist axis.")]
    [Range(-180f, 180f)] public float twistAngleOffset;
}
