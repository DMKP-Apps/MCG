using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ObstacleCollision : MonoBehaviour {

    private List<string> collidingIds = new List<string>();

    public Transform moveToPosition;

    void OnCollisionEnter(Collision collision)
    {
        //collision.contacts.Select(x => x.otherCollider.gameObject)
        //    .ToList()
        //    .ForEach(x => {

        //        var cannon = GetParentObjectByTag(x, "Player");

        //    });
        foreach (ContactPoint contact in collision.contacts)
        {
            var cannon = GetParentObjectByTag(contact.otherCollider.gameObject, "Player");


            if (cannon != null)
            {
                var rbody = cannon.GetComponent<Rigidbody>();
                cannon.transform.position = collision.collider.ClosestPointOnBounds(contact.point);// moveToPosition.position;
                if (rbody != null)
                {
                    rbody.AddForce(rbody.transform.right * 10f, ForceMode.Impulse);
                    break;
                }
            }
        }

    }

    void OnCollisionExit(Collision collision)
    {

        //collidingIds = collision.contacts.Select(x => x.otherCollider.gameObject.name).Distinct().ToList();
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
