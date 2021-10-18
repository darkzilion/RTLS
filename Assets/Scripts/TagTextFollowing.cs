using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagTextFollowing : MonoBehaviour
{
    private GameObject tagObject;
    // Start is called before the first frame update
    void Start()
    {
        tagObject = GameObject.Find(gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        float offsetPosY = tagObject.transform.position.y + 1.5f;
    }
}
