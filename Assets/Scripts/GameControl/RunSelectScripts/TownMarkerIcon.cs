using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TownMarkerIcon : MonoBehaviour {
	//We'll need to display extra town information, but for the moment lets see about simply displaying basics

	public Image playerPresent;
	public string townName = "";

	public void setPlayerLocation(bool toThis)
    {
		playerPresent.gameObject.SetActive(toThis);
    }

	public void selectRunToHere()
    {
		//this needs to send a command through to select the correct run and setup accordingly
		GameController.Instance.DoRunTo(townName);
		//I've got a problem that everything is referenced and cross-referenced in the files. This needs cleaned up
    }

}
