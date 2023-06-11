using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HumanoidStaticPoseAnimations
{
    public class PreviewHandler : MonoBehaviour
    {
        public List<Animator> ModlesAnmaterList = new List<Animator>();

       public void PlayNext()
        {
            SetTriggerToModels("Next");
        }

        public void PlayPrevt()
        {
            SetTriggerToModels("Prev");
        }

        private void SetTriggerToModels(string triggerName)
        {
            foreach (Animator anim in ModlesAnmaterList)
            {
                anim.SetTrigger(triggerName);
            }
        }
    }
}
