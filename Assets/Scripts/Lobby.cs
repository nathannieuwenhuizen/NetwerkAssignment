using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{

    [SerializeField]
    private Text messageText;

    [SerializeField]
    private InputField nameField;

    public void SetMessage(string line)
    {
        messageText.text = line;
    }


    public void SendName()
    {
        string name = nameField.text;
        //...
    }
}
