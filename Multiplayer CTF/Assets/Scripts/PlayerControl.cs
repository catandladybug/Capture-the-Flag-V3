using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerControl : MonoBehaviourPun
{

    [Header("Info")]
    public int id;
    private int curAttackerId;
  

    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;

    private bool flashingDamage;

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;
    public MeshRenderer mr;
    public PlayerWeapon weapon;
    public GameObject hatObject;


    [PunRPC]
    public void Initialize (Player player)
    {

        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        if (id == 1)
            GameManager.instance.GiveHat(id, true);

        if (!photonView.IsMine)
        {

            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;

        }
        else
        {

            GameUI.instance.Initialize(this);

        }

    }

    void Update()
    {

        if (!photonView.IsMine || dead)
            return;

        Move();

        if (Input.GetKeyDown(KeyCode.Space)) 
            TryJump();

        if(Input.GetMouseButtonDown(0))
            weapon.TryShoot();

    }

    void Move()
    {

        // get input axis
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // calculate a direction relative to where we're facing
        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;

        // set that as velocity
        rig.velocity = dir;

    }

    void TryJump()
    {

        // create ray facing down
        Ray ray = new Ray(transform.position, Vector3.down);

        // shoot raycast
        if (Physics.Raycast(ray, 1.5f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

    }

    [PunRPC]
    public void TakeDamage (int attackerId, int damage)
    {

        if (dead)
            return;

        curHp -= damage;
        curAttackerId = attackerId;

        // flash player red
        photonView.RPC("DamageFlash", RpcTarget.Others);

        // update the health bar
        GameUI.instance.UpdateHealthBar();

        // die if no health left
        if (curHp <= 0)
            photonView.RPC("Die", RpcTarget.All);
    }

    [PunRPC]
    void DamageFlash ()
    {

        if (flashingDamage)
            return;

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine ()
        {

            flashingDamage = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;

        }


    }

    [PunRPC]
    void Die ()
    {

        curHp = 0;
        dead = true;

        GameManager.instance.alivePlayers--;

        // host checks win condition
        if(PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();

        // is this our local player?
        if(photonView.IsMine)
        {

            if (curAttackerId != 0)
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);

            // set cam to spectator
            GetComponentInChildren<CameraControl>().SetAsSpectator();

            //disable physics and hide player
            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);

        }

    }

    [PunRPC]
    public void AddKill ()
    {

        kills++;

        // update UI
        GameUI.instance.UpdatePlayerInfoText();

    }

    [PunRPC]
    public void Heal (int amountToHeal)
    {

        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        // update health bar UI
        GameUI.instance.UpdateHealthBar();

    }

    public void SetHat(bool hasHat)
    {

        hatObject.SetActive(hasHat);

    }
    void OnCollisionEnter(Collision collision)
    {

        if (!photonView.IsMine)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {

            if (GameManager.instance.GetPlayer(collision.gameObject).id == GameManager.instance.playerWithHat)
            {

                if (GameManager.instance.CanGetHat())
                {

                    GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, id, false);

                }

            }

        }

    }

}
