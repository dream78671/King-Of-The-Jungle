using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using TMPro;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    //Turn Change Variables
    private const int TURN_CHANGE = 1;
    object[] NotMasterTurn = new object[] { false, true }; //MasterClient Disabled, Other Enabled
    object[] MasterTurn = new object[] { true, false }; //MasterClient Enabled, Other Disabled
    object[] DisableAll = new object[] { false, false }; //MasterClient Enabled, Other Disabled
    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; //Send event to all clients


    public static GameManager Instance;
    public GameState State;
    [SerializeField] private Canvas Overlay; 
    [SerializeField] private TextMeshProUGUI MessageText; 
    public static event Action<GameState> OnGameStateChanged;

    public enum GameState { Setup, Wait, HostTurn, NotHostTurn, Victory, Lose }

    //Player Spawn Coordinates
    public float spawn1x;
    public float spawn1y;
    public float spawn2x;
    public float spawn2y;


    private bool nextTurnHost = true;
    private int timePerMove = 3;

    private void Awake()
    {
        //Singleton - If RoomManager exists, delete it
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject); //If only one, make this the manager
        Instance = this;
        UpdateGameState(GameState.Wait);

    }

    //Needed for event calls
    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
        PhotonNetwork.AddCallbackTarget(this);
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PhotonNetwork.RemoveCallbackTarget(this);
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    //If scene has loaded, Create a new playerManager object for each player
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 1 && PhotonNetwork.IsMasterClient)
        {
            GameObject Master = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(spawn1x, spawn1y, -8), Quaternion.identity);
        }
        else if (scene.buildIndex == 1 && !PhotonNetwork.IsMasterClient)
        {
            GameObject NotMaster = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(spawn2x, spawn2y, -8), Quaternion.identity);
        }
    }

    //Function to change GameStates
    public void UpdateGameState(GameState newState)
    {
        State = newState;  

        switch (newState)
        {
            case GameState.Wait:
                StartCoroutine(Wait());
                break;
            case GameState.HostTurn:
                StartCoroutine(HostTurn());
                break;
            case GameState.NotHostTurn:
                StartCoroutine(NotHostTurn()); 
                break;
            case GameState.Victory:
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null); 
        }

        OnGameStateChanged?.Invoke(newState);
    }

    //Wait between each player move
    private IEnumerator Wait()
    {
        StartCoroutine(ShowMessage("End Of Turn", 2));
        yield return new WaitForSeconds(2);

        if (nextTurnHost)
        {
            nextTurnHost = false;
            UpdateGameState(GameState.HostTurn);  
        }
        else
        {
            nextTurnHost = true;
            UpdateGameState(GameState.NotHostTurn);
        }
    }

    private IEnumerator HostTurn()
    {
        //Another wait method called - if not called, player can move while overlay is showing
        StartCoroutine(ShowMessage("Host Turn", 1));
        yield return new WaitForSeconds(1);

        //Raise Event - Event sent to all listeners
        PhotonNetwork.RaiseEvent(TURN_CHANGE, MasterTurn, raiseEventOptions, SendOptions.SendReliable);
        yield return new WaitForSeconds(timePerMove);

        PhotonNetwork.RaiseEvent(TURN_CHANGE, DisableAll, raiseEventOptions, SendOptions.SendReliable);
        UpdateGameState(GameState.Wait);
    }

    private IEnumerator NotHostTurn()
    {
        StartCoroutine(ShowMessage("Not Host Turn", 1));
        yield return new WaitForSeconds(1);

        //Raise event - Event sent to all listeners
        PhotonNetwork.RaiseEvent(TURN_CHANGE, NotMasterTurn, raiseEventOptions, SendOptions.SendReliable);
        yield return new WaitForSeconds(timePerMove);

        PhotonNetwork.RaiseEvent(TURN_CHANGE, DisableAll, raiseEventOptions, SendOptions.SendReliable);
        UpdateGameState(GameState.Wait);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 1)
            Debug.Log("Turn_Change Called");         
    }

    //Show pop up message in game - text = msg shown, time = length of msg popup
    private IEnumerator ShowMessage(String text, int time)
    {
        MessageText.text = text;
        Overlay.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        Overlay.gameObject.SetActive(false);
    }
}


