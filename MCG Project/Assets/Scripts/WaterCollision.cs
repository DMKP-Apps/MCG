using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCollision : MonoBehaviour {

	public GameObject splash;
	private GameController GameController;
	void Start()
	{
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
	}

	void OnTriggerEnter(Collider other) {
		
		GameController.WaterHazard (this.gameObject, other.gameObject.transform.position);
		//var bulletHit
		//this.gameObject.SetActive (false);
		if (splash != null) {
			var ps = (GameObject)Instantiate (splash, other.gameObject.transform.position, splash.transform.rotation);
			Destroy (ps, 5f);
		}
		//Destroy(this.gameObject, 5f);
	}
}
