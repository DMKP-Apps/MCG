using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectItem : MonoBehaviour {

    public GameObject collectParticleSystem;
    public const string tagBullet = "Bullet";

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != tagBullet)
        {
            return;
        }

        var playerState = other.gameObject.GetComponent<AssociatedPlayerState>().playerState;
        if (playerState != null && !playerState.isOnlinePlayer && GameSettings.playerMode != PlayerMode.LocalMultiplayer) {
            GameSettings.preferences.coins++;
            GameSettings.preferences.Save();
        }

        var item = (GameObject)Instantiate(
                collectParticleSystem, transform.position, transform.rotation);


        Destroy(this.gameObject.transform.parent.gameObject);
        Destroy(item, 5f);

    }
}
