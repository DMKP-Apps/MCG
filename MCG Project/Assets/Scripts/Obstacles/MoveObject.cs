using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour {

	public float Speed = 5f;

	public List<Transform> targetPositions;

	private int currentTargetPosition = 0;
	public bool Enabled = true;
		
	// Update is called once per frame
	void Update () {
		if (!Enabled) {
			return;
		}
		var step = Speed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, targetPositions[currentTargetPosition].position, step);
		if (transform.position == targetPositions [currentTargetPosition].position) {
			currentTargetPosition++;
			if (currentTargetPosition >= targetPositions.Count) {
				currentTargetPosition = 0;		
			}
		}

	}
}
