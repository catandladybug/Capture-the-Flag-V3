using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerWeapon : MonoBehaviour
{

    [Header("Stats")]
    public int damage;
    public int curAmmo;
    public int maxAmmo;
    public float bulletSpeed;
    public float shootRange;

    private float lastShootTime;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;

    private PlayerControl player;

    void Awake()
    {

        // get required components
        player = GetComponent<PlayerControl>(); 

    }

    public void TryShoot ()
    {

        // can we shoot?
        if (curAmmo <= 0 || Time.time - lastShootTime < shootRange)
            return;

        curAmmo--;
        lastShootTime = Time.time;

        // update the ammo UI
        GameUI.instance.UpdateAmmoText();

        // spawn bullet
        player.photonView.RPC("SpawnBullet", RpcTarget.All, bulletSpawnPos.position, Camera.main.transform.forward);

    }

    [PunRPC]
    void SpawnBullet (Vector3 pos, Vector3 dir)
    {

        // spawn and orrientate it
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;

        //get bullet script
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        // initialize it and set velocity
        bulletScript.Initialize(damage, player.id, player.photonView.IsMine);
        bulletScript.rig.velocity = dir * bulletSpeed;

    }

    [PunRPC]
    public void GiveAmmo (int ammoToGive)
    {

        curAmmo = Mathf.Clamp(curAmmo + ammoToGive, 0, maxAmmo);

        // update ammo text
        GameUI.instance.UpdateAmmoText();

    }


}
