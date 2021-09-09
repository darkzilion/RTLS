using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.Networking;
using System;

public class Tag : MonoBehaviour
{
    public GameObject prefab;

    JsonData TagInfo;
    string[] TagArray;

    public float APITimer = 0.1f;

    [Range(0f, 1f)]
    public float TagSize = 0.2f;

    Dictionary<string, DateTime> TagTimeTable = new Dictionary<string, DateTime>();
    Dictionary<string, TagStruct> TagDict = new Dictionary<string, TagStruct>();

    List<string> TagList;  //List를 화면에 expanable list화 할 때 

    struct TagStruct
    {
        public string name;
        public Vector3 smoothedPosition;
        public DateTime LastTime;
        public Color color;
        public string coordinateSystemName;
        public float smoothedPositionAccuracy;
        public string zones;
        public long positionTS;

    }

    //float GrowSpeed = 1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        //TagInstantiate();
        StartCoroutine(LoadData(APITimer));
        StartCoroutine(TagDelete());
    }

    void Update()
    {
        TagSizeChange();
    }

    //API Get Tag Info
    IEnumerator LoadData(float delayTime)
    {
        string jsonResult;
        string GetDataUrl = "http://192.168.30.39:8080/qpe/getTagPosition?version=2&coord=5a674eaf-3426-4db3-94d6-c2f96f572122&maxAge=10000";
        using (UnityWebRequest www = UnityWebRequest.Get(GetDataUrl))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success) //불러오기 실패 시
            {
                Debug.Log(www.error);
                yield break;
            }
            else
            {
                if (www.isDone) //호출 완료 시
                {
                    jsonResult = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data); //byte를 string으로
                    if (jsonResult == "")
                    {
                        Debug.Log("Tag Info API Result is empty");
                    }
                    InitData(jsonResult);
                }
            }
        }
        yield return new WaitForSeconds(delayTime);
        StartCoroutine(LoadData(APITimer));
    }

    //Coordinate System(층) 별로 분류하여 배열에 담음
    public void InitData(string jsonResult)
    {
        JsonData TagInfo;
        //Dictionary<string, JsonData> TagDict = new Dictionary<string, JsonData>();

        if (jsonResult == "")
        {
            return;
        }
        //json구조체에 응답 결과 담음
        TagInfo = JsonMapper.ToObject(jsonResult);
        //Debug.Log(TagInfo.Count);


        string TagID;
        TagList = new List<string>();

        for (int i = 0; i < TagInfo["tags"].Count; i++)
        {
            Color color;
            DateTime NowTime = DateTime.Now;
            TagStruct TagPositionInfo = new TagStruct();

            if (TagInfo["tags"][i]["name"] != null)
            {
                TagID = TagInfo["tags"][i]["name"].ToString();
            }
            else
            {
                TagID = TagInfo["tags"][i]["id"].ToString();
            }
            TagPositionInfo.name = TagID;
            if (ColorUtility.TryParseHtmlString(TagInfo["tags"][i]["color"].ToString(), out color))
            {
                TagPositionInfo.color = color;
            }
            TagPositionInfo.coordinateSystemName = TagInfo["tags"][i]["coordinateSystemName"].ToString();
            TagPositionInfo.smoothedPosition = new Vector3(float.Parse(TagInfo["tags"][i]["smoothedPosition"][0].ToString()), float.Parse(TagInfo["tags"][i]["smoothedPosition"][1].ToString()));
            TagPositionInfo.smoothedPositionAccuracy = float.Parse(TagInfo["tags"][i]["smoothedPositionAccuracy"].ToString());
            TagPositionInfo.zones = TagInfo["tags"][i]["zones"].ToString();
            TagPositionInfo.LastTime = NowTime;
            TagPositionInfo.positionTS = long.Parse(TagInfo["tags"][i]["positionTS"].ToString());
            if (!TagDict.ContainsKey(TagID))
            {
                TagDict.Add(TagID, TagPositionInfo);
            } else
            {
                TagDict[TagID] = TagPositionInfo;
            }
            TagList.Add(TagID);
        }
        

        //for (int i = 0; i < TagInfo["tags"].Count; i++)
        //{
        //    if (TagInfo["tags"][i]["name"] != null)
        //    {
        //        TagID = TagInfo["tags"][i]["name"].ToString();
        //    }
        //    else
        //    {
        //        TagID = TagInfo["tags"][i]["id"].ToString();
        //    }
        //
        //    TagDict.Add(TagID, TagInfo["tags"][i]);
        //    TagList.Add(TagID);
        //}
        //foreach(string myName in TagList)
        //{
        //    Debug.Log(myName);
        //}
        //if (TagArray != null)
        //{
        //    Array.Clear(TagArray, 0, TagArray.Length);
        //}
        //TagArray = TagList.ToArray();
        //Dictionary<string, JsonData> TagResponse = TagDict;

        TagInstantiate();
    }


    void TagInstantiate()
    {
        foreach (KeyValuePair<string, TagStruct> item in TagDict)
        {
            //Debug.LogFormat("{0}:, {1}, {2}, {3}", item.Key, float.Parse(item.Value["smoothedPosition"][0].ToString()), float.Parse(item.Value["smoothedPosition"][1].ToString()), float.Parse(item.Value["smoothedPosition"][2].ToString()));
            Vector3 tagPosition = item.Value.smoothedPosition;
            if (null != GameObject.Find(item.Key))
            { 
                GameObject tagg = GameObject.Find(item.Key);
                tagg.GetComponent<Transform>().position = tagPosition;

                //Debug.Log("GOT CHAA");

            } else
            {
                GameObject tag = Instantiate(prefab, tagPosition, Quaternion.identity);
                tag.transform.parent = transform;
                tag.name = item.Key;
                tag.GetComponent<SpriteRenderer>().color = item.Value.color;
                //Debug.Log("GOT CHAA2");
            }
            //Debug.LogFormat("{0}: {1}", item.Key, item.Value.LastTime);
        }
    }

    void TagSizeChange()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.transform.localScale = new Vector3(TagSize, TagSize, 1); //Parent에서 Scale을 변경하면 위치가 순간적으로 바뀌는 버그발생
            //transform.localScale = new Vector3(TagSize, TagSize, 1.0f) //Parent에서 Scale을 변경하면 위치가 순간적으로 바뀌는 버그발생, Parent Pivot 때문인듯
        }
    }

    // Tag position 정보가 1분간 갱신 안되면 화면에서 지우기 위한 함수
    IEnumerator TagDelete()
    {
        Debug.Log("5sec");
        DateTime currentTime = DateTime.Now;
        DateTime oneMinBefore = currentTime.AddMinutes(-1);
        string ttttime = oneMinBefore.ToString("HH mm ss");
        foreach (KeyValuePair<string, TagStruct> item in TagDict)
        {
            int gap = DateTime.Compare(item.Value.LastTime, oneMinBefore);
            if (gap < 0)
            {
                string tttime = item.Value.LastTime.ToString("HH mm ss"); 
                Debug.LogFormat("{0} is removed due to one minute timeout. {1} {2}", item.Value.name, item.Value.positionTS, tttime, ttttime);
                TagDict.Remove(item.Key);
                GameObject tagg = GameObject.Find(item.Key);
                Destroy(tagg);
            }
        }
        yield return new WaitForSeconds(5);
        StartCoroutine(TagDelete());
    }
}
