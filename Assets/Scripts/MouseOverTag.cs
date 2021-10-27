using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MouseOverTag : MonoBehaviour
{
    public GameObject tagTextPrefab;
    public bool tagNameActive;

    private GameObject canvas;
    private GameObject tagTextParent;
    private string tagName;
    private RectTransform canvasRect;
    private Vector2 canvasPos;
    private Vector2 screenPoint;
    private GameObject tagTextGameObject;
    private Text tagText;
    private Vector3 offsetPos;
    private float offsetPosY;


    private void Start()
    {
        tagNameActive = true;

        Debug.Log("name test");
        tagTextParent = GameObject.Find("TagName");
        tagName = gameObject.name;

        canvas = GameObject.Find("Canvas");
        canvasRect = canvas.GetComponent<RectTransform>();

        offsetPosY = transform.position.y + 0.75f;
        offsetPos = new Vector3(transform.position.x, offsetPosY, transform.position.z);

        screenPoint = Camera.main.WorldToScreenPoint(offsetPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out canvasPos);

        tagTextGameObject = Instantiate(tagTextPrefab, canvasPos, Quaternion.identity);
        tagTextGameObject.name = tagName;
        tagText = tagTextGameObject.GetComponent<Text>();
        tagText.text = tagName;
        tagText.transform.SetParent(tagTextParent.transform);
        tagText.transform.localPosition = canvasPos;

        tagTextGameObject.SetActive(false);
    }

    private void Update()
    {
        if (tagText != null)
        {
            tagText.transform.localPosition = canvasPos;
            offsetPosY = transform.position.y + 0.6f;
            offsetPos = new Vector3(transform.position.x, offsetPosY, transform.position.z);
            screenPoint = Camera.main.WorldToScreenPoint(offsetPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out canvasPos);
            tagText.transform.localPosition = canvasPos;
        }

    }

    private void OnMouseEnter()
    {
        tagTextGameObject.SetActive(tagNameActive);
        tagNameActive = false;
    }

    private void OnMouseExit()
    {
        tagTextGameObject.SetActive(tagNameActive);
        tagNameActive = true;
    }
}
