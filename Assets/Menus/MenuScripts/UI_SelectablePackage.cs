using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_SelectablePackage : UI_ToggleButton {
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemValueTitle;
    public TextMeshProUGUI itemWeightTitle;

    float itemValue = 10;
    float itemWeight = 1;

    public townPackage targetPackage;

    UI_PackageSelectHandler ourPackageHandler;

    public void setPackageDetails(townPackage newTargetPackage, UI_PackageSelectHandler newPackageHandler) //string newItemName, float newItemValue, float newItemWeight)
    {
        ourPackageHandler = newPackageHandler;

        targetPackage = newTargetPackage;
        itemName.text = targetPackage.basicDescription;
        itemValue = targetPackage.value;
        itemWeight = targetPackage.weight;

        //Do we need a reference back to the array item that this is working on?
        itemValueTitle.text = "$" + itemValue.ToString();
        if (itemWeight >= 1f) {
            itemWeightTitle.text = itemWeight.ToString() + "kg";
        } else
        {
            itemWeightTitle.text = (1000f * itemWeight).ToString() + "g";
        }
    }

    public override void toggleSelectState()
    {
        base.toggleSelectState(); //call our super, but we also need to pass through the information to our panel handler to keep an eye on the tickers
        if (bIsSelected)
        {
            ourPackageHandler.SetPanelSelected(itemValue, itemWeight);
        } else
        {
            ourPackageHandler.SetPanelSelected(-itemValue, -itemWeight);
        }
    }
}
