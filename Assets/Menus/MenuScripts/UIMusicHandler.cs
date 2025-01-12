﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMusicHandler : MonoBehaviour {
	private static UIMusicHandler instance = null;
	public static UIMusicHandler Instance { get { return instance; } }
	public AudioSource ourAudioSource;

	void Awake()
	{
		if (instance)
		{
			Debug.Log("Somehow there's a duplicate UIMusicHandler in the scene");
			Debug.Log(gameObject.name);
			Destroy(gameObject);    //Remove ourselves from the scene
		}
		else
		{
			instance = this;
			if (!ourAudioSource)
			{
				ourAudioSource = gameObject.GetComponent<AudioSource>();
			}
		}
	}

	public AudioClip menuTheme;
	public AudioClip runTheme;

	public void SetMusicTrack(bool bMenuMusic)
    {
		if (!bMenuMusic)
        {
			ourAudioSource.clip = runTheme;
			ourAudioSource.Play();
        } else
        {
			ourAudioSource.clip = menuTheme;
			ourAudioSource.Play();
        }
	}
}


