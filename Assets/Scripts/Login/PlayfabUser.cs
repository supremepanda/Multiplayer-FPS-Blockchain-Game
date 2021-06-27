using System;
using UnityEngine;

public class PlayfabUser : MonoBehaviour
{
    private PlayfabUser instance = null;
    public PlayfabUser Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlayfabUser>();
                if (instance == null)
                {
                    instance = new GameObject().AddComponent<PlayfabUser>();
                }
            }

            return instance;
        }
    }

    public string PlayfabId { get; set; }
    public string Address { get; set; }
    public string PrivateKey { get; set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }

        instance = this;
        DontDestroyOnLoad( this.gameObject );
    }
}
