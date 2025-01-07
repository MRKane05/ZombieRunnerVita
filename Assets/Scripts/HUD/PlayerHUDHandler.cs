using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDHandler : MonoBehaviour {
	private static PlayerHUDHandler instance = null;
	public static PlayerHUDHandler Instance { get { return instance; } }

	public Image StaminaBar; //What's our players stamina?
	public DamageIndicatorHandler damageIndicator; //Really this should go through a UI handler, but for the moment...
	public Image healthBar;
	public GameObject ChaserIndicatorsBase;
	RectTransform ourRect;
	void Awake()
	{
		if (instance)
		{
			Debug.Log("Somehow there's a duplicate PlayerHUDHandler in the scene");
			Debug.Log(gameObject.name);
			return; //cancel this
		}

		instance = this;
	}

	void Start()
	{
		ourRect = gameObject.GetComponent<RectTransform>();
	}

	public void setStaminaBar(float t)
	{
		StaminaBar.fillAmount = t;
	}

	public void setHealthBar(float t)
    {
		healthBar.fillAmount = t;
    }

	public void takeDamage(Vector3 DamageDirection, float damage)
	{
		damageIndicator.TakeDamage(DamageDirection, damage);
	}

	//Called when we die, or when we've successfully finished the run
	public void changeMenuState(string newState)
    {

    }

    #region Indicator Functions
	public GameObject AddIndicatorIcon(GameObject thisPrefab)
    {
		GameObject newIndicator = Instantiate(thisPrefab, ChaserIndicatorsBase.transform);
		newIndicator.transform.localScale = Vector3.one;
		return newIndicator;
    }
    #endregion
}
