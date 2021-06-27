using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;
using Nethereum.RPC.Eth;
using PlayFab;
using PlayFab.ClientModels;
[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerHealth : MonoBehaviourPunCallbacks, IPunObservable {
    public delegate void Respawn(float time);
    public delegate void AddMessage(string Message);
    public event Respawn RespawnEvent;
    public event AddMessage AddMessageEvent;
    public EtherTransferCoroutinesUnityWebRequest eth;
    [SerializeField]
    private int startingHealth = 100;
    [SerializeField]
    private float sinkSpeed = 0.12f;
    [SerializeField]
    private float sinkTime = 2.5f;
    [SerializeField]
    private float respawnTime = 8.0f;
    [SerializeField]
    private AudioClip deathClip;
    [SerializeField]
    private AudioClip hurtClip;
    [SerializeField]
    private AudioSource playerAudio;
    [SerializeField]
    private float flashSpeed = 2f;
    [SerializeField]
    private Color flashColour = new Color(1f, 0f, 0f, 0.1f);
    [SerializeField]
    private NameTag nameTag;
    [SerializeField]
    private Animator animator;
    private FirstPersonController fpController;
    private IKControl ikControl;
    private Slider healthSlider;
    private Image damageImage;
    private int currentHealth;
    private bool isDead;
    private bool isSinking;
    private bool damaged;
    private string winnerPID;
    private string winnerAdrs;
    private string winnerName;
    private PlayfabUser _playfabUser;
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        winnerPID = "";
        winnerAdrs = "";
        winnerName = "";


        _playfabUser = FindObjectOfType<PlayfabUser>();


        Debug.Log(_playfabUser.Instance.Address + "\n" + _playfabUser.Instance.PrivateKey);

        EtherTransferCoroutinesUnityWebRequest eth = GameObject.FindGameObjectWithTag("ETH")
            .GetComponent<EtherTransferCoroutinesUnityWebRequest>();
        fpController = GetComponent<FirstPersonController>();
        ikControl = GetComponentInChildren<IKControl>();
        damageImage = GameObject.FindGameObjectWithTag("Screen").transform.Find("DamageImage").GetComponent<Image>();
        healthSlider = GameObject.FindGameObjectWithTag("Screen").GetComponentInChildren<Slider>();
        currentHealth = startingHealth;
        if (photonView.IsMine) {
            gameObject.layer = LayerMask.NameToLayer("FPSPlayer");
            healthSlider.value = currentHealth;
        }
        damaged = false;
        isDead = false;
        isSinking = false;
    }
    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update() {
        if (damaged) {
            damaged = false;
            damageImage.color = flashColour;
        } else {
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
        if (isSinking) {
            transform.Translate(Vector3.down * sinkSpeed * Time.deltaTime);
        }
    }
    /// <summary>
    /// RPC function to let the player take damage.
    /// </summary>
    /// <param name="amount">Amount of damage dealt.</param>
    /// <param name="enemyName">Enemy's name who cause this player's death.</param>
    [PunRPC]
    public void TakeDamage(int amount, string enemyName) {
        if (isDead) return;
        if (photonView.IsMine) {
            damaged = true;
            currentHealth -= amount;
            if (currentHealth <= 0) {
                photonView.RPC("Death", RpcTarget.All, enemyName);
            }
            healthSlider.value = currentHealth;
            animator.SetTrigger("IsHurt");
        }
        playerAudio.clip = hurtClip;
        playerAudio.Play();
    }
    /// <summary>
    /// RPC function to declare death of player.
    /// </summary>
    /// <param name="enemyName">Enemy's name who cause this player's death.</param>
    [PunRPC]
    void Death(string enemyName)
    {
        isDead = true;
        ikControl.enabled = false;
        nameTag.gameObject.SetActive(false);
        if (photonView.IsMine) {
            fpController.enabled = false;
            animator.SetTrigger("IsDead");
            AddMessageEvent(PhotonNetwork.LocalPlayer.NickName + " was killed by " + enemyName + "!");
            RespawnEvent(respawnTime);
            StartCoroutine("DestoryPlayer", respawnTime);
        }
        playerAudio.clip = deathClip;
        playerAudio.Play();
        StartCoroutine("StartSinking", sinkTime);
        winnerName = enemyName;
        sendETH();
    }
    public void sendETH()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { Username = winnerName},
            result =>
            {
                //Handle AccountInfo
                Debug.Log(result.AccountInfo.PlayFabId);
                winnerPID = result.AccountInfo.PlayFabId;
            },
            error => { Debug.LogError(error.GenerateErrorReport()); });
        GetUserData(winnerPID);
        eth.PrivateKey = _playfabUser.Instance.PrivateKey;
        eth.AddressTo = winnerAdrs;
        eth.Url = "https://ropsten.infura.io/v3/64941807daee4f26864ec8e8d1a12620";
        eth.Amount = 0.1m;
        
        Debug.Log(" Killer = " +  winnerName + "\n" +
                  "Dead = " + _playfabUser.Instance.PrivateKey.ToString() + "\n" +
                  " eth.PrivateKey = " +  eth.PrivateKey.ToString() + "\n" +
                  "eth.AddressTo = " + eth.AddressTo.ToString() + "\n" +
                  "eth.Url = " +  eth.Url + "\n" +
                  "eth.Amount" + eth.Amount + "\n" +
                  "eth.GasPriceGwei = " + eth.GasPriceGwei + "\n" );
        eth.TransferRequest();
    }
    /// <summary>
    /// Coroutine function to destory player game object.
    /// </summary>
    /// <param name="delayTime">Delay time before destory.</param>
    IEnumerator DestoryPlayer(float delayTime) {
        yield return new WaitForSeconds(delayTime);
        PhotonNetwork.Destroy(gameObject);
    }
    /// <summary>
    /// RPC function to start sinking the player game object.
    /// </summary>
    /// <param name="delayTime">Delay time before start sinking.</param>
    IEnumerator StartSinking(float delayTime) {
        yield return new WaitForSeconds(delayTime);
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = false;
        isSinking = true;
    }
    /// <summary>
    /// Used to customize synchronization of variables in a script watched by a photon network view.
    /// </summary>
    /// <param name="stream">The network bit stream.</param>
    /// <param name="info">The network message information.</param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(currentHealth);
        } else {
            currentHealth = (int)stream.ReceiveNext();
        }
    }
    void GetUserData(string winnerPID) {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest() {
            PlayFabId = winnerPID,
            Keys = null
        }, result => {
            Debug.Log("Got user data:");
            if (result.Data == null || !result.Data.ContainsKey("address") || !result.Data.ContainsKey("privateKey")) Debug.Log("No address");
            else //Debug.Log("address: "+result.Data["address"].Value);
            winnerAdrs = result.Data["address"].Value;
        }, (error) => {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
    }
}
