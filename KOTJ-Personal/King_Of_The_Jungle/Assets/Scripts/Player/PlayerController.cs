using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks, IOnEventCallback, IDamageable
{
    private const int TURN_CHANGE = 1;
    private const int WINNER = 2;

    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; //Send event to all clients

    private Rigidbody2D body;
    private PhotonView PV;

    [SerializeField] Slider slider; 
    
    public bool canMove = false;
    public bool canShoot = false; 
    private bool master;

    private const float maxHealth = 100;
    public float health;

    //Needed for handling Events
    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void Awake() //Called Everytime script is loaded
    {
        body = GetComponent<Rigidbody2D>(); //Gets component from game object from inspector tab
        PV = GetComponent<PhotonView>();
        gameObject.tag = "Player";

        if (PV.IsMine && PhotonNetwork.IsMasterClient)
            master = true;
        else
            master = false;

        if (!PV.IsMine)
        {
            Destroy(body);
        }
    }

    private void Update() //Runs every frame
    {
        if (!PV.IsMine)
            return;

        //Needs to check even if player cannot move
        if (health <= 0)
            Die();

        if (!canMove)
        {
            canShoot = false; //Just to ensure it is false when player cannot move
            return;
        }
        
    }


    //Photon - Deals with events that are called
    public void OnEvent(EventData photonEvent)
    {
        //TURN CHANGE EVENT - CALL SENT BY GAMEMANAGER
        if (photonEvent.Code == TURN_CHANGE)
        {
            object[] data = (object[])photonEvent.CustomData;

            //Index 0 - Master, Index 1 - Other Player
            if (master)
            {
                canMove = (bool)data[0];
                canShoot = (bool)data[0];
            }
            else
            {
                canMove = (bool)data[1];
                canShoot= (bool)data[1];
            }
        }
    }

    
    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        health -= damage;
        slider.value = 1 - (health / maxHealth);
    }

    void Die()
    {
        object[] winner = new object[] { PhotonNetwork.NickName, PhotonNetwork.PlayerListOthers[0].ToString()};
        PhotonNetwork.RaiseEvent(WINNER, winner, raiseEventOptions, SendOptions.SendReliable);
    }

}
