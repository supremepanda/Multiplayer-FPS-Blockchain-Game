using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.RPC.Eth.DTOs;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.RPC.Eth;
using Nethereum.Util;
using Nethereum.Web3;

[System.Serializable]
public class WalletData
{
    public static WalletData Instance;
    public string name;
    public string address;
    public string cachedPassword;
    public string encryptedJson;
    public string privateKey;
}

public class TokenRequests : MonoBehaviour
{
    private const string Url = "https://mainnet.infura.io/v3/4394d608f8694f62ac54a673f7940e11";
    
    private void Start()
    {
        StartCoroutine(CreateAccountCoroutine("furkan", "panda"));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            //StartCoroutine(TransferEther(Url, WalletData.Instance.privateKey,
                //"0x61253F6Ab5ceA82B01db64B042461341d1ED0A0A", "0.1"));
            GetBalance(WalletData.Instance.address);
        }
    }

    #region Wallet Creation

    public IEnumerator CreateAccountCoroutine(string password, string accountName)
    {
        yield return 0; // allow UI to update

        CreateAccount(password, (address, encryptedJson, privateKey) =>
        {
            Debug.Log(address);
            //Debug.Log(encryptedJson);
            //Debug.Log(privateKey);
            //Debug.Log(accountName);

            WalletData.Instance = new WalletData();
            
            WalletData.Instance.name = accountName;
            WalletData.Instance.address = address;
            WalletData.Instance.cachedPassword = password;
            WalletData.Instance.encryptedJson = encryptedJson;
            WalletData.Instance.privateKey = privateKey;
        });
    }
    
    private void CreateAccount(string password, System.Action<string, string, string> callback)
    {
        // We use the Nethereum.Signer to generate a new secret key
        var ecKey = Nethereum.Signer.EthECKey.GenerateKey();

        // After creating the secret key, we can get the public address and the private key with
        // ecKey.GetPublicAddress() and ecKey.GetPrivateKeyAsBytes()
        // (so it return it as bytes to be encrypted)
        var address = ecKey.GetPublicAddress();
        var privateKeyBytes = ecKey.GetPrivateKeyAsBytes();
        var privateKey = ecKey.GetPrivateKey();

        // Then we define a new KeyStore service
        var keystoreservice = new Nethereum.KeyStore.KeyStoreService();

        // And we can proceed to define encryptedJson with EncryptAndGenerateDefaultKeyStoreAsJson(),
        // and send it the password, the private key and the address to be encrypted.
        var encryptedJson = keystoreservice.EncryptAndGenerateDefaultKeyStoreAsJson(password, privateKeyBytes, address);
        // Finally we execute the callback and return our public address and the encrypted json.
        // (you will only be able to decrypt the json with the password used to encrypt it)
        callback(address, encryptedJson, privateKey);
    }

    #endregion

    #region Transfer Token

    public IEnumerator TransferEther(string url, string privateKey, string addressTo, string amountText)
    {
        var amount = System.Decimal.Parse(amountText);
        var ethTransfer = new EthTransferUnityRequest(url, privateKey);
        var receivingAddress = addressTo;
        var gasPriceGwei = 2;
        
        yield return ethTransfer.TransferEther(receivingAddress, amount, gasPriceGwei);

        if (ethTransfer.Exception != null)
        {
            Debug.Log(ethTransfer.Exception.Message);
            yield break;
        }

        var transactionHash = ethTransfer.Result;

        Debug.Log("Transfer transaction hash:" + transactionHash);

        //create a poll to get the receipt when mined
        var transactionReceiptPolling = new TransactionReceiptPollingRequest(url);
        //checking every 2 seconds for the receipt
        yield return transactionReceiptPolling.PollForReceipt(transactionHash, 2);
        
        Debug.Log("Transaction mined");

        var balanceRequest = new EthGetBalanceUnityRequest(url);
        yield return balanceRequest.SendRequest(receivingAddress, BlockParameter.CreateLatest());
        
        var balanceAddressTo = UnitConversion.Convert.FromWei(balanceRequest.Result.Value);

        Debug.Log("Balance of account:" + balanceAddressTo);
    }

    #endregion

    #region GetBalance

    public async void GetBalance(string address)
    {
        //var publicKey = address;
        //var web3 = new Web3(Url);
        ////var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(publicKey);
        //var balance = await web3.Eth.GetBalance.SendRequestAsync(publicKey);
        //var etherAmount = Web3.Convert.FromWei(balance.Value);
        //
        //Console.WriteLine(web3);
        //Console.WriteLine("Get txCount " + etherAmount);
        //Console.ReadLine();
    }
    
    #endregion
}
