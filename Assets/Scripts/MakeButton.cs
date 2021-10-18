using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeButton : MonoBehaviour
{
    public GameObject tagPrefab;
    public GameObject list;

    // Start is called before the first frame update
    void awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(800, 400, 80, 20), "button"))
        {
            GameObject tag = Instantiate(tagPrefab, list.transform);
            string tagNum = tag.transform.GetSiblingIndex().ToString();
            tag.name = tagNum;
            GameObject tagHeaderText = GameObject.Find(tagNum).transform.Find("Header").transform.Find("Text").gameObject;
            tagHeaderText.GetComponent<Text>().text = string.Format("{0}th Tag", tagNum);
            int textNum = GameObject.Find(tagNum).transform.Find("Body").transform.childCount;
            GameObject bodyText;
            for (int i = 0; i < textNum; i++)
            {
                bodyText = GameObject.Find(tagNum).transform.Find("Body").transform.GetChild(i).gameObject;
                bodyText.GetComponent<Text>().text = string.Format("{0}th Tag Info", i);
            }
        }
    }
}
