using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSelector : MonoBehaviour
{

    private PhotonView PV;
    public GameObject[] Players;

    [SerializeField] private GameObject CurrentPlayer;
    private int currentIndex = 0;


    // Start is called before the first frame update
    void Awake()
    {
        PV = GetComponent<PhotonView>();

        if (!PV.IsMine)
        {
            Destroy(gameObject);
        }

        if (PV.IsMine && PhotonNetwork.IsMasterClient)
        {
            Players = GameObject.FindGameObjectsWithTag("MasterPlayer");
        }
        else
        {
            Players = GameObject.FindGameObjectsWithTag("Player");
        }

        CurrentPlayer = Players[currentIndex];
    }

    public string ChangePlayer(int playerIndex)
    {
        if (playerIndex == -1)
            Debug.Log("Error");

        if (playerIndex == currentIndex)
            return "Player already Selected";
        else if (Players[playerIndex] == null)
            return "Dead";

        CurrentPlayer.GetComponent<PlayerController>().canMove = false;
        CurrentPlayer.GetComponent<PlayerController>().canShoot = false;

        currentIndex = playerIndex;
        CurrentPlayer = Players[currentIndex];

        CurrentPlayer.GetComponent<PlayerController>().canMove = true;
        CurrentPlayer.GetComponent<PlayerController>().canShoot = true;

        return "Player Changed";
    }

    public int NextUsablePlayer()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i] != null)
                return i;
        }

        return -1;
    }
}