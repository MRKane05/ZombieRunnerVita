﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class loadedScene
{
    public Scene scene;
    public PanelHandler callingPanel;
    public GameObject callingButton;
    public bool IsValid()
    {
        if (scene != null)
        {
            return scene.IsValid();
        }
        return false;
    }
    public loadedScene(Scene newScene, PanelHandler newCallingPanel, GameObject newCallingButton)
    {
        scene = newScene;
        callingPanel = newCallingPanel;
        callingButton = newCallingButton;
    }
}

//This script handles menu elements, loading menus as scenes and the chatter/functionality between them
public class UIMenuHandler : MonoBehaviour
{
    public List<loadedScene> loadedScenes = new List<loadedScene>();
    private Scene lastLoadedScene;
    public PanelHandler baseMenu;    //This will be our fallback reference menu
    private static UIMenuHandler instance = null;
    public static UIMenuHandler Instance { get { return instance; } }

    void Awake()
    {
        if (instance)
        {
            Debug.Log("Somehow there's a duplicate UIMenuHandler in the scene");
            Debug.Log(gameObject.name);
            Destroy(gameObject);    //Remove ourselves from the scene
        }

        instance = this;
    }

    //This is called by a menu that's marked as "Base" to set itself as the menu that the entire system can return back to
    public void AssertMenuAsBase(PanelHandler thisMenu)
    {
        Debug.Log("Menu asserted as base: " + thisMenu.name);
        if (baseMenu) //we should do something about this...
        {

        }
        baseMenu = thisMenu;
    }

    public void OpenBaseScene(string targetSceneName, string targetButtonName)
    {
        //Kick off with the easy way first
        if (baseMenu)
        {
            baseMenu.gameObject.SetActive(true); //Just turn this back on again
            baseMenu.DoEnable(targetButtonName);
        }
    }

    //Function to unload menus that have been loaded into the scene. This'll be used with the menu close function
    public void UnloadMenu(GameObject targetMenu)
    {
        Scene menuScene = targetMenu.scene;
        List<int> removeEntries = new List<int>();
        if (menuScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(menuScene.name);
            //foreach (loadedScene thisScene in loadedScenes)
            for (int i = loadedScenes.Count; i > 0; i--)
            {
                if (loadedScenes[i].scene.name == menuScene.name)
                {
                    //we need to remove this from our list. This should be fine as is as I hope we don't get doubles
                    removeEntries.Add(i);
                }
            }
            if (removeEntries.Count > 0)
            {
                foreach(int i in removeEntries)
                {
                    loadedScenes.RemoveAt(i);   //Will this backfire?
                }
            }
        }
        else
        {
            Debug.LogWarning("Menu scene is not loaded, cannot unload.");
        }
    }

    // Function to load a scene additively and store its reference
    public void LoadMenuSceneAdditively(string sceneName, PanelHandler caller, GameObject callingButton)
    {
        StartCoroutine(LoadMenuSceneAsync(sceneName, caller, callingButton));
    }

    private IEnumerator LoadMenuSceneAsync(string sceneName, PanelHandler caller, GameObject callingButton)
    {
        // Begin loading the scene additively
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Wait until the scene loading is complete
        while (!asyncLoad.isDone)
        {
            yield return null;
        }


        // Once loaded, store the scene reference
        lastLoadedScene = SceneManager.GetSceneByName(sceneName);
        loadedScene newScene = new loadedScene(lastLoadedScene, caller, callingButton);
        //Debug.Log($"Scene '{loadedScene.name}' loaded and reference stored.");
        caller.LoadMenuSceneCallback(newScene, true);
    }

    // Function to unload the stored scene
    /*
    public void UnloadLoadedMenuScene()
    {
        if (loadedScene.IsValid())
        {
            SceneManager.UnloadSceneAsync(loadedScene);
            Debug.Log($"Scene '{loadedScene.name}' unloaded.");
        }
        else
        {
            Debug.LogWarning("No valid scene to unload.");
        }
    }
    */
}