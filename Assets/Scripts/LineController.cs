using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json; //Json encoding을 위한 모듈
using UnityEngine.EventSystems; // 다른 UI 클릭 시 작동 안되게끔 

// Zone 생성 버튼 클릭 시 Line Prefab을 통해 Zone gameobject가 새로 생성됨
// Prefab에 이 Script가 미리 포함되어 있음

public class LineController : MonoBehaviour
{
    [System.Serializable] //Zone의 꼭지점 정보를 Chicago로 Json형태로 전달하기 위해 데이터 시리얼라이징
    public class ZoneData //ZoneData Object 생성
    {
        [SerializeField] public string zone_name;
        [SerializeField] public List<List<float>> zone_points; //Zone의 꼭지점을 담을 list 생성

        public void setData(string _zone_name, List<List<float>> _zone_points) //변수의 값 넣기 위한 setData 함수
        {
            this.zone_name = _zone_name;
            this.zone_points = _zone_points;
        }
    }


    public GameObject circlePrefab; //마우스 클릭시 찍히는 원을 prefab으로 받음(import 한다고 생각하면 됨)

    private Camera cm; //마우스 클릭시 좌표를 받기 위해 사용되는 Main Camera
    private LineRenderer lr; //실제 Line을 그려주는 LineRenderer Component zone 게임 오브젝트마다 생성됨
    private Vector2 mousePosition; //mouse position을 담을 변수
    private Vector3[] Vector3PostionArray; //line renderer로 그린 zone의 position을 저장하기 위한 array
    private Vector2[] positionArray; //collider를 그리기 위해 필요한 Vector2 position array
    private Color LineColor;

    // Start is called before the first frame update
    void Start()
    {
        cm = Camera.main; //main 카메라를 담음
        lr = GetComponent<LineRenderer>(); //gameObject 내에 LineRenderer Component를 찾음
        lr.positionCount = 0; //LineRenderer내 Default로 정의된 꼭지점을 삭제 (디폴트는 1개)
        lr.SetWidth(0.05f, 0.05f); //Line의 굵기 정의
        LineColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        lr.startColor = LineColor;
        lr.endColor = LineColor;

    }

    // Sample button
    //void OnGUI()
    //{
    //    if (GUI.Button(new Rect(10, 70, 100, 30), "Create Zone"))
    //    {
    //        Debug.Log("Clicked the button with text");
    //        lr.enabled = true;
    //    }
    //}with

    
    // Update is called once per frame
    void Update()
    {
        // Zone Create 버튼 누를 시 활성화 됨 Line Renderer 활성화 됨

        // 마우스 버튼 누를 시 해당 위치에 원을 만들고 해당 좌표를 Line의 꼭지점으로 보냄
        if (Input.GetMouseButtonDown(0)) // 클릭 시
        {
            if (!EventSystem.current.IsPointerOverGameObject()) // 다른 UI 클릭 시 작동 안되게끔 
            {
                mousePosition = cm.ScreenToWorldPoint(Input.mousePosition); // 클릭 시점의 마우스 포지션 저장
                _SetPosition(); // line renderer에 클릭 시점의 마우스 포지션을 라인 꼭지점으로 전달
                createCircle(); // 클릭 지점에 원을 만듬
                castRay(); // 클릭 지점에 레이저 광선을 쏜 후 레이저 광선에 걸리는 Collider(충돌체)를 식별하여 함수 실행
            }

        }
    }

    // Line Renderer에 꼭지점 좌표를 보내기 위한 함수
    void _SetPosition()
    {
        int currentCount = lr.positionCount; // 현재 꼭지점 개수
        lr.positionCount = lr.positionCount + 1; // 꼭지점 개수에 한개를 추가
        lr.SetPosition(currentCount, mousePosition); // 추가된 꼭지점에, 마우스 포지션을 저장
    }

    // 클릭 지점에 원을 생성
    void createCircle()
    {
        GameObject circle = Instantiate(circlePrefab, mousePosition, Quaternion.identity); // Prefab을 통해 클릭 지점에 원 생성
        circle.transform.SetParent(gameObject.transform); // 생성 되는 Circle Object를 Line GameObejct의 Child로 만듬
        if (null == gameObject.transform.GetChild(0).gameObject.GetComponent<CircleCollider2D>()) // 첫번째 원의 Collider(충돌체)가 없다면 원보다 조금 큰 충돌체를 생성
        {
            gameObject.transform.GetChild(0).gameObject.AddComponent<CircleCollider2D>();
            gameObject.transform.GetChild(0).gameObject.GetComponent<CircleCollider2D>().radius = 2.0f;
        }
        
    }

    // 클릭 지점에 충돌체 감지 후 도형 만들기를 끝냄
    void castRay()
    {
        GameObject target = null; // 충돌한 GameObject를 담기위한 변수
        //RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, 0f); //직선으로 날라가는 점(선) 형태의 raycast, 방향과 거리가 0이기 때문에 마우스가 클릭한 지점에만 존재
        RaycastHit2D hit = Physics2D.CircleCast(mousePosition, 0.1f, Vector2.zero, 0f); //클릭지점에 원 형태의 Raycast 생성
        //Debug.DrawRay(mousePosition, Vector2.right, Color.green, 1f); //직선 안내선
        
        if(hit) // 충돌체를 감지했다면
        {
            Debug.Log(hit.collider.name);
            target = hit.collider.gameObject; // 변수에 충돌한 Gameobject를 담고,
            if (gameObject.transform.childCount > 0) // 원이 만들어진 후에 동작하기 위함, Line Obejct에 원이 없으면 동작하지 않음
            {
                if (target == gameObject.transform.GetChild(0).gameObject && gameObject.transform.childCount > 2) // 충돌체가 첫번째 원이고(도형 그리기를 끝내기 위함), 꼭지점이 2개 이상이라면(도형이 완성되기 위함)
                {
                    Debug.Log("gotchaa");
                    lr.positionCount = lr.positionCount - 1; // 마지막 클릭 지점의 꼭지점을 삭제하고
                    lr.loop = true; // 라인 loop 설정을하여, 첫번째 꼭지점과 (마지막 -1) 꼭지점을 이어지게 함
                    //Delete Circles
                    foreach (Transform child in transform) // 도형이 완성되었으므로, 라인 오브젝트에 있는 원을 다지움
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                    //line collider
                    addCollider(); // 도형에 충돌체를 추가함, 향 후 Zone 도형에 들어온 Tag를 처리하기 위함
                    gameObject.GetComponent<LineController>().enabled = false; // Zone 도형 생성이 끝났으므로, 코드를 비활성화 시킴
                    StartCoroutine(PostZoneInfo()); // Zone의 꼭지점 정보를 API를 통해 외부로 전송
                }
            }
        }
    }

    // 도형에 충돌체 추가하는 함수
    void addCollider() 
    {
        Vector3PostionArray = new Vector3[lr.positionCount]; // 꼭지점 개수 만큼 크기의 빈 Vector3 어레이를 만듬
        lr.GetPositions(Vector3PostionArray); // 도형 꼭지점을 빈 어레이에 담음
        positionArray = ConvertVector2List(Vector3PostionArray); // 2D Collider에 꼭지점을 적용하기 위해 Vector3 어레이를 Vector2 어레이로 변경
        gameObject.AddComponent<PolygonCollider2D>(); // 라인 게임오브젝트에 2D Collider 추가
        PolygonCollider2D zoneCollider = gameObject.GetComponent<PolygonCollider2D>();
        zoneCollider.SetPath(0, positionArray); // Collider에 꼭지점을 추가하여 도형형태로 만듬
    }

    Vector2[] ConvertVector2List(Vector3[] v3)
    {
        Vector2[] v2 = new Vector2[v3.Length];
        for (int i = 0; i < v3.Length; i++)
        {
            Vector3 tempV3 = v3[i];
            v2[i] = new Vector2(tempV3.x, tempV3.y);
        }
        return v2;
    }

    //float[][] ConvertPureArray(Vector3[] v)
    //{
    //    float[][] _v = new float[v.Length][];
    //
    //    for (int i = 0; i < v.Length; i++)
    //    {
    //        _v[i] = new float[] { v[i].x, v[i].y, v[i].z };
    //    }
    //    return _v;
    //}

    // 꼭지점 정보를 담은 Vector3 어레이를 Json Object로 전달하기 위해 리스트로 변경하는 함수
    List<List<float>> ConvertPureList(Vector3[] v)
   {
        List<List<float>> _v = new List<List<float>>();
   
       for (int i = 0; i < v.Length; i++)
       {
            _v.Add(new List<float>() { v[i].x, v[i].y, v[i].z });
       }
       return _v;
   }


    // Zone의 이름 및 꼭지점 정보를 Post, Zone생성 시 이름 추가 필요, 현재 default 이름 사용
    IEnumerator PostZoneInfo()
    {
        string url = "http://172.100.100.161:8000/rtls/create_zone";
        UnityWebRequest request = new UnityWebRequest(); // 웹 리퀘스트 생성

        List<List<float>> zonePositions = ConvertPureList(Vector3PostionArray);
        //print(zonePositions[0][0]);
        //print(zonePositions[0][1]);
        //print(zonePositions[0][2]);

        ZoneData zonedata = new ZoneData(); // Json Object로 변환하기 위한 Object 생성
        zonedata.setData("test_zone", zonePositions); // Object의 이름과 꼭지점 데이터를 Object에 저장

        //string jsonRequest = JsonUtility.ToJson(zonedata);
        string jsonRequest = JsonConvert.SerializeObject(zonedata); // Object를 Json Object로 변환
        //print(jsonRequest);

        //WWWForm form = new WWWForm();
        //form.AddField("zone_name", "test_zone");

        using (request = UnityWebRequest.Post(url, jsonRequest)) // 웹 리퀘스트 using
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonRequest); // json Object를 바이트로 변환
            request.uploadHandler = new UploadHandlerRaw(jsonToSend); // json Object를 웹 리퀘스트에 추가
            request.SetRequestHeader("Content-Type", "application/json"); // json Header
            yield return request.SendWebRequest(); // 웹 리퀘스트가 완료 되면 함수 종료
            if (request.result != UnityWebRequest.Result.Success) // 실패 시
            {
                Debug.Log(request.error);
                yield break; // 리턴
            }
            else // 성공 시
            {
                if (request.isDone) // 성공 시
                {
                    string result = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data); // Byte를 result로 변환
                    if (result == "")
                    {
                        Debug.Log("result is empty");
                    }
                    Debug.Log(result);
                }
            }
        }
    }

    //Raycast를 그려주는 안내선
    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireSphere(mousePosition, 0.1f);
    //}
}
