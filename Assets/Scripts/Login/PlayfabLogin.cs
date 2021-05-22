using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using SceneManager = UnityEngine.SceneManagement.SceneManager;
using TMP_InputField = TMPro.TMP_InputField;

public class PlayfabLogin : MonoBehaviour
{
    private void Start()
    {
        if (PlayerPrefs.HasKey("USERNAME"))
        {
            _loginUsernameField.text = PlayerPrefs.GetString("USERNAME");
        }
    }

    #region Playfab Register

    public void Register(string username, string email, string password)
    {
        var req = new RegisterPlayFabUserRequest
        {
            Username = username,
            Email = email,
            Password = EncryptPassword.Encrypt(password)
        };

        PlayFabClientAPI.RegisterPlayFabUser(request: req, resultCallback: OnRegisterRequestSuccess, errorCallback: OnRegisterRequestFailure);
    }

    private void OnRegisterRequestSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log(string.Format("Success registering from {0}", result.Username ));
        PlayerPrefs.SetString("USERNAME", result.Username);
        LoginSuccess();
    }

    private void OnRegisterRequestFailure(PlayFabError error)
    {
        Debug.LogError(error.ErrorMessage);
    }

    #endregion

    #region Playfab Login

    public void Login(string username, string password)
    {
        var req = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = EncryptPassword.Encrypt(password)
        };

        PlayFabClientAPI.LoginWithPlayFab(request: req, resultCallback: OnLoginRequestSuccess, errorCallback: OnLoginRequestFailure);
    }

    private void OnLoginRequestSuccess(LoginResult result)
    {
        Debug.Log("Login succesfull");
        LoginSuccess();
    }

    private void OnLoginRequestFailure(PlayFabError error)
    {
        Debug.LogError(error.ErrorMessage);
    }

    #endregion

    #region Authenticated

    private void LoginSuccess()
    {
        SceneManager.LoadScene("Start");
    }

    #endregion

    #region UI Functions

    private const int LoginPanel = 0;
    private const int RegisterPanel = 1;

    [SerializeField] private TMP_InputField _loginUsernameField, _loginPasswordField,
        _registerUsernameField, _registerEmailField, _registerPasswordField;

    [SerializeField] private GameObject _loginPanel, _registerPanel;

    public void OnClickChangePanel(int panel)
    {
        switch (panel)
        {
            case LoginPanel:
                _loginPanel.SetActive(true);
                _registerPanel.SetActive(false);
                break;
            case RegisterPanel:
                _loginPanel.SetActive(false);
                _registerPanel.SetActive(true);
                break;
        }
    }

    public void OnClickRegister()
    {
        Register(username: _registerUsernameField.text, email: _registerEmailField.text, password: _registerPasswordField.text);
    }

    public void OnClickLogin()
    {
        Login(username: _loginUsernameField.text, password: _loginPasswordField.text);
    }

    #endregion
}
