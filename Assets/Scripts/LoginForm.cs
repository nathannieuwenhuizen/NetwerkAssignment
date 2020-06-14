using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginForm : MonoBehaviour
{
    [SerializeField]
    private InputField passwordField;
    [SerializeField]
    private InputField emailField;

    [SerializeField]
    private Text message;

    [SerializeField]
    private GameObject registerField;

    [SerializeField]
    private GameObject anonymousButton;

    [SerializeField]
    private GameObject menu;



    public void Start()
    {
        StartCoroutine(LoggingIntoServer());
    }

    public void LogInUser()
    {
        string url = "user_login.php?session_id=" + DataBaseHandeler.sessionID + "&password=" + passwordField.text + "&email=" + emailField.text;
        StartCoroutine(LoggingIn(url));
    }

    public IEnumerator LoggingIn(string url)
    {
        yield return StartCoroutine(DataBaseHandeler.GetHttp(url));

        if (DataBaseHandeler.response == 0 + "")
        {
            message.text = "invalid email or username";
        }
        else
        {
            LoginData loginData = new LoginData();
            loginData = JsonUtility.FromJson<LoginData>(DataBaseHandeler.response);

            Debug.Log(loginData.id);
            Debug.Log(loginData.first_name);
            DataBaseHandeler.userID = loginData.id;
            DataBaseHandeler.userNickname = loginData.first_name + " " + loginData.last_name;
            
            GoToMenu();
        }
    }


    public IEnumerator LoggingIntoServer()
    {
        yield return StartCoroutine(DataBaseHandeler.GetHttp("server_login.php?id=" + DataBaseHandeler.serverID + "&password=" + DataBaseHandeler.serverPassword));
        DataBaseHandeler.sessionID = DataBaseHandeler.response;
        Debug.Log("session id: " + DataBaseHandeler.sessionID);
    }

    public void GoToMenu()
    {
        registerField.SetActive(false);
        anonymousButton.SetActive(false);
        gameObject.SetActive(false);
        menu.SetActive(true);
    }
}

[System.Serializable]
public class LoginData
{
    public int id;
    public string first_name;
    public string last_name;
}


