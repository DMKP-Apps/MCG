using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasController : MonoBehaviour {

	private Vector2 size;
	GameObject Gauge;
	GameObject Panel;
	GameObject PanelWind;
	// Use this for initialization
	void Start () {
		Gauge = transform.FindChild ("Gauge").gameObject;
		Panel = transform.FindChild ("Panel").gameObject;
		PanelWind = transform.FindChild ("PanelWind").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		var rect = this.GetComponent<RectTransform> ();
		var currentSize = new Vector2(rect.rect.width, rect.rect.height);
		if (size != currentSize) {
			size = currentSize;
			Gauge.transform.localPosition = new Vector3 ((size.x / 2f) - 52f, (size.y / 2f * -1) + 44f, 0f);
			Panel.transform.localPosition = new Vector3 ((size.x / 2f * -1) + 98f, (size.y / 2f) - 76f, 0f);
			PanelWind.transform.localPosition = new Vector3 (110f, (size.y / 2f * -1) + 30f, 0f);

		}
	}
}
