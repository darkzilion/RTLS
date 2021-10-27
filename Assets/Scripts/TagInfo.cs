using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class TagInfo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetTagInfo());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator GetTagInfo()
    {
        string jsonResult;
        string getDataURL = "http://172.100.100.170:8080/qpe/getTagInfo?version=2&maxAge=80000";

        using (UnityWebRequest www = UnityWebRequest.Get(getDataURL))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                yield break;
            }
            else
            {
                if (www.isDone)
                {
                    jsonResult = www.downloadHandler.text;
                    if (jsonResult == "")
                    {
                        Debug.Log("Tag Info API Result is empty");
                        yield break;
                    }
                    print(string.Format("TagInfo {0}", jsonResult));
                }
            }

        }
    }
}
