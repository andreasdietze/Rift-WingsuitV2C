using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public static GameController instance = null;
    public string ip;
    public int currentScene;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        currentScene = 0;
    }

    void Update()
    {
        //controls
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            switch(currentScene)
            {
                case 0:
                    Application.Quit();
                    break;
                case 1:
                    SceneManager.LoadScene(0);
                    currentScene = 0;
                    break;
            }
        }



        //scenes
        switch(currentScene)
        {
            case 0:
                break;
            case 1:
                break;
        }
    }
}