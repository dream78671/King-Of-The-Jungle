using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    [SerializeField] private TextMeshProUGUI WaitText;

    public GameState State;
    public static event Action<GameState> OnGameStateChanged;

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

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //If scene has loaded, Create a new playerManager object for each player
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.buildIndex == 1)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }

    //Function to chnage GameStates
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
        WaitText.gameObject.SetActive(true);
        Debug.Log("Wait Called");

        yield return new WaitForSeconds(3);
        WaitText.gameObject.SetActive(false);

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

    private IEnumerator NotHostTurn()
    {
        Debug.Log("NotHostTurn");

        yield return new WaitForSeconds(timePerMove);

        UpdateGameState(GameState.Wait);
    }

    private IEnumerator HostTurn()
    {
        Debug.Log("HostTurn");

        yield return new WaitForSeconds(timePerMove);

        UpdateGameState(GameState.Wait);
    }
}


public enum GameState
{
    Wait,
    HostTurn,
    NotHostTurn,
    Victory,
    Lose
}