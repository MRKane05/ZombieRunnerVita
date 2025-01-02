using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_Chasericon : MonoBehaviour {
	Vector2 positionSize = new Vector2(480f, 272);
	public EnemyBehavior parentEnemy;
	Image ourImage;
	RectTransform ourRect;
	float fadeDistance = 150f;  //At what point do our chasers drop off?
	float strikeDistance = 15f;	//At what point do we make our warning icon white
								//we need an uneven fade control for our distance measure. Lets put this on a curve
	public AnimationCurve fadeCurve = new AnimationCurve();
	public float maxSize = 300f;

	//A few details to indicate when we're within striking distance
	public Color Color_Standard = Color.red;
	public Color Color_StrikingDistance = Color.red;
	public Color Color_DoingStrike = Color.white;

	// Use this for initialization
	void Start () {
		ourRect = gameObject.GetComponent<RectTransform>();
		ourImage = gameObject.GetComponent<Image>();
		//Lazy setup stuff
		positionSize = new Vector2(Screen.width / 2f, Screen.height / 2F);
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (parentEnemy)
        {
			Vector3 widgetLocation = parentEnemy.GetWidgetPosition();
			//Interesting that they're both inverted
			ourRect.anchoredPosition = new Vector2(-widgetLocation.x * positionSize.x, -widgetLocation.y * positionSize.y);
			//Debug.Log(widgetLocation.z);
			float closeFrac = fadeCurve.Evaluate(1f - Mathf.Clamp01(widgetLocation.z / fadeDistance));
			ourRect.sizeDelta = Vector2.one * maxSize * closeFrac;
			if (widgetLocation.z < strikeDistance)
            {
				ourImage.color = Color_DoingStrike;
            } else
            {
				ourImage.color = Color.Lerp(Color_Standard, Color_StrikingDistance, closeFrac);
			}
		}
	}
}
