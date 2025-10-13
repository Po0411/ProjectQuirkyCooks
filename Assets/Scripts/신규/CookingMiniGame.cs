using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Enumerates the different cooking mini‑games described in the design document. Each
/// value corresponds to a unique interaction (slicing, grilling, grinding, boiling,
/// frying, plating) that the player can perform when preparing dishes. These mini‑
/// games are deliberately abstracted here; the actual interactive logic (mouse
/// swipes, timing bars, etc.) should be implemented within the RunMiniGame coroutine
/// as appropriate for your project.
/// </summary>
public enum CookingMiniGameType
{
    Slice,   // 썰기
    Grill,   // 굽기
    Grind,   // 갈기
    Boil,    // 끓이기
    Fry,     // 튀기기
    Plate    // 플레이팅
}
public enum Cooking_State { Ready, Playing, Finishd }

/// <summary>
/// Represents a single cooking mini‑game session. When started, it runs for a
/// specified duration and then reports a success or failure outcome via the
/// OnCompleted event. The successThreshold controls the probability of success in
/// this simplified implementation. Developers should replace the contents of
/// RunMiniGame with actual mini‑game mechanics and determine success based on
/// player input rather than random chance.
/// </summary>
public class CookingMiniGame : MonoBehaviour
{
    [Header("Mini‑game Settings")]
    public CookingMiniGameType miniGameType = CookingMiniGameType.Slice;
    public Cooking_State mini_game_state = Cooking_State.Ready;
    [Header("제한시간")]
    [Tooltip("How long the mini‑game lasts in seconds before determining a result.")]
    public float duration = 5f;//지속 되는 시간(제한시간)
    public bool is_random_time;
    public float min_time;
    public bool is_multi_time;//쿨타임이 여러개인지
    public TextMeshProUGUI timer_text;
    [Header("미니게임 UI")]
    public GameObject mini_game_UI;
    public MouseLook mouseLook;
    [Header("횟수")]
    public int max_playing_count;   
    int playing_count;
    public float re_play_cool_time;
    [Header("썰기 미니게임")]
    public GameObject fill_obj;
    public int target_num;//목표치
    public float plus_num;//증가 수치
    public float now_num;//실제로 변경되는 수치
    public float down_fill_num;//수치가 내려가는 수치

    [Header("갈기 미니게임")]
    public float max_out_rimite;//최대 시간
    public float range_out_rimite;//범위 벗어날 수 있는 시간
    public bool range_out;
    public float bar_move_speed;//바가 움직이는 속도 최대치는 0.1 초당 0.02씩 증가 최대치 
    public float max_bar_move_range;//바 움직이는 범위 초당 0.1씩 증가 최대치는 1
    public float min_bar_move_range;//바 움직이는 범위 초당 0.1씩 증가 최대치는 1
    public bool L_R_switch;//참이면 아래, 거짓이면 위
    public Slider G_slider;

    [Header("끓이기 미니게임")]
    public Slider B_slider;
    public Slider line_slider;
    public bool L_R;//참이면 - 거짓이면 +
    public float B_move_speed;

    [Header("굽기 미니게임")]
    public List<int> arrow_list = new List<int>();//화살표 보관 리스트 //0=상/1=하/2=좌/3=우
    public List<int> arrow_count = new List<int>();//화살표 갯수 리스트 //3
    public List<Sprite> arrow_sprite = new List<Sprite>();//화살표 이미지
    public List<Sprite> fail_arrow_sprite=new List<Sprite>();//실패 화살표 이미지

    [Tooltip("Event invoked when the mini‑game finishes. The boolean indicates success (true) or failure (false).")]
    [HideInInspector] public UnityEvent<bool> OnCompleted;


    bool success = false;
    private bool running = false;
    float elapsed;

    /// <summary>
    /// Begins the mini‑game if it is not already in progress. This method can be
    /// called by an external script (e.g. a cooking station) when the player
    /// interacts with the station. Internally it starts a coroutine that waits
    /// for the specified duration and then determines the outcome.
    /// </summary>
    /// 
    private void Start()
    {
        if (max_playing_count == 0) max_playing_count = 1;//초기화

    }


    public void StartMiniGame()
    {
        if (running) return;
        running = true;
        mini_game_state = Cooking_State.Ready;
        mini_game_UI.SetActive(true);
        mouseLook.enabled = false;
        playing_count = max_playing_count;

        if (miniGameType == CookingMiniGameType.Boil) line_slider.value = Random.Range(0f, 1f);

        StartCoroutine(RunMiniGame(3f));
    }


    /// <summary>
    /// Coroutine that simulates the mini‑game. Replace the loop body with
    /// interactive mechanics (UI bars, button presses, etc.) to create a real
    /// gameplay experience. When the timer elapses, a success or failure is
    /// calculated using the successThreshold and the result is emitted via
    /// OnCompleted.
    /// </summary>
    /// 


    private void Update()
    {
        if (mini_game_state == Cooking_State.Playing)
            elapsed -= 1 * Time.deltaTime;
    }

    private IEnumerator RunMiniGame(float wait_time) //미니게임 작동 중
    {
        if (is_random_time) elapsed = Random.Range(min_time,duration);
        else elapsed = duration;
        yield return new WaitForSeconds(wait_time);

        Debug.LogWarning("스타트");
        mini_game_state = Cooking_State.Playing;
        while (elapsed > 0 && mini_game_state == Cooking_State.Playing)
        {
            switch (miniGameType)
            {
                case CookingMiniGameType.Slice:
                    {
                        now_num -= down_fill_num * Time.deltaTime;
                        fill_obj.GetComponent<Image>().fillAmount = now_num / target_num; 
                    }
                    break;
                case CookingMiniGameType.Plate://플레이팅
                    {
                    }
                    break;
                case CookingMiniGameType.Fry://튀기기
                    {
                    }
                    break;
                case CookingMiniGameType.Boil://끓이기
                    {
                        if (L_R) B_slider.value += B_move_speed * Time.deltaTime;
                        else B_slider.value -= B_move_speed * Time.deltaTime;

                        if (B_slider.value == 1) L_R = false;
                        if (B_slider.value == 0) L_R = true;
                    }
                    break;
                case CookingMiniGameType.Grill://굽기
                    {

                    }
                    break;
                case CookingMiniGameType.Grind:
                    {
                        if (L_R_switch) G_slider.value += Mathf.Clamp( bar_move_speed * Time.deltaTime, 0, 1);
                        else G_slider.value -= Mathf.Clamp(bar_move_speed * Time.deltaTime, 0, 1);


                        if (G_slider.value == max_bar_move_range) L_R_switch = false;
                        else if (G_slider.value <= min_bar_move_range) L_R_switch = true;

                        if (G_slider.value >= max_bar_move_range - 0.001f)
                        {
                            L_R_switch = false;
                            if (max_bar_move_range < 1)
                                max_bar_move_range += Mathf.Clamp(0.1f, 0f, 1f);
                            else max_bar_move_range = 1;
                        }
                        else if (G_slider.value <= min_bar_move_range - 0.001f)
                        {
                            L_R_switch = true;
                            if (min_bar_move_range > 0)
                                min_bar_move_range -= Mathf.Clamp(0.1f, 0f, 1f);
                            else min_bar_move_range = 0;
                        }
                        //yield return null;
                        bar_move_speed += 0.01f;
                    }
                    break;
            }
            timer_text.text = elapsed.ToString("F0");
            yield return null;
        }
        playing_count -= 1;
        Debug.Log(playing_count);
        if (playing_count <= 0) End_Mini_Game();
        else
        {
            Information_Reset();    
            StartCoroutine(RunMiniGame(re_play_cool_time));
        }
    }

    public void UI_click_input()//UI의 입력이 들어왔을 떄 작동
    {
        if (mini_game_state != Cooking_State.Playing) return;

        switch (miniGameType)
        {
            case CookingMiniGameType.Slice://썰기
                {
                    //일정 범위까지 연타
                    now_num += plus_num;
                    //fill_obj.GetComponent<Image>().fillAmount = now_num / target_num;
                    if (now_num >= target_num) mini_game_state = Cooking_State.Finishd;
                    success = now_num >= target_num;
                }
                break;
            case CookingMiniGameType.Plate://플레이팅
                {
                    //이건 그냥 플레이팅인듯?
                }
                break;
            case CookingMiniGameType.Fry://튀기기
                {
                    //특정 초의 이후, 이내의 범위 내에서 버튼을 누른다
                    //이건 기다렸다가 하는건가? 시간초맞추기?
                }
                break;
            case CookingMiniGameType.Boil://끓이기
                {
                    //일정위치에 움직이는 선을 멈추게한다. 
                    if (line_slider.value > B_slider.value - 0.1f && line_slider.value < line_slider.value + 0.1f)//오차범위 +-0.1
                    {
                        Debug.Log("통과");
                        playing_count -= 1;
                        Debug.LogWarning(playing_count);
                        if (playing_count <= 0)
                        {
                            success = true;
                            End_Mini_Game();
                        }
                        else
                        {
                            B_move_speed += 0.5f;
                            //elapsed = duration;
                            StopAllCoroutines();
                            if (miniGameType == CookingMiniGameType.Boil) line_slider.value = Random.Range(0f, 1f);
                            StartCoroutine(RunMiniGame(re_play_cool_time));
                        }
                    }
                    else End_Mini_Game();
                }
                break;
            case CookingMiniGameType.Grill://굽기
                {
                    //제시된 방향키나 wasd를 누른다 
                    //일단 랜덤으로 0~3의 숫자 생성(방향키), 생성 최대 갯수, 최소개수
                }
                break;
            case CookingMiniGameType.Grind://갈기
                {
                    //일정시간동안 움직이는 선(범위)에서 마우스가 벗어나지 않도록 한다. //스타듀벨리 낚시?
                    StartCoroutine(Num_Repeat_Chage(0.1f));
                    success = true;
                }
                break;
        }
    }

    IEnumerator Num_Repeat_Chage(float minus_num)
    {
        range_out_rimite = max_out_rimite;
        while (range_out && range_out_rimite > 0f)
        {
            range_out_rimite -= 1*Time.deltaTime;

            if(range_out_rimite <= 0)
            {
                End_Mini_Game();
                break;
            }
            yield return null;
        }
    }   


    public void End_Mini_Game()
    {
        Debug.Log("끝");
        StopAllCoroutines();
        mini_game_state = Cooking_State.Finishd;
        OnCompleted?.Invoke(success);
        mouseLook.enabled = true;
        mini_game_UI.SetActive(false);
        Information_Reset();
    }

    private void Information_Reset()
    {
        StopAllCoroutines();
        running = false;
        timer_text.text = "0";
        playing_count = 0;
        switch (miniGameType)
        {
            case CookingMiniGameType.Slice:
                {
                    now_num = 0;
                    fill_obj.GetComponent<Image>().fillAmount = 0;
                }
                break;
            case CookingMiniGameType.Grind:
                {
                    L_R_switch = false;
                    max_bar_move_range = 0.6f;
                    min_bar_move_range = 0.4f;
                    bar_move_speed = 1f;
                    G_slider.value = 0.5f;
                }
                break;
            case CookingMiniGameType.Boil://끓이기
                {
                    B_slider.value = 0.5f;
                    B_move_speed = 1;
                    L_R = false;
                }
                break;
        }
    }
}