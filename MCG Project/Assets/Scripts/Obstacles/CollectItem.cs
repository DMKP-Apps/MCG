using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectItem : MonoBehaviour {

    public GameObject collectParticleSystem;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Bullet")
        {
            return;
        }

        var item = (GameObject)Instantiate(
                collectParticleSystem, transform.position, transform.rotation);


        Destroy(this.gameObject);
        Destroy(item, 5f);

    }
}
