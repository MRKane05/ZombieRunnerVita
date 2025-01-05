using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A go-to for enemies that will be chasing the player. This promises to get tricky
public class EnemyBehavior_Chaser : EnemyBehavior {

	[Space]
	[Header("Chaser Zombie Extras")]
	public GameObject ChaserIndicatorPrefab;
	public bool bNeedsRegrounded = true;
	public bool bIsVirutal = false;
	public Range runSpeeds = new Range(7f, 15f);
	public Range rubberbandRange = new Range(12, 40);
	LayerMask groundLayerMask = 1 << 0; // default layer (0)

	float hitPauseTime = 0; //A ticker to have this enemy slow down after it hits the player, thus "repriming" the strike
	float hitPauseDuration = 2f; //This'll change to adjust difficulty
	float runSpeed_slow = 1;	//After striking the player how much do we fall back?

	GameObject ourChaserIcon;

	void Start()
	{
		characterController = gameObject.GetComponent<CharacterController>();
		startPosition = gameObject.transform.position;
		attention_radius = attentionRange.GetRandom();
		//PickZombieStartingState();
		PickRunState();
		speed_move = speed_amble;
		//We need to add our tracking marker to the HUD
		ourChaserIcon = PlayerHUDHandler.Instance.AddIndicatorIcon(ChaserIndicatorPrefab);
		Enemy_Chasericon newChaser = ourChaserIcon.GetComponent<Enemy_Chasericon>();
		newChaser.parentEnemy = this;
	}

	void PickRunState()
    {
		if (Random.value > 0.5f)
		{
			setCurrentAnimation("ZombieRun");
		}
		else
		{
			setCurrentAnimation("ZombieRunB");
		}
	}

	//The idea behind this function is to make sure that our pawn is on the ground so that it may move as expected
	public bool doReground()
	{
		characterController.enabled = true;
		bIsVirutal = false;
		return true;
	}

	public void doVirtual()
	{
		characterController.enabled = false;
	}

	//Move our enemy without having to do all the controller movement stuff
	public void DoVirtualEnemyMove(float playerAngle, Vector3 playerDir, float playerDist)
	{
		Vector3 StrafeDirection = LevelController.Instance.GetEnemyStrafeDirection(gameObject, true);
		StrafeDirection *= (strafeIntensity * (1f - Mathf.Clamp01(playerDist / 155f)));	//Add a falloff to our strafe intensity
		gameObject.transform.position += (playerDir + StrafeDirection) * speed_move * Time.deltaTime;  //Just follow our player while we're in virtual
																					//Lets do a raycast and just plonk our zombie on whatever we hit. Yes this could be a major issue, but for the moment phuckit
		RaycastHit hit;
		// Does the ray intersect any objects excluding the player layer
		if (Physics.Raycast(transform.position + Vector3.up * 20f, -Vector3.up, out hit, 50f, groundLayerMask))

		{
			gameObject.transform.position = hit.point + Vector3.up * 2f; //Give us plenty of clearance
		}
	}

	public override void TriggerStrikePlayer(Collider other)
	{
		PC_FPSController playerController = other.gameObject.GetComponent<PC_FPSController>();
		if (playerController)
		{
			//We can strike this player
			HitPlayer();    //Handles our animation
			playerController.EnemyHitPlayer(gameObject, true);  //This needs to be a different call
			hitPauseTime = hitPauseDuration;
		}
	}

	public override void DoUpdate()
	{
		//Calculate the player details to hand through to the movement systems
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		Vector3 right = transform.TransformDirection(Vector3.right);
		playerDir = target.transform.position - gameObject.transform.position;
		distToPlayer = playerDir.sqrMagnitude;

		//At some point we've got to get so far behind our player that we give up
		//Alternatively there's got to be a point where we're sick of chasing our player and we give up

		float playerAngle = Mathf.Atan2(playerDir.x, playerDir.z);
		playerDir.y = 0; //Flatten our movement so we don't fly...
		playerDir = playerDir.normalized;

		//So we could do with a bit of a rubberband to keep the pressure on for the zombies...
		speed_move = runSpeeds.GetLerp((distToPlayer - rubberbandRange.Min) / (rubberbandRange.Max - rubberbandRange.Min));
		hitPauseTime -= Time.deltaTime;
		if (hitPauseTime > 0)
        {
			speed_move = runSpeed_slow;
        }
		//Debug.Log("speed: " + speed_move);
		if (distToPlayer > attention_radius)
		{
			Vector3 StrafeDirection = LevelController.Instance.GetEnemyStrafeDirection(gameObject, true);
			DoVirtualEnemyMove(playerAngle, playerDir, distToPlayer);

		} else
		{
			DoGravity();
			DoEnemyMove(playerAngle, playerDir, distToPlayer, true);
		}
	}

	public override void RespawnEnemy(Vector3 thisPos)
	{
		gameObject.transform.position = thisPos + Vector3.up * 1.5f;
		gameObject.transform.eulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
		bHasStruckPlayer = false;
		bZombieWaiting = false;
		attention_radius = attentionRange.GetRandom() * attentionRange.GetRandom();
		strafeIntensity = StrafeIntensityRange.GetRandom();
		target = PC_FPSController.Instance.gameObject;
		//PickZombieStartingState();
		PickRunState();	//Chaser zombies have only one state, and that's ON
	}

}
