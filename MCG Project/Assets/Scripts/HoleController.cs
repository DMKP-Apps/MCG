using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleController : MonoBehaviour {

	public int Par = 3;

	private Transform teePosition = null;
	private Transform holePosition = null;

	public Transform tee {
		get { 
			if (teePosition == null) {
				foreach (Transform child in this.transform) {
					if (child.tag == "Tee") {
						teePosition = child.gameObject.transform;
						break;
					}
				}
			}
			return teePosition;
		}
	}

	public Transform hole {
		get { 
			if (holePosition == null) {
				foreach (Transform child in this.transform) {
					if (child.tag == "Hole") {
						holePosition = child.gameObject.transform;
						break;
					}
				}
			}
			return holePosition;
		}
	}

}
