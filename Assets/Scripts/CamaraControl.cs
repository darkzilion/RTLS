using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CamaraControl : MonoBehaviour
{
    public Camera Cam; //main camera를 public 변수로 받음
    public float ZoomStep = 1;
    public float MinCamSize = 1.0f;
    public float MaxCamSize = 20.0f;
    public float SmoothSpeed = 15.0f;
    public float mapMinX = -50.0f;  // 아래 방향 최대 위치
    public float mapMaxX = 50.0f;  // 위 방향 최대 위치
    public float mapMinY = -50.0f; // 왼쪽 방향 최대 위치
    public float mapMaxY = 50.0f; // 오른쪽 방향 최대 위치

    private Vector3 dragOrigin; // 카메라 드래그 시, 첫번 째 클릭의 마우스 위치
    private float targetOrtho = 13.0f; // 현재 카메라의 크기

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PanCamera(); // 프레임 마다 PanCamera 실행
        Zoom(); // 프레임 마다 Zoom 실행
    }

    void PanCamera() // 드래그로 카메라 이동
    {
        if(!Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0)) // 마우스 버튼 클릭 시
            {
                if (!EventSystem.current.IsPointerOverGameObject()) //UI 클릭시 false
                {
                    dragOrigin = Cam.ScreenToWorldPoint(Input.mousePosition); // 현재 클릭 위치의 마우스 포지션을 저장
                }
            }

            if (Input.GetMouseButton(0)) // 마우스 버튼이 계속 눌려 있을 시
            {
                if (!EventSystem.current.IsPointerOverGameObject()) //UI 클릭시 false
                {
                    Vector3 difference = dragOrigin - Cam.ScreenToWorldPoint(Input.mousePosition); // 클릭 시 포지션에서 현재 위치의 차이 저장
                    Cam.transform.position += difference; // 카메라 포지션을 차이 만큼 이동, 반대로 이동해야 정상임
                }
            }
        }
        ClampCamera(Cam.transform.position); // 카메라가 이동할 수 있는 위치 제한
    }

    void Zoom() // 카메라 줌인, 줌아웃
    {
        float scroll = 0.0f;
        if (!EventSystem.current.IsPointerOverGameObject()) //UI 클릭시 false
        {
            scroll = Input.GetAxis("Mouse ScrollWheel"); // 스크롤 휠 동작에 해당하는 float 값 저장
        }

        if (scroll != 0.0f) // 휠이 돌아갔다면
        {
            targetOrtho -= scroll * ZoomStep; // 현재 카메라에 사이즈에서 스크롤 * ZoomStep 만큼 뺌
            targetOrtho = Mathf.Clamp(targetOrtho, MinCamSize, MaxCamSize); // Mathf.Clamp으로 최대, 최소 카메라 사이즈 만큼 클램핑
        }

        Cam.orthographicSize = Mathf.MoveTowards(Cam.orthographicSize, targetOrtho, SmoothSpeed * Time.deltaTime); // 카메라의 사이즈를 Mathf.MoveTowards를 통해 스무스하게 이동시킴
        ClampCamera(Cam.transform.position); // 카메라 클램핑
    }

    void ClampCamera(Vector3 targetposition)
    {
        float camHeight = Cam.orthographicSize;
        float camWidth = Cam.orthographicSize * Cam.aspect;

        float minX = mapMinX + camWidth;
        float minY = mapMinY + camHeight;

        float maxX = mapMaxX - camWidth;
        float maxY = mapMaxY - camHeight;

        float newX = Mathf.Clamp(targetposition.x, minX, maxX);
        float newY = Mathf.Clamp(targetposition.y, minY, maxY);

        Cam.transform.position = new Vector3(newX, newY, -10); // 위 코드에서 변경되는 값들의 z 값은 0이기 때문에, 카메라의 z 기본 위치인 -10으로 재조정
    }
}
