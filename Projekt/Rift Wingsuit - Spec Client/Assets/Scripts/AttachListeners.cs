using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AttachListeners : MonoBehaviour {

    void Awake()
    {
        Button confirm = GameObject.Find("Confirm").GetComponent<Button>();
        Button skip = GameObject.Find("Skip").GetComponent<Button>();
        InputField input = GameObject.Find("InputField").GetComponent<InputField>();
        confirm.onClick.AddListener(() => { OnConfirm(); });
        skip.onClick.AddListener(() => { OnSkip(); });
        input.onEndEdit.AddListener(delegate { GetInputIP(input); });
    }

    void OnConfirm()
    {
        SceneManager.LoadScene(1);
        GameController.instance.currentScene = 1;
    }

    void OnSkip()
    {
        GameController.instance.ip = null;
        SceneManager.LoadScene(1);
        GameController.instance.currentScene = 1;
    }

    void GetInputIP(InputField input)
    {
        Debug.Log("You entered IP: " + input.text);
        GameController.instance.ip = input.text;
    }
}
