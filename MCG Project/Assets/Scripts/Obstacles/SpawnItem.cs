using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItem : MonoBehaviour {

    public GameObject spawnItemPrefab;
    private GameObject item;
	// Use this for initialization
	void Start () {
        item = (GameObject)Instantiate(
                spawnItemPrefab, transform.position, transform.rotation);
        item.transform.parent = transform;
    }
	
	
}
