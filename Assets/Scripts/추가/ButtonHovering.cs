using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHovering : MonoBehaviour
{
    public GameObject hidButton;

    public void mouseEnter()
    {
        hidButton.SetActive(true);
    }

    public void mouseExit()
    {
        hidButton.SetActive(false);
    }
}
