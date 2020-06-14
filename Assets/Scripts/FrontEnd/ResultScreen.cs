using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class ResultScreen : MonoBehaviour
{

    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text scoreText;

    public void BackToMenu()
    {
        foreach(ClientBehaviour client in FindObjectsOfType<ClientBehaviour>())
        {
            Destroy(client.gameObject);
        }

        Destroy(FindObjectOfType<ServerBehaviour>());
        SceneManager.LoadScene(0);
    }

    public void ShowScore(EndGameMessage message, List<PlayerData> players, PlayerData myData)
    {

        List<HighScorePair> highscores = new List<HighScorePair>();
        for (int i = 0; i < message.PlayerIDHighscorePairs.Length; i++)
        {
            if (myData.playerIndex == message.PlayerIDHighscorePairs[i].playerID)
            {
                myData.score = message.PlayerIDHighscorePairs[i].score;
            }
            highscores.Add(message.PlayerIDHighscorePairs[i]);
        }
        highscores = highscores.OrderBy(x => x.score).Reverse().ToList();

        nameText.text = "";
        scoreText.text = "";

        foreach (HighScorePair scores in highscores)
        {
            nameText.text += players.Find(x => x.playerIndex == scores.playerID).name + "\n";
            scoreText.text += scores.score + "\n";
        }

        //upload score
        if (DataBaseHandeler.userID != -1)
        {
            Debug.Log("user: " + DataBaseHandeler.userID);
            Debug.Log("score: " + myData.score);
            string url = "score_insert.php?user=" + DataBaseHandeler.userID + "&score=" + myData.score + "&session_id=" + DataBaseHandeler.sessionID;
            StartCoroutine(DataBaseHandeler.GetHttp(url));
        }

    }

}
