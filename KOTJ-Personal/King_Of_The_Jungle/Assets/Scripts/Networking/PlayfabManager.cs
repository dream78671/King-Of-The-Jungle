using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayfabManager : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public TMP_InputField RegisterEmailInput;
    public TMP_InputField RegisterPasswordInput;
    public TMP_InputField RegisterUsernameInput;
    public TMP_InputField LoginEmailInput;
    public TMP_InputField LoginPasswordInput;

    private string localID = "NotSet";

    void OnError(PlayFabError error)
    {
        messageText.text = error.ErrorMessage; 
        Debug.Log("Error while logging in/creating account");
        Debug.Log(error.GenerateErrorReport());
    }

    public void RegisterButton()
    {
        if (RegisterPasswordInput.text.Length < 6)
        {
            messageText.text = "Password too short (Minimum 6 Characters)";
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            Email = RegisterEmailInput.text,
            Password = RegisterPasswordInput.text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
        
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        messageText.text = "Registered and Logged in!";
        var nameRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = RegisterUsernameInput.text
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(nameRequest, OnDisplaNameUpdate, OnError); PlayFabClientAPI.UpdateUserTitleDisplayName(nameRequest, OnDisplaNameUpdate, OnError);
        GetPlayerProfile(result.PlayFabId);
        PlayerPrefs.SetString("PlayFabID", result.PlayFabId);
        StartGame();
    }

    void OnDisplaNameUpdate(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Player display name set - " + result.DisplayName);
    }

    public void LoginButton()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = LoginEmailInput.text,
            Password = LoginPasswordInput.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);

    }

    void OnLoginSuccess(LoginResult result)
    {
        PlayerPrefs.SetString("PlayFabID", result.PlayFabId);
        GetPlayerProfile(result.PlayFabId);
        Debug.Log("Successful login");
        StartGame();
    }

    void GetPlayerProfile(string playFabId)
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
        {
            PlayFabId = playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowDisplayName = true
            }
        },
        result => PlayerPrefs.SetString("PlayerName", result.PlayerProfile.DisplayName),
        error => Debug.LogError(error.GenerateErrorReport()));

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
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate , OnError); 
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successful leaderboard sent");
    }

    void StartGame()
    {
        SceneManager.LoadScene("ConnectLobby");
    }

}
