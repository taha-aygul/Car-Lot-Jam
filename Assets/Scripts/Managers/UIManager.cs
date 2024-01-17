using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    [SerializeField] GameObject levelSuccesfulUI;
    [SerializeField] private TextMeshProUGUI coinText,coinTextLevelSuccesful, levelText;
    
    ScoreManager scoreManager;
    public static UIManager Instance;


    void Awake()
    {
        MakeSingleton();
    }

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
    void Start()
    {
        scoreManager = ScoreManager.Instance;
        UpdateCoinText();
        UpdateLevelText();
    }


    public void LevelSuccesful()
    {
        levelSuccesfulUI.SetActive(true);
        coinTextLevelSuccesful.text = LevelGenerator.Instance.levelData.coin.ToString();
        scoreManager.GainCoin(LevelGenerator.Instance.levelData.coin);
    }

  
    public void UpdateCoinText()
    {
        coinText.text = scoreManager.CurrentCoin.ToString();
    }

    public void UpdateLevelText()
    {
        String name = SceneManager.GetActiveScene().name;
        name = name.Replace("Level", "Lv");
        levelText.text = name;
    }
}
