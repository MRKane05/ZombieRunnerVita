using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A singleton for playing any audio of buttons being pressed/interacted with
public class UIInteractionSound : MonoBehaviour {
	private static UIInteractionSound instance = null;
	public static UIInteractionSound Instance { get { return instance; } }
	void Awake()
	{
		if (instance)
		{
			Debug.Log("Somehow there's a duplicate UIInteractionSound in the scene");
			Debug.Log(gameObject.name);
			Destroy(gameObject);	//Remove ourselves from the scene
		}

		instance = this;
	}

	public void PlayClick()
    {

    }

	public void PlaySelect()
    {

    }
}
