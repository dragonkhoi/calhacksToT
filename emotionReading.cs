using UnityEngine;

using System.IO;

using System.Collections.Generic;



public class emotionReading : MonoBehaviour
{
    public static emotionReading Instance { get; private set; }
    string EmotionAPIKey = "[key]";
    public GameObject NewText;
    IEnumerator<object> PostToEmotionAPI(byte[] imageData)
    {
        NewText = GameObject.Find("Emotion Text");
        var url = "https://api.projectoxford.ai/emotion/v1.0/recognize";
        var headers = new Dictionary<string, string>() {
            { "Ocp-Apim-Subscription-Key", EmotionAPIKey },
            { "Content-Type", "application/octet-stream" }
        };

        WWW www = new WWW(url, imageData, headers);
        yield return www;
        string responseString = www.text;


        JSONObject j = new JSONObject(responseString);
        Debug.Log(j);
        foreach (var result in j.list)
        {
            var p = result.GetField("faceRectangle");
            //string id = string.Format("{0},{1},{2},{3}", p.GetField("left"), p.GetField("top"), p.GetField("width"), p.GetField("height"));
            //var txtMesh = textmeshes[id];

            var obj = result.GetField("scores");
            string highestEmote = "Unknown";
            string secondHighestEmote = "Unknown";
            float secondHighestC = 0;
            float highestC = 0;
            for (int i = 0; i < obj.list.Count; i++)
            {
                string key = obj.keys[i];
                float c = obj.list[i].f;
                if (c > highestC)
                {
                    highestEmote = key;
                    highestC = c;
                }
                else if (c > secondHighestC)
                {
                    secondHighestEmote = key;
                    secondHighestC = c;
                }
            }
            TextMesh t = NewText.GetComponent<TextMesh>();
            t.text = adjective(highestEmote);
        }
    

}
    //looks us adjectives (case insensitive) in dict
    //assumes adjective is valid
    public string adjective(string noun)
    {
        Dictionary<string,string> noun_to_adj = new Dictionary<string,string>();

        noun_to_adj.Add("anger","Angry");
        noun_to_adj.Add("contempt","Contempt");
        noun_to_adj.Add("disgust","Disgusted");
        noun_to_adj.Add("sadness","Sad");
        noun_to_adj.Add("neutral","Neutral");
        noun_to_adj.Add("happiness","Happy");
        noun_to_adj.Add("fear","Afraid");
        noun_to_adj.Add("surprise","Surprise");
        // assume adjective in dict
        return noun_to_adj[noun.toLower()];
    }
    



    // Use this for initialization
    void Awake()
    {
        //Instance = this;
        
    }

    public void callToAPI()
    {
        byte[] imageData = File.ReadAllBytes(NewText.GetComponent<GazeGestureManager>().fileName);
        StartCoroutine(PostToEmotionAPI(imageData));
    }
}