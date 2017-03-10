using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonWheelController : MonoBehaviour {

	public GameObject CannonWheelR;
	public GameObject CannonWheelL;

	public InputController inputController;

    void Start()
    {
        //var touchUtility = GameObject.FindObjectOfType<TouchUtility>();
        //if (touchUtility != null)
        //{
        //    touchUtility.Subscribe(this, (touches) => OnTouchMoved(), TouchEventType.Moved);
        //}
    }

    void Update () {
		if (inputController.InputPosition.x != 0) {
			CannonWheelR.transform.Rotate(new Vector3(inputController.InputPosition.x * -1,0,0));
			CannonWheelL.transform.Rotate(new Vector3(inputController.InputPosition.x,0,0));
		}
	}
}
