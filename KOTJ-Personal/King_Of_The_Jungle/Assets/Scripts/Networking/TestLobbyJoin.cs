using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class TestLobbyJoin : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject ConnectPanel;
    [SerializeField] private TextMeshProUGUI StatusText;

    private bool isConnecting = false;
    private const string gameVersion = "1.0";

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; //Syncs master scene to everyone else
    }


    //Called when Connect Button is clicked
    public void Connect()
    {
        isConnecting = true;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
        }
        else //Connect to Photon Servers
        {
            ShowStatus("Connecting...");
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }


    private void ShowStatus(string text)
    {
        if (StatusText == null)
        {
            return; //do nothing
        }

        //Show the status message and update it with the text passed
        StatusText.gameObject.SetActive(true);
        StatusText.text = text;
    }

    //If connect Button clicked and Photon server joined, Join a random room 
    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            ShowStatus("Connected, joining room...");
            PhotonNetwork.JoinRandomRoom();
        }
    }

    //If no room available, create new room
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        ShowStatus("Creating a new room...");
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
    }

    //If Disconnected or no room made/found, take back to Lobby
    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;
        ConnectPanel.SetActive(true);
    }

    //When room has been joined
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game2");
    }

    //Once 2 players in a room, master client changes everyone to the game scene
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1 && PhotonNetwork.IsMasterClient)
        {
            //Used instead of SceneManager.LoadScene, Using PhotonsLoadLevel ensures all players load into the new scene. Look at Awake()
            PhotonNetwork.LoadLevel("Game2");
        }
    }


}
