using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameUI : MonoBehaviour
{
    public PlayerUIContainer[] playerContainers;
    public TextMeshProUGUI winText;

    //instance
    public static GameUI instance;


    private void Awake()
    {
        //set the instance to this script
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerUI();
    }

    void InitializePlayerUI()
    {
        //loop through all containers
        for(int x = 0; x < playerContainers.Length; ++x)
        {
            PlayerUIContainer container = playerContainers[x];

            //only enable and modify UI containers we need
            if (x < PhotonNetwork.PlayerList.Length)
            {
                container.obj.SetActive(true);
                container.nameText.text = PhotonNetwork.PlayerList[x].NickName;
                container.hatTimeSlider.maxValue = GameManagerScript.instance.timeToWin;
            }

            else
                container.obj.SetActive(false);
        }
    }

    void UpdatePlayerUI()
    {
        //loop through all players
        for(int x =0; x < GameManagerScript.instance.players.Length; ++x)
        {
            if (GameManagerScript.instance.players[x] != null)
                playerContainers[x].hatTimeSlider.value = GameManagerScript.instance.players[x].curHatTime;
        }
    }

    public void SetWinText(string winnerName)
    {
        winText.gameObject.SetActive(true);
        winText.text = winnerName + "wins!";
    }
}




[System.Serializable]
public class PlayerUIContainer
{
    public GameObject obj;
    public TextMeshProUGUI nameText;
    public Slider hatTimeSlider;
}