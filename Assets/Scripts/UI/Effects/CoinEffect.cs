using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinEffect : MonoBehaviour
{
    public static CoinEffect Instance;
    private Stack<GameObject> coinEffect = new Stack<GameObject>();
    private GameObject coinObj;

    void Awake()
    {
        Instance  = this;
        coinObj = Resources.Load<GameObject>(Constants.COIN_EFFECT_PATH);
    }

    private void InitPool(int numsCoin)
    {
        for(int i = 0; i < numsCoin; i++)
        {
            GameObject newCoin = Instantiate(coinObj, this.transform);
            newCoin.SetActive(false);
            coinEffect.Push(newCoin);   
        }
    }

    public GameObject GetCoin()
    {
        if (coinEffect.Count == 0) 
        {
            InitPool(10);
        }

        GameObject coin = coinEffect.Pop();
        coin.SetActive(true);             
        return coin;
    }

    public void ReturnCoin(GameObject coin)
    {
        coin.SetActive(false); // Tắt đi
        coinEffect.Push(coin);    // Quăng lại vào kho
    }

    
}
