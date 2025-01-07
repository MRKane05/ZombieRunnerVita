using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageIndicatorHandler : MonoBehaviour {
	public GameObject ScratchEffectPrefab;
	//public GameObject ScratchEffect;
	float DamageDecayRate = 0.5f;
	RectTransform ourRect;
	public List<GameObject> scratchEffects = new List<GameObject>();

	void Start() {
		ourRect = gameObject.GetComponent<RectTransform>();
	}

	public GameObject GetScratchEffect()
    {
		bool effectFound = false;
		foreach (GameObject thisScratch in scratchEffects)
        {
			if (!thisScratch.GetComponent<DamageEffectHandler>().bIsInUse)	//Don't ever spawn more than we need
            {
				effectFound = true;
				return thisScratch;
            }
        }

		if (!effectFound)
        {
			GameObject newScratchEffect = Instantiate(ScratchEffectPrefab, gameObject.transform);
			scratchEffects.Add(newScratchEffect);
			return newScratchEffect;
        }

		return null;
    }

	//PROBLEM: This display is wholey substandard, but suitable for the moment
	public void TakeDamage(Vector2 ScreenDamageAngles, float damage) {
		float screenAngleNormal = 90f;	//Which should really be the view space of our rect

		// Map angle to position in parent rectangle
		Vector2 rectSize = ourRect.rect.size;
		float normalizedAngle = ScreenDamageAngles.x / screenAngleNormal; // Normalize angle between -1 and 1
		float xPosition = normalizedAngle * (rectSize.x * 0.5f);
		//This works fine unless the damage is behind us
		// Set y-position to the bottom of the screen
		float yPosition = (-ScreenDamageAngles.y / screenAngleNormal) * (rectSize.y * 0.5f);

		// Clamp indicator position within parent rectangle
		xPosition = Mathf.Clamp(xPosition, -rectSize.x / 2, rectSize.x / 2);
		yPosition = Mathf.Clamp(yPosition, -rectSize.y / 2, rectSize.y / 2);
		// Update indicator position
		GetScratchEffect().GetComponent<DamageEffectHandler>().TakeDamage(new Vector2(xPosition, yPosition), normalizedAngle, damage);
	}
}
