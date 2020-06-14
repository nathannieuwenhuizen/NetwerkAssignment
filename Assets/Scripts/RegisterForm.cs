using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RegisterForm : MonoBehaviour
{
    [SerializeField]
    private InputField firstNameField;
    [SerializeField]
    private InputField lastNameField;
    [SerializeField]
    private InputField passwordField;
    [SerializeField]
    private InputField emailField;

    [SerializeField]
    private Text message;
    public void RegisterUser()
    {
        string url = "user_register.php?first_name=" + firstNameField.text + "&last_name="+ lastNameField.text+"&password="+passwordField.text+"&email=" + emailField.text;
        StartCoroutine(Registering(url));
    }

    public IEnumerator Registering(string url)
    {
        yield return StartCoroutine(DataBaseHandeler.GetHttp(url));

        if (DataBaseHandeler.response == 0 + "")
        {
            message.text ="something went wrong...";
        } else
        {
            message.text ="You're registerd succesfully!";
        }
    }

}
