using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasController : MonoBehaviour {

	private Vector2 size;
	GameObject Gauge;
	GameObject Panel;
	GameObject PanelWind;
	GameObject BulletSelect;

	GameObject ControlsContainer;

	// Use this for initialization
	void Start () {
		ControlsContainer = transform.FindChild ("Controls").gameObject;
		Gauge = transform.FindChild("Controls").FindChild ("Gauge").gameObject;
		Panel = transform.FindChild("Controls").FindChild ("Panel").gameObject;
		PanelWind = transform.FindChild("Controls").FindChild ("PanelWind").gameObject;
		BulletSelect = transform.FindChild("Controls").FindChild ("BulletSelect").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		var rect = this.GetComponent<RectTransform> ();
		var currentSize = new Vector2(rect.rect.width, rect.rect.height);
		//var multiplier = 1f;
		//if (rect.rect.width >= 1280 || rect.rect.height >= 1280) {
		//	multiplier = Gauge.transform.localScale.x / 3f + 1f;
		//	Gauge.transform.localScale = new Vector3(3f, 3f, 3f);
		//}
		if (size != currentSize) {
			size = currentSize;
			ScaleGuage (rect.rect);
			ScalePanel (rect.rect);
			ScaleBulletSelect (rect.rect);
			//Gauge.transform.localPosition = new Vector3 ((size.x / 2f) - (50f  * multiplier), (size.y / 2f * -1) + (44f * multiplier) , 0f);
			//Panel.transform.localPosition = new Vector3 ((size.x / 2f * -1) + 98f, (size.y / 2f) - 76f, 0f);
			//PanelWind.transform.localPosition = new Vector3 (110f, (size.y / 2f * -1) + 30f, 0f);
			//BulletSelect.transform.localPosition = new Vector3 ((size.x / 2f * -1) + 65, (size.y / 2f * -1) + 45, 0f);

		}
	}

	private void ScaleGuage(Rect rect)
	{
		var multiplier = 1f;
		Vector2 offset = new Vector2 (50f, 44f);
		if (rect.width >= 1280 || rect.height >= 1280) {
			multiplier = 2f / 4f + 1f;
			Gauge.transform.localScale = new Vector3(4f, 4f, 4f);
			offset = new Vector2 (70f, 70f);
		}
		else {
			Gauge.transform.localScale = new Vector3 (2f, 2f, 2f);
		}

		Gauge.transform.localPosition = new Vector3 ((size.x / 2f) - (offset.x  * multiplier), (size.y / 2f * -1) + (offset.y * multiplier) , 0f);


	}

	private void ScalePanel(Rect rect)
	{
		var multiplier = 1f;
		if (rect.width >= 1280 || rect.height >= 1280) {
			multiplier = 1 / 1.5f + 1f;
			Panel.transform.localScale = new Vector3 (2f, 2f, 2f);
		} else {
			Panel.transform.localScale = new Vector3 (1f, 1f, 1f);
		}
		Panel.transform.localPosition = new Vector3 ((size.x / 2f * -1) + (98f * multiplier), (size.y / 2f) - (78f * multiplier), 0f);
	}

	private void ScaleBulletSelect(Rect rect)
	{
		var multiplier = 1f;
		if (rect.width >= 1280 || rect.height >= 1280) {
			multiplier = 1 / 2f + 1f;
			BulletSelect.transform.localScale = new Vector3 (2f, 2f, 2f);
		} else {
			BulletSelect.transform.localScale = new Vector3 (1f, 1f, 1f);
		}
		BulletSelect.transform.localPosition = new Vector3 ((size.x / 2f * -1) + (65 * multiplier), (size.y / 2f * -1) + (45 * multiplier), 0f);
	}
}
