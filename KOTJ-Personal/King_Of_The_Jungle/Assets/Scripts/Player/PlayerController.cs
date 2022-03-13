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
    private PlayerSelector PlayerChange;

    [SerializeField] Slider slider;
    [SerializeField] Canvas ActiveIcon;
    [SerializeField] Canvas Crown; 

    public bool canMove = false;
    public bool canShoot = false;

    private bool master;
    public int playerNum = 0;

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

        if (PV.IsMine && PhotonNetwork.IsMasterClient)
        {
            master = true;
            gameObject.tag = "MasterPlayer";
        }
        else
        {
            master = false;
            gameObject.tag = "Player";
        }

        if (!PV.IsMine)
        {
            Destroy(body);
        }
    }

    private void Update() //Runs every frame
    {
        if (!PV.IsMine)
            return;

        //Does not work in awake - Getting player number if not already set
        if (playerNum == 0)
        {
            string name = gameObject.name;
            playerNum = int.Parse(name.Substring(name.Length - 1));
            if (playerNum == 1)
                WearCrown(true);
        }

        //Fetching playerselector component
        if (PlayerChange == null)
            PlayerChange = GameObject.FindGameObjectWithTag("PlayerSelector").GetComponent<PlayerSelector>();

        //Needs to check even if player cannot move
        if (health <= 0)
            Die();

        if (!canMove)
        {
            canShoot = false; //Just to ensure it is false when player cannot move
            ActiveIcon.gameObject.SetActive(false);
            return;
        }

        ActiveIcon.gameObject.SetActive(true);

        for (int i = 0; i < 3; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                PlayerChange.ChangePlayer(i);
                break;
            }
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
            if (master && playerNum == 1) //PlayerNum == 1 - refers to KING
            {
                canMove = (bool)data[0];
                canShoot = (bool)data[0];
                PlayerChange.ChangePlayer(0);
            }
            else if (master && playerNum != 1)
            {
                canMove = false;
                canShoot = false;
            }
            else if (playerNum == 1)
            {
                canMove = (bool)data[1];
                canShoot = (bool)data[1];
                PlayerChange.ChangePlayer(0);
            }
            else if (playerNum != 1)
            {
                canMove = false;
                canShoot = false;
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

    public void WearCrown(bool wear)
    {
        PV.RPC("RPC_WearCrown", RpcTarget.All, wear);
    }

    [PunRPC]
    void RPC_WearCrown(bool wear)
    {
        Crown.gameObject.SetActive(wear);
    }

    void Die()
    {
        if(playerNum == 1)
        {
            object[] winner = new object[] { PhotonNetwork.NickName, PhotonNetwork.PlayerListOthers[0].ToString() };
            PhotonNetwork.RaiseEvent(WINNER, winner, raiseEventOptions, SendOptions.SendReliable);
        }
        PhotonNetwork.Destroy(gameObject);
    }

}
