using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayfabRegistrationDataController : MonoBehaviour
{
    [SerializeField] private GameObject tokenRegistrationPanel;

    private PlayfabUser _playfabUser;

    private string _address;
    private string _privateKey;

    public void ChangeAddress(string value)
    {
        _address = value;
    }

    public void ChangePrivateKey(string value)
    {
        _privateKey = value;
    }

    private void GetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest
        {
            PlayFabId = _playfabUser.Instance.PlayfabId,
            Keys = null
        }, result => {
            Debug.Log("Got user data:");
            if (result.Data == null || !result.Data.ContainsKey("address") || !result.Data.ContainsKey("privateKey"))
            {
                Debug.Log("No data");
                EnableTokenRegistration();
            }
            else
            {
                Debug.Log(result.Data);
                _playfabUser.Instance.Address = result.Data["address"].Value;
                _playfabUser.Instance.PrivateKey = result.Data["privateKey"].Value;
                LoadGame();
            }

        }, (error) => {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
    }

    public void UpdateUserData()
    {
        if (_address.Length != 42)
        {
            Debug.Log("Invalid address");
            return;
        }

        if (_privateKey.Length != 64)
        {
            Debug.Log("Invalid private key");
            return;
        }

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
            {
                Data = new Dictionary<string, string> {
                    {"address", _address },
                    {"privateKey", _privateKey}
                }
            },
            result =>
            {
                _playfabUser.Instance.Address = _address;
                _playfabUser.Instance.PrivateKey = _privateKey;
                LoadGame();
            },
            error => {
                Debug.Log("Got error setting user data Ancestor to Arthur");
                Debug.Log(error.GenerateErrorReport());
            });
    }

    private void EnableTokenRegistration()
    {
        tokenRegistrationPanel.SetActive(true);
    }

    private void LoadGame()
    {
        SceneManager.LoadScene("Start");
    }

    private void Start()
    {
        _playfabUser = FindObjectOfType<PlayfabUser>();
        GetUserData();
    }
}
