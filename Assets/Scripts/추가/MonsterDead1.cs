using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDead1 : MonoBehaviour
{
    public static bool dead = false;

    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(dead == true)
        {
            anim.SetBool("isDead", true);
        }
    }
}
