using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class CannonCollisonController : MonoBehaviour {

    private Rigidbody _rigidBody;


	void Update () {
        if (_rigidBody == null)
        {
            var parent = GetParentObjectByTag(this.gameObject, "Player");
            if (parent != null)
            {
                _rigidBody = parent.GetComponent<Rigidbody>();
            }
        }


        if (_rigidBody != null && _rigidBody.velocity.magnitude > 5)
        {
            var rotation = normalizeRotationVector(_rigidBody.transform.localRotation.eulerAngles);
            if (rotation.x > 25 || rotation.x < -25 || rotation.z > 25 || rotation.z < -25)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3(0, rotation.y, 0)), Time.deltaTime * 50);
            }

        }
        else if (_rigidBody != null)
        {
            var rotation = normalizeRotationVector(_rigidBody.transform.localRotation.eulerAngles);
            if (rotation.x > 50 || rotation.x < -50 || rotation.z > 50 || rotation.z < -50)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3(0, rotation.y, 0)), Time.deltaTime * 50f);
            }

        }

    }

    private Vector3 normalizeRotationVector(Vector3 r)
    {
        if (r.x > 180)
        {
            r.x -= 360;
        }
        else if (r.x < -180)
        {
            r.x += 360;
        }
        if (r.y > 180)
        {
            r.y -= 360;
        }
        else if (r.y < -180)
        {
            r.y += 360;
        }
        if (r.z > 180)
        {
            r.z -= 360;
        }
        else if (r.z < -180)
        {
            r.z += 360;
        }

        return r;
    }

    private GameObject GetParentObjectByTag(GameObject parent, string tag)
    {
        GameObject result = parent;
        while (result != null && result.tag != tag)
        {
            result = result.transform.parent == null ? null : result.transform.parent.gameObject;
        }

        return result;
    }

}
