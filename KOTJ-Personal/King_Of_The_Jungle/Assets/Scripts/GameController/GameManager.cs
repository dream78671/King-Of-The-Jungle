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
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    //Turn Change Variables
    private const int TURN_CHANGE = 1;
    private const int WINNER = 2;
    private const int QUIT = 3;
    object[] NotMasterTurn = new object[] { false, true }; //MasterClient Disabled, Other Enabled
    object[] MasterTurn = new object[] { true, false }; //MasterClient Enabled, Other Disabled
    object[] DisableAll = new object[] { false, false }; //MasterClient Enabled, Other Disabled
    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; //Send event to all clients

    public static GameManager Instance;
    public GameState State;

    [SerializeField] private GameObject SendLeaderboard;
    private PlayfabLeaderboardManager LeaderboardManager;

    [SerializeField] private Canvas Overlay; 
    [SerializeField] private Text MessageText;
    [SerializeField] private Text ExtraMessageText;

    [SerializeField] private Canvas TimerOverlay;
    [SerializeField] private Text TimerText;
    public static event Action<GameState> OnGameStateChanged;
    private string message = "";
    private string winner;

    public enum GameState { Start, Wait, HostTurn, NotHostTurn, Victory, Lose }

    [SerializeField] private Vector3 P1Spawn1;
    [SerializeField] private Vector3 P1Spawn2;
    [SerializeField] private Vector3 P1Spawn3;
    [SerializeField] private Vector3 P2Spawn1;
    [SerializeField] private Vector3 P2Spawn2;
    [SerializeField] private Vector3 P2Spawn3;

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
        PhotonNetwork.AutomaticallySyncScene = false;
        LeaderboardManager = SendLeaderboard.GetComponent<PlayfabLeaderboardManager>();

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
        if (PhotonNetwork.IsMasterClient)
        {
            InstantiatePlayer("MasterPlayer1", P1Spawn1);
            InstantiatePlayer("MasterPlayer2", P1Spawn2);
            InstantiatePlayer("MasterPlayer3", P1Spawn3);
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerSelector"), new Vector3(0, 0, 0), Quaternion.identity, 0);
        }// scene.buildIndex == 3 && 
        else if (!PhotonNetwork.IsMasterClient)
        {
            InstantiatePlayer("Player1", P2Spawn1);
            InstantiatePlayer("Player2", P2Spawn2);
            InstantiatePlayer("Player3", P2Spawn3);
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerSelector"), new Vector3(0, 0, 0), Quaternion.identity, 0);
        }
    }

    private void InstantiatePlayer(string playerName, Vector3 spawn)
    {
        GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), spawn, Quaternion.identity);
        player.name = playerName;
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
                Victory();
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

    private void Victory()
    {
        ShowMessage(winner + " WINS!");
    }

    public void OnEvent(EventData photonEvent)
    {
        //WINNER EVENT - SENT BY LOSING PLAYER
        if (photonEvent.Code == WINNER)
        {
            StopAllCoroutines();
            object[] data = (object[])photonEvent.CustomData;
            //Index 0 - PLayer calling event, Index 1 - Other Player
            winner = data[1].ToString().Remove(0,4).Replace("'", "");
            message = data[0].ToString() + " King has been killed!";
            Debug.Log("Winner = " + winner);
            Debug.Log("PhotonNetwork.Nickname = " + PhotonNetwork.NickName);

            if (winner == PhotonNetwork.NickName)
                Debug.Log("It matches!");

            updateLeaderboard(winner);
            UpdateGameState(GameState.Victory);
        }
        else if (photonEvent.Code == QUIT)
        {
            StopAllCoroutines();
            object[] data = (object[])photonEvent.CustomData;
            //Index 0 - PLayer calling event, Index 1 - Other Player
            winner = data[1].ToString();
            updateLeaderboard(data[1].ToString());
            message = "Enemy Quit!";
            UpdateGameState(GameState.Victory);
        }
    }

    private void updateLeaderboard(String name)
    {
        if (PhotonNetwork.NickName == name)
        {
            LeaderboardManager.SendLeaderboard(1);
        }
    }

    //Show pop up message in game - text = msg shown, time = length of msg popup
    private IEnumerator ShowMessage(String text ,int time)
    {
        MessageText.text = text;
        ExtraMessageText.text = message;
        Overlay.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        Overlay.gameObject.SetActive(false);
    }

    //Show pop up message in game - text = msg shown, time = length of msg popup
    private void ShowMessage(String text)
    {
        MessageText.text = text;
        ExtraMessageText.text = message;
        Overlay.gameObject.SetActive(true);
    }

    public void LoadMenu()
    {
        //Time.timeScale = 1f;
        object[] quit = new object[] { PhotonNetwork.NickName, PhotonNetwork.PlayerListOthers[0].ToString() };
        PhotonNetwork.RaiseEvent(QUIT, quit, raiseEventOptions, SendOptions.SendReliable);
        SceneManager.LoadScene("ConnectLobby");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        object[] quit = new object[] { PhotonNetwork.NickName, PhotonNetwork.PlayerListOthers[0].ToString() };
        PhotonNetwork.RaiseEvent(QUIT, quit, raiseEventOptions, SendOptions.SendReliable);
        Application.Quit();


    }

}


