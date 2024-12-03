using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Loading shouldn't go here. Loading will be here for the time being
public class UI_RunSelectHandler : MonoBehaviour {

    public void LoadRunLevel(string thisLevelName)
    {
        //StartCoroutine(DoLevelLoad(thisLevelName));
        //RoadSetup_Terrain_HeadOriented_VehicleDrop
        SceneManager.LoadScene(thisLevelName, LoadSceneMode.Single);
    }
    /*
    IEnumerator DolevelLoad(string thisLevelName) { 
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(thisLevelName, LoadSceneMode.Single);
        yield return null;
    }
    */
}
