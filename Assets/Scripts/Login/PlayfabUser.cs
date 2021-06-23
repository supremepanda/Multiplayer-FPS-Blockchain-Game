using System;
using UnityEngine;

public class PlayfabUser : MonoBehaviour
{
    private static PlayfabUser instance = null;
    public static PlayfabUser Instance
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
    public string Adress { get; set; }
    public string PrivateKey { get; set; }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            DontDestroyOnLoad(this);
        }
    }
}
