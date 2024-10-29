using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class RoadPointValueScan : MonoBehaviour {
	public PathCreator pathCreator;

	void Update () {
		//GeneratePointDropValue(gameObject.transform.position);
		//Debug.Log(GetScenePath());
		//Debug.Log(GetPRBPath());
		DoNumberCount();
	}

	void DoNumberCount()
    {
		for (int i=0; i<16; i++)
        {
			Debug.Log(GetPointPattern(i));
        }
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

	public string GetPRBPath()
	{
		return Application.streamingAssetsPath + "/" + gameObject.scene.name;
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

	public float GeneratePointDropValue(Vector3 thisPoint)
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
