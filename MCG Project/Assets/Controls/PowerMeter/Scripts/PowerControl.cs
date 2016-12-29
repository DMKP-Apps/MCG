using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerControl : MonoBehaviour {
	
	public CannonController CannonController;
	void Start(){
		//CannonController = GameObject.Find("MeatCannon").GetComponent<CannonController>();
	}

    enum MeterState
    {
        Paused,
        Running
    }

    public float Speed = 5.0f;
    private float direction = -1;

    private float startValue = 155;
    private float endValue = 0;
    private MeterState state = MeterState.Paused;



    public float PowerRating = 0.0f;

    // Update is called once per frame
    void Update () {

        if (state == MeterState.Running)
        {
			/*var currentValue = transform.rotation.eulerAngles.z + (Speed * direction);
			if (direction == -1 && currentValue <= endValue)
			{
				direction = 1;
			}
			else if (direction == 1 && currentValue >= startValue)
			{
				direction = -1;
			}*/

            transform.Rotate(new Vector3(0, 0, Speed * direction));

            var currentValue = transform.rotation.eulerAngles.z;
            if (currentValue > 180)
            {
                currentValue -= 360;
            }

            if (direction == -1 && currentValue <= endValue)
            {
                direction = 1;
            }
            else if (direction == 1 && currentValue >= startValue)
            {
                direction = -1;
            }
        }
    }

    public void StartMeter()
    {
        state = MeterState.Running;
        PowerRating = 0;
    }

    public void StopMeter()
    {
        state = MeterState.Paused;

        var currentValue = transform.rotation.eulerAngles.z;
        PowerRating = 1 - (currentValue / startValue);
        if (PowerRating < 0) {
            PowerRating *= -1;
        }
    }

    public void ToggleMeter()
    {
		
		CannonController.Execute ();

		if (CannonController.State == CannonController.CannonState.Ready) {
			StartMeter ();
		} else {
			StopMeter ();
			if (CannonController.State == CannonController.CannonState.Set) {
				CannonController.PowerRate = PowerRating;
			} else {
				transform.rotation = Quaternion.Euler (0, 0, 155);
			}
		}

        /*if (state == MeterState.Paused)
        {
            StartMeter();
        }
        else {
            StopMeter();
        }*/
    }

}
