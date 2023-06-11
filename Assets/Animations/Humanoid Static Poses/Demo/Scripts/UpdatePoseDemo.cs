using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HumanoidStaticPoseAnimations
{
    [RequireComponent(typeof(Animator))]
    public class UpdatePoseDemo : MonoBehaviour
    {
        public string DefaultAnimName = "Stand_1";
        public float Interval = 0.3f;

        private Animator _animator;
        private float _timer = 0f;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _animator.Play(DefaultAnimName);
        }

        // Update is called once per frame
        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer > Interval)
            {
                _timer = 0f;
                _animator.SetTrigger("Next");
            }
        }
    }
}