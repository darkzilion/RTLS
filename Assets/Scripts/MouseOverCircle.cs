using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOverCircle : MonoBehaviour
{
    private void OnMouseEnter()
    {
        gameObject.GetComponent<Transform>().localScale = new Vector3(0.5f, 0.5f);
        gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
    }

    private void OnMouseExit()
    {
        gameObject.GetComponent<Transform>().localScale = new Vector3(0.2f, 0.2f);
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
    }
}
