using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DamageEffectHandler : MonoBehaviour {
	Vector2 positionSize = new Vector2(640, 272);
	RectTransform ourRect;
	public AnimationCurve EffectAlphaCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
	public Image damageGraphic; //really there'll be a couple that we'll select from I'd guess
	public Image damageGlow;
	float effectDuration = 1f; //How long we're playing that curve for
	float effectStart = 0f;
	public Color StartColor, EndColor;
	public Color GlowStartColor, GlowEndColor;
	public List<Sprite> damageSprites = new List<Sprite>();

	void Start()
	{
		positionSize = new Vector2(Screen.width / 2f, Screen.height / 2F);
		ourRect = gameObject.GetComponent<RectTransform>();
	}

	public bool bIsInUse = false;
	float angleRand = 15f;
	float angleDirection = 45f;

	//PROBLEM: This display is wholey substandard, but suitable for the moment
	public void TakeDamage(Vector2 ScreenDamagePosition, float normalizedAngle, float damage)
	{
		positionSize = new Vector2(Screen.width / 2f, Screen.height / 2F);
		if (ourRect == null)
		{
			ourRect = gameObject.GetComponent<RectTransform>();
		}

		bIsInUse = true;
		damageGraphic.enabled = true; //.gameObject.SetActive(true);
		damageGlow.enabled = true; //.gameObject.SetActive(true);
		//Pick a graphic to display here
		effectStart = Time.time;
		ourRect.anchoredPosition = ScreenDamagePosition;// new Vector2(-ScreenDamagePosition.x, -ScreenDamagePosition.y * positionSize.y);
		
		ourRect.localEulerAngles = new Vector3(Random.Range(-angleRand, angleRand), Random.Range(-angleRand, angleRand), Mathf.Lerp(angleDirection, -angleDirection, normalizedAngle) + Random.Range(-angleRand, angleRand));
		ourRect.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(angleDirection, -angleDirection, normalizedAngle) + Random.Range(-angleRand, angleRand));
		//Need to figure out angle, and what effect we'll be playing here
		damageGraphic.gameObject.transform.DOShakeScale(0.75f, 1.1f);
		damageGraphic.sprite = damageSprites[Mathf.RoundToInt(Mathf.Lerp(0, damageSprites.Count - 1, damage / 30f))];
		gameObject.transform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one * 1.5f, damage / 30f);	//Scale this according to the damage we've taken
		//A quick scale flip for variance
		if (Random.value > 0.5f)
        {
			gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
        }
	}

	void Update()
    {
		if (Time.time > effectStart + 0.25f + effectDuration && bIsInUse)
        {
			bIsInUse = false;
			damageGraphic.enabled = false; //.gameObject.SetActive(false);
			damageGlow.enabled = false; //.gameObject.SetActive(false);
			Debug.Log("Disabling Damage Effect");
		}

        if (bIsInUse)
        {
			float t = 1f- ((effectStart + effectDuration) - Time.time)/effectDuration;
			damageGraphic.color = Color.Lerp(StartColor, EndColor, t);
			damageGlow.color = Color.Lerp(GlowStartColor, GlowEndColor, t);
		}
    }
}
