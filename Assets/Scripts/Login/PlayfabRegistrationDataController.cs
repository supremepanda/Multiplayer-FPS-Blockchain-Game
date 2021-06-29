using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using SmartContract;
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
                StartCoroutine(TokenDeployAndSend.Transaction("https://ropsten.infura.io/v3/4394d608f8694f62ac54a673f7940e11",
                    "622bdcf3915f11859a8657af0aa0dea840fbbf52c9fb9607adfa156f18f734e1",
                    "0x88144534Bd291b9c3D7BDB9A92D7270566f5622d", 100, "0x3ad4016c64A0B4601c873861597033f6e76efE7A", _address, true));
                //LoadGame();


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
