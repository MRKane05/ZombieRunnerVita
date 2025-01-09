using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILevelCompleteHandler : MonoBehaviour {
	public UIResultsLine StylePointsLine;
	public UIResultsLine PickupsLine;
	public UIResultsLine NestsDestroyedLine;
	public UIResultsLine PackagesDeliveredLine;
	public UIResultsLine TotalsLine;

	// Use this for initialization
	void Start () {
		//StartCoroutine(DoRunResults());
		DoRunResults();
	}

	void DoRunResults()
    {
		//yield return new WaitForSeconds(0.5f);
		//Would be cool to have these animate in some way with some sort of impactful effect

		//For the moment we need to only worry about our packages and totals :)
		float packagesTotal = 0;
		int deliveredPackages = 0;
		List<townPackage> packagesToRemove = new List<townPackage>();
		//We'll need some clever multiplier for our packages, for the moment lets just use a random value from 1.5 to 2
		foreach (townPackage thisPackage in GameController.Instance.carriedPackages)
        {
			if (thisPackage.townName == GameController.Instance.RunDetails.endLocation || true)	//PROBLEM: Hack to allow all packages to be delivered to any town
            {
				packagesTotal += thisPackage.value * Random.RandomRange(1.5f, 2.0f);
				deliveredPackages++;
				packagesToRemove.Add(thisPackage);
            }
        }

		Debug.Log("Doing remove");
		GameController.Instance.RemoveCarriedPackages(packagesToRemove);
		packagesTotal = Mathf.RoundToInt(packagesTotal);

		PackagesDeliveredLine.setLineDetails(deliveredPackages.ToString() + " Packages Delivered", "$" + packagesTotal.ToString());
		TotalsLine.setLineDetails("Total: ", packagesTotal.ToString() + "+" + GameController.Instance.playerInfo.avaliableCash.ToString() + " = " + ((int)packagesTotal + (int)GameController.Instance.playerInfo.avaliableCash).ToString());
		GameController.Instance.playerInfo.avaliableCash = Mathf.RoundToInt(packagesTotal + GameController.Instance.playerInfo.avaliableCash);
	}
}
