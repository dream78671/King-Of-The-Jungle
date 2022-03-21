using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class PlayfabLeaderboardManager : MonoBehaviour
{

    public GameObject LeaderboardEntry;
    public Transform TableParent;

    void OnError(PlayFabError error)
    {
        Debug.Log("Error while accessing Leaderboard");
        Debug.Log(error.GenerateErrorReport());
    }

    public void SendLeaderboard(int wins)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "KOTJ-Total-Wins",
                    Value = wins
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successful leaderboard sent");
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "KOTJ-Total-Wins",
            StartPosition = 0,
            MaxResultsCount = 10,
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardGet(GetLeaderboardResult result) {
        foreach (Transform item in TableParent){
            Destroy(item.gameObject);
        }

        foreach (var item in result.Leaderboard)
        {
            GameObject newGo = Instantiate(LeaderboardEntry, TableParent);
            Text[] text = newGo.GetComponentsInChildren<Text>();
            text[0].text = (item.Position + 1).ToString();
            text[1].text = item.DisplayName;
            text[2].text = item.StatValue.ToString();

            Debug.Log(item.Position + " " + item.DisplayName + " " + item.StatValue);
        };
    }
}
