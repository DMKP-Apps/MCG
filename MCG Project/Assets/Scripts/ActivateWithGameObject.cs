using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateWithGameObject : MonoBehaviour {

	public GameObject WatchObject;
	public GameObject ControlObject;
	
	// Update is called once per frame
	void Update () {
		ControlObject.gameObject.SetActive (WatchObject.activeInHierarchy);
	}
}
