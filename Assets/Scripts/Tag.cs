using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.Networking;
using System;
using System.Threading;

public class Tag : MonoBehaviour
{
    public GameObject prefab;

    JsonData TagInfo;
    string[] TagArray;

    public float APITimer = 0.1f;

    [Range(0f, 1f)]
    public float TagSize = 0.2f;

    //Dictionary<string, TagStruct> TagDict = new Dictionary<string, TagStruct>();
    Dictionary<string, Dictionary<string, TagStruct>> CoordTagDict = new Dictionary<string, Dictionary<string, TagStruct>>();  //CoordSys와 Tag정보를 모두 담는 Dict
    Dictionary<string, TagStruct> tempDict = new Dictionary<string, TagStruct>();
    List<string> TagList = new List<string>();  //List를 화면에 expanable list화 할 때

    string[] CoordArray;
    List<string> CoordList;
    public string coordsystem = "TempA";
    public string coordsystemTemp = "TempB";

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
        StartCoroutine(GetCoordSys());
    }

    private void Start()
    {
        Thread.Sleep(2000);
        StartCoroutine(TagDelete());
    }

    void Update()
    {
        TagSizeChange();
    }

    IEnumerator GetCoordSys()
    {
        string jsonResult;
        string GetDataUrl = "http://172.100.100.170:8080/qpe/getProjectInfo?version=2&noImageBytes=true";
        CoordList = new List<string>();
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

                    JsonData projectInfo = JsonMapper.ToObject(jsonResult);
                    projectInfo = projectInfo["coordinateSystems"];
                    for (int i = 0; i < projectInfo.Count; i++)
                    {
                        string CoordID;
                        if (projectInfo[i]["name"].ToString() != "")
                        {
                            CoordID = projectInfo[i]["name"].ToString();
                        }
                        else
                        {
                            CoordID = projectInfo[i]["id"].ToString();
                        }
                        CoordList.Add(CoordID);
                    }
                    CoordArray = CoordList.ToArray();
                    coordsystem = CoordArray[0];
                    for (int i = 0; i < CoordArray.Length; i++)
                    {
                        CoordTagDict.Add(CoordArray[i], new Dictionary<string, TagStruct>());
                    }
                    print("LoadData");
                    StartCoroutine(LoadData(APITimer));
                }
            }
        }
    }


    //API Get Tag Info
    IEnumerator LoadData(float delayTime)
    {
        // 맵 바뀔 시
        if (coordsystemTemp != coordsystem)
        {
            //TagDict.Clear();
            TagList.Clear();

            // tag 모두 삭제
            int childs = transform.childCount;
            for (int i = childs - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }

        }
        coordsystemTemp = coordsystem;
        string jsonResult;
        string GetDataUrl = string.Format("http://172.100.100.170:8080/qpe/getTagPosition?version=2&maxAge=80000");
        //string GetDataUrl = string.Format("http://172.100.100.170:8080/qpe/getTagPosition?version=2&coord={0}&maxAge=80000",coordsystemTemp);
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

        Dictionary<string, TagStruct> TagDict = new Dictionary<string, TagStruct>();

        if (jsonResult == "")
        {
            return;
        }
        //json구조체에 응답 결과 담음
        TagInfo = JsonMapper.ToObject(jsonResult);
        //Debug.Log(TagInfo.Count);

        //string coordSysName = TagInfo["tags"][0]["coordinateSystemName"].ToString();

        string TagID;

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

            foreach (string item in CoordArray)
            {
                if (CoordTagDict[item].ContainsKey(TagID))
                {
                    if (item != TagPositionInfo.coordinateSystemName)
                    {
                        CoordTagDict[item].Remove(TagID);

                        if (null != GameObject.Find(TagID))
                        {
                            Destroy(GameObject.Find(TagID));
                        }
                    }
                }
            }

            if (!CoordTagDict[TagPositionInfo.coordinateSystemName].ContainsKey(TagID))
            {
                CoordTagDict[TagPositionInfo.coordinateSystemName].Add(TagID, TagPositionInfo);
            }
            else
            {
                CoordTagDict[TagPositionInfo.coordinateSystemName][TagID] = TagPositionInfo;
            }
        }
            
        TagInstantiate();
    }


    void TagInstantiate()
    {
        Dictionary<string, TagStruct> TempTagDict = new Dictionary<string, TagStruct>(CoordTagDict[coordsystem]);
        foreach (KeyValuePair<string, TagStruct> item in TempTagDict)
        {
            //Debug.LogFormat("{0}:, {1}, {2}, {3}", item.Key, float.Parse(item.Value["smoothedPosition"][0].ToString()), float.Parse(item.Value["smoothedPosition"][1].ToString()), float.Parse(item.Value["smoothedPosition"][2].ToString()));
            Vector3 tagPosition = item.Value.smoothedPosition;
            if (null != GameObject.Find(item.Key))
            {
                GameObject tagg = GameObject.Find(item.Key);
                tagg.GetComponent<Transform>().position = tagPosition;

                //Debug.Log("GOT CHAA");

            }
            else
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
        Dictionary<string, TagStruct> TempTagDictt = new Dictionary<string, TagStruct>(CoordTagDict[coordsystem]);
        Debug.Log("5sec");
        DateTime currentTime = DateTime.Now;
        DateTime oneMinBefore = currentTime.AddMinutes(-1);
        string ttttime = oneMinBefore.ToString("HH mm ss");
        foreach (KeyValuePair<string, TagStruct> item in TempTagDictt)
        {
            int gap = DateTime.Compare(item.Value.LastTime, oneMinBefore);
            if (gap < 0)
            {
                string tttime = item.Value.LastTime.ToString("HH mm ss");
                Debug.LogFormat("{0} is removed due to one minute timeout. {1} {2}", item.Value.name, item.Value.positionTS, tttime, ttttime);
                CoordTagDict[coordsystem].Remove(item.Key);
                GameObject tagg = GameObject.Find(item.Key);
                Destroy(tagg);
            }
        }
        yield return new WaitForSeconds(5);
        StartCoroutine(TagDelete());
    }

    // CoordSystem change, 층 전환
    public void CoordChange(int index)
    {
        coordsystem = CoordArray[index];
    }
}
