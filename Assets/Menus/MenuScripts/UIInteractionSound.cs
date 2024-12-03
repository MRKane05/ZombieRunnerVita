using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A singleton for playing any audio of buttons being pressed/interacted with
public class UIInteractionSound : MonoBehaviour {
	private static UIInteractionSound instance = null;
	public static UIInteractionSound Instance { get { return instance; } }

	public AudioSource ourAudioSource;
	public AudioClip SelectionChangeSound;
	public AudioClip ClickButtonSound;
	void Awake()
	{
		if (instance)
		{
			Debug.Log("Somehow there's a duplicate UIInteractionSound in the scene");
			Debug.Log(gameObject.name);
			Destroy(gameObject);    //Remove ourselves from the scene
		}
		else
		{
			instance = this;
			ourAudioSource = gameObject.GetComponent<AudioSource>();
		}
	}

	public void PlayClick()
    {

    }

	public void PlaySelect()
    {
		if (ourAudioSource)
		{
			ourAudioSource.PlayOneShot(SelectionChangeSound);
		}
	}
}
