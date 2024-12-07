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
public class MapLocation {
	public string locationName = "town";
	public string description = "A little center positioned by an abandoned railway";
	public GameObject referenceObject;	//I'm not 100% sure how this'll work actually, especially when it comes to saving
	//An array of run buttons that will be enabled while we're at this location
	public List<GameObject> avaliableRuns = new List<GameObject>();
	//We could do with storing a bit of information about this town as well
	public float townHealth = 33f;  //How is the town doing? Lower values will net higher returns for supplying this town

	public List<townPackage> avaliablePackages = new List<townPackage>();	//What packages do we have for other towns?
}

public class RunSelectMenu : MonoBehaviour {

	public List<MapLocation> mapLocations = new List<MapLocation>();
	UI_LocationDetailsMenuHandler ourDetailsPanel;
	// Use this for initialization
	void Start () {
		SelectRunLocation();
		ourDetailsPanel = gameObject.GetComponentInChildren<UI_LocationDetailsMenuHandler>();

		//Do our necessary setup
		foreach(MapLocation thisLocation in mapLocations)
        {
			thisLocation.referenceObject.GetComponent<TownMarkerIcon>().ourSelectMenu = this;
			List<townPackage> newTownPackages = addTownPackages((int)Random.RandomRange(3, 6));
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
					foreach (GameObject linkedTown in mapLoc.avaliableRuns)
                    {
						linkedTown.GetComponent<Button>().interactable = true;
						if (!bPickedStart)
                        {
							bPickedStart = true;
							UIHelpers.SetSelectedButton(linkedTown);
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

	public void DoRunToTown(string townName)
	{
		//This needs to pass information through to our game controller to select the packages
		//Find the correct run
		//There'll be an OK button to kick things off in the next menu :)
		//GameController.Instance.DoRunTo(townName);
		foreach (MapLocation mapLoc in mapLocations)
		{
			if (mapLoc.locationName == townName)
			{
				GameController.Instance.SetAvaliablePackages(mapLoc.avaliablePackages); //This is so that we can access this before the run
																						//Really we need to include the map we'll be running here too based off of from/to details (somehow!)
																						//Our panel should be on this gameobject (damn this is bad coding!)
				gameObject.GetComponentInChildren<PanelHandler>().LoadMenuScene("Game_RunPayloadSelectMenu");
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

	public List<townPackage> addTownPackages(int thisNumber)
    {
		List<townPackage> newPackages = new List<townPackage>();
		for(int i=0; i<thisNumber; i++)
        {
			newPackages.Add(createTownPackage());
        }
		return newPackages;
	}

	townPackage createTownPackage()	//PROBLEM: This will need expansion, and concept of identity
    {
		townPackage newTownPackage = new townPackage();
		newTownPackage.basicDescription = "Package to bring to a town";
		newTownPackage.healthImprovement = 5;
		newTownPackage.value = Random.RandomRange(10, 25);
		newTownPackage.weight = Random.RandomRange(0.5f, 3);
		newTownPackage.townName = "Any Town";
		return newTownPackage;
    }
}
