using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ObstacleCollision : MonoBehaviour {

    [Serializable]
    public struct BulletCollisionHandle
    {
        public bool SetUnplayable;
        public GameObject Animation;

    }

    [Serializable]
    public struct ObjectCollisionHandle
    {
        public string tag;
        public GameObject Animation;

    }

    public BulletCollisionHandle bulletCollision;

    public List<ObjectCollisionHandle> otherCollisions;

    private List<string> collidingIds = new List<string>();

    private List<ObjectCollisionHandle> collisionsAnimations;

    void OnCollisionEnter(Collision collision)
    {
        
        foreach (ContactPoint contact in collision.contacts)
        {

            if (contact.otherCollider.tag == "Bullet")
            {
                if (bulletCollision.SetUnplayable)
                {
                    var hitController = contact.otherCollider.gameObject.GetComponent<BulletHitController>();
                    if (hitController != null)
                    {
                        hitController.HitHazard(this.tag, this.gameObject, contact.point, bulletCollision.Animation);
                    }
                }
            }
            
            else
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

            otherCollisions.Where(x => x.tag == contact.otherCollider.tag && x.Animation != null)
                .ToList()
                .ForEach(x => {
                    var ps = (GameObject)Instantiate(x.Animation, contact.point, Quaternion.Euler(0, 0, 0));
                    collisionsAnimations.Add(new ObjectCollisionHandle() { Animation = ps, tag = x.tag });
                });

        }

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

    void OnCollisionExit(Collision collision)
    {
        var collidingIds = collision.contacts.Select(x => x.otherCollider.gameObject.tag).ToList();
        collisionsAnimations.Where(x => !collidingIds.Any(y => y == x.tag) && x.Animation != null).ToList()
            .ForEach(x => {
                Destroy(x.Animation);
            });

        collisionsAnimations = collisionsAnimations.Where(x => collidingIds.Any(y => y == x.tag)).ToList();
    }

}
