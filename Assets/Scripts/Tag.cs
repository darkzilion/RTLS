using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.Networking;
using System;
using System.Threading;
using UnityEngine.UI;

// 태그 Position Info 질의
// 맵에 태그 생성, timeout 시 삭제
// 좌측 패널에 태그 리스팅
// 태그 마우스오버 시 이름 표시
public class Tag : MonoBehaviour
{
    public GameObject prefab; // Tag Prefab

    public float APITimer = 0.1f; // Tag 정보를 불러올 시간, 0.1초

    [Range(0f, 1f)] // Tag 사이즈 변경 레인징
    public float TagSize = 0.2f;

    //Dictionary<string, TagStruct> TagDict = new Dictionary<string, TagStruct>();
    Dictionary<string, Dictionary<string, TagStruct>> CoordTagDict = new Dictionary<string, Dictionary<string, TagStruct>>();  //CoordSys와 Tag정보를 모두 담는 Dict
    List<string> TagList = new List<string>();  //List를 화면에 expanable list화 할 때 필요

    string[] CoordArray; // CoordSystem 이름(층 이름)을 담을 어레이
    List<string> CoordList; // Array로 변환하기 위한 리스트, 동적 Append를 위함(C#은 Array append가 안됨)
    private string coordsystem = "TempA"; // 첫 실행 시 LoadMap을 하기 위한 Trick
    private string coordsystemBefore = "TempB"; // 층 변환 시 이전 층과 바뀐 층 정보를 비교해서 바뀌였다면 바뀐 층으로 LoadMap을 새로 진행

    public GameObject TagListItem; // 리스트에 추가 되는 Tag 정보 Object
    public GameObject TagListContent; // Tag 정보가 추가 될 List UI

    struct TagStruct // 각 Tag의 정보를 담을 Struct
    {
        public string name;
        public Vector3 smoothedPosition;
        public DateTime LastTime;
        public Color color;
        public string coordinateSystemName;
        public float smoothedPositionAccuracy;
        public string zones;
        public long positionTS;
        public string deviceAddress;
        public string id;
        public string timeGap;
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
        Thread.Sleep(2000); //2초 후에
        StartCoroutine(TagDelete()); //포지션 업데이트가 없는 Tag 삭제
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
                        yield break;
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
                        CoordList.Add(CoordID); //CoordList에 CoordID 추가
                    }
                    CoordArray = CoordList.ToArray(); // CoordList를 Array로 추가
                    coordsystem = CoordArray[0]; // 첫 실행시에는 등록된 첫번 째 층을 보여줄 층으로 지정
                    for (int i = 0; i < CoordArray.Length; i++) // Coord 이름과 해당 Coord에서 잡히는 태그의 정보를 담는 딕셔너리에 Coord 이름을 먼저 키로 담음, 향후 Tag 정보를 해당하는 Coordsystem에 담기 위함
                    {
                        CoordTagDict.Add(CoordArray[i], new Dictionary<string, TagStruct>()); //
                    }
                    StartCoroutine(LoadData(APITimer)); // 실제 Tag정보를 가져오는 함수 실행
                }
            }
        }
    }


    //API Get Tag Info
    IEnumerator LoadData(float delayTime)
    {
        // 맵 바뀔 시
        if (coordsystemBefore != coordsystem) // 이전 맵과 틀리다면(맵이 바뀌었다면)
        {
            //TagDict.Clear();
            TagList.Clear(); // 생성 된 모든 태그 GameObject를 삭제

            // tag 모두 삭제
            int childs = transform.childCount; // 현재 태그 숫자 저장
            for (int i = childs - 1; i >= 0; i--) // 차일드의 순서는 0부터 시작하기에, 현재 태그 총 숫자에서 -1을 함
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
                GameObject.Destroy(TagListContent.transform.GetChild(i).gameObject);
            }

        }
        coordsystemBefore = coordsystem; // 맵이 바뀔 경우를 대비해 현재 Coordsystem 이름을 CoordsystemBefore로 저장
        string jsonResult; // TagInfo 저장하기 위한 변수 
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
                        yield break;
                    }
                    InitData(jsonResult); //향후 yeild StartCorouine(InitData(jsonResult)로 바꿔야 할 수도,,,
                }
            }
        }
        yield return new WaitForSeconds(delayTime); // APITimer 주기로 무한 실행됨
        StartCoroutine(LoadData(APITimer)); // Tag 정보를 불러오는 것 무한 반복
    }

    //TagInfo를 Coordinate System(층) 별로 분류하여 배열에 담음
    public void InitData(string jsonResult)
    {
        JsonData TagInfo; // Json Object 생성

        //Dictionary<string, TagStruct> TagDict = new Dictionary<string, TagStruct>();

        if (jsonResult == "")
        {
            return;
        }
        //json구조체에 응답 결과 담음
        TagInfo = JsonMapper.ToObject(jsonResult);

        //string coordSysName = TagInfo["tags"][0]["coordinateSystemName"].ToString();

        string TagID; //key가 될 TagID를 담는 String

        for (int i = 0; i < TagInfo["tags"].Count; i++) // TagInfo 내 Tag별 데이터 처리
        {
            Color color;
            DateTime NowTime = DateTime.Now; // 적재되는 시점의 시각 저장, Timeout 시 Tag 삭제를 위함
            TagStruct TagPositionInfo = new TagStruct(); // Tag 적재를 위한 구조체 생성 

            if (TagInfo["tags"][i]["name"] != null) // Tag name이 있다면 Tag name을 Key로 활용
            {
                TagID = TagInfo["tags"][i]["name"].ToString();
            }
            else // 없다면 id를 key로 활용
            {
                TagID = TagInfo["tags"][i]["id"].ToString();
            }
            TagPositionInfo.name = TagID; 
            if (ColorUtility.TryParseHtmlString(TagInfo["tags"][i]["color"].ToString(), out color)) // RBG 값을 변환 color object로 변환
            {
                TagPositionInfo.color = color;
            }
            TagPositionInfo.coordinateSystemName = TagInfo["tags"][i]["coordinateSystemName"].ToString(); 
            TagPositionInfo.smoothedPosition = new Vector3(float.Parse(TagInfo["tags"][i]["smoothedPosition"][0].ToString()), float.Parse(TagInfo["tags"][i]["smoothedPosition"][1].ToString())); 
            TagPositionInfo.smoothedPositionAccuracy = float.Parse(TagInfo["tags"][i]["smoothedPositionAccuracy"].ToString());

            if (TagInfo["tags"][i]["zones"].Count > 0)
            {
                TagPositionInfo.zones = TagInfo["tags"][i]["zones"][0]["name"].ToString();
            } else
            {
                TagPositionInfo.zones = "";
            }
            TagPositionInfo.LastTime = NowTime;
            TagPositionInfo.positionTS = long.Parse(TagInfo["tags"][i]["positionTS"].ToString());
            TagPositionInfo.deviceAddress = TagInfo["tags"][i]["deviceAddress"].ToString();
            TagPositionInfo.id = TagInfo["tags"][i]["id"].ToString();

            long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            TagPositionInfo.timeGap = DateTimeOffset.FromUnixTimeMilliseconds(timeNow - TagPositionInfo.positionTS).DateTime.ToString("mm:ss");
            print(TagPositionInfo.timeGap);

            // 맵 이동되면 Tag를 삭제 하기 위함, InitData에서 분리가 필요할 수 있음
            foreach (string item in CoordArray)
            {
                if (CoordTagDict[item].ContainsKey(TagID)) // 이미 저장된 Tag정보가 있고,
                {
                    if (item != TagPositionInfo.coordinateSystemName) // Tag 현재 정보의 Coord와 틀리다면
                    {
                        CoordTagDict[item].Remove(TagID); // 현재 Dict에서 해당 Tag를 삭제

                        if (null != GameObject.Find(TagID)) // 해당 Tag GameObject를 찾은 뒤 
                        {
                            Destroy(GameObject.Find(TagID)); // 삭제
                        }
                    }
                }
            }

            if (!CoordTagDict[TagPositionInfo.coordinateSystemName].ContainsKey(TagID)) // 현재 Coord에 Tag 정보가 없다면
            {
                CoordTagDict[TagPositionInfo.coordinateSystemName].Add(TagID, TagPositionInfo); // Dict에 새로 추가
            }
            else // 있다면
            {
                CoordTagDict[TagPositionInfo.coordinateSystemName][TagID] = TagPositionInfo; // 업데이트
            }
        }
            
        TagInstantiate(); //TagDict 정보를 토대로 태그 생성
    }

    // Tag 생성 함수
    void TagInstantiate()
    {
        Dictionary<string, TagStruct> TempTagDict = new Dictionary<string, TagStruct>(CoordTagDict[coordsystem]); // 현재 Coord의 TagDict를 새로 생성
        foreach (KeyValuePair<string, TagStruct> item in TempTagDict)
        {
            Vector3 tagPosition = item.Value.smoothedPosition;
            if (null != transform.Find(item.Key)) // 태그 이름으로 검색했을 때 이미 GameObject가 있다면,
            {
                GameObject tagg = transform.Find(item.Key).gameObject;
                tagg.GetComponent<Transform>().position = tagPosition; //Tag의 Position을 Dict 정보 기준으로 Update
                TagListUICreate(item.Value);
            }
            else // 없다면
            {
                GameObject tag = Instantiate(prefab, tagPosition, Quaternion.identity); // Tag를 새로 생성
                tag.transform.parent = transform; //TagDrawer gameObject에 child로 지정
                tag.name = item.Key; // tag name 
                tag.GetComponent<SpriteRenderer>().color = item.Value.color; // tag에 설정된 색으로 색 변경
                TagListUICreate(item.Value);
                //Debug.Log("GOT CHAA2");
            }
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
        Dictionary<string, TagStruct> TempTagDictt = new Dictionary<string, TagStruct>(CoordTagDict[coordsystem]); // 현재 Coord의 TagDict를 새로 생성, 문제 있음 변경 필요(아무것도 없을 때 삭제하면 에러남)
        //Debug.Log("5sec"); 
        DateTime currentTime = DateTime.Now; 
        DateTime oneMinBefore = currentTime.AddMinutes(-1); //현재 시각 기준 1분 전
        string ttttime = oneMinBefore.ToString("HH mm ss");
        foreach (KeyValuePair<string, TagStruct> item in TempTagDictt)
        {
            int gap = DateTime.Compare(item.Value.LastTime, oneMinBefore); //현재 시각 기준 1분전과 PositionTS 시각을 비교
            if (gap < 0) // 1분이 넘었다면
            {
                string tttime = item.Value.LastTime.ToString("HH mm ss");
                Debug.LogFormat("{0} is removed due to one minute timeout. {1} {2}", item.Value.name, item.Value.positionTS, tttime, ttttime);
                CoordTagDict[coordsystem].Remove(item.Key); // TagDict에서 삭제
                GameObject tagg = GameObject.Find(item.Key); // 해당 Tag를 찾아서
                Destroy(tagg); // 삭제
                //Destroy(TagListContent.transform.Find(item.Key).GetChild(1).gameObject); //content 내 body만 삭제 시 나중에 다시 생성할 때 오류...
            }
        }
        yield return new WaitForSeconds(5); // 5초마다 돔
        StartCoroutine(TagDelete());
    }

    // CoordSystem change, 층 전환, 층 전환 Game Object에서 이 함수를 불러서, 층 전환 시킴
    public void CoordChange(int index)
    {
        coordsystem = CoordArray[index];
    }

    // Tag Info로 생성 된 태그 gameobject 먼저 찾은 뒤 없다면 Return
    private void TagListUICreate(TagStruct TagPositionInfo)
    {
        string tagName = TagPositionInfo.name;
        GameObject tagInfo;

        if (TagListContent.transform.Find(tagName) != null)
        {
            tagInfo = TagListContent.transform.Find(tagName).gameObject;
        } else
        {
            tagInfo = Instantiate(TagListItem, TagListContent.transform);
        }

        tagInfo.name = tagName;
        GameObject tagHeaderText = tagInfo.transform.GetChild(0).GetChild(2).gameObject;
        tagHeaderText.GetComponent<Text>().text = tagName;
        GameObject tagBodyText = tagInfo.transform.GetChild(1).gameObject;
        tagBodyText.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Text>().text = TagPositionInfo.id;
        tagBodyText.transform.GetChild(1).GetChild(1).gameObject.GetComponent<Text>().text = TagPositionInfo.id;
        tagBodyText.transform.GetChild(2).GetChild(1).gameObject.GetComponent<Text>().text = TagPositionInfo.coordinateSystemName;
        tagBodyText.transform.GetChild(3).GetChild(1).gameObject.GetComponent<Text>().text = string.Format("{0} ({1}s ago)", TagPositionInfo.smoothedPosition.ToString(), TagPositionInfo.timeGap);
        tagBodyText.transform.GetChild(4).GetChild(1).gameObject.GetComponent<Text>().text = TagPositionInfo.smoothedPositionAccuracy.ToString();
        tagBodyText.transform.GetChild(5).GetChild(1).gameObject.GetComponent<Text>().text = TagPositionInfo.zones;
        tagBodyText.transform.GetChild(6).GetChild(1).gameObject.GetComponent<Text>().text = TagPositionInfo.deviceAddress;
        tagBodyText.transform.GetChild(7).GetChild(1).gameObject.GetComponent<Text>().text = ColorUtility.ToHtmlStringRGBA(TagPositionInfo.color);
        //print(tagListItem.name);
        //GameObject tagHeaderText = GameObject.Find(tagName).transform.Find("Header").transform.Find("Text").gameObject;
        //GameObject tagHeaderText = GameObject.Find(tagName).transform.Find("Header").transform.Find("Text").gameObject;
        //tagHeaderText.GetComponent<Text>().text = tagName;
    }
}
