using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerControllerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector]
    public int id;


    [Header("Info")]
    public float moveSpeed;
    public float jumpForce;
    public GameObject hatObject;

    [HideInInspector]
    public float curHatTime;

    [Header("Components")]
    public Rigidbody rig;
    public Player PhotonPlayer;

    // Start is called before the first frame update
    void Start()
    {
        rig = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        // the host will check if the player has won
        if (PhotonNetwork.IsMasterClient)
        {
            if(curHatTime >= GameManagerScript.instance.timeToWin && !GameManagerScript.instance.gameEnded)
            {
                GameManagerScript.instance.gameEnded = true;
                GameManagerScript.instance.photonView.RPC("WinGame", RpcTarget.All, id);
            }
        }

        if (photonView.IsMine)
        {

            Move();

            if (Input.GetKeyDown(KeyCode.Space))
                TryJump();

            // track the amount of time we're wearing the hat
            if (hatObject.activeInHierarchy)
                curHatTime += Time.deltaTime;
        }

    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        rig.velocity = new Vector3(x, rig.velocity.y, z);
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, 0.7f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    //called hwne the player object is instantiated
    [PunRPC]
    public void Initalize(Player player)
    {
        PhotonPlayer = player;
        id = player.ActorNumber;

        GameManagerScript.instance.players[id - 1] = this;

        // give the first player the hat
        if (id == 1)
            GameManagerScript.instance.GiveHat(id, true);

        //if this isn't our local player, diable physics as that's controlled by the user and synced to all other clients
        if (!photonView.IsMine)
            rig.isKinematic = true;

    }

    // sets the player's hat active or not
    public void SetHat(bool hasHat)
    {
        hatObject.SetActive(hasHat);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine)
            return;

        //did we hit another player?
        if (collision.gameObject.CompareTag("Player"))
        {
            // do they have the hat?
            if(GameManagerScript.instance.GetPlayer(collision.gameObject).id ==
                GameManagerScript.instance.playerWithHat)
            {
                //can we get the hat?
                if (GameManagerScript.instance.CanGetHat())
                {
                    //give us the hat
                    GameManagerScript.instance.photonView.RPC("GiveHat", RpcTarget.All, id, false);
                }
            }
        }
    }


    public void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(curHatTime);
        }
        else if (stream.IsReading)
        {
            curHatTime = (float)stream.ReceiveNext();
        }
    }

}
