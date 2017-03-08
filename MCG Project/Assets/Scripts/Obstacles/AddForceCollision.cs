using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddForceCollision : MonoBehaviour {


	void OnTriggerEnter(Collider other) {

        if (other.tag == "Bullet")
        {
            Debug.Log("Add force to bullet");
            var force = 10f;
            var rbody = other.GetComponent<Rigidbody>();
            if (rbody != null)
            {
                rbody.AddForce(transform.up * (rbody.velocity.magnitude * 0.8f), ForceMode.Impulse);
            }
        }
	}
}
