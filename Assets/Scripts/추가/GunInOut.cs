using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GunInOut : MonoBehaviour
{
    private Animator anim;
    public GameObject hidingObject;

    public static bool gunInOut = false;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() 
    {
        if (Input.GetMouseButtonDown(1))
        {
            gunInOut = !gunInOut;
            anim.SetBool("gunOut", gunInOut);

        }
        else if(Input.GetMouseButtonUp(1))
        {
            gunInOut = !gunInOut;
            anim.SetBool("gunOut", gunInOut);
        }

        if (hidingObject != null && gunInOut == true)
        {
            hidingObject.SetActive(true);
        }
        else if(hidingObject != null && gunInOut == false)
        {
            hidingObject.SetActive(false);
        }
    }
}