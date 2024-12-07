using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_StrongholdMenuHandler : MonoBehaviour {
	//None of this is working.
	/*
	// Use this for initialization
	IEnumerator Start () {
		yield return null;
		//There's a possibility we're opening back up after a run, so we need to check our game controller here and if we should
		//be changing our menu
		while (!GameController.Instance)
        {
			yield return null;
        }
		yield return null;
		if (GameController.Instance.RunDetails.runState == runDetails.enRunState.RUNNING)
        {
			//We've arrived at our stronghold, we need to change this to suit
			OpenRunReturnMenu();
        }
	}

	void OpenRunReturnMenu()
    {
		//Our panel handler should be on the same game object
		gameObject.GetComponent<PanelHandler>().LoadMenuScene("Game_RunReturnMenu");

		GameController.Instance.RunDetails.runState = runDetails.enRunState.ARRIVED;
	}
	*/
}
