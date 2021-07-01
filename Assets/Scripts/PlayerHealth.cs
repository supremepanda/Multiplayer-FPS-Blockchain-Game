using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;
using Nethereum.RPC.Eth;
using PlayFab;
using PlayFab.ClientModels;
using SmartContract;

[RequireComponent(typeof(FirstPersonController))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerHealth : MonoBehaviourPunCallbacks, IPunObservable {
    public delegate void Respawn(float time);
    public delegate void AddMessage(string Message);
    public event Respawn RespawnEvent;
    public event AddMessage AddMessageEvent;
   // public EtherTransferCoroutinesUnityWebRequest eth;
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

    private EtherTransferCoroutinesUnityWebRequest eth;
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

        /*EtherTransferCoroutinesUnityWebRequest eth = GameObject.FindGameObjectWithTag("ETH")
            .GetComponent<EtherTransferCoroutinesUnityWebRequest>(); */

        eth = FindObjectOfType<EtherTransferCoroutinesUnityWebRequest>();


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
                /*StartCoroutine(Transaction("https://ropsten.infura.io/v3/4394d608f8694f62ac54a673f7940e11",
                    "622bdcf3915f11859a8657af0aa0dea840fbbf52c9fb9607adfa156f18f734e1",
                    "0x88144534Bd291b9c3D7BDB9A92D7270566f5622d", 100, "0x3ad4016c64a0b4601c873861597033f6e76efe7a", "0x6E603794Ac88E8a4Ebc978671384329aaD1ADd18"));
                */

                //Handle AccountInfo
                Debug.Log(result.AccountInfo.PlayFabId);
                winnerPID = result.AccountInfo.PlayFabId;
                GetUserData(winnerPID, adress =>
                {

                   /* StartCoroutine(Transaction("https://ropsten.infura.io/v3/4394d608f8694f62ac54a673f7940e11",
                        "622bdcf3915f11859a8657af0aa0dea840fbbf52c9fb9607adfa156f18f734e1",
                        "0x88144534Bd291b9c3D7BDB9A92D7270566f5622d", 100, "0x3ad4016c64a0b4601c873861597033f6e76efe7a", "0x6E603794Ac88E8a4Ebc978671384329aaD1ADd18"));
                    */
                   Debug.Log(adress);
                   /*StartCoroutine(
                        TokenDeployAndSend.Transaction(
                            "https://ropsten.infura.io/v3/4394d608f8694f62ac54a673f7940e11",_playfabUser.Instance.PrivateKey,
                            _playfabUser.Instance.Address,10,"0x3ad4016c64A0B4601c873861597033f6e76efE7A",adress, false));*/
                   
                   StartCoroutine(TokenDeployAndSend.Transaction("https://ropsten.infura.io/v3/4394d608f8694f62ac54a673f7940e11",
                       "622bdcf3915f11859a8657af0aa0dea840fbbf52c9fb9607adfa156f18f734e1",
                       "0x88144534Bd291b9c3D7BDB9A92D7270566f5622d", 10, "0x3ad4016c64A0B4601c873861597033f6e76efE7A", adress, false));

                    //eth.TransferRequest();
                });


            },
            error => { Debug.LogError(error.GenerateErrorReport()); });

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
    void GetUserData(string winnerPID, System.Action<string> callback) {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest {

            PlayFabId = winnerPID,
            Keys = null
        }, result => {
            Debug.Log("Got user data:");
            Debug.Log(result.Data["address"].Value);
            Debug.Log(result.Data["privateKey"].Value);

            Debug.Log(result.Data.ContainsKey("address"));
            Debug.Log(result.Data.ContainsKey("privateKey"));
            if (result.Data == null || !result.Data.ContainsKey("address") || !result.Data.ContainsKey("privateKey")) Debug.Log("No address");
            else
            {
                Debug.Log("Icerde");
                winnerAdrs = result.Data["address"].Value;
                Debug.Log($"Winner adress:{winnerAdrs}");
                callback(winnerAdrs);
                
            }

        }, (error) => {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
    }
}
