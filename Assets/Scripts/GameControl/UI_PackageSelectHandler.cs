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

    float totalValue = 0;
    float totalWeight = 0;

    void OnEnable()
    {
        //See if populating our menu from this is a good approach (I dunno). lets try a message first
        setupPackageOptions();
    }

    void setupPackageOptions()
    {
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
    }

    public void SetPanelSelected(float valueChange, float weightChange)
    {
        totalValue += valueChange;
        totalWeight += weightChange;

        //We'll need some sort of handler to prevent the player from running if they've exhausted their funds/weight or similar
        weightDisplay.text = "Total Weight: " + totalWeight.ToString() + "kg";
        moneyDisplay.text = "Total Value $" + totalValue.ToString();
    }
}
