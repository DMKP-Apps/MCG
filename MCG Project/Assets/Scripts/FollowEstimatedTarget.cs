using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowEstimatedTarget : MonoBehaviour {

    public GameObject target;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void LateUpdate () {

        if (!target.activeInHierarchy && GameSettings.ShotPower.HasValue)
        {
            target.SetActive(true);
        }
        else if (target.activeInHierarchy && !GameSettings.ShotPower.HasValue)
        {
            target.SetActive(false);
        }
        //
        var location = GameSettings.EstimatedShotLocation;
        location.y += 1;

        target.transform.position = location;

    }
}
