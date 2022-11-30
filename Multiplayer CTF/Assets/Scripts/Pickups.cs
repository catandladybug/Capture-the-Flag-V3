using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum PickupType
{

    Health,
    Ammo

}

public class Pickups : MonoBehaviour
{
    
    public PickupType type;
    public int value;

    void OnTriggerEnter(Collider other)
    {
        
        if(!PhotonNetwork.IsMasterClient)
            return;

        if(other.CompareTag("Player"))
        {

            // get the player
            PlayerControl player = GameManager.instance.GetPlayer(other.gameObject);

            if (type == PickupType.Health)
                player.photonView.RPC("Heal", player.photonPlayer, value);
            else if (type == PickupType.Ammo)
            {

                player.photonView.RPC("GiveAmmo", player.photonPlayer, value);

            }

            // destroy object
            PhotonView.Destroy(gameObject);

        }

    }

}
