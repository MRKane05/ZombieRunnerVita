using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class townPackage
{
	public string townName = "";
	public string basicDescription = "";
	public float healthImprovement = 5;
	public float value = 10;
	public float weight = 2;
	public float deliveryUrgence = 0.5f; //0-1 scale on how urgent this package is. This'll relate to the value of the package and the odds that it won't be there when the player gets back
	//This needs some sense of delivery urgence
}

[System.Serializable]
public class runDetails
{
	public enum enRunState { NULL, HOME, RUNNING, ARRIVED }
	public enRunState runState = enRunState.HOME;
	public string endLocation = "";
	public string startLocation = "";
	public List<townPackage> carriedPackages = new List<townPackage>();
}

//Because we need some class that can act as a unifier for many of our systems
//It's either that or save everything to a file all the time, so I'm choosing this
public class GameController : MonoBehaviour {
	private static GameController instance = null;
	public static GameController Instance { get { return instance; } }

	public runDetails RunDetails = new runDetails();

	public List<townPackage> avaliablePackages = new List<townPackage>();
	void Awake()
	{
		if (instance)
		{
			Debug.Log("Somehow there's a duplicate GameController in the scene");
			Debug.Log(gameObject.name);
			Destroy(gameObject);    //Remove ourselves from the scene
		}
		else
		{
			instance = this;
		}
	}

	public void DoRunTo(string chosenLocation)
    {
		//So at this point we've got to update our from/to for our run details
		RunDetails.endLocation = chosenLocation;
		RunDetails.startLocation = "Musselbrough"; //Quick hardcoding to test
		RunDetails.runState = runDetails.enRunState.RUNNING;
		//search reference tables to find the correct run that we need to do
		LoadRunLevel("RoadSetup_Terrain_HeadOriented_VehicleDrop");

	}

	public void LoadRunLevel(string thisLevelName)
	{
		//StartCoroutine(DoLevelLoad(thisLevelName));
		//RoadSetup_Terrain_HeadOriented_VehicleDrop
		SceneManager.LoadScene(thisLevelName, LoadSceneMode.Single);
	}

	public void RunSuccessful()
    {
		//StrongholdScene
		//We need to handle our from/to for our menu stuff
		RunDetails.startLocation = RunDetails.endLocation;
		RunDetails.runState = runDetails.enRunState.ARRIVED;
		//Now that that's updated lets move to loading our scene
		SceneManager.LoadScene("StrongholdScene", LoadSceneMode.Single);
		//We need a callback that tells this scene that we've just arrived after doing a run. Or something to that extent.
	}

	public void SetAvaliablePackages(List<townPackage> newPackages)
    {
		avaliablePackages = newPackages;
	}
}
