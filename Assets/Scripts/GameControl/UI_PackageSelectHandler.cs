using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_PackageSelectHandler : PanelHandler
{
    //Couple of complexities here: we've got to somehow get the information for the packages avaliable at the target site...
    //I reckon it can sit on the GameController :)
    public GameObject LayoutGroup;
    public GameObject PackageEntryPrefab;

    public TextMeshProUGUI weightDisplay;
    public TextMeshProUGUI moneyDisplay;


    public TextMeshProUGUI avaliableWeightDisplay;
    public TextMeshProUGUI avaliableMoneyDisplay;

    public float totalValue = 0;
    public float totalWeight = 0;

    void OnEnable()
    {
        //See if populating our menu from this is a good approach (I dunno). lets try a message first
        StartCoroutine(setupPackageOptions());
        SetPanelSelected(0, 0);
    }

    IEnumerator setupPackageOptions()
    {
        //This needs to take into account anything we're already carrying...
        avaliableWeightDisplay.text = "Avaliable: " + (GameController.Instance.playerInfo.avaliableWeight - GameController.Instance.GetWeightOfCarriedPackages()).ToString();
        avaliableMoneyDisplay.text = "Avaliable: $" + GameController.Instance.playerInfo.avaliableCash.ToString();

        if (LayoutGroup.transform.childCount > 0)
        {
            //we need to clear children
            foreach (Transform Child in LayoutGroup.transform)
            {
                Destroy(Child.gameObject);
            }
        }

        //Go through from the gamecontroller and populate our panel entries
        foreach (townPackage thisPackage in GameController.Instance.avaliablePackages)
        {
            GameObject newPackage = Instantiate(PackageEntryPrefab) as GameObject;
            newPackage.transform.SetParent(LayoutGroup.transform);

            UI_SelectablePackage newSelectable = newPackage.GetComponent<UI_SelectablePackage>();
            newSelectable.setPackageDetails(thisPackage, this);
        }

        yield return null;
        //After we've done this we should set our selection at the top item
        UIHelpers.SetSelectedButton(LayoutGroup.transform.GetChild(0).gameObject);
    }

    public void SetPanelSelected(float valueChange, float weightChange)
    {
        totalValue += valueChange;
        totalWeight += weightChange;

        //We'll need some sort of handler to prevent the player from running if they've exhausted their funds/weight or similar
        weightDisplay.text = "Total Weight: " + totalWeight.ToString() + "kg";
        moneyDisplay.text = "Total Value $" + totalValue.ToString();
    }

    public void StartRun()
    {
        List<townPackage> selectedPackages = new List<townPackage>();
        foreach (Transform thisChild in LayoutGroup.transform)
        {
            UI_SelectablePackage thisPackage = thisChild.gameObject.GetComponent<UI_SelectablePackage>();
            if (thisPackage.bIsSelected)
            {
                selectedPackages.Add(thisPackage.targetPackage);
            }
        }

        if (selectedPackages.Count > 0)
        {
            GameController.Instance.AddCarriedPackages(selectedPackages);
        }

        //We'll need to go through our selected packages and set things correctly before doing the run
        GameController.Instance.DoRunTo(GameController.Instance.RunDetails.endLocation);
    }
}
