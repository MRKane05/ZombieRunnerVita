using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterAudioBank : MonoBehaviour {
	public enum enFootfallSubstrate { NULL, PAVEMENT, METAL, GRASS }

	private static MasterAudioBank instance = null;
	public static MasterAudioBank Instance { get { return instance; } }


	public List<AudioClip> Footfall_Pavement = new List<AudioClip>();
	public List<AudioClip> Footfall_Metal = new List<AudioClip>();
	void Awake()
	{
		if (instance)
		{
			//Debug.Log("Somehow there's a duplicate GameController in the scene");
			//Debug.Log(gameObject.name);
			Destroy(gameObject);    //Remove ourselves from the scene
		}
		else
		{
			instance = this;
		}
	}
	public AudioClip GetRandomFootfallSound(enFootfallSubstrate footfallSubstrate)
    {
		switch (footfallSubstrate)
        {
			case enFootfallSubstrate.NULL:
				//Whoops!
				break;
			case enFootfallSubstrate.PAVEMENT:
				return (Footfall_Pavement[Random.Range(0, Footfall_Pavement.Count-1)]);
				break;
			case enFootfallSubstrate.METAL:
				return (Footfall_Metal[Random.Range(0, Footfall_Metal.Count-1)]);
				break;
			default:
				return (Footfall_Pavement[Random.Range(0, Footfall_Pavement.Count-1)]);
				break;

		}
		//For the moment everything is...PAVEMENT
		return (Footfall_Pavement[Random.Range(0, Footfall_Pavement.Count)]);
	}
}
