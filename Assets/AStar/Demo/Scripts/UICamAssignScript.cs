using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICamAssignScript : MonoBehaviour
{
    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.worldCamera = Camera.main;
    }
}
