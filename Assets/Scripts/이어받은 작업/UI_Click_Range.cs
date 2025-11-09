using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Playing_Type { Click, maintain}

public class UI_Click_Range : MonoBehaviour, IPointerDownHandler,IPointerUpHandler//  , IPointerEnterHandler, IPointerExitHandler
{
    public GameObject mini_game_obj;
    public Playing_Type type;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerDown(PointerEventData data)
    {
        if (mini_game_obj.GetComponent<CookingMiniGame>().miniGameType == CookingMiniGameType.Grind)
            mini_game_obj.GetComponent<CookingMiniGame>().is_cliking = true;
        if (type == Playing_Type.Click)
            mini_game_obj.GetComponent<CookingMiniGame>().UI_click_input();//미니게임 진행 중의 상태에 맞는 올바른 동작 실행
    }
    public void OnPointerUp(PointerEventData data)
    {
        if (mini_game_obj.GetComponent<CookingMiniGame>().miniGameType == CookingMiniGameType.Grind)
        {
            mini_game_obj.GetComponent<CookingMiniGame>().is_cliking = false;
            if (type == Playing_Type.Click)
                mini_game_obj.GetComponent<CookingMiniGame>().UI_click_input();//미니게임 진행 중의 상태에 맞는 올바른 동작 실행
        }
    }

    /*    public void OnPointerEnter(PointerEventData data)
        {
            if (mini_game_obj.GetComponent<CookingMiniGame>().miniGameType == CookingMiniGameType.Grind)
            {
                mini_game_obj.GetComponent<CookingMiniGame>().range_out = false;
                mini_game_obj.GetComponent<CookingMiniGame>().range_out_rimite = 1.5f;
            }
        }
        public void OnPointerExit(PointerEventData data)
        {
            if (mini_game_obj.GetComponent<CookingMiniGame>().miniGameType == CookingMiniGameType.Grind)
                mini_game_obj.GetComponent<CookingMiniGame>().range_out = true;
            mini_game_obj.GetComponent<CookingMiniGame>().UI_click_input();//미니게임 진행 중의 상태에 맞는 올바른 동작 실행
        }*/
}
