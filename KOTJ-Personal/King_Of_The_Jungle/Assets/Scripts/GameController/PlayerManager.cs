using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO; 

public class PlayerManager : MonoBehaviourPunCallbacks
{
    PhotonView PV;

    public float spawn1x;
    public float spawn1y;
    public float spawn2x;
    public float spawn2y;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (PV.IsMine)
        {
            CreatePlayer();
        }
    }

    void CreatePlayer()
    {
        //Added Z value of -8 so they show on the map
        if (PhotonNetwork.IsMasterClient) //if host, spawn on left
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(spawn1x, spawn1y, -8), Quaternion.identity);
        }
        else //if not host, spawn on right
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), new Vector3(spawn2x, spawn2y, -8), Quaternion.identity);
        }
    }
}
