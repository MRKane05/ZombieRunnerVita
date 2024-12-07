using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_LocationDetailsMenuHandler : MonoBehaviour {
    public TextMeshProUGUI townName;
    public TextMeshProUGUI description;
    public TextMeshProUGUI status;
    public TextMeshProUGUI packages;


    public void setNewDetails(string newName, string newDescription, float newStatus, int newPackages)
    {
        townName.text = newName;
        description.text = newDescription;
        status.text = "Status: " + getStatusDescription(newStatus);
        packages.text = "Packages Avaliable: " + newPackages.ToString();
    }

    string getStatusDescription(float thisValue)
    {
        return "Desperate";
    }
}
