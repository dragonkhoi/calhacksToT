using UnityEngine;

using System.IO;

using System.Collections.Generic;



public class emotionReading : MonoBehaviour
{
    public static emotionReading Instance { get; private set; }
    string EmotionAPIKey = "c113d34d43f64934b89b80b09d7bfdd1";
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
            t.text = noun(highestEmote);
        }
    

}
    //Uses distinct third letters of adjectives to create nouns
    public string noun(string adjective)
    {
        switch (adjective[2])
        {
            case 'g': return "Angry";
            case 'n': return "Contempt";
            case 's': return "Disgusted";
            case 'd': return "Sad";
            case 'u': return "Neutral";
            case 'p': return "Happy";
            case 'a': return "Afraid";
            case 'r': return "Surprised";
        }
        return "Neutral";

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