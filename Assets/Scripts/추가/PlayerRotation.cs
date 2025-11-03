using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (GunInOut.gunInOut == true)
            {
                transform.Rotate(Vector3.up, 50, Space.Self);

            }
        }
        if(Input.GetMouseButtonUp(1))
        {
            if(GunInOut.gunInOut == false)
            {
                transform.Rotate(Vector3.up, -50, Space.Self);
            }
        }
    }
}
