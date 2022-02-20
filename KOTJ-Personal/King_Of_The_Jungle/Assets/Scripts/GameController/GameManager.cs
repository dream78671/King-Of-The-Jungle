using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public GameState State; 

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

    public void UpdateGameState(GameState newState)
    {
        State = newState;  

        switch (newState)
        {
            case GameState.StartGame:
                break;
            case GameState.HostTurn:
                break;
            case GameState.NotHostTurn:
                break;
            case GameState.Victory:
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null); 
        }
    }

}


public enum GameState
{
    StartGame,
    HostTurn,
    NotHostTurn,
    Victory,
    Lose
}