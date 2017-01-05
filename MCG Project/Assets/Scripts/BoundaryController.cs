using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryController : MonoBehaviour {

	private const string tagBullet = "Bullet";

	private GameController GameController;
	void Start()
	{
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
	}

	void OnTriggerEnter(Collider other) {

		if (other.gameObject.tag != tagBullet) {
			return;		
		}

		GameController.OutOfBounds (other.gameObject.transform.position);
		Destroy(other.gameObject);
	}
}
