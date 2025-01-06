using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PC_AnimationEventForwarder : MonoBehaviour {
    public PC_FPSController ourParentController;
    public void DoFootstepSound()
    {
        ourParentController.DoFootstepSound();
    }
}
