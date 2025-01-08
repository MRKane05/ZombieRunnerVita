using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//So on top of this being where we select runs from different paths we need
//A concept of our centers and what they need
//A concept of the location we're at and what it has
//A concept of which paths we can run from here
//Location exchange rates for being able to amplify trade
//Player funds in exchange for goods to "run"
//Concept of what "goods" are
//Handler for goods dropped on a run by accident

//From this the "paths" themselves need to be related to runs
//Runs need to have handlers for difficulty
//Runs need to support the possibility of roaving hordes that may set up residence
//Paths need to remember the nests they had, and the nature of the nests that have been cleared

[System.Serializable]
public class RunDetails
{
	public GameObject markerTarget;
	public string runLevelName = "";
	public bool bDoRunForward = true;
}

[System.Serializable]
public class MapLocation {
	public string locationName = "town";
	public string description = "A little center positioned by an abandoned railway";
	public GameObject referenceObject;	//I'm not 100% sure how this'll work actually, especially when it comes to saving
	//An array of run buttons that will be enabled while we're at this location
	public List<RunDetails> avaliableRuns = new List<RunDetails>();
	//We could do with storing a bit of information about this town as well
	public float townHealth = 33f;  //How is the town doing? Lower values will net higher returns for supplying this town

	public List<townPackage> avaliablePackages = new List<townPackage>();	//What packages do we have for other towns?
}

[System.Serializable]
public class PackageType
{
	public string type = "";
	public string description = "";
	public Range value;
	public Range weight;
	public Range healthImprovement;
	public float commonality = 1;	//Odds of this item showing up as a package
}

public class RunSelectMenu : MonoBehaviour {

	public List<MapLocation> mapLocations = new List<MapLocation>();
	public List<PackageType> packageTypes = new List<PackageType>();
	UI_LocationDetailsMenuHandler ourDetailsPanel;
	// Use this for initialization
	void Start () {
		SelectRunLocation();
		ourDetailsPanel = gameObject.GetComponentInChildren<UI_LocationDetailsMenuHandler>();

		//Do our necessary setup
		foreach(MapLocation thisLocation in mapLocations)
        {
			thisLocation.referenceObject.GetComponent<TownMarkerIcon>().ourSelectMenu = this;
			List<townPackage> newTownPackages = addTownPackages(thisLocation, (int)Random.RandomRange(3, 5));
			thisLocation.avaliablePackages = newTownPackages;
		}
	}

	public void SelectRunLocation()
	{
		//Kick off by finding out where we are from our game controller
		if (GameController.Instance && GameController.Instance.RunDetails.startLocation.Length > 3)
		{
			foreach (MapLocation mapLoc in mapLocations)
            {
				if (mapLoc.locationName == GameController.Instance.RunDetails.startLocation)
                {
					mapLoc.referenceObject.GetComponent<TownMarkerIcon>().setPlayerLocation(true);  //Set this ast our start location
																									//Now we need to set our starting point and our enabled run locations
					bool bPickedStart = false;
					foreach (RunDetails linkedTown in mapLoc.avaliableRuns)
                    {
						linkedTown.markerTarget.GetComponent<Button>().interactable = true;
						if (!bPickedStart)
                        {
							bPickedStart = true;
							UIHelpers.SetSelectedButton(linkedTown.markerTarget);
                        }
                    }
				}
			}
		}
		else
		{
			mapLocations[0].referenceObject.GetComponent<TownMarkerIcon>().setPlayerLocation(true);
		}
	}

	public void DoRunToTown(string startTown, string destinationTown)
	{
		GameController.Instance.RunDetails.endLocation = destinationTown; //Set this so that the system will know where to go
		//This needs to pass information through to our game controller to select the packages
		//Find the correct run
		//There'll be an OK button to kick things off in the next menu :)
		//GameController.Instance.DoRunTo(townName);
		foreach (MapLocation mapLoc in mapLocations)
		{
			if (mapLoc.locationName == startTown)	//For the packages we need to have the town we're IN
			{
				GameController.Instance.SetAvaliablePackages(mapLoc.avaliablePackages); //This is so that we can access this before the run
																						//Really we need to include the map we'll be running here too based off of from/to details (somehow!)
																						//Our panel should be on this gameobject (damn this is bad coding!)
				gameObject.GetComponentInChildren<PanelHandler>().LoadMenuScene("Game_RunPayloadSelectMenu");
				foreach (RunDetails thisAvaliable in mapLoc.avaliableRuns)
                {
					if (thisAvaliable.markerTarget.name.Contains(destinationTown)) //This is where we're going to. Lets grab our map and run direction
                    {
						GameController.Instance.RunDetails.targetMap = thisAvaliable.runLevelName;
						GameController.Instance.RunDetails.bRunMapForward = thisAvaliable.bDoRunForward;
                    }
                }
			}
		}
	}

	public void updateDetailsPanel(string thisTown)
    {
		foreach (MapLocation thisLocation in mapLocations)
		{
			if (thisLocation.locationName == thisTown) {
				ourDetailsPanel.setNewDetails(thisTown, thisLocation.description, thisLocation.townHealth, thisLocation.avaliablePackages.Count);
			}
		}
	}

	public List<townPackage> addTownPackages(MapLocation thisLocation, int thisNumber)
    {
		List<townPackage> newPackages = new List<townPackage>();
		for(int i=0; i<thisNumber; i++)
        {
			//We need the possible destinations here to pass through as options
			int randomTown = (int)Random.Range(0, thisLocation.avaliableRuns.Count);
			//Debug.Log("Random Town: " + randomTown);
			string targetTown = thisLocation.avaliableRuns[randomTown].markerTarget.GetComponent<TownMarkerIcon>().townName;
			newPackages.Add(createTownPackage(targetTown));
        }
		return newPackages;
	}

	townPackage createTownPackage(string targetTown)	//PROBLEM: This will need expansion, and concept of identity
    {
		//So basically we'd like to get a package randomly made from our list packageTypes
		int randomPackage = (int)Random.Range(0, packageTypes.Count);
		PackageType newPackage = packageTypes[randomPackage];

		townPackage newTownPackage = new townPackage();
		newTownPackage.townName = targetTown;
		newTownPackage.basicDescription = newPackage.description;
		newTownPackage.packageType = newPackage.type;
		newTownPackage.value = Mathf.CeilToInt(newPackage.value.GetRandom());
		newTownPackage.weight = Mathf.CeilToInt(newPackage.weight.GetRandom());

		return newTownPackage;
	}
}
