using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A go-to for enemies that will be chasing the player. This promises to get tricky
public class EnemyBehavior_Chaser : EnemyBehavior {

	public bool bNeedsRegrounded = true;
	public bool bIsVirutal = false;
	public Range runSpeeds = new Range(7f, 15f);
	public Range rubberbandRanges = new Range(25f, 40f);
	float rubberbandRange = 30f;
	LayerMask groundLayerMask = 1 << 0; // default layer (0)

	void Start()
	{
		rubberbandRange = rubberbandRanges.GetRandom();
		characterController = gameObject.GetComponent<CharacterController>();
		startPosition = gameObject.transform.position;
		attention_radius = attentionRange.GetRandom();
		//PickZombieStartingState();
		PickRunState();
		speed_move = speed_amble;
		//We need to add our tracking marker to the HUD
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
	public void DoVirtualEnemyMove(float playerAngle, Vector3 playerDir)
	{
		gameObject.transform.position += playerDir * speed_move * Time.deltaTime;  //Just follow our player while we're in virtual
																					//Lets do a raycast and just plonk our zombie on whatever we hit. Yes this could be a major issue, but for the moment phuckit
		RaycastHit hit;
		// Does the ray intersect any objects excluding the player layer
		if (Physics.Raycast(transform.position + Vector3.up * 20f, -Vector3.up, out hit, 50f, groundLayerMask))

		{
			gameObject.transform.position = hit.point + Vector3.up * 2f; //Give us plenty of clearance
		}
	}

	public override void DoUpdate()
	{
		//Calculate the player details to hand through to the movement systems
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		Vector3 right = transform.TransformDirection(Vector3.right);
		playerDir = PC_FPSController.Instance.gameObject.transform.position - gameObject.transform.position;
		distToPlayer = playerDir.sqrMagnitude;

		float playerAngle = Mathf.Atan2(playerDir.x, playerDir.z);
		playerDir.y = 0; //Flatten our movement so we don't fly...
		playerDir = playerDir.normalized;

		//So we could do with a bit of a rubberband to keep the pressure on for the zombies...
		speed_move = runSpeeds.GetLerp((distToPlayer / rubberbandRange));
		Debug.Log("speed: " + speed_move);
		if (distToPlayer > attention_radius)
		{
			DoVirtualEnemyMove(playerAngle, playerDir);

		} else
		{
			DoGravity();
			DoEnemyMove(playerAngle, playerDir);
		}
	}
}
