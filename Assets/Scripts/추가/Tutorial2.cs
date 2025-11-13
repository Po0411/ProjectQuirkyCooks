using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial2 : MonoBehaviour
{
    public GameObject targetUI;
    public GameObject recipe;
    // Start is called before the first frame update
    
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


        if (Input.GetKey(KeyCode.R))
        {
            if (!recipe.activeSelf)
            {
                recipe.SetActive(true);
            }
        }
        else
        {
            if (recipe.activeSelf)
            {
                recipe.SetActive(false);
            }
        }
    }
}
