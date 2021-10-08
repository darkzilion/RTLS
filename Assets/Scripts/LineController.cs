using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class LineController : MonoBehaviour
{
   //public class ZoneData
   //{
   //    public string zone_name;
   //    public float[][] zone_points;
   //
   //    public void SetData(string _zone_name, float[][] _zone_points)
   //    {
   //        zone_name = _zone_name;
   //        zone_points = _zone_points;
   //    }
   //
   //    public string SavetoJsonString()
   //    {
   //        return JsonUtility.ToJson(this);
   //    }
   //}

    [System.Serializable]
    public class ZoneData
    {
        [SerializeField] public string zone_name;
        [SerializeField] public List<List<float>> zone_points;

        public void setData(string _zone_name, List<List<float>> _zone_points)
        {
            this.zone_name = _zone_name;
            this.zone_points = _zone_points;
        }
    }


    public GameObject circlePrefab;

    private Camera cm;
    private LineRenderer lr;
    private Vector2 mousePosition;
    private Vector3[] Vector3PostionArray;
    private Vector2[] positionArray;
    //private GameObject circleParent;

    // Start is called before the first frame update
    void Start()
    {
        cm = Camera.main;
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.SetWidth(0.011f, 0.011f);
        //circleParent = new GameObject();
        //circleParent.name = "circleParent";
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 70, 100, 30), "Create Zone"))
        {
            Debug.Log("Clicked the button with text");
            lr.enabled = true;
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePosition = Input.mousePosition;
            mousePosition = cm.ScreenToWorldPoint(mousePosition);
            SetPosition();
            createCircle();
            castRay();
            
            
        }
    }
    
    void SetPosition()
    {
        int currentCount = lr.positionCount;
        lr.positionCount = lr.positionCount + 1;
        lr.SetPosition(currentCount, mousePosition);
    }

    void createCircle()
    {
        GameObject circle = Instantiate(circlePrefab, mousePosition, Quaternion.identity);
        circle.transform.SetParent(gameObject.transform);
        if (null == gameObject.transform.GetChild(0).gameObject.GetComponent<CircleCollider2D>())
        {
            gameObject.transform.GetChild(0).gameObject.AddComponent<CircleCollider2D>();
            gameObject.transform.GetChild(0).gameObject.GetComponent<CircleCollider2D>().radius = 2.0f;
        }
        
    }

    void castRay()
    {
        GameObject target = null;
        //RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, 0f); //직선으로 날라가는 점(선) 형태의 raycast, 방향과 거리가 0이기 때문에 마우스가 클릭한 지점에만 존재
        RaycastHit2D hit = Physics2D.CircleCast(mousePosition, 0.1f, Vector2.zero, 0f); //원 형태의 Raycast;
        //Debug.DrawRay(mousePosition, Vector2.right, Color.green, 1f); //직선 안내선
        
        if(hit)
        {
            Debug.Log(hit.collider.name);
            target = hit.collider.gameObject;
            if (gameObject.transform.childCount > 0)
            {
                if (target == gameObject.transform.GetChild(0).gameObject && gameObject.transform.childCount > 2)
                {
                    Debug.Log("gotchaa");
                    lr.positionCount = lr.positionCount - 1;
                    lr.loop = true;
                    //Delete Circles
                    foreach (Transform child in transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                    //line collider
                    addCollider();
                    gameObject.GetComponent<LineController>().enabled = false;
                    StartCoroutine(PostZoneInfo());
                }
            }
        }
    }

    void addCollider()
    {
        Vector3PostionArray = new Vector3[lr.positionCount];
        lr.GetPositions(Vector3PostionArray);
        positionArray = ConvertVector2List(Vector3PostionArray);
        gameObject.AddComponent<PolygonCollider2D>();
        PolygonCollider2D zoneCollider = gameObject.GetComponent<PolygonCollider2D>();
        zoneCollider.SetPath(0, positionArray);
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
        UnityWebRequest request = new UnityWebRequest();

        List<List<float>> zonePositions = ConvertPureList(Vector3PostionArray);
        print(zonePositions[0][0]);
        print(zonePositions[0][1]);
        print(zonePositions[0][2]);

        List<float> tempfloat = new List<float>() { 1f, 2f, 3f }; 

        ZoneData zonedata = new ZoneData();
        zonedata.setData("test_zone", zonePositions);

        //string jsonRequest = JsonUtility.ToJson(zonedata);
        string jsonRequest = JsonConvert.SerializeObject(zonedata);
        print(jsonRequest);

        //WWWForm form = new WWWForm();
        //form.AddField("zone_name", "test_zone");

        using (request = UnityWebRequest.Post(url, jsonRequest))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonRequest);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
                yield break;
            }
            else
            {
                if (request.isDone)
                {
                    string result = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
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
