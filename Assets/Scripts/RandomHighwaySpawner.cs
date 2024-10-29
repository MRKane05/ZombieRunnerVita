using PathCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ClutterPropPosition
{
	public Vector3 pos = Vector3.zero;
	public Vector3 rot = Vector3.zero;
	public string objectName = "";	//Don't feel this will be useful
	public GameObject clutterObject;

	public void toPosition()
    {
		pos = clutterObject.transform.position;
		rot = clutterObject.transform.eulerAngles;
    }
	public string toString()
    {
		string outString = "";
		outString += "pos:" + pos.ToString() + "\n";
		outString += "rot:" + rot.ToString() + "\n";
		return outString;
    }
}


//Just a script to drop thins down along a line and try to flesh out levels to see how they feel
[ExecuteInEditMode]
public class RandomHighwaySpawner : MonoBehaviour {
	public List<ClutterPropPosition> clutterProps = new List<ClutterPropPosition>();
	public bool bDoSpawn = false;	//Spawns all of our props
	public bool bWritePositions = false;	//Writes positions to a txt file after doing a physics drop. This will also remove the rigidbodies
	public bool bSetPositions = false;  //Reads in the "dropped" positions after hitting play
	public bool bSetupSpawnTable = false;
	public bool bClearChildren = false;
	public List<GameObject> PrefabObjects = new List<GameObject>();
	public float SpawnLength = 500;
	public Range SpawnWidth = new Range(3, 7);
	public Range DropHeight = new Range(3, 15); //What height will the props be spawned in at before doing the "drop"
	public PathCreator pathCreator; //The path that we'll spawn clutter along
	public float roadWidth = 8;

	//We need to make a table of points with information so that we can properly spawn Zombies. This will be a 1x1 table of ints
	public byte[,] ZombieSpawnValues;

	// Update is called once per frame
	void Update () {
		if (bDoSpawn) {
			bDoSpawn = false;
			DropPrefabs();
		}
		if (bClearChildren) {
			bClearChildren = false;

			StartCoroutine(DoClearChildren());
		}
		if (bWritePositions)
        {
			bWritePositions = false;
			ExportCurrentPositions();
        }
		if (bSetPositions)
        {
			bSetPositions = false;
			ImportAndSetPositions();
        }
		if (bSetupSpawnTable)
        {
			bSetupSpawnTable = false;
			SetupSpawnTable();
        }
	}

	void ExportCurrentPositions()
    {
		string PosRotString = "";
		for (int i=0; i<clutterProps.Count; i++)
        {
			PosRotString += i.ToString() + "\n";
			clutterProps[i].toPosition();
			PosRotString += clutterProps[i].toString();
        }
		
		string path = Application.streamingAssetsPath + "/ClutterPositions.txt";
		//Create File if it doesn't exist
		//if (!File.Exists(path))
		//{
		File.WriteAllText(path, PosRotString);
		//}
	}

	Vector3 Vec3FromString(string vstring)
	{
		//Remove our brackets
		vstring = vstring.Replace('(', ' ');
		vstring = vstring.Replace(')', ' ');
		//Debug.Log(vstring);
		string[] values = vstring.Split(',');
		float vecX = float.Parse(values[0].Trim());
		float vecY = float.Parse(values[1].Trim());
		float vecZ = float.Parse(values[2].Trim());
        //Debug.Log("X: " + vecX + " Y: " + vecY + " Z: " + vecZ);
        return new Vector3(vecX, vecY, vecZ);
	}

	void ImportAndSetPositions()
    {
		string filePath = Application.streamingAssetsPath + "/ClutterPositions.txt";


		// Open the file and read each line
		using (StreamReader reader = new StreamReader(filePath))
		{
			string line;
			int currentIndex = -1;
			Vector3 currentPos = Vector3.zero;
			Vector3 currentRot = Vector3.zero;
			while ((line = reader.ReadLine()) != null)
			{
				// Process the data on each line
				if (!line.Contains("pos") && !line.Contains("rot")) { //this is our index. We'll want to add our current entry and then set it
					if (currentIndex >=0)
                    {
						clutterProps[currentIndex].pos = currentPos;
						clutterProps[currentIndex].rot = currentRot;
						//Debug.Log("Set prop position: " + currentIndex);
						clutterProps[currentIndex].clutterObject.transform.position = currentPos;
						clutterProps[currentIndex].clutterObject.transform.eulerAngles = currentRot;
						DestroyImmediate(clutterProps[currentIndex].clutterObject.GetComponent<Rigidbody>());	//Remove our rigidbody
					}
					currentIndex = int.Parse(line);
				} else if (line.Contains("pos")) {
					//We want to trim out the first part of our line and use the second
					string[] data = line.Split(':');
					currentPos = Vec3FromString(data[1]);

				}
				else if (line.Contains("rot"))
				{
					//We want to trim out the first part of our line and use the second
					string[] data = line.Split(':');
					currentRot = Vec3FromString(data[1]);
				}
			}
		}
	}

    IEnumerator DoClearChildren()
    {
		clutterProps.Clear();
		clutterProps = new List<ClutterPropPosition>();
		while (gameObject.transform.childCount > 0)
        {
			foreach (Transform child in transform)
			{
				DestroyImmediate(child.gameObject);
			}
		}
		yield return null;
    }

	public float meanDistance = 10f;
	public float deviation = 3f;

	float RandomGaussian(float mean, float deviation)
	{
		// Generate a Gaussian random value using the Box-Muller transform
		float u1 = 1.0f - UnityEngine.Random.Range(0f, 1f); // uniform(0,1] random numbers
		float u2 = 1.0f - UnityEngine.Random.Range(0f, 1f);
		float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
		return mean + deviation * randStdNormal;
	}

	public float laneAngle = 30;	//What's the random limit on the angle of the cars in relation to the lane?
	void DropPrefabs() {
		//So logically our highway has two sides, and goes for a set length. To kick off with we need to be spawning along these
		float Lane_LeftCrawl = 0;
		float Lane_RightCrawl = 0;

		//I might simplify everything to just dump junk everywhere
		while (Lane_LeftCrawl < SpawnLength) {
			//Lane_LeftCrawl += UnityEngine.Random.Range(SpawnFrequency.x, SpawnFrequency.y);
			Lane_LeftCrawl += RandomGaussian(meanDistance, deviation);
			//A new spawn position X
			float SpawnX = -SpawnWidth.GetRandom();

			GameObject newObject = Instantiate(PrefabObjects[UnityEngine.Random.Range(0, PrefabObjects.Count)], transform);

			Vector3 linePos = pathCreator.path.GetPointAtDistance(Lane_LeftCrawl);
			Vector3 forward = pathCreator.path.GetDirectionAtDistance(Lane_LeftCrawl);
			float vectorAngle = Mathf.Atan2(forward.z, forward.x);

			newObject.transform.localPosition = linePos + Quaternion.AngleAxis(90f, Vector3.up) * forward * SpawnX + Vector3.up * DropHeight.GetRandom();//new Vector3(SpawnX, 0, Lane_LeftCrawl);

			//newObject.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(-60, 60), 45 + UnityEngine.Random.value * 90, UnityEngine.Random.Range(-60, 60));
			newObject.transform.eulerAngles = new Vector3(0, 270f - vectorAngle * 180f/Mathf.PI, 0);
			newObject.transform.rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(-laneAngle, laneAngle), newObject.transform.up);
			//Do a rotation so that we could end up on our side/back (maybe)
			if (UnityEngine.Random.value > 0.75f)
			{
				newObject.transform.rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(-120, 120), newObject.transform.forward);
			} else
            {
				newObject.transform.rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(-90, 90), newObject.transform.forward);
			}

			Rigidbody newRigid = newObject.AddComponent<Rigidbody>();
			newRigid.mass = 10f;

			ClutterPropPosition newClutterProp = new ClutterPropPosition();
			newClutterProp.clutterObject = newObject;
			clutterProps.Add(newClutterProp);
		}

		while (Lane_RightCrawl < SpawnLength)
		{
			//Lane_RightCrawl += UnityEngine.Random.Range(SpawnFrequency.x, SpawnFrequency.y);
			Lane_RightCrawl += RandomGaussian(meanDistance, deviation);
			//A new spawn position X
			float SpawnX = SpawnWidth.GetRandom();

			GameObject newObject = Instantiate(PrefabObjects[UnityEngine.Random.Range(0, PrefabObjects.Count)], transform);
			Vector3 linePos = pathCreator.path.GetPointAtDistance(Lane_RightCrawl);
			Vector3 forward = pathCreator.path.GetDirectionAtDistance(Lane_RightCrawl);
			float vectorAngle = Mathf.Atan2(forward.z, forward.x);

			newObject.transform.localPosition = linePos + Quaternion.AngleAxis(90f, Vector3.up) * forward * SpawnX + Vector3.up * DropHeight.GetRandom();//new Vector3(SpawnX, 0, Lane_LeftCrawl);

			//newObject.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(-60, 60), 45 + UnityEngine.Random.value * 90, UnityEngine.Random.Range(-60, 60));
			newObject.transform.eulerAngles = new Vector3(0, 90f - vectorAngle * 180f / Mathf.PI, 0);
			newObject.transform.rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(-laneAngle, laneAngle), newObject.transform.up);
			//Do a rotation so that we could end up on our side/back (maybe)
			if (UnityEngine.Random.value > 0.75f)
			{
				newObject.transform.rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(-120, 120), newObject.transform.forward);
			}
			else
			{
				newObject.transform.rotation *= Quaternion.AngleAxis(UnityEngine.Random.Range(-90, 90), newObject.transform.forward);
			}

			Rigidbody newRigid = newObject.AddComponent<Rigidbody>();
			newRigid.mass = 10f;

			ClutterPropPosition newClutterProp = new ClutterPropPosition();
			newClutterProp.clutterObject = newObject;
			clutterProps.Add(newClutterProp);
		}

	}

	void SetupSpawnTable()
    {
		//I guess we start by assuming that we can do a spawn table in metres :)
		int sDistance = Mathf.RoundToInt(SpawnLength);  //Just a simple thing for the moment
		//sDistance = 10;
		int sWidth = Mathf.RoundToInt(roadWidth * 2);
		ZombieSpawnValues = new byte[sDistance, sWidth];	//Distances starts at zero, width starts from the left (negative values)
		for (int sD = 0; sD < sDistance; sD ++)
        {
			Vector3 pathPoint = pathCreator.path.GetPointAtDistance(sD);
			Vector3 pathDirection = pathCreator.path.GetDirectionAtDistance(sD);
			Vector3 pathRight = Quaternion.AngleAxis(90f, Vector3.up) * pathDirection;
			for (int sW = 0; sW < sWidth; sW ++)
            {
				Vector3 targetPoint = pathPoint + pathRight * (sW - sWidth * 0.5f);  //So this is where out point should be
				ZombieSpawnValues[sD, sW] = (byte)GeneratePointDropValue(targetPoint);

			}
        }

		string propensityPath = GetPRBPath();   //A probability file!
		WriteArrayToBinaryFile(ZombieSpawnValues, sDistance, sWidth, propensityPath);
		//SaveTextureToFile(ConvertIntArrayToTexture(ZombieSpawnValues), propensityPath);
	}

	public Texture2D ConvertIntArrayToTexture(int[,] array)
	{
		int width = array.GetLength(0);
		int height = array.GetLength(1);

		// Create a new Texture2D with the size of the array
		Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);

		// Loop through the array and set each pixel's color
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				// If value is 1, set pixel to white; otherwise, set to black
				Color color = Color.Lerp(Color.black, Color.white, (float)array[x, y] / (30 + 30)); //Maximum sphere + maximum distance
				texture.SetPixel(x, y, color);
			}
		}

		// Apply the pixel changes to the texture
		texture.Apply();

		return texture;
	}

	// This method saves the texture as a PNG file
	public void SaveTextureToFile(Texture2D texture, string filePath)
	{
		byte[] bytes = texture.EncodeToPNG();
		File.WriteAllBytes(filePath, bytes);
		Debug.Log("Texture saved to: " + filePath);
	}


	//This should be a universal function somewhere as it's used in two different places
	public string GetPRBPath()
	{
		return Application.streamingAssetsPath + "/Propensities/" + gameObject.scene.name + "_props.bin";
	}

	public string GetScenePath()
	{
		// Get the current scene where the object is located
		Scene currentScene = gameObject.scene;

		// Get the path of the scene
		string scenePath = currentScene.path;

		Debug.Log("Scene Path: " + scenePath);

		return scenePath;
	}

	public void WriteArrayToBinaryFile(byte[,] array, int distance, int width, string filePath)
	{
		UnityEngine.Debug.Log("writing array file: " + filePath);
		try
		{
			using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
			{
				// Write the width and height into the file
				writer.Write(distance);
				writer.Write(width);

				// Write the array elements
				for (int i = 0; i < distance; i++)
				{
					for (int j = 0; j < width; j++)
					{
						writer.Write(array[i, j]);
					}
				}

				Debug.Log("Array successfully written to binary file.");
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Error writing array to binary file: " + e.Message);
		}
	}

	// Function to read the array and dimensions from a binary file
	public int[,] ReadArrayFromBinaryFile(string filePath)
	{
		try
		{
			using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
			{
				// Read the width and height from the file
				int width = reader.ReadInt32();
				int height = reader.ReadInt32();

				// Initialize the array using the width and height
				int[,] array = new int[width, height];

				// Read the array elements
				for (int i = 0; i < width; i++)
				{
					for (int j = 0; j < height; j++)
					{
						array[i, j] = reader.ReadByte();
					}
				}

				Debug.Log("Array successfully read from binary file.");
				return array;
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Error reading array from binary file: " + e.Message);
			return null;
		}
	}

	public int GeneratePointDropValue(Vector3 thisPoint)
	{
		float pathDistance = pathCreator.path.GetClosestDistanceAlongPath(thisPoint);
		//Vector3 pathDirection = Vector3.Normalize(pathCreator.path.GetPointAtDistance(pathDistance) - pathCreator.path.GetPointAtDistance(pathDistance - 1f));
		Vector3 pathDirection = pathCreator.path.GetDirectionAtDistance(pathDistance);
		Vector3 backDirection = Quaternion.AngleAxis(180f, Vector3.up) * pathDirection;
		Vector3 pathPoint = pathCreator.path.GetPointAtDistance(pathDistance);

		Vector3 startPoint = new Vector3(thisPoint.x, pathPoint.y + 1f, thisPoint.z);

		float sphereRadius = 0f; //Start of our impact sphere for the given point
		bool bHitPoint = false;

		while (!bHitPoint && sphereRadius < 30f)
		{

			Collider[] hitObjects = Physics.OverlapSphere(startPoint, sphereRadius);
			foreach (Collider hit in hitObjects)
			{
				if (hit.gameObject.tag == "Clutter")
				{
					bHitPoint = true;
				}
			}
			if (!bHitPoint) { sphereRadius++; }//size this up by one
		}
		Debug.DrawLine(startPoint, startPoint + Vector3.up * sphereRadius, Color.yellow, 5f);
		//Cool, so unless this is 0 (we're inside something) we can do a spherecast forward to see what what value is
		float clearDistance = 0;
		if (sphereRadius > 0)
		{
			RaycastHit Hit;
			Ray sphereRay = new Ray(startPoint, backDirection);
			RaycastHit[] sphereHits = Physics.SphereCastAll(sphereRay, 0.8f, 30f);// .SphereCast(startPoint, 0.8f, backDirection, out Hit))
			clearDistance = 30f;
			foreach (RaycastHit thisHit in sphereHits)
			{
				if (thisHit.collider.gameObject.tag == "Clutter")
				{
					clearDistance = Mathf.Min(clearDistance, Vector3.Distance(startPoint, thisHit.point));
				}
			}
		}
		if (clearDistance > 0f)
		{
			Debug.DrawLine(startPoint, startPoint + backDirection * clearDistance, Color.red, 5f);
		}
		int pointValue = Mathf.RoundToInt(sphereRadius + clearDistance);

		return pointValue;
		//Debug.Log("s: " + sphereRadius + " c: " + clearDistance + " p: " + pointValue);
		//Debug.Log(backDirection * clearDistance);
	}
}
