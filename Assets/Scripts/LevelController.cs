using PathCreation;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LevelController : MonoBehaviour {
	private static LevelController instance = null;
	public static LevelController Instance { get { return instance; } }

	public enum enLevelPlayState { NULL, START, PLAYING, ENDED, PAUSED }
	public enLevelPlayState levelPlayState = enLevelPlayState.START;

	public PathCreator pathCreator;
	public Texture2D zombiePropTexture; //This needs to be specific to each level
	public System.Byte[,] zombiePropTable;
	public int tableDistance = 0;   //What's the maximum distance on our table?
	public int tableWidth = 0;

	bool bHasPropensityFile = false;

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
		if (File.Exists(PBRPath))
		{
			//ReadArrayFromBinaryFile(PBRPath);
			ReadArrayFromResourcesBinaryFile();

		} else
		{
			Debug.LogError("No propensity file for this level! " + PBRPath);
		}

		setPlayState(enLevelPlayState.START); //Prepare our start functionality stuff
	}

	void setTimescale(float newTimescale)
	{
		Time.timeScale = newTimescale;
	}

	public void setPlayState(enLevelPlayState newPlaystate)
	{
		levelPlayState = newPlaystate;
		switch(levelPlayState)
        {
			case enLevelPlayState.NULL:
				break;
			case enLevelPlayState.START:
				setTimescale(0f); //Pause our play
								  //We need to bring up our menu, and at some stage do something fancy,  but for the moment: menu
				UIMenuHandler.Instance.LoadMenuSceneAdditively("Game_LevelStart", null, null);	//Load the game start menu. Hardcoded for the moment
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
#if !UNITY_EDITOR
		return "ux0:app/VHZR12345/Media/StreamingAssets/Propensities/" + gameObject.scene.name + "_props.bin";
#endif
		//return Application.streamingAssetsPath + "/Propensities/" + gameObject.scene.name + "_props.bin";
		return Application.streamingAssetsPath + "/Propensities/" + gameObject.scene.name + "_props.bin";
		return Application.dataPath + "/StreamingAssets/Propensities/" + gameObject.scene.name + "_props.bin";
	}

	public void ReadArrayFromResourcesBinaryFile()
    {
		try
		{
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
		}
		catch (System.Exception e)
		{
			Debug.LogError("Error reading zombie prop array from binary file from resources: " + e.Message);
		}
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

    public Vector3 GetEnemyDropPoint(LayerMask redropMask)
	{

		if (!bHasPropensityFile)
        {
			Debug.Log("No propensity file loaded");
			//Debug.Log(GetPRBPath());
			ReadArrayFromResourcesBinaryFile();
			return Vector3.zero;
        }
		//So for this we're going to need our player distance for our curve...
		int rowDistance = Mathf.RoundToInt(PC_FPSController.Instance.bestDistance); //This'll be in metres. We're pretty fine :)
		int fieldWidth = Mathf.FloorToInt(zombiePropTexture.height*0.5f);    //This is the width that we could drop something along

		int cycles = 0;
		//Ok, so lets start 30m ahead of the player
		int spawnPoint = rowDistance + 30;
		bool bFoundDropPoint = false;
		while (!bFoundDropPoint && cycles < 30)
        {
			spawnPoint = Mathf.Clamp(spawnPoint, 0, tableDistance-1);
			//int distancePos = zombiePropTexture.width - spawnPoint; //Because 0,0 is the lower left
			int distancePos = spawnPoint;	//PROBLEM: there's a chance that this'll go out of bounds and cause an access violation

			//Create a list of points that we could start dropping in, and then randomize them for searching
			List<int> potentialPositions = new List<int>();
			for (int i = 0; i < fieldWidth * 2; i++)
			{
				int widthPos = Mathf.Clamp(Mathf.Abs(GetPointPattern(i)), 0, tableWidth);
				potentialPositions.Add(widthPos);
			}

			// Randomly Order it by Guid..
			potentialPositions = potentialPositions.OrderBy(i => System.Guid.NewGuid()).ToList();

			for (int i=0; i < potentialPositions.Count; i++)
            {
				int widthPos = potentialPositions[i]; //This will go from 0, 1, -1, 2, -2...
													  //Color pixelColor = zombiePropTexture.GetPixel(distancePos, widthPos + fieldWidth);
													  //float alphaValue = pixelColor.a;	//Which is the value of our pixel point. We can handle this as is, or change it
				float alphaValue = zombiePropTable[distancePos, widthPos];
				//For the moment let's just roll with 0-1 for the values
				if (alphaValue>0.5f)	//We've got an acceptable point to drop our zombie down :)
                {
					bFoundDropPoint = true; //Yay!
											//We need to do a raycast to find the proper drop position for our Zombie
					RaycastHit hit;
					// Does the ray intersect any objects excluding the player layer
					Vector3 dropPoint = GetPixelPathPosition(spawnPoint, widthPos);
					if (Physics.Raycast(dropPoint, -Vector3.up, out hit, 50f, redropMask))
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
		Vector3 worldPoint = pathCreator.path.GetPointAtDistance(distance);
		Vector3 pathDir = pathCreator.path.GetDirectionAtDistance(distance);
		Vector3 pathRight = Quaternion.AngleAxis(90f, Vector3.up) * pathDir;

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
			Vector3 CurveDropPoint = pathCreator.path.GetPointAtDistance(RandomCurveDistance);
			Vector3 CurveDirection = pathCreator.path.GetDirectionAtDistance(RandomCurveDistance);
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
		float pathDistance = pathCreator.path.GetClosestDistanceAlongPath(thisPoint);
		Vector3 pathDirection = pathCreator.path.GetDirectionAtDistance(pathDistance);
		Vector3 pathPoint = pathCreator.path.GetPointAtDistance(pathDistance);

		Vector3 startPoint = new Vector3(thisPoint.x, pathPoint.y, thisPoint.z);

		float sphereRadius = 1f; //Start of our impact sphere for the given point
		for (float i = 1; i<30; i++)
        {

        }
    }
    #endregion
}
