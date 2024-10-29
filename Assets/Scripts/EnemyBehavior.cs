using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour {

	public LayerMask redropMask;
	public float speed_walk = 3f;
	public float speed_amble = 6f;
	float speed_move = 0f;

	[HideInInspector]
	public bool bHasStruckPlayer = false;

	public float dash_radius = 7f;	//At what point do we speed up?

	public float attention_radius = 15f; //If our player is outside of this range we just do Zombie stuff
	public VATCharacterAnimator targetCharacter;


	float EnemyFallSpeed = 0;
	float gravity = 20f;

	CharacterController characterController;
	Vector3 startPosition = Vector3.zero;

	float redropTime = 0;
	bool bZombieWaiting = true;

	void Start() {
		characterController = gameObject.GetComponent<CharacterController>();
		startPosition = gameObject.transform.position;
		PickZombieStartingState();
	}

	public void Update() {
		DoEnemyMove();  //Move our enemy towards our player
						//PickEnemyFrame(); //Our enemies will play a "grab" animation when they're close
						//If we're behind the player we should "re-drop" forward of the player somewhere to be an enemy a second time around (same as if we die)
						//if (PC_FPSController.Instance.gameObject.transform.position.z > gameObject.transform.position.z || gameObject.transform.position.z - PC_FPSController.Instance.gameObject.transform.position.z > 50) {
						//So we need a smarter way to tell if we're behind our player...
		//Debug.Log(Vector3.Dot(PC_FPSController.Instance.gameObject.transform.forward, Vector3.Normalize(PC_FPSController.Instance.gameObject.transform.position - gameObject.transform.position)));
		
		if (Vector3.Dot(PC_FPSController.Instance.gameObject.transform.forward, Vector3.Normalize(gameObject.transform.position - PC_FPSController.Instance.gameObject.transform.position)) < -0.5f) { 
			ReDropEnemy();
		}
		
	}

	string currentAnimation = "";
	public void setCurrentAnimation(string animName)
	{
		if (currentAnimation != animName)
		{
			currentAnimation = animName;
			targetCharacter.CrossFade(animName, 0.5f);
		}
	}

	public void HitPlayer() {
		//Play our animation stuff for hitting our player. This'll also have to be reflective of our current animation state
		setCurrentAnimation("ZombieStrike");
	}


	public void ReDropEnemy() {

		if (Time.time < redropTime) { return; }

		Debug.Log("Doing Enemy redrop");
		Vector3 dropPoint = Vector3.zero;
		dropPoint = LevelController.Instance.GetEnemyDropPoint(redropMask);
		if (dropPoint == Vector3.zero)  //We failed
		{
			Debug.Log("redrop failed");
			redropTime = Time.time + 3f;    //Give this a rest until later
		}
		else
		{
			RespawnEnemy(dropPoint);
		}
	}

	void PickZombieStartingState()
    {
		float randState = Random.value;
		if (randState > 0.6f)
        {
			setCurrentAnimation("ZombieIdleAlert");
        }
        else
        {
			//Lets have this zombie doing something - like eating something on the ground
			setCurrentAnimation("Zombie_Biting");
        }
    }

	void PickZombieMoveState()
    {
		float rndState = Random.value;
		if (rndState > 0.3f)
        {
			speed_move = speed_amble;
			setCurrentAnimation("ZombieRun");
		} else
        {
			speed_move = speed_walk;
			setCurrentAnimation("Zombie_Walk_Aggressive");
        }
    }

	public void RespawnEnemy(Vector3 thisPos) {
		gameObject.transform.position = thisPos + Vector3.up * 1.5f;
		gameObject.transform.eulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
		bHasStruckPlayer = false;
		bZombieWaiting = true;
		PickZombieStartingState();
	}

	public void DoEnemyMove()
	{
		//PROBLEM: This will need to be replaced with a curve sample for our direction
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		Vector3 right = transform.TransformDirection(Vector3.right);
		Vector3 playerDir = PC_FPSController.Instance.gameObject.transform.position - gameObject.transform.position;
		float distToPlayer = playerDir.magnitude;

		if (characterController.isGrounded)
		{
			EnemyFallSpeed = 0;
		}
		else
		{
			EnemyFallSpeed -= gravity * Time.deltaTime;
		}

		if (distToPlayer > attention_radius) {
			characterController.Move(Vector3.up * EnemyFallSpeed * Time.deltaTime); //So that we'll fall into position before the player sees us
			return; //Don't movie our zombie, save some process
		} else if (bZombieWaiting)
        {
			//We just noticed our player. We need to decide if we're a walker, or a runner, and possibly play a challenge sound/start audio
			bZombieWaiting = false;
			PickZombieMoveState();
		}

		float playerAngle = Mathf.Atan2(playerDir.x, playerDir.z);
		//Debug.Log(playerAngle);
		playerDir.y = 0; //Flatten our movement so we don't fly...
		playerDir = playerDir.normalized;
		//It's actually a little pointless to have these different speeds as the player doesn't get to see it
		float moveSpeed = speed_move; // distToPlayer > dash_radius ? speed_amble : speed_dash;

		Vector3 moveDirection = playerDir * moveSpeed;  //Get the net of how we should be ambling

		moveDirection.y = EnemyFallSpeed;
		characterController.Move(moveDirection * Time.deltaTime);   //Actually do our move
																	//We need to point our enemy at our player
																	//This isn't good enough for our direction. While I don't feel we need pathfinding we do need enemies that don't operate like turrets
																	//gameObject.transform.LookAt(PC_FPSController.Instance.gameObject.transform.position, Vector3.up);
																	//gameObject.transform.eulerAngles = new Vector3(0, playerAngle * 180f/ Mathf.PI, 0);
		gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(new Vector3(0, playerAngle * 180f / Mathf.PI, 0)), Time.deltaTime*4f);
	}
}
