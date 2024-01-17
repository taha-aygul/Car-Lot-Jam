using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int CurrentCoin { get; private set; }
    public int coinGainedThisRound = 0;

    public static ScoreManager Instance;
    private void MakeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    private void Awake()
    {
        coinGainedThisRound = 0;
        CurrentCoin = PlayerPrefs.GetInt("Coin");
        MakeSingleton();
    }
    public void GainCoin(int amount)
    {
        coinGainedThisRound += amount;
        CurrentCoin += amount;
        PlayerPrefs.SetInt("Coin", CurrentCoin);
        UIManager.Instance.UpdateCoinText();
    }
   
}
