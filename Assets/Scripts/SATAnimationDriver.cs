using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SATAnimationDriver : MonoBehaviour {
	VATCharacterAnimator attachedAnimator;
	float nexChangeTime = 5f;
	// Use this for initialization
	void Start () {
		attachedAnimator = gameObject.GetComponent<VATCharacterAnimator>();
		attachedAnimator.SetCurrentAnimation("ZombieRun");
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > nexChangeTime)
        {
			attachedAnimator.CrossFade(attachedAnimator.VATAnims[Random.Range(0, attachedAnimator.VATAnims.Count)].name, 0.5f);
			nexChangeTime = Time.time + 5f;
		}
	}
}
