using PathCreation;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class LevelZombie //Essentially this is metadata
{
	public enum enZombieState { NULL, DISABLED, PENDING, ENABLED } //PENDING: We're going to be enabled after a delay
	public enZombieState ZombieState = enZombieState.DISABLED;
	public GameObject Zombie;
	public void SetActive(bool state)
    {
		if (!state)
        {
			Zombie.SetActive(false);
			ZombieState = enZombieState.DISABLED;
        } else
        {
			Zombie.SetActive(true);
			ZombieState = enZombieState.ENABLED;
        }
    }

	public void SetPending()
    {
		ZombieState = enZombieState.PENDING;
    }
}

public class LevelController : MonoBehaviour {
	private static LevelController instance = null;
	public static LevelController Instance { get { return instance; } }

	public enum enLevelPlayState { NULL, START, STARTPLAY, PLAYING, ENDED, PAUSED }
	public enLevelPlayState levelPlayState = enLevelPlayState.START;
	[Header("Level Setup Details")]
	public PathCreator pathCreatorReference;
	//public PathCreator pathCreator_Reverse;
	public GameObject startPointForward, startPointReverse;
	public GameObject PlayerCharacter;	//This is really just for positioning our player at the start/end of the run
	public Texture2D zombiePropTexture; //This needs to be specific to each level
	public System.Byte[,] zombiePropTable;
	public int tableDistance = 0;   //What's the maximum distance on our table?
	public int tableWidth = 0;
	public float RunDistance = 250f;	//What's the overall distance of our run?
	bool bHasPropensityFile = false;

	//A few details handling our run direction (this could be all kinds of nasty...)
	public bool bRunForward = true;

	[Space]
	[Header("Level Zombie Details")]

	public int maxZombies = 6;	//What's the maximum number of zombies we'll have?
	public int startingZombies = 3; //How many zombies do we start with?
	public int maxChaserZombies = 3;    //What's the maximum number of chasers we can have in this level
	public float zombieBecomeChaserChance = 0.125f; //1 == every zombie becomes a chaser
	public Range StartZombieDelays = new Range(0f, 5f);
	public AnimationCurve ZombieDensityCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));	//To control the distribution of zombies throughout the run
	public GameObject ZombiePrefab; //This'll need expanded. It's getting what it gets for the moment
	public LayerMask zombieDropMask;
	public GameObject ChaserZombiePrefab;
	public List<LevelZombie> SpawnedZombies = new List<LevelZombie>();
	public List<LevelZombie> SpawnedChasers = new List<LevelZombie>();

	void Awake()
	{
		if (instance)
		{
			Debug.Log("Somehow there's a duplicate LevelController in the scene");
			Debug.Log(gameObject.name);
			return; //cancel this
		}

		instance = this;

		string PBRPath = GetPRBPath();
		/*
		string PBRPath = GetPRBPath();
		if (File.Exists(PBRPath))
		{
			ReadArrayFromBinaryFile(PBRPath);
			//ReadArrayFromResourcesBinaryFile();
			return Vector3.zero;
		} else
		{
			Debug.LogError("No PBR file found at: " + PBRPath);
		}*/
		ReadArrayFromResourcesBinaryFile();

		setPlayState(enLevelPlayState.START); //Prepare our start functionality stuff

		bRunForward = GameController.Instance.RunDetails.bRunMapForward;	//Make sure we've grabbed this value right off of the hammer so that we'll know what we're doing here
	}

	public int GetActiveZombieCount()
    {
		int activeZombies = 0;
		foreach(LevelZombie thisZombie in SpawnedZombies)
        {
			if (thisZombie.ZombieState == LevelZombie.enZombieState.PENDING || thisZombie.ZombieState == LevelZombie.enZombieState.ENABLED) {
				activeZombies++;
            }
        }
		return activeZombies;
    }

	public int GetActiveZombieChaserCount()
	{
		int activeZombies = 0;
		foreach (LevelZombie thisZombie in SpawnedChasers)
		{
			if (thisZombie.ZombieState == LevelZombie.enZombieState.PENDING || thisZombie.ZombieState == LevelZombie.enZombieState.ENABLED)
			{
				activeZombies++;
			}
		}
		return activeZombies;
	}

	public LevelZombie GetLevelZombieByObject(GameObject thisZombieGO)
    {
		foreach (LevelZombie thisLevelZombie in SpawnedZombies)
        {
			if (thisLevelZombie.Zombie == thisZombieGO)
            {
				return thisLevelZombie;
            }
        }
		return null; //we've failed here
    }

	//This function is called whenever a zombie is repositioned and will also check to see how many we should have in our level, either turning the zombie off, or adding more
	public bool CheckZombieCount(GameObject checkingZombie)
    {
		LevelZombie thisLevelZombie = GetLevelZombieByObject(checkingZombie);
		//Of course if the above has failed then we've got an issue and should complain about it. A lot.
		if (thisLevelZombie == null)
        {
			Debug.LogError("Unable to get LevelZombie reference for " + checkingZombie.name);
        }
		int ActiveZombies = GetActiveZombieCount();
		int TargetZombies = GetTargetZombieCount();
		//Debug.Log("Active: " + ActiveZombies + " Target: " + TargetZombies);
		if (ActiveZombies == TargetZombies)
        {
			return true;
        } else if (ActiveZombies > TargetZombies)	//We've got too many zombies, turn this one off
        {

			thisLevelZombie.SetActive(false);
			return false;
        } else
        {
			//We don't have enough zombies in the level, we need to enable another one
			//Go through and find a zombie that's free :)
			foreach (LevelZombie thisZombie in SpawnedZombies)
            {
				if (thisZombie.ZombieState == LevelZombie.enZombieState.DISABLED)
                {
					StartCoroutine(DelayPlaceZombie(StartZombieDelays.GetRandom(), thisZombie));
					break;
                }
            }
        }
		return true;	//Have this zombie
    }

	public int GetTargetZombieCount()
    {
		//So here we need to find out how far along the run our player is, convert it to a t value, read the graphs, and do the evaluation before returning the number
		//Of course this mightn't be where our physical wall is, so there's that
		//PROBLEM: Distance needs to be calculated between walls
		float playerT = PC_FPSController.Instance.bestDistance / RunDistance;
		if (!bRunForward) {  //reverse our density evaluation for the reverse run
			playerT = 1f - playerT;
		}
		int TargetZombies = Mathf.RoundToInt(maxZombies * ZombieDensityCurve.Evaluate(playerT));
		return TargetZombies;
	}

	public void SpawnLevelZombies()
    {
		//Debug.Log("Doing Zombie Spawn Loop");
		//spawn all the zombies that'll be used in this level
		for (int i=0; i<maxZombies; i++)
        {
			GameObject newZombie = Instantiate(ZombiePrefab);
			newZombie.SetActive(false);
			LevelZombie newLevelZombie = new LevelZombie(); //Gawd. Look at this line. I'm almost as good at writing lyrics as Slim Shady
			newLevelZombie.Zombie = newZombie;
			newLevelZombie.SetActive(false); //turn this reference off to begin with.
			SpawnedZombies.Add(newLevelZombie);
        }

		//Spawn the zombies that'll be standing around
		for (int i=0; i<startingZombies; i++)
        {
			StartCoroutine(DelayPlaceZombie(StartZombieDelays.GetRandom(), SpawnedZombies[i]));
        }
    }

	IEnumerator DelayPlaceChaser(float spawnDelay, Vector3 dropPoint)
    {
		//Debug.Log("Adding Chaser Zombie");
		//We need to find a free zombie to use for this
		LevelZombie thisZombie = null;
		foreach(LevelZombie newZombie in SpawnedChasers)
        {
			if (newZombie.ZombieState == LevelZombie.enZombieState.DISABLED || newZombie.ZombieState == LevelZombie.enZombieState.NULL)
            {
				newZombie.ZombieState = LevelZombie.enZombieState.PENDING;
				thisZombie = newZombie;
				break;
            }
        }

		if (thisZombie != null)
		{
			//Prepare our zombie
			thisZombie.SetActive(false);
			thisZombie.ZombieState = LevelZombie.enZombieState.PENDING;

			//Do our delay
			yield return new WaitForSeconds(spawnDelay);

			//Set things in action
            thisZombie.SetActive(true);
			thisZombie.Zombie.GetComponent<EnemyBehavior>().RespawnEnemy(dropPoint);
		}
    }

	//Wait for a duration and then place a zombie on the road. This is used to stratify zombies as the player encounters them
	IEnumerator DelayPlaceZombie(float spawnDelay, LevelZombie thisZombie)
    {
		//Debug.Log("Placing Zombie");
		thisZombie.SetActive(false);								//Disable to begin with so that we've got a clean slate
		thisZombie.ZombieState = LevelZombie.enZombieState.PENDING;	//Set the zombie to state pending so that it'll be present in the level counts
		yield return new WaitForSeconds(spawnDelay);    //Because time is the same as distance
														//Have our system look for a good spawning point
		
		Vector3 dropPoint = Vector3.zero;
		bool bValidSpawn = false;
		while (!bValidSpawn)
		{
			dropPoint = GetEnemyDropPoint();

			if (dropPoint == Vector3.zero)  //We failed
			{
				//Debug.Log("redrop failed");
				yield return new WaitForSeconds(0.25f);  //Wait and then see about doing this again
			}
			else
			{
				//Place the enemy at this dropPoint
				thisZombie.SetActive(true);
				
				thisZombie.Zombie.GetComponent<EnemyBehavior>().RespawnEnemy(dropPoint);

				bValidSpawn = true;
			}
		}
	}

	public void TestForChaserZombie(GameObject thisZombie)
    {
		//So this is a great point to assess if we're going to turn this "zombie" into a chaser
		if (Random.value < zombieBecomeChaserChance && GetActiveZombieChaserCount() < maxChaserZombies)
		{
			//Debug.Log("Calling add Chaser Zombie");
			//We want to drop a chaser in where this zombie was before the zombie is moved
			StartCoroutine(DelayPlaceChaser(Random.Range(0.5f, 2f), thisZombie.transform.position));
		}
	}

	public void SpawnChaserZombies()
	{
		//Debug.Log("Doing Chaser Zombie Spawn Loop");
		//spawn all the zombies that'll be used in this level
		for (int i = 0; i < maxChaserZombies; i++)
		{
			GameObject newZombie = Instantiate(ChaserZombiePrefab);
			newZombie.SetActive(false);
			LevelZombie newLevelZombie = new LevelZombie(); //Gawd. Look at this line. I'm almost as good at writing lyrics as Slim Shady
			newLevelZombie.Zombie = newZombie;
			newLevelZombie.SetActive(false); //turn this reference off to begin with.
			SpawnedChasers.Add(newLevelZombie);
		}
	}

	void setTimescale(float newTimescale)
	{
		Time.timeScale = newTimescale;
	}

	//Public because we might need to call this remotely
	public void setPlayerPosition()
    {
		Debug.Log("Setting player start position");
		if (bRunForward)
        {
			PlayerCharacter.transform.position = startPointForward.transform.position;
			PlayerCharacter.transform.rotation = startPointForward.transform.rotation;
        } else
        {
			PlayerCharacter.transform.position = startPointReverse.transform.position;
			PlayerCharacter.transform.rotation = startPointReverse.transform.rotation;
		}
    }

	public void setPlayState(enLevelPlayState newPlaystate)
	{
		levelPlayState = newPlaystate;
		switch(levelPlayState)
        {
			case enLevelPlayState.NULL:
				break;
			case enLevelPlayState.START:
				setPlayerPosition();
				setTimescale(0f); //Pause our play
								  //We need to bring up our menu, and at some stage do something fancy,  but for the moment: menu
				UIMenuHandler.Instance.LoadMenuSceneAdditively("Game_LevelStart", null, null);	//Load the game start menu. Hardcoded for the moment
				break;
			case enLevelPlayState.STARTPLAY:    //Everything we need to do when we start the level running
												//We need to set our player position to start the run
				levelPlayState = enLevelPlayState.PLAYING; //Set our start correctly after our initial tick
				break;
			case enLevelPlayState.PLAYING:
				//Will need to clear any UI and other bits and bobbs
				setTimescale(1f);
				break;
			case enLevelPlayState.ENDED:
				//Pull up end menus, or do they slave us to that? I need to figure that one out
				UIMenuHandler.Instance.LoadMenuSceneAdditively("Game_LevelComplete", null, null);
				setTimescale(0f);
				//We're also going to have to sort out our collected items from our run, but that might be a "after we've a game loop happening" thing :)
				break;
			case enLevelPlayState.PAUSED:
				//There will be a pause menu! It'll be legendary!
				setTimescale(1f);
				break;
			default:
				break;
        }
	}

    #region UI Button Calls
	public void StartRun()
    {
		//Logically we'll have some sort of opening. But for the moment we're getting things onto the floor so lets just jump straight into the game
		setTimescale(1f);
		SpawnLevelZombies();
		SpawnChaserZombies();
		levelPlayState = enLevelPlayState.PLAYING;
    }
	#endregion

	void Update()
    {
		if (!zombiePropTexture)
        {
			Debug.LogError("No zombie propensity texture assigned to this level!!!");
        }
    }

    #region Level Generation Functions
    //This should be a universal function somewhere as it's used in two different places
    public string GetPRBPath()
	{
		/*
#if !UNITY_EDITOR
		//return "ux0:app/VHZR12345/Media/Resources/" + gameObject.scene.name + "_props.bin";
		return "ux0:app/VHZR12345/Media/StreamingAssets/Propensities/" + gameObject.scene.name + "_props.bin";
#endif*/
		//return Application.streamingAssetsPath + "/Propensities/" + gameObject.scene.name + "_props.bin";
		return Application.streamingAssetsPath + "/" + gameObject.scene.name + "_props.bin";
		//return Application.dataPath + "/StreamingAssets/Propensities/" + gameObject.scene.name + "_props.bin";
		//return Application.dataPath + "/Resources/" + gameObject.scene.name + "_props.bin";
	}

	public void ReadArrayFromResourcesBinaryFile()
    {
		//try
		//{
			TextAsset asset = Resources.Load(gameObject.scene.name + "_props") as TextAsset;

			Stream s = new MemoryStream(asset.bytes);
			BinaryReader reader = new BinaryReader(s);

			// Read the width and height from the file
			int distance = reader.ReadInt32();
			tableDistance = distance;
			int width = reader.ReadInt32();
			tableWidth = width;
			// Initialize the array using the width and height
			zombiePropTable = new System.Byte[distance, width];

			// Read the array elements
			for (int i = 0; i < distance; i++)
			{
				for (int j = 0; j < width; j++)
				{
					zombiePropTable[i, j] = reader.ReadByte();
				}
			}

			Debug.Log("Zombie Prop Array successfully read from binary file.");
			bHasPropensityFile = true;
		/*
		}
		catch (System.Exception e)
		{
			Debug.LogError("Error reading zombie prop array from binary file from resources: " + e.Message);
		}*/
	}

	// Function to read the array and dimensions from a binary file
	public void ReadArrayFromBinaryFile(string filePath)
	{
		try
		{
			using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
			{
				// Read the width and height from the file
				int distance = reader.ReadInt32();
				tableDistance = distance;
				int width = reader.ReadInt32();
				tableWidth = width;
				// Initialize the array using the width and height
				zombiePropTable = new System.Byte[distance, width];

				// Read the array elements
				for (int i = 0; i < distance; i++)
				{
					for (int j = 0; j < width; j++)
					{
						zombiePropTable[i, j] = reader.ReadByte();
					}
				}

				Debug.Log("Zombie Prop Array successfully read from binary file.");
				bHasPropensityFile = true;
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError("Error reading zombie prop array from binary file: " + e.Message);
		}
	}
    #endregion

    #region Enemy Placement Functions
	//PROBLEM: This function seems to have a bias to select positions on the right
    public Vector3 GetEnemyDropPoint()
	{

		if (!bHasPropensityFile)
        {
			Debug.Log("No propensity file loaded");
			/*
			string PBRPath = GetPRBPath();
			if (File.Exists(PBRPath))
			{
				ReadArrayFromBinaryFile(PBRPath);
				//ReadArrayFromResourcesBinaryFile();
				return Vector3.zero;
			} else
			{
				Debug.LogError("No PBR file found at: " + PBRPath);
			}*/
			ReadArrayFromResourcesBinaryFile();
		}
		//So for this we're going to need our player distance for our curve...
		int rowDistance = Mathf.RoundToInt(PC_FPSController.Instance.bestDistance); //This'll be in metres. We're pretty fine :)
		
		int cycles = 0;
		//Ok, so lets start 30m ahead of the player
		int spawnPoint = rowDistance + 30;
		if (!bRunForward)
        {
			spawnPoint = rowDistance - 30;
        }

		bool bFoundDropPoint = false;
		while (!bFoundDropPoint && cycles < 30)
        {
			spawnPoint = Mathf.Clamp(spawnPoint, 0, tableDistance-1);
			int distancePos = spawnPoint;

			//Create a list of points that we could start dropping in, and then randomize them for searching
			List<int> potentialPositions = new List<int>();
			for (int i = 0; i < tableWidth; i++)
			{
				//int widthPos = i;// Mathf.Clamp(Mathf.Abs(GetPointPattern(i)), 0, tableWidth);	//We're scrambling this array in a later step so don't need to be this sophisticated
				potentialPositions.Add(i);
			}

			// Randomly Order it by Guid..
			potentialPositions = potentialPositions.OrderBy(i => System.Guid.NewGuid()).ToList();

			for (int i=0; i < potentialPositions.Count; i++)
            {
				int widthPos = potentialPositions[i]; 
				
				float alphaValue = zombiePropTable[distancePos, widthPos];
				//For the moment let's just roll with 0-1 for the values
				if (alphaValue > 0.5f)	//We've got an acceptable point to drop our zombie down :)
                {
					//Debug.Log("Drop Position: (" + distancePos + ", " + widthPos + ")");
					bFoundDropPoint = true; //Yay!
											//We need to do a raycast to find the proper drop position for our Zombie
					RaycastHit hit;
					// Does the ray intersect any objects excluding the player layer
					Vector3 dropPoint = GetPixelPathPosition(spawnPoint, widthPos);
					if (Physics.Raycast(dropPoint, -Vector3.up, out hit, 50f, zombieDropMask))
					{
						dropPoint = hit.point;
					}
					return dropPoint;
                }
			}
			cycles++;
			spawnPoint++; //Add another row and continue checking
        }
		Debug.LogError("Failed to find a zombie drop point");
		return Vector3.zero;
	}

	public Vector3 GetPixelPathPosition(int distance, int widthPos)
    {
		Vector3 worldPoint = Vector3.zero;
		Vector3 pathDir = Vector3.zero;
		Vector3 pathRight = Vector3.zero;
		//if (bRunForward)
		//{
			worldPoint = pathCreatorReference.path.GetPointAtDistance(distance);
			pathDir = pathCreatorReference.path.GetDirectionAtDistance(distance);
			/*
		} else
        {
			worldPoint = pathCreator_Reverse.path.GetPointAtDistance(distance);
			pathDir = pathCreator_Reverse.path.GetDirectionAtDistance(distance);
		}*/
		pathRight = Quaternion.AngleAxis(90f, Vector3.up) * pathDir;
		return worldPoint + pathRight * widthPos;// + Vector3.up * 1.2f;
    }

	public int GetPointPattern(int index)
	{
		// Divide index by 2 to get the base number
		int baseNumber = index / 2;

		// If the index is even, return the base number (positive)
		if (index % 2 == 0)
		{
			return baseNumber;
		}
		// If the index is odd, return the negative of the base number
		else
		{
			return -baseNumber;
		}
	}

	public Vector3 GetRandomDropPoint(LayerMask redropMask) { 
		bool bFoundDropPoint = false;
		int cycles = 0;

		while (!bFoundDropPoint && cycles < 10)
		{
			cycles++;
			//PROBLEM: This'll need re-worked once we've got curves
			//float baseRandZ = PC_FPSController.Instance.gameObject.transform.position.z + Random.RandomRange(30f, 40f);
			//float baseRandX = Random.RandomRange(-10f, 10f);

			float RandomCurveDistance = PC_FPSController.Instance.bestDistance + Random.RandomRange(30f, 40f);
			Vector3 CurveDropPoint = Vector3.zero;
			Vector3 CurveDirection = Vector3.zero;

			if (bRunForward)
            {
				CurveDropPoint = pathCreatorReference.path.GetPointAtDistance(RandomCurveDistance);
				CurveDirection = pathCreatorReference.path.GetDirectionAtDistance(RandomCurveDistance);
			}

			CurveDropPoint += Quaternion.AngleAxis(90f, Vector3.up) * CurveDirection * Random.RandomRange(-10f, 10f);
			//Move our curve point up so we're elevated for the hit
			CurveDropPoint += Vector3.up * 30f;

			//Do a raycast down to see if we hit the ground and not a vehicle, then plonk our enemy here
			RaycastHit hit;
			// Does the ray intersect any objects excluding the player layer
			if (Physics.Raycast(CurveDropPoint, -Vector3.up, out hit, 50f, redropMask))
			{
				Debug.DrawRay(CurveDropPoint, -Vector3.up * hit.distance, Color.yellow);
				if (hit.collider.gameObject.tag == "Ground")
				{   //We're good
					Collider[] hitColliders = Physics.OverlapSphere(hit.point, 1f);
					bool bClearDropArea = true;
					foreach (var hitCollider in hitColliders)
					{
						if (hitCollider.gameObject.tag != "Ground")
                        {
							bClearDropArea = false;
                        }
					}
					if (bClearDropArea)
					{
						return hit.point;
						bFoundDropPoint = true;
					}
				}
			}
			else
			{
				//This shouldn't have happened...
				Debug.DrawRay(CurveDropPoint, -Vector3.up * 30, Color.red);
			}
		}
		Debug.Log("Failed drop point");
		return Vector3.zero; //This is a failed drop. This needs a handler
	}

	public void GeneratePointDropValue(Vector3 thisPoint)
    {
		//PROBLEM: We technically need two different tables for forward/reverse
		float pathDistance = pathCreatorReference.path.GetClosestDistanceAlongPath(thisPoint);
		Vector3 pathDirection = pathCreatorReference.path.GetDirectionAtDistance(pathDistance);
		Vector3 pathPoint = pathCreatorReference.path.GetPointAtDistance(pathDistance);

		Vector3 startPoint = new Vector3(thisPoint.x, pathPoint.y, thisPoint.z);

		float sphereRadius = 1f; //Start of our impact sphere for the given point
		for (float i = 1; i<30; i++)
        {

        }
    }
    #endregion

    #region Enemy Helper Functions
	public Vector3 GetEnemyStrafeDirection(GameObject thisEnemy, bool bIsChaser)
    {
		float intendedSpacing = 1f;
		//Basically I want to somehow have Enemies "Spread out" instead of going straight in for the player
		//Start by collapsing our positions along the "curve forward" axis
		//Then compare our position within the stack of the different enemies so that we can space ourselves out
		float DistanceSum = 0;
		if (!bIsChaser)
		{
			foreach (LevelZombie thisZombie in SpawnedZombies)
			{
				if (thisZombie.ZombieState == LevelZombie.enZombieState.ENABLED)
				{
					//We want a function where the closer we are to something the more we repel from them
					float flatDistance = Vector3.Dot(PC_FPSController.Instance.PlayerRight, thisEnemy.transform.position - thisZombie.Zombie.transform.position);
					flatDistance /= intendedSpacing;
					//At this stage we've still got a direction here, but we need to know how much based off of our sphere of infulence
					float flatPow = Mathf.Clamp01(Mathf.Abs(1f - flatDistance));    //Get the magnitude of our distance and clamp01
					DistanceSum += Mathf.Sign(flatDistance) * flatPow;
				}
			}
		} else
        {
			foreach (LevelZombie thisZombie in SpawnedChasers)
			{
				if (thisZombie.ZombieState == LevelZombie.enZombieState.ENABLED)
				{
					//Sadly our prior approach won't work for the chasers while in virtual...
					
					float flatDistance = Vector3.Dot(PC_FPSController.Instance.PlayerRight, thisEnemy.transform.position - thisZombie.Zombie.transform.position);
					flatDistance /= intendedSpacing;
					//At this stage we've still got a direction here, but we need to know how much based off of our sphere of infulence
					float flatPow = Mathf.Clamp01(Mathf.Abs(1f - flatDistance));    //Get the magnitude of our distance and clamp01
					DistanceSum += Mathf.Sign(flatDistance) * flatPow;
				}
			}
		}

		return PC_FPSController.Instance.PlayerRight * DistanceSum;	//This could be all sorts of wacky
    }

	public void TriggerDistraction(GameObject distraction, float radius, float effectiveness)
    {
		//Basically we need to go through our zombies and see if they're getting assigned to this as a target
		foreach (LevelZombie thisZombie in SpawnedZombies)
        {
			if (Vector3.SqrMagnitude(distraction.transform.position-thisZombie.Zombie.transform.position) < radius)
            {
				if (Random.value < effectiveness)	//Distract this enemy
                {
					Debug.Log("Enemy Distracted");
					thisZombie.Zombie.GetComponent<EnemyBehavior>().setTarget(distraction);
                }
            }
        }

		foreach (LevelZombie thisZombie in SpawnedChasers)
		{
			if (Vector3.SqrMagnitude(distraction.transform.position - thisZombie.Zombie.transform.position) < radius)
			{
				if (Random.value < effectiveness)   //Distract this enemy
				{
					Debug.Log("Chaser Distracted");
					thisZombie.Zombie.GetComponent<EnemyBehavior>().setTarget(distraction);
				}
			}
		}
	}
    #endregion
}
