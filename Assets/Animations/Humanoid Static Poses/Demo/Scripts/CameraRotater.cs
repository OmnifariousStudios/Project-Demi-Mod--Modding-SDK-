using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HumanoidStaticPoseAnimations
{
    public class CameraRotater : MonoBehaviour
    {
        public Transform TargetObject;
        public float Speed = 0.3f;

        void Update()
        {
            if (TargetObject == null) return;
            transform.RotateAround(TargetObject.transform.position, new Vector3(0, 1, 0), Speed);
        }
    }
}
