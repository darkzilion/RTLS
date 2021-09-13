using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using LitJson;  //json 파싱 모듈

public class RestAPI : MonoBehaviour
{
    JsonData ProjectInfo; //Project Info 응답을 담는 json 구조체
    string jsonResult; //ProjectInfo json 구조체에 담기 전 byte응답을 string으로 변환하여 담을 변수
    //public GameObject BackgroundPrefab; //Sprite를 담을 game object
    string[] CoordArray;
    List<string> CoordList;
    Dictionary<string, JsonData> CoordsysDict = new Dictionary<string, JsonData>(); // ProjectInfo를 coordinateSystems(층)별로 담을 딕셔너리
    GameObject BackgroundPrefab;
    GameObject BackgroundImage;

    void Awake()
    {
        BackgroundPrefab = Resources.Load("Prefabs/Background Holder") as GameObject;
        StartCoroutine(LoadData());
    }

    //API Get Project Info
    IEnumerator LoadData()
    {
        string GetDataUrl = "http://192.168.30.39:8080/qpe/getProjectInfo?version=2";
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
                        Debug.Log("Project Info API Result is empty");
                    }
                    InitData();

                }
            }
        }
    }

    //Coordinate System(층) 별로 분류하여 배열에 담음
    public void InitData()
    {
        if (jsonResult == "")
        {
            return;
        }
        //json구조체에 응답 결과 담음
        ProjectInfo = JsonMapper.ToObject(jsonResult);
        ProjectInfo = ProjectInfo["coordinateSystems"];
        //Debug.Log(ProjectInfo.Count);

        //CoordID를 Key로 각 Coord의 정보를 dictionary와, Coord name만 따로 어레이에 담음
        CoordList = new List<string>();
        for (int i = 0; i < ProjectInfo.Count; i++)
        {
            //CoordID를 추출
            string CoordID;
            if (ProjectInfo[i]["name"].ToString() != "")
            {
                CoordID = ProjectInfo[i]["name"].ToString();
            }
            else
            {
                CoordID = ProjectInfo[i]["id"].ToString();
            }

            CoordsysDict.Add(CoordID, ProjectInfo[i]); 
            CoordList.Add(CoordID);
        }
        this.CoordArray = CoordList.ToArray();
        string DefaultCoord = this.CoordArray[0];  //기본으로 보여줄 Coord(맵)
        JsonData BackgroundResponse = CoordsysDict[DefaultCoord]["backgroundImages"][0];
        LoadBackgroundImage(BackgroundResponse);
        DropdownList();
    }

    //Backgoundimage를 Sprite화 후 Scale(척도), Position(앵커와 이미지 위치) 조정 후 gameObject에 추가
    public void LoadBackgroundImage(JsonData BackgroundResponse)
    {
        //BackGroundImage 존재 시 삭제
        if (null != GameObject.Find("BackgroundImage"))
        {
            Destroy(GameObject.Find("BackgroundImage"));
        }

        //base64 응답 앞 부분의 "data:image/png;base64" 코드 제거
        string imageBase64 = BackgroundResponse["base64"].ToString();
        char splitChar = ',';
        string[] tempList = imageBase64.Split(splitChar);
        imageBase64 = tempList[1];

        float imagePositionX = float.Parse(BackgroundResponse["xMeter"].ToString());
        float imagePositionY = float.Parse(BackgroundResponse["yMeter"].ToString());
        Vector3 imagePosition = new Vector3(imagePositionX, imagePositionY);

        //base64를 이미지(texture)로 변경 후 sprite화
        byte[] byteTexture = Convert.FromBase64String(imageBase64);
        Texture2D texture = new Texture2D(0, 0);
        texture.LoadImage(byteTexture);
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        float imageScale = texture.width / float.Parse(BackgroundResponse["widthMeter"].ToString());
        Sprite BackGroundSprite; //texture(raw image)를 담을 sprite
        BackGroundSprite = Sprite.Create(texture, rect, new Vector2(0.0f, 0.0f), imageScale);
        //BackGroundSprite.texture.dimension
        BackgroundImage = Instantiate(BackgroundPrefab, new Vector2(imagePositionX, imagePositionY), Quaternion.identity); //prefab을 gameObject로 instantiate
        BackgroundImage.name = "BackgroundImage";
        BackgroundImage.GetComponent<SpriteRenderer>().sprite = BackGroundSprite; //gameObejct에 backgoundimage sprite적용
    }

    public void DropdownList()
    {
        GameObject obj = GameObject.Find("CoordDropdown");
        var objDropdown = obj.GetComponent<Dropdown>();
        Transform childObjtr = objDropdown.transform.Find("Label");
        GameObject childObj = childObjtr.gameObject;
        Debug.Log(CoordArray[0]);
        childObj.GetComponent<Text>().text = CoordArray[0];

        objDropdown.options.Clear();
        foreach (string option in CoordList)
        {
            objDropdown.options.Add(new Dropdown.OptionData(option));
        }
    }

    public void DropdownSelect(int index)
    {   
        JsonData BackgroundResponse = CoordsysDict[CoordArray[index]]["backgroundImages"][0];
        LoadBackgroundImage(BackgroundResponse);
    }

}