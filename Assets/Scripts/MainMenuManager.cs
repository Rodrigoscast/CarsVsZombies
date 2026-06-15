using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string NameGamelevel;
    [SerializeField] private string MainMenuName;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) &&
            SceneManager.GetActiveScene().name != MainMenuName)
        {
            BackToMenu();
        }
    }

    public void Play()
    {
        SceneManager.LoadScene(NameGamelevel);
    }

    public void Exit()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(MainMenuName);
    }
}