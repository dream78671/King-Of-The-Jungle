using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayfabManager : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public TMP_InputField RegisterEmailInput;
    public TMP_InputField RegisterPasswordInput;
    public TMP_InputField RegisterUsernameInput;
    public TMP_InputField LoginEmailInput;
    public TMP_InputField LoginPasswordInput;

    void OnSuccess(LoginResult result)
    {
        messageText.text = "Login Successful";
        Debug.Log("Successful login/account created");
    }

    void OnError(PlayFabError error)
    {
        messageText.text = error.ErrorMessage; 
        Debug.Log("Error while logging in/creating account");
        Debug.Log(error.GenerateErrorReport());
    }

    public void RegisterButton()
    {
        if(RegisterPasswordInput.text.Length < 6)
        {
            messageText.text = "Password too short (Minimum 6 Characters)";
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            Email = RegisterEmailInput.text,
            Password = RegisterPasswordInput.text,
            Username = RegisterUsernameInput.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }
    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        messageText.text = "Registered and Logged in!";
    }


    public void LoginButton()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = LoginEmailInput.text,
            Password = LoginPasswordInput.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnSuccess, OnError);

    }

    public void ResetPassword()
    {

    }

    void OnPasswordReset(SendAccountRecoveryEmailRequest result)
    {

    }



}
