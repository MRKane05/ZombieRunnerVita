using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_StrikeTriggerArea : MonoBehaviour {
	EnemyBehavior ourBaseEnemy;

	void Start() {
		ourBaseEnemy = gameObject.transform.parent.GetComponent<EnemyBehavior>(); //Grab our controller
	}

	void OnTriggerEnter(Collider other)
    {
		//This could do with getting passed to a delegate function on the Enemy controller
		ourBaseEnemy.TriggerStrikePlayer(other);
		/*
		if (!ourBaseEnemy.bHasStruckPlayer) {
			PC_FPSController playerController = other.gameObject.GetComponent<PC_FPSController>();
			if (playerController) {
				//Debug.Log("HitPlayer");
				//We can strike this player
				ourBaseEnemy.HitPlayer();
				playerController.EnemyHitPlayer(gameObject);
			}
		}*/
    }
}
