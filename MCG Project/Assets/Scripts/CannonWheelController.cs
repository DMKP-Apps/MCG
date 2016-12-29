using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonWheelController : MonoBehaviour {

	public GameObject CannonWheelR;
	public GameObject CannonWheelL;

	public InputController inputController;

	void Update () {
		if (inputController.InputPosition.x != 0) {
			CannonWheelR.transform.Rotate(new Vector3(0, 0,inputController.InputPosition.x * -1));
			CannonWheelL.transform.Rotate(new Vector3(0, 0,inputController.InputPosition.x));
		}
	}
}
