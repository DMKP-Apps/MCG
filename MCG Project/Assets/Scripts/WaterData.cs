using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaterData : MonoBehaviour {

    public List<Transform> respawnPositions;
    
    void Start () {
        // add any child elements that are respawn points
        SetChildrenByTag(transform, "Respawn");

    }
	
	
    private void SetChildrenByTag(Transform parent, string tag)
    {
        if (respawnPositions == null) {
            respawnPositions = new List<Transform>();
        }

        if (parent != null)
        {
            foreach (Transform child in parent)
            {
                if (child.tag == tag && !respawnPositions.Any(x => x.GetInstanceID() == child.GetInstanceID()))
                {
                    respawnPositions.Add(child.gameObject.transform);
                }
            }
        }
        

    }
}
