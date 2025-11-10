using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public GameObject targetUI;

    void Start()
    {
        if (targetUI != null)
        {
            targetUI.SetActive(false);
        }
    }

    void Update()
    {
        if (targetUI == null)
        {
            return;
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            if (!targetUI.activeSelf)
            {
                targetUI.SetActive(true);
            }
        }
        else
        {
            if (targetUI.activeSelf)
            {
                targetUI.SetActive(false);
            }
        }
    }
}
