using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownObject : MonoBehaviour {
	public float lifeSpan = 5f; //How many seconds will this be active for?
	public float effectiveRadius = 100f; //Anything outside of this won't get distracted
	public float effectiveness = 0.5f; //0-1 for how many enemies will be distracted by this

	void Start () {
		Destroy(gameObject, lifeSpan);
		triggerDoDistraction();
	}
	
	public void triggerDoDistraction()
    {
		//We need some way of making this a notification that'll go through to our zombies...
		LevelController.Instance.TriggerDistraction(gameObject, effectiveRadius, effectiveness);

    }

	// Update is called once per frame
	void Update () {
		
	}
}
