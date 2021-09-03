using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManagerScript : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    public bool gameEnded = false;          //has the game ended?
    public float timeToWin;                 //time a palyer needs to hold the hat to win
    public float invincibleDuration;        //how long after a player gets the hat, are they invincible
    private float hatPickupTime;            //The time the hat was picked up by the current holder

    [Header("Players")]
    public string playerPrefabLocation;     //path in Resources folder to the Player prefab
    public Transform[] spawnPoints;         // array of all available spawn points
    public PlayerControllerScript[] players;// array of all players
    public int playerWithHat;               //ID of the player with the hat
    private int playersInGame;              // number of players in the game

    //instance
    public static GameManagerScript instance;

    private void Awake()
    {
        //instance
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        players = new PlayerControllerScript[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.All);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;

        if (playersInGame == PhotonNetwork.PlayerList.Length)
            SpawnPlayer();
    }

    //spawns a player and initalizes it 
    void SpawnPlayer()
    {
        //instantiate the palyer across the network
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);

        // get the player script
        PlayerControllerScript playerScript = playerObj.GetComponent<PlayerControllerScript>();

        //Initalize the player
        playerScript.photonView.RPC("Initalize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    public PlayerControllerScript GetPlayer(int playerId)
    {
        return players.First(x => x.id == playerId);
    }
    public PlayerControllerScript GetPlayer(GameObject playerObject)
    {
        return players.First(x => x.gameObject == playerObject);
    }

    // called when a player hits the hatted player - giving them the hat
    [PunRPC]
    public void GiveHat(int playerId, bool initialGive)
    {
        // remove the hat from the currently hatted player
        if (!initialGive)
            GetPlayer(playerWithHat).SetHat(false);
        // give the hat to the new player
        playerWithHat = playerId;
        GetPlayer(playerId).SetHat(true);
        hatPickupTime = Time.time;
    }

    // is the player able to take the hat at this current time?
    public bool CanGetHat()
    {
        if (Time.time > hatPickupTime + invincibleDuration)
            return true;
        else
            return false;
    }


    [PunRPC]
    void WinGame (int playerId)
    {
        gameEnded = true;
        PlayerControllerScript player = GetPlayer(playerId);

        //set the UI to show who's won
        Invoke("GoBackToMenu", 3.0f);
        GameUI.instance.SetWinText(player.PhotonPlayer.NickName);

    }

    void GoBackToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetworkManagerScript.instance.ChangeScene("Menu");
    }

}
