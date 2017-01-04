using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonPlayerState : MonoBehaviour {

	public GameObject Spark;

	public int Stroke = 1;
	public int TotalScore = 0;

	private GameObject CannonBarrow;

	void Start()
	{
		CannonBarrow = transform.FindChild ("Cannon").gameObject;
	
	}

	public enum State
	{
		None,
		Ready,
		Set,
		Fire
	}

	private State _state = State.None;

	public State CurrentState {
		get { 
			return _state;
		}
		set { 
			_state = value;
			if (_state == State.Set) {
				var system = Spark.GetComponent<ParticleSystem> ();
				system.Play ();
			} else {
				var system = Spark.GetComponent<ParticleSystem>();
				system.Stop ();


			}
		}
	}

	public void ResetBarrowPosition()
	{
		CannonBarrow.transform.localRotation = Quaternion.Euler(new Vector3 (0, 0, 0));
	}

	// Update is called once per frame
	void Update () {
		
	}


}
