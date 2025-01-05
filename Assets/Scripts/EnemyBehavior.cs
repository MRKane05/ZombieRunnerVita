using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour {

	public LayerMask redropMask;
	public float speed_walk = 3f;
	public float speed_amble = 6f;
	protected float speed_move = 0f;

	public GameObject target; //What is our zombie interested in?

	[HideInInspector]
	public bool bHasStruckPlayer = false;

	public float dash_radius = 7f;  //At what point do we speed up?

	public Range attentionRange = new Range(7f, 15f);
	protected float attention_radius = 15f; //If our player is outside of this range we just do Zombie stuff
	public VATCharacterAnimator targetCharacter;


	float EnemyFallSpeed = 0;
	float gravity = 20f;

	protected CharacterController characterController;
	protected Vector3 startPosition = Vector3.zero;

	float redropTime = 0;
	protected bool bZombieWaiting = true;

	protected Vector3 playerDir = Vector3.zero;
	protected float distToPlayer = 100f;

	public Range StrafeIntensityRange = new Range(0.05f, 0.1f);
	protected float strafeIntensity = 0.05f;

	void Start() {
		characterController = gameObject.GetComponent<CharacterController>();
		startPosition = gameObject.transform.position;
		attention_radius = attentionRange.GetRandom() * attentionRange.GetRandom();
		PickZombieStartingState();
		target = PC_FPSController.Instance.gameObject;
	}

	public void setTarget(GameObject newTarget)
    {
		target = newTarget;
    }

	public void Update() {
		//We need to check that our targets are valid before running through with this commandset
		if (target == null)
        {
			target = target = PC_FPSController.Instance.gameObject;
		}
		DoUpdate();
	}

	public virtual void DoUpdate() {
		//Debug.Log(Vector3.Dot(PC_FPSController.Instance.gameObject.transform.forward, Vector3.Normalize(gameObject.transform.position - PC_FPSController.Instance.gameObject.transform.position)));
		//Check to see if we're behind the player. It's important that this doesn't get avoided
		if (Vector3.Dot(PC_FPSController.Instance.gameObject.transform.forward, Vector3.Normalize(gameObject.transform.position - PC_FPSController.Instance.gameObject.transform.position)) < -0.5f)
		{
			//Debug.Log("Behind Player");
			ReDropEnemy();
		}

		DoGravity();

		//Calculate the player details to hand through to the movement systems
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		Vector3 right = transform.TransformDirection(Vector3.right);
		playerDir = target.transform.position - gameObject.transform.position;
		distToPlayer = playerDir.sqrMagnitude;

		//Really we should piggyback our screen position information here too


		float playerAngle = Mathf.Atan2(playerDir.x, playerDir.z);
		playerDir.y = 0; //Flatten our movement so we don't fly...
		playerDir = playerDir.normalized;


		if (distToPlayer > attention_radius)
		{
			characterController.Move(Vector3.up * EnemyFallSpeed * Time.deltaTime); //So that we'll fall into position before the player sees us
			return; //Don't movie our zombie, save some process
		}
		else if (bZombieWaiting)
		{
			//We just noticed our player. We need to decide if we're a walker, or a runner, and possibly play a challenge sound/start audio
			bZombieWaiting = false;
			PickZombieMoveState();
		}

		if (!bZombieWaiting)
		{
			DoEnemyMove(playerAngle, playerDir, distToPlayer, false);  //Move our enemy towards our player
		}
		//PickEnemyFrame(); //Our enemies will play a "grab" animation when they're close
		//If we're behind the player we should "re-drop" forward of the player somewhere to be an enemy a second time around (same as if we die)
		//if (PC_FPSController.Instance.gameObject.transform.position.z > gameObject.transform.position.z || gameObject.transform.position.z - PC_FPSController.Instance.gameObject.transform.position.z > 50) {
		//So we need a smarter way to tell if we're behind our player...
		
	}

	Vector2 behindStandardizedVector (Vector2 thisVec)
    {
		//thisVec = thisVec.normalized;
		//We're expecting that this will always be behind us
		float factor = Mathf.Abs(thisVec.y);
		return new Vector2(Mathf.Clamp(thisVec.x / factor, -1f, 1f), 1f);
    }

	Vector2 standardizeVector(Vector2 thisVec)
    {
		//thisVec = thisVec.normalized;
		Vector2 localVec = Vector2.zero;
		if (Mathf.Abs(thisVec.x) > Mathf.Abs(thisVec.y))
        {
			//Standardize to X
			float factor = Mathf.Abs(thisVec.x);
			localVec = new Vector2(thisVec.x / factor, thisVec.y / factor);
        } else
        {
			//Standardize to y
			float factor = Mathf.Abs(thisVec.y);
			localVec = new Vector2(thisVec.x / factor, thisVec.y / factor);
		}
		return localVec;
    }

	public virtual Vector3 GetWidgetPosition()
    {
		/*
		playerDir = target.transform.position - gameObject.transform.position;
		distToPlayer = playerDir.sqrMagnitude; 
		 
		Vector3 localPlayerDir = Camera.main.transform.InverseTransformDirection(playerDir);
		Vector2 flatPlayerDir = new Vector2(localPlayerDir.x, localPlayerDir.z);
		flatPlayerDir = standardizeVector(flatPlayerDir);   //This could be problematic as we don't want normalized, we want standardized...
		return new Vector3(flatPlayerDir.x, flatPlayerDir.y, distToPlayer);
		*/
		Vector3 playerDirection = (PC_FPSController.Instance.transform.position - gameObject.transform.position);
		float playerDistance = playerDirection.sqrMagnitude;
		//Debug.Log(playerDistance);
		//playerDirection = playerDirection.normalized;

		//Vector3 localPlayerDir = Camera.main.transform.InverseTransformDirection(playerDirection);
		Vector3 localPlayerDir = Camera.main.transform.InverseTransformPoint(gameObject.transform.position);
		//Debug.Log(localPlayerDir);
		Vector2 flatPlayerDir = new Vector2(localPlayerDir.x, localPlayerDir.z);
		flatPlayerDir = behindStandardizedVector(flatPlayerDir);   //This could be problematic as we don't want normalized, we want standardized...
		//Debug.Log(flatPlayerDir);
		return new Vector3(flatPlayerDir.x, flatPlayerDir.y, playerDistance);
    }

	public virtual void TriggerStrikePlayer(Collider other)
    {
		if (!bHasStruckPlayer)
		{
			PC_FPSController playerController = other.gameObject.GetComponent<PC_FPSController>();
			if (playerController)
			{
				//We can strike this player
				HitPlayer();	//Handles our animation
				playerController.EnemyHitPlayer(gameObject, false);
			}
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


	public virtual void ReDropEnemy() {

		if (Time.time < redropTime) { return; }

		//We want to check and see if we're at our target Zombie Count here too, and this could affect if we respawn
		if (LevelController.Instance.CheckZombieCount(gameObject))
		{
			//Debug.Log("Doing Enemy redrop");
			Vector3 dropPoint = Vector3.zero;
			dropPoint = LevelController.Instance.GetEnemyDropPoint();
			if (dropPoint == Vector3.zero)  //We failed
			{
				//Debug.Log("redrop failed");
				redropTime = Time.time + 3f;    //Give this a rest until later
			}
			else
			{
				LevelController.Instance.TestForChaserZombie(gameObject);   //See if we become a chaser
				RespawnEnemy(dropPoint);				
			}
		}
		else
        {
			//We should be turned off by our level controller
        }
	}

	protected virtual void PickZombieStartingState()
    {
		float randState = Random.value;
		if (randState > 0.75f)
        {
			setCurrentAnimation("Zombie_Scream");
        }
        else if (randState > 0.5f)
        {
			setCurrentAnimation("Zombie_Biting");
        }
		else if (randState > 0.25f)
        {
			setCurrentAnimation("ZombieAgonizing");
        } else
        {
			setCurrentAnimation("ZombieIdleAlert");
		}
    }

	protected virtual void PickZombieMoveState()
    {
		float rndState = Random.value;
		if (rndState > 0.3f)
        {
			speed_move = speed_amble;
			if (Random.value > 0.5f)
			{
				setCurrentAnimation("ZombieRun");
			} else
            {
				setCurrentAnimation("ZombieRunB");
			}
		} else
        {
			speed_move = speed_walk;
			setCurrentAnimation("Zombie_Walk_Aggressive");
        }
    }

	public virtual void RespawnEnemy(Vector3 thisPos) {
		gameObject.transform.position = thisPos + Vector3.up * 1.5f;
		gameObject.transform.eulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
		bHasStruckPlayer = false;
		bZombieWaiting = true;
		attention_radius = attentionRange.GetRandom() * attentionRange.GetRandom();
		strafeIntensity = StrafeIntensityRange.GetRandom();
		target = PC_FPSController.Instance.gameObject;
		PickZombieStartingState();
	}

	public void DoGravity()
    {
		if (characterController.isGrounded)
		{
			EnemyFallSpeed = 0;
		}
		else
		{
			EnemyFallSpeed -= gravity * Time.deltaTime;
		}
	}

	public virtual void DoEnemyMove(float playerAngle, Vector3 playerDir, float distToPlayer, bool bIsChaser)
	{

		//It's actually a little pointless to have these different speeds as the player doesn't get to see it
		float moveSpeed = speed_move; // distToPlayer > dash_radius ? speed_amble : speed_dash;

		Vector3 moveDirection = playerDir * moveSpeed;  //Get the net of how we should be ambling

		moveDirection.y = EnemyFallSpeed;
		//We could do with having a function to make our zombies spread out from each other so that they don't end up clustering
		//This needs to be different if we're chaser zombies...
		Vector3 StrafeDirection = LevelController.Instance.GetEnemyStrafeDirection(gameObject, bIsChaser);
		StrafeDirection *= Mathf.Lerp(0, strafeIntensity, distToPlayer / 30f);  //Some graduation to lessen the strength of the effect. Of course we could also do with a proximity function here too
		StrafeDirection = Vector3.zero;

		characterController.Move((moveDirection + StrafeDirection) * Time.deltaTime);   //Actually do our move
																	//We need to point our enemy at our player
																	//This isn't good enough for our direction. While I don't feel we need pathfinding we do need enemies that don't operate like turrets
																	//gameObject.transform.LookAt(PC_FPSController.Instance.gameObject.transform.position, Vector3.up);
																	//gameObject.transform.eulerAngles = new Vector3(0, playerAngle * 180f/ Mathf.PI, 0);
		gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(new Vector3(0, playerAngle * 180f / Mathf.PI, 0)), Time.deltaTime*4f);
	}
}
