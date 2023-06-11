using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HumanoidStaticPoseAnimations
{
    [RequireComponent(typeof(Animator))]
    public class PoseSetter : MonoBehaviour
    {
        public string TargetAnimName = "Stand_1";
        private void OnEnable()
        {
            GetComponent<Animator>().Play(TargetAnimName);
        }
    }
}
