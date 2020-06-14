using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Statistics : MonoBehaviour
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text scoreText;



    public void UpdateStatistics()
    {
        StartCoroutine(ShowStatistics());
    }
    public IEnumerator ShowStatistics()
    {

        yield return StartCoroutine(DataBaseHandeler.GetHttp("statistics.php"));
        Debug.Log("Response: " + DataBaseHandeler.response);

        DataBaseStatistic highscores = new DataBaseStatistic();
        highscores = JsonUtility.FromJson<DataBaseStatistic>(DataBaseHandeler.response);

        Debug.Log("highscores: " + highscores.test);
        nameText.text = "";
        scoreText.text = "";

        foreach (DatabaseHighScore scores in highscores.allTime)
        {
            Debug.Log("Gemiddelde: " + scores.gemiddelde);
            nameText.text += scores.first_name + " " + scores.last_name + "\n";
            scoreText.text += scores.gemiddelde + "\n";
        }

    }

}
[System.Serializable]
public class DataBaseStatistic
{
    public DatabaseHighScore[] allTime;
    public int test;
}
[System.Serializable]
public class DatabaseHighScore
{
    public int gemiddelde;
    public int amountofwins;
    public string first_name;
    public string last_name;
}
