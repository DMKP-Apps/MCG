using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerControl : MonoBehaviour {
	
	private GameController GameController;
	void Start() {
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
	}

    public enum MeterState
    {
        Paused,
        Running
    }

    public float Speed = 5.0f;
    private float direction = -1;

    public float startValue = 155;
    public float endValue = 0;
    public MeterState state = MeterState.Paused;



    public float PowerRating = 0.0f;

    // Update is called once per frame
    void Update () {

        if (state == MeterState.Running)
        {
			transform.Rotate(new Vector3(0, 0, (Speed) * direction));

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

			//currentValue = transform.rotation.eulerAngles.z;
			var startValueOffset = startValue;
			var offset = 0f;
			if (endValue < 0) {
				offset = endValue * -1;
			}
			startValueOffset += offset;
			currentValue += offset;
			var power = 1 - (currentValue / startValueOffset);
			if (power < 0) {
				power *= -1;
			}

			if (power < 0) {
				power = 0;
			}
			else if (power > 1) {
				power = 1;
			}

			power = float.Parse((System.Math.Ceiling ((power * 100) / 5) * 5 / 100).ToString());

			PowerRating = power; 


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

        /*var currentValue = transform.rotation.eulerAngles.z;
        PowerRating = 1 - (currentValue / startValue);
        if (PowerRating < 0) {
            PowerRating *= -1;
        }
		// testing
		//PowerRating = 1;
		*/

    }

	public void ResetMeter()
	{
		state = MeterState.Paused;

		transform.rotation = Quaternion.Euler (0, 0, startValue);
	}

    
}
