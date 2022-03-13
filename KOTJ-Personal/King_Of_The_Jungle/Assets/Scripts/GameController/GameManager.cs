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
    private const int WINNER = 2;
    object[] NotMasterTurn = new object[] { false, true }; //MasterClient Disabled, Other Enabled
    object[] MasterTurn = new object[] { true, false }; //MasterClient Enabled, Other Disabled
    object[] DisableAll = new object[] { false, false }; //MasterClient Enabled, Other Disabled
    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; //Send event to all clients

    private string winner;

    public static GameManager Instance;
    public GameState State;
    [SerializeField] private Canvas Overlay; 
    [SerializeField] private TextMeshProUGUI MessageText;
    [SerializeField] private Canvas TimerOverlay;
    [SerializeField] private TextMeshProUGUI TimerText;
    public static event Action<GameState> OnGameStateChanged;

    public enum GameState { Start, Wait, HostTurn, NotHostTurn, Victory, Lose }

    //Player Spawn Coordinates
    public float spawn1x;
    public float spawn1y;
    public float spawn2x;
    public float spawn2y;

    private bool nextTurnHost = true;
    private float timePerMove = 15f;
    private bool activeTimer = false;
    private float currentTime = 0f;

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

    private void Update()
    {
        if (!activeTimer)
        {
            TimerOverlay.gameObject.SetActive(false);
            return;
        }
        else
            TimerOverlay.gameObject.SetActive(true);

        currentTime -= 1 * Time.deltaTime;
        TimerText.text = currentTime.ToString("0");

        if (currentTime <= 0)
            activeTimer = false;
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

    //If scene has loaded, Create a new playerManager object for each player - sceneIndex 2 refers to map 1 (sceneIndex is the scene order in build settings)
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 2 && PhotonNetwork.IsMasterClient)
        {
            GameObject player1 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(spawn1x, spawn1y, -8), Quaternion.identity);
            player1.name = "MasterPlayer1";
            GameObject player2 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(spawn1x + 1, spawn1y, -8), Quaternion.identity);
            player2.name = "MasterPlayer2";
            GameObject player3 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(spawn1x + 2, spawn1y, -8), Quaternion.identity);
            player3.name = "MasterPlayer3";
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerSelector"), new Vector3(0, 0, 0), Quaternion.identity, 0);
        }
        else if (scene.buildIndex == 2 && !PhotonNetwork.IsMasterClient)
        {
            GameObject player1 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(spawn2x, spawn2y, -8), Quaternion.identity);
            player1.name = "Player1";
            GameObject player2 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(spawn2x - 1, spawn2y, -8), Quaternion.identity);
            player2.name = "Player2";
            GameObject player3 = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(spawn2x - 2, spawn2y, -8), Quaternion.identity);
            player3.name = "Player3";
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerSelector"), new Vector3(0, 0, 0), Quaternion.identity, 0);
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
                StartCoroutine(Victory());
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null); 
        }
        OnGameStateChanged?.Invoke(newState);
    }

    
    //Wait between each player move
    private IEnumerator Wait()
    {
        StartCoroutine(ShowMessage("Get Ready To Fight", 2));
        yield return new WaitForSeconds(2);

        if(nextTurnHost)
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
        StartCoroutine(ShowMessage("Host Turn", 2));
        yield return new WaitForSeconds(2);

        //Raise Event - Event sent to all listeners
        PhotonNetwork.RaiseEvent(TURN_CHANGE, MasterTurn, raiseEventOptions, SendOptions.SendReliable);
        activeTimer = true;
        currentTime = timePerMove;
        yield return new WaitForSeconds(timePerMove);
        activeTimer = false;

        PhotonNetwork.RaiseEvent(TURN_CHANGE, DisableAll, raiseEventOptions, SendOptions.SendReliable);
        UpdateGameState(GameState.Wait);
    }

    private IEnumerator NotHostTurn()
    {
        StartCoroutine(ShowMessage("Not Host Turn", 2));
        yield return new WaitForSeconds(2);

        //Raise event - Event sent to all listeners
        PhotonNetwork.RaiseEvent(TURN_CHANGE, NotMasterTurn, raiseEventOptions, SendOptions.SendReliable);
        activeTimer = true;
        currentTime = timePerMove;
        yield return new WaitForSeconds(timePerMove);
        activeTimer = false;

        PhotonNetwork.RaiseEvent(TURN_CHANGE, DisableAll, raiseEventOptions, SendOptions.SendReliable);
        UpdateGameState(GameState.Wait);
    }

    private IEnumerator Victory()
    {
        StartCoroutine(ShowMessage(winner + " WINS!", 10));
        yield return new WaitForSeconds(10);
    }

    public void OnEvent(EventData photonEvent)
    {
        //WINNER EVENT - SENT BY LOSING PLAYER
        if (photonEvent.Code == WINNER)
        {
            StopAllCoroutines();
            object[] data = (object[])photonEvent.CustomData;
            //Index 0 - Master, Index 1 - Other Player
            winner = data[1].ToString();
            UpdateGameState(GameState.Victory);
        }
    }

    //Show pop up message in game - text = msg shown, time = length of msg popup
    private IEnumerator ShowMessage(String text ,int time)
    {
        MessageText.text = text;
        Overlay.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        Overlay.gameObject.SetActive(false);
    }
}


