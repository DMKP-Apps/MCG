using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AttachToObject : MonoBehaviour {


    public List<string> lookForTag = new List<string>();
    private GameController GameController;

    void Start() {
        GameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    void OnCollisionEnter(Collision collision)
    {
        var allParents = getAllParents();
        
        foreach (ContactPoint contact in collision.contacts)
        {
            if (lookForTag.Any(x => contact.otherCollider.gameObject.tag == x) && !allParents.Take(1).Any(x => x.GetInstanceID() == contact.otherCollider.gameObject.GetInstanceID()))
            {
                // move the current item to the parent.
                this.transform.parent = contact.otherCollider.gameObject.transform;
                var associatedPlayerState = this.GetComponent<AssociatedPlayerState>();
                if (associatedPlayerState != null && associatedPlayerState.playerState != null)
                {
                    associatedPlayerState.playerState.MoveToParent = contact.otherCollider.gameObject.transform;
                }
                else
                {
                    allParents.Select(x => x.GetComponent<CannonPlayerState>()).Where(x => x != null).Take(1)
                        .ToList().ForEach(x =>
                        {
                            x.MoveToParent = contact.otherCollider.gameObject.transform;
                        });
                    
                }
                break;
            }

        }
    }

    private List<GameObject> getAllParents()
    {
        List<GameObject> parents = new List<GameObject>();
        Transform iparent = this.transform.parent;
        while (iparent != null)
        {
            parents.Add(iparent.gameObject);
            iparent = iparent.parent;
        }

        return parents;
    }
}
