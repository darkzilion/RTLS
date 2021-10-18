using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using LitJson;  //json 파싱 모듈

public class Background : MonoBehaviour
{
    public GameObject TagName; //CoordSys 변환 시 Tagname을 모두 지움

    private JsonData ProjectInfo; //Project Info 응답을 담는 json 구조체
    private string jsonResult; //ProjectInfo json 구조체에 담기 전 byte응답을 string으로 변환하여 담을 변수
    private string[] CoordArray; // Coord System에 이름을 담아 놀 어레이
    private List<string> CoordList; // Coord System에 이름을 담아 놀 리스트
    private Dictionary<string, JsonData> CoordsysDict = new Dictionary<string, JsonData>(); // ProjectInfo를 coordinateSystems(층)별로 담을 딕셔너리
    private GameObject BackgroundPrefab; //Background Image를 담을 Gameobject를 Intiate 하기 위한 BackgroundPrefab 
    private GameObject BackgroundImage; //Backgound Image GameObject

    // 시작 프레임(첫 프레임)이전에 완료되는 함수 Awake()
    void Awake()
    {
        BackgroundPrefab = Resources.Load("Prefabs/Background Holder") as GameObject; //BackgroundPrefab 변수에 실제 prefab을 로드 시킴
        StartCoroutine(LoadData()); // LoadData 호출
    }

    //API Get Project Info
    IEnumerator LoadData()
    {
        string GetDataUrl = "http://172.100.100.170:8080/qpe/getProjectInfo?version=2"; //API URL 정의
        using (UnityWebRequest www = UnityWebRequest.Get(GetDataUrl)) // UnityWebRequest 리소스 할당 후 자동으로 dispose 하기 위해 using문 사용, try/finally와 같음
        {
            yield return www.SendWebRequest(); // 함수 내 모든 코드 완료 시 리턴
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                yield break; // 실패 시 리턴
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

        //응답결과(String)을 json object에 변환 담음
        ProjectInfo = JsonMapper.ToObject(jsonResult);
        ProjectInfo = ProjectInfo["coordinateSystems"];


        //CoordID를 Key로 각 Coord의 정보(결과)를 dictionary에 담고, Coord name만 list와 어레이로 만듬
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
        this.CoordArray = CoordList.ToArray(); //list를 arrary화
        string DefaultCoord = this.CoordArray[0];  //기본으로 보여줄 Coord(맵)을 첫번째에 담아서 첫 실행 시 기본 맵을 보여줌
        JsonData BackgroundResponse = CoordsysDict[DefaultCoord]["backgroundImages"][0]; //backgroundimage base64 인코딩을 따로 담음
        LoadBackgroundImage(BackgroundResponse); //Background 이미지 처리 함수 호출
        DropdownList(); //Dropdown 리스트에 Coordname을 담음
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

        float imagePositionX = float.Parse(BackgroundResponse["xMeter"].ToString()); //image의 x위치
        float imagePositionY = float.Parse(BackgroundResponse["yMeter"].ToString()); //image의 y위치
        Vector2 imagePosition = new Vector2(imagePositionX, imagePositionY); //image의 position을 imagePosition으로 정의

        //base64를 이미지(texture)로 변경 후 sprite화
        byte[] byteTexture = Convert.FromBase64String(imageBase64); //base64 string을 byte로 변환
        Texture2D texture = new Texture2D(0, 0); //image를 입힐 texture 생성 
        texture.LoadImage(byteTexture); // texture에 image(byte로 전환한)를 입힙
        Rect rect = new Rect(0, 0, texture.width, texture.height); // 이미지의 크기를 image width pixel, image heigt pixel로 변환
        float imageScale = texture.width / float.Parse(BackgroundResponse["widthMeter"].ToString()); // Unit(meter)당 pixel 계산
        Sprite BackGroundSprite; //texture(raw image)를 담을 sprite 생성, texture를 바로 gameObject에 입히지 못함
        BackGroundSprite = Sprite.Create(texture, rect, new Vector2(0.0f, 0.0f), imageScale);
        BackgroundImage = Instantiate(BackgroundPrefab, imagePosition, Quaternion.identity); //Background prefab을 gameObject로 instantiate
        BackgroundImage.name = "BackgroundImage"; //향후 image 업데이트를 위해 backgroundimage gameobject에 이름을 할당
        BackgroundImage.GetComponent<SpriteRenderer>().sprite = BackGroundSprite; //gameObejct에 backgoundimage sprite를 입힘
    }

    public void DropdownList()
    {
        GameObject obj = GameObject.Find("CoordDropdown"); //CoordDropdown 이라는 이름을 가진 gameobject를 찾음
        var objDropdown = obj.GetComponent<Dropdown>(); //Dropdown component를 objDropdown이라는 오브젝트로 정의
        Transform childObjtr = objDropdown.transform.Find("Label"); //objDropdown 하위의 child ojbect Label을 찾아 정의
        GameObject childObj = childObjtr.gameObject; //childObjctr이 transform이기 때문에 gameObject로 다시 정의
        Debug.Log(CoordArray[0]);
        childObj.GetComponent<Text>().text = CoordArray[0]; //첫번째 Coord의 이름을 Dropdown의 기본 이름으로 정의

        objDropdown.options.Clear(); //Dropdown 내의 option을 빈 값으로 초기화
        foreach (string option in CoordList) //option에 Coord 각각의 이름을 추가
        {
            objDropdown.options.Add(new Dropdown.OptionData(option));
        }
    }


    //Dropdown list에서 Coord가 선택되었을 때 호출되는 함수, Coord 이름 순서의 int값을 변수로 전달
    //새로운 Coord 선택 시 Background 이미지를 새로 로딩
    public void DropdownSelect(int index)
    {
        int childs = TagName.transform.childCount; // 현재 태그 숫자 저장
        for (int i = childs - 1; i >= 0; i--) // 차일드의 순서는 0부터 시작하기에, 현재 태그 총 숫자에서 -1을 함
        {
            GameObject.Destroy(TagName.transform.GetChild(i).gameObject);
        }


        JsonData BackgroundResponse = CoordsysDict[CoordArray[index]]["backgroundImages"][0];
        LoadBackgroundImage(BackgroundResponse);
    }

}