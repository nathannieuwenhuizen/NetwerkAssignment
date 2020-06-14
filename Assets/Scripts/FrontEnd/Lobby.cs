using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{

    [SerializeField]
    private GameObject connectHostButton;
    [SerializeField]
    private GameObject connectClientButton;

    [SerializeField]
    private InputField setNameField;
    [SerializeField]
    private GameObject setNameButton;


    [SerializeField]
    private GameObject listObject;
    [SerializeField]
    private Text listContent;

    [SerializeField]
    private GameObject startButton;

    public bool isHost = false;

    private ClientBehaviour clientB;

    public void Start()
    {
        startButton.SetActive(false);
        setNameButton.SetActive(false);
        listObject.SetActive(false);
    }

    public void CreateClient(bool _isHost = false)
    {
        isHost = _isHost;

        GameObject client = new GameObject("client");
        if (isHost) client.AddComponent<ServerBehaviour>();

        client.AddComponent<DataHolder>().lobby = this;
        clientB = client.AddComponent<ClientBehaviour>();
        DontDestroyOnLoad(client);

        connectClientButton.SetActive(false);
        connectHostButton.SetActive(false);
        setNameField.gameObject.SetActive(true);
        setNameButton.SetActive(true);
    }

    public void SetName()
    {
        setNameButton.SetActive(false);
        setNameField.gameObject.SetActive(false);

        var setNameMessage = new SetNameMessage
        {
            Name = "" + setNameField.text
        };
        clientB.SendMessage(setNameMessage);
    }

    public void StartGame()
    {
        gameObject.SetActive(false);
    }

    public void StartButtonClicked()
    {
        if (clientB.GetComponent<ServerBehaviour>() != null) {
            clientB.GetComponent<ServerBehaviour>().StartGame();
        }
    }

    public void UpdateLobby(PlayerData[] datas)
    {
        startButton.SetActive(isHost && datas.Length != 0);
        listObject.SetActive(true);
        string content = "";

        foreach(PlayerData data in datas)
        {
            content += " | #" + data.playerIndex + ", " + data.name + ", color: " + data.color.r + data.color.g + data.color.b + "\n";
        }
        listContent.text = content;
    }
}
