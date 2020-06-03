using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

public class DataBaseHandeler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    async void GetHttpAsync()
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

    IEnumerator GetHttp(string url = "url")
    {
        var request = UnityWebRequest.Get(url);
        {
            yield return request.SendWebRequest();

            if (request.isDone && !request.isHttpError)
            {
                Debug.Log(request.downloadHandler.text);
            }
        }
    }
}
