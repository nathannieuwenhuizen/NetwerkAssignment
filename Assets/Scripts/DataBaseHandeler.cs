using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

public static class DataBaseHandeler
{
    public static int serverID = 1;
    public static string serverPassword = "password";

    public static string sessionID;
    public static int userID = -1;
    public static string userNickname = "anonymous";

    public static string base_url = "http://localhost/kernmodule4/";

    public async static void GetHttpAsync()
    {
        using (var client = new HttpClient())
        {
            var result = await client.GetAsync("url");
            if (result.IsSuccessStatusCode)
            {
                Debug.Log(await result.Content.ReadAsStringAsync());
            }
        }
    }

    public static string response;
    public static IEnumerator GetHttp(string url = "url")
    {
        var request = UnityWebRequest.Get( base_url + url);
        {
            yield return request.SendWebRequest();

            if (request.isDone && !request.isHttpError)
            {
                response = request.downloadHandler.text;
                Debug.Log(request.downloadHandler.text);
            }
        }
    }
}
