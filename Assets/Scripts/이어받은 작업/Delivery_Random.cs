using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_Random : MonoBehaviour
{
    public float max_summon_cool_time;
    public List<DeliveryTarget> blue_npc = new List<DeliveryTarget>();
    public List<DeliveryTarget> red_npc = new List<DeliveryTarget>();
    public List<DeliveryTarget> green_npc = new List<DeliveryTarget>();

    public bool blue_on;
    public bool red_on;
    public bool green_on;

    bool blue_complte;
    bool red_complte;
    bool green_complte;

    float blue_cool_time;
    float red_cool_time;
    float green_cool_time;

    //DeliveryTarget now_deliry

    // Start is called before the first frame update
    void Start()
    {
/*        blue_cool_time = max_summon_cool_time;
        red_cool_time=max_summon_cool_time;
        green_cool_time = max_summon_cool_time;*/
    }

    // Update is called once per frame
    void Update()
    {

        //이미 소환된 놈들 중 랜덤으로 배달 설정


        //쿨타임
        if (blue_on && !blue_complte)
        {
            blue_cool_time += Time.deltaTime;
        }
        if (red_on && !red_complte)
        {
            red_cool_time += Time.deltaTime;
        }
        if (green_on && !green_complte)
        {
            green_cool_time += Time.deltaTime;
        }


        if (blue_cool_time >= max_summon_cool_time) //쿨타임이 다 사라졌을 시 블루
        {
            blue_on = true;
            //배달 설정
        }

        //쿨타임이 다 사라졌을 시 레드

        //쿨타임이 다 사라졌을 시 그린
    }

    void Delivery_Summon(int color)
    {
        switch (color)
        {
            case 0:
                {
                    blue_npc[Random.Range(0, blue_npc.Count)].enabled = true;
                    blue_complte = true;
                } break;//파랑
            case 1:
                {
                    red_npc[Random.Range(0, blue_npc.Count)].enabled = true;
                    red_complte = true;
                } break;//빨강
            case 2:
                {
                    green_npc[Random.Range(0, blue_npc.Count)].enabled = true;
                    green_complte = true;
                } break;//초록
        }
       
    }
}
