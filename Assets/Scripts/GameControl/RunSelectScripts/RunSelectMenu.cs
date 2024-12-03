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
	public GameObject referenceObject;	//I'm not 100% sure how this'll work actually, especially when it comes to saving
	//An array of run buttons that will be enabled while we're at this location
	public List<GameObject> avaliableRuns = new List<GameObject>();
	//An array of towns that can be reached from this location

}

public class RunSelectMenu : MonoBehaviour {

	public List<MapLocation> mapLocations = new List<MapLocation>();

	// Use this for initialization
	void Start () {
		SelectRunLocation();
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
}
