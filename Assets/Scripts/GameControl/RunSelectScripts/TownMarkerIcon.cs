using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TownMarkerIcon : MonoBehaviour, ISelectHandler
{
	//We'll need to display extra town information, but for the moment lets see about simply displaying basics
	public RunSelectMenu ourSelectMenu;	//This will be set by the menu itself when it goes through the references
	public Image playerPresent;
	public string townName = "";

	public void setPlayerLocation(bool toThis)
    {
		playerPresent.gameObject.SetActive(toThis);
    }

	public void selectRunToHere()
    {
		//this needs to send a command through to select the correct run and setup accordingly
		//GameController.Instance.DoRunTo(townName);
		//I've got a problem that everything is referenced and cross-referenced in the files. This needs cleaned up
		ourSelectMenu.DoRunToTown(GameController.Instance.RunDetails.startLocation, townName);
    }

	public void OnSelect(BaseEventData eventData)
	{
		//Notify our details panel that this section is selected. Because everything is distributed this may be a little messy
		//There's a reference on the RunSelectMenu, assuming I can get a reference to that...
		if (ourSelectMenu)
		{
			ourSelectMenu.updateDetailsPanel(townName);
		}
	}

}
