using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddForceCollision : MonoBehaviour {

    public float forcePercentage = 0.8f;

	void OnTriggerEnter(Collider other) {

        if (other.tag == "Bullet")
        {
            var rbody = other.GetComponent<Rigidbody>();
            if (rbody != null)
            {
                var force = rbody.velocity.magnitude * forcePercentage;
                Debug.Log(string.Format("Add force to bullet: {0}", force));
                rbody.AddForce(transform.up * force, ForceMode.Impulse);
            }
        }
	}
}
