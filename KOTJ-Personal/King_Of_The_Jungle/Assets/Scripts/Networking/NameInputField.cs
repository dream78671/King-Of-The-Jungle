using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NameInputField : MonoBehaviourPunCallbacks
{

    //Sets players name, if empty, logs error and doesn't set NickName
    public void SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("Player Name is empty");
            return;
        }

        PhotonNetwork.NickName = name; 
    }
}
