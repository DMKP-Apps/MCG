using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveForTime : MonoBehaviour {

	public int TotalMilliseconds = 5000;

	private System.DateTime StartTime = System.DateTime.MaxValue;

	// Update is called once per frame
	void Update () {
		if (this.gameObject.activeInHierarchy && System.DateTime.Now.Subtract(StartTime).TotalMilliseconds >= TotalMilliseconds) {
			Hide ();
		}
	}

	public void Show()
	{
		if (!this.gameObject.activeInHierarchy) {
			StartTime = System.DateTime.Now;
			this.gameObject.SetActive (true);
		}

	}

	public void Hide()
	{
		if (this.gameObject.activeInHierarchy) {
			StartTime = System.DateTime.MaxValue;
			this.gameObject.SetActive (false);
		}
	}
}
