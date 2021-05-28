using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;
using TMP_InputField = TMPro.TMP_InputField;

namespace Login
{
    public class PlayfabLogin : MonoBehaviour
    {
        private const int LoginPanel = 0;
        private const int RegisterPanel = 1;

        [SerializeField] private TMP_InputField loginUsernameField, loginPasswordField,
            registerUsernameField, registerEmailField, registerPasswordField;

        [SerializeField] private GameObject loginPanel, registerPanel;

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
            Debug.Log($"Success registering from {result.Username}");
            PlayerPrefs.SetString("USERNAME", result.Username);
            LoginSuccess();
        }

        private void OnRegisterRequestFailure(PlayFabError error)
        {
            Debug.LogError(error.ErrorMessage);
        }

        #endregion

        #region Playfab Login

        private string GetSavedUsername()
        {
            return PlayerPrefs.GetString("USERNAME");
        }
    
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
        public void OnClickChangePanel(int panel)
        {
            switch (panel)
            {
                case LoginPanel:
                    loginPanel.SetActive(true);
                    registerPanel.SetActive(false);
                    break;
                case RegisterPanel:
                    loginPanel.SetActive(false);
                    registerPanel.SetActive(true);
                    break;
            }
        }

        public void OnClickRegister()
        {
            Register(username: registerUsernameField.text, email: registerEmailField.text, password: registerPasswordField.text);
        }

        public void OnClickLogin()
        {
            Login(username: loginUsernameField.text, password: loginPasswordField.text);
        }

        #endregion
    
        private void Start()
        {   
            loginUsernameField.text = GetSavedUsername();
        }
    }
}
