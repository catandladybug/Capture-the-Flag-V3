using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPun
{

    public float postGameTime;

    [Header("Players")]
    public string playerPrefabLocation;
    public PlayerControl[] players;
    public int alivePlayers;
    public int playerWithHat;

    [Header("Team Stuff")]
    public int nextPlayersTeam;
    public Transform[] spawnPointsTeamOne;
    public Transform[] spawnPointsTeamTwo;
    public int myTeam;

    private float hatPickupTime;
    public float invincibleDuration;

    private int playersInGame;

    // instance
    public static GameManager instance;

    void Awake()
    {
        
        instance = this;

    }

    void Start()
    {
        
        players = new PlayerControl[PhotonNetwork.PlayerList.Length];
        alivePlayers = players.Length;

        if(photonView.IsMine)
        {
            photonView.RPC("RPC_GetTeam", RpcTarget.MasterClient);
        }

        photonView.RPC("ImInGame", RpcTarget.AllBuffered);

    }

    [PunRPC]
    void ImInGame ()
    {

        playersInGame++;

        if (PhotonNetwork.IsMasterClient && playersInGame == PhotonNetwork.PlayerList.Length)
            photonView.RPC("SpawnPlayer", RpcTarget.All);

    }

    [PunRPC]
    void SpawnPlayer ()
    {
        if (myTeam == 1)
        {
            GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPointsTeamOne[Random.Range(0, spawnPointsTeamOne.Length)].position, Quaternion.identity);

            // initialize player for all players
            playerObj.GetComponent<PlayerControl>().photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
        }
        else
        {
            GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPointsTeamTwo[Random.Range(0, spawnPointsTeamTwo.Length)].position, Quaternion.identity);

            // initialize player for all players
            playerObj.GetComponent<PlayerControl>().photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
        }
    }

    public void UpdateTeam()
    {
        if (nextPlayersTeam == 1)
        {
            nextPlayersTeam = 2;
        }
        else
        {
            nextPlayersTeam = 1;
        }
    }

    [PunRPC]
    void RPC_GetTeam()
    {
        myTeam = nextPlayersTeam;
        UpdateTeam();
        photonView.RPC("RPC_SentTeam", RpcTarget.OthersBuffered, myTeam);
    }

    [PunRPC]
    void RPC_SentTeam(int whichTeam)
    {
        myTeam = whichTeam;
    }

    public PlayerControl GetPlayer (int playerId)
    {

        foreach(PlayerControl player in players)
        {

            if(player != null && player.id == playerId)
                return player;

        }

        return null;

    }

    public PlayerControl GetPlayer (GameObject playerObject)
    {

        foreach (PlayerControl player in players)
        {

            if (player != null && player.gameObject == playerObject)
                return player;

        }

        return null;

    }

    public void CheckWinCondition ()
    {

        if (alivePlayers == 1)
            photonView.RPC("WinGame", RpcTarget.All, players.First(x => !x.dead).id);
        

    }

    [PunRPC]
    void WinGame (int winningPlayer)
    {

        // set UI win text
        GameUI.instance.SetWinText(GetPlayer(winningPlayer).photonPlayer.NickName);

        Invoke("GoBackToMenu", postGameTime);

    }

    void GoBackToMenu ()
    {

        NetworkManager.instance.ChangeScene("Menu");

    }

    [PunRPC]
    public void GiveHat(int playerId, bool initialGive)
    {

        if (!initialGive)
            GetPlayer(playerWithHat).SetHat(false);

        playerWithHat = playerId;
        GetPlayer(playerId).SetHat(true);
        hatPickupTime = Time.time;

    }

    public bool CanGetHat()
    {

        if (Time.time > hatPickupTime + invincibleDuration)
            return true;
        else
            return false;

    }



}
