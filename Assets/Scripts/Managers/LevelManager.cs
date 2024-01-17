
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public int CurrentLevel { get; private set; }
    public static LevelManager Instance;

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
        MakeSingleton();
    }
    private void Start()
    {
        CurrentLevel = SceneManager.GetActiveScene().buildIndex;
    }

    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadLevel(int sceneIndex)
    {
        if (!(sceneIndex >= SceneManager.sceneCountInBuildSettings))
        {
            CurrentLevel = sceneIndex;
            SceneManager.LoadScene(sceneIndex);
        }
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        CurrentLevel++;
        if (CurrentLevel >= SceneManager.sceneCountInBuildSettings)
        {
            CurrentLevel = 0;
        }
        else
        {
            SceneManager.LoadScene(CurrentLevel);
            UIManager.Instance.UpdateLevelText();
        }
    }

    public void LoadLevelWithIndex(int index)
    {
        SceneManager.LoadScene(index);
        CurrentLevel = SceneManager.GetSceneByBuildIndex(index).buildIndex;

        CurrentLevel++;
        UIManager.Instance.UpdateLevelText();

    }



}
