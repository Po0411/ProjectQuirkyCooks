using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    public float start_time;
    public TextMeshProUGUI timer_text;
    [Header("미니게임 UI")]
    public GameObject mini_game_UI;
    public MouseLook mouseLook;
    public CameraRotation camera_rotation;
    public PlayerController player_controller;
    public bool is_not_minigame;
    public InventoryManager inven_MG;
    [Header("횟수")]
    public int max_playing_count;   
    [SerializeField] private int playing_count;
    public float re_play_cool_time;
    [Header("썰기 미니게임")]
    public GameObject fill_obj;
    public int target_num;//목표치
    public float plus_num;//증가 수치
    public float now_num;//실제로 변경되는 수치
    public float down_fill_num;//수치가 내려가는 수치

    [Header("갈기 미니게임")]
/*    public float max_out_rimite;//최대 시간
    public float range_out_rimite;//범위 벗어날 수 있는 시간
    public bool range_out;*/
    public float bar_move_speed;//바가 움직이는 속도 최대치는 0.1 초당 0.02씩 증가 최대치 
    public float max_bar_move_range;//바 움직이는 범위 초당 0.1씩 증가 최대치는 1
    public float min_bar_move_range;//바 움직이는 범위 초당 0.1씩 증가 최대치는 1
    public bool L_R_switch;//참이면 아래, 거짓이면 위
    public Slider G_slider;
    public Slider G_line_slider;
    public Image taget_slider;
    public float G_line_move_speed;
    public float G_move_speed;
    public bool is_cliking;//클릭 중
    public float fill_plus_num;

    [Header("끓이기 미니게임")]
    public Slider B_slider;
    public Slider line_slider;
    public bool L_R;//참이면 - 거짓이면 +
    public float B_move_speed;
    public float line_move_speed;
    bool is_not_click = true;

    [Header("굽기 미니게임")]
    public List<int> arrow_list = new List<int>();//화살표 보관 리스트 //0=상/1=하/2=좌/3=우 
    public List<int> arrow_count_target = new List<int>();//목표 정답 횟수 //0이 첫번쨰 1이 두번째 2가 세번쨰 //1개//3개//5개
    public List<int> arrow_round_count = new List<int>();//라운드마다 나타나는 화살표 갯수 //3개//5개//7개
    public List<GameObject> arrow_obj=new List<GameObject>();
    public GameObject arrow_filld;
    public int now_complte_count;//완료 카운트
    public int now_arrow_num;
    public float max_input_cooltime;
    [SerializeField] float input_cooltime;
    [SerializeField]List<GameObject> ins_arrow_obj = new List<GameObject>();
    int playing_complte_count;
    public int count = 0;
    bool is_wait_input = false;

    [Tooltip("Event invoked when the mini‑game finishes. The boolean indicates success (true) or failure (false).")]
    public UnityEvent<bool> OnCompleted;

    bool success = false;
    private bool running = false;
    [SerializeField]float elapsed;

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
        if(!is_not_minigame) mini_game_UI.SetActive(true);
        mouseLook.enabled = false;
        player_controller.enabled = false;
        camera_rotation.enabled = false;
        inven_MG.enabled = false;
        //움직임, 인벤토리 락
        playing_count = max_playing_count;
        //쿨타임 설정(여러개면 여러개를 설정, 아니면 걍 진행
        switch (miniGameType)
        {
            case CookingMiniGameType.Boil: line_slider.value = Random.Range(0f, 1f); break;
            case CookingMiniGameType.Grill:
                {
                    now_complte_count = 0;
                    for (int i = 0; i < arrow_round_count[max_playing_count - playing_count]; i++)
                    {
                        //Debug.Log(arrow_round_count[max_playing_count - playing_count]);
                        arrow_list.Add(Random.Range(0, 4));
                        ins_arrow_obj.Add(Instantiate(arrow_obj[arrow_list[i]], transform.position, Quaternion.identity));
                        ins_arrow_obj[i].transform.SetParent(arrow_filld.transform);
                    }
                    now_arrow_num = arrow_list[0];
                }
                break;
        }
        //화살표 설정
        StartCoroutine(RunMiniGame(start_time));
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
        if (is_wait_input && miniGameType == CookingMiniGameType.Grill)
        {
            input_cooltime -=1 * Time.deltaTime;
            if(input_cooltime <= 0)is_wait_input = false;
        }
    }

    private IEnumerator RunMiniGame(float wait_time) //미니게임 작동 중 //미니게임 중 계속해서 처리가 필요한 작업들
    {
        if (is_random_time) elapsed = Random.Range(min_time,duration);
        else elapsed = duration;

        input_cooltime = max_input_cooltime;
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
                        if (L_R) B_slider.value += line_move_speed * Time.deltaTime;
                        else B_slider.value -= line_move_speed * Time.deltaTime;

                        if (B_slider.value == 1) L_R = false;
                        if (B_slider.value == 0) L_R = true;
                    }
                    break;
                case CookingMiniGameType.Grill://굽기
                    {

                        Debug.Log("[현재 카운트 :]"+ playing_complte_count);

                        if (count >= arrow_round_count[max_playing_count - playing_count])//입력 전부 받음
                        {
                            //Debug.Log("모든 화살표 입력 완료");
                            if (playing_complte_count >= arrow_count_target[max_playing_count - playing_count])
                            {
                                Debug.Log("반복 중 통과");
                                elapsed = 0;
                                break;
                            }
                            else 
                            {
                                elapsed = 0;
                                break;
                            }
                        }

                        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && !is_wait_input)
                        {
                            Debug.Log("상");
                            //is_wait_input = true;
                            //화살표 스테이더스에서 이미지 변경
                            if (now_arrow_num == 0)
                            {
                                ins_arrow_obj[count].GetComponent<Arrow_State>().Arrow_Image_Chage(true);
                                playing_complte_count += 1;
                                count += 1;
                            }
                            else
                            {
                                elapsed = 0;
                                playing_count = 0;
                                End_Mini_Game();
                                /* ins_arrow_obj[count].GetComponent<Arrow_State>().Arrow_Image_Chage(false);
                                 count += 1;*/
                            }
                        }
                        else if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && !is_wait_input)
                        {
                            Debug.Log("하");
                            //is_wait_input = true;
                            if (now_arrow_num == 1)
                            {
                                ins_arrow_obj[count].GetComponent<Arrow_State>().Arrow_Image_Chage(true);
                                playing_complte_count += 1;
                                count += 1;
                            }
                            else
                            {
                                elapsed = 0;
                                playing_count = 0;
                                End_Mini_Game();
                                /*                                ins_arrow_obj[count].GetComponent<Arrow_State>().Arrow_Image_Chage(false);
                                                                count += 1;*/
                            }
                        }
                        else if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && !is_wait_input)
                        {
                            Debug.Log("좌");
                            //is_wait_input = true;
                            if (now_arrow_num == 2)
                            {
                                ins_arrow_obj[count].GetComponent<Arrow_State>().Arrow_Image_Chage(true);
                                playing_complte_count += 1;
                                count += 1;
                            }
                            else
                            {
                                elapsed = 0;
                                playing_count = 0;
                                End_Mini_Game();
                                /*                ins_arrow_obj[count].GetComponent<Arrow_State>().Arrow_Image_Chage(false);
                                                count += 1;*/
                            }
                        }
                        else if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && !is_wait_input)
                        {
                            Debug.Log("우");
                            //is_wait_input = true;
                            if (now_arrow_num == 3)
                            {
                                ins_arrow_obj[count].GetComponent<Arrow_State>().Arrow_Image_Chage(true);
                                playing_complte_count += 1;
                                count += 1;
                            }
                            else
                            {
                                elapsed = 0;
                                playing_count = 0;
                                End_Mini_Game();
                                /*                                ins_arrow_obj[count].GetComponent<Arrow_State>().Arrow_Image_Chage(false);
                                                                count += 1;*/
                            }
                        }
                        if (count < arrow_list.Count && !is_wait_input) //정답 카운트가 화살표 목록의 개수 이하일 경우에만
                            now_arrow_num = arrow_list[count];

                        //비교하고, 입력이 맞다면 카운트 증가, 입력이 틀렸다면 아무작업도 안함
                        //전부 완료했을떄, 카운트가 이번 반복의 정답 횟수 이상이라면, 성공, 아니면 실패
                        //생성 방식은 프리팹에서 소환
                    } break;
                case CookingMiniGameType.Grind:
                    {
                        if (L_R_switch) G_line_slider.value += Mathf.Clamp(G_line_move_speed * Time.deltaTime, 0, 1);
                        else G_line_slider.value -= Mathf.Clamp(G_line_move_speed * Time.deltaTime, 0, 1);

                        if (G_line_slider.value == max_bar_move_range) L_R_switch = false;
                        else if (G_line_slider.value <= min_bar_move_range) L_R_switch = true;


                        if (G_line_slider.value >= max_bar_move_range - 0.001f)
                        {
                            L_R_switch = false;
                            if (max_bar_move_range < 1)
                                max_bar_move_range += Mathf.Clamp(0.1f, 0f, 1f);
                            else max_bar_move_range = 1;
                        }
                        else if (G_line_slider.value <= min_bar_move_range - 0.001f)
                        {
                            L_R_switch = true;
                            if (min_bar_move_range > 0)
                                min_bar_move_range -= Mathf.Clamp(0.1f, 0f, 1f);
                            else min_bar_move_range = 0;
                        }
                        //yield return null;
                        bar_move_speed += 0.01f;

                        if (is_cliking)
                        {
                            Debug.Log("눌러짐");
                            G_slider.value += G_move_speed * Time.deltaTime;
                        }
                        else
                        {
                            Debug.Log("때짐");
                            G_slider.value -= G_move_speed * Time.deltaTime;
                        }

                        if (G_slider.value > G_line_slider.value - 0.1f && G_slider.value < G_line_slider.value + 0.1f)//오차범위 +-0.1
                        {
                            taget_slider.fillAmount += fill_plus_num * Time.deltaTime;
                            if (taget_slider.fillAmount == 1)
                            {
                                success = true;
                                //Debug.Log("WDawf");
                                End_Mini_Game();
                            }
                        }
                    }
                    break;
                default:
                    {
                      if(is_not_minigame)   success = true;
                    }
                    break;
            }
            timer_text.text = elapsed.ToString("F0");
            yield return null;
        }

        switch (miniGameType)
        {
            case CookingMiniGameType.Boil:
                {
                    if (is_not_click)
                        playing_count = 0;
                }
                break;
            case CookingMiniGameType.Grill:
                {
                    if (playing_complte_count >= arrow_count_target[max_playing_count - playing_count])//지금 실패를 카운트 0으로 처리해서 그렇구나
                    {
                        Debug.Log("통과");
                        now_complte_count += 1;
                    }
                    else
                    {
                        playing_count = 0;
                    }
                    if (now_complte_count == max_playing_count)
                    {
                        Debug.Log("조건 충족");
                        success = true;
                    }
                }
                break;
        }

        playing_count -= 1;
        //Debug.Log(playing_count);
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
                    if (line_slider.value > B_slider.value - 0.15f && line_slider.value < B_slider.value + 0.15f)//오차범위 +-0.1
                    {
                        Debug.Log("현재 값"+ B_slider.value+"현제 기준"+ line_slider.value);
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
                    
                }
                break;
        }
    }


    public void End_Mini_Game()
    {
        Debug.Log("끝");
        StopAllCoroutines();
        OnCompleted?.Invoke(success);
        success = false;
        mini_game_state = Cooking_State.Finishd;
        mouseLook.enabled = true;
        player_controller.enabled = true;
        camera_rotation.enabled = true;
        inven_MG.enabled = true;
        if (!is_not_minigame)
        {
            mini_game_UI.SetActive(false);
        }
        Information_Reset();
    }

    private void Information_Reset()
    {
        StopAllCoroutines();
        running = false;
        timer_text.text = "Ready?";
        //playing_count = 0;
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
            case CookingMiniGameType.Grill://굽기
                {
                    arrow_list.Clear();
                    playing_complte_count = 0;
                    now_arrow_num = 0;
                    count = 0;

                    for (int i = 0; i < ins_arrow_obj.Count; i++) Destroy(ins_arrow_obj[i]);
                        ins_arrow_obj.Clear();
                    //화살표 다시 설정`
                    if (playing_count > 0)
                    {
                        for (int i = 0; i < arrow_round_count[max_playing_count - playing_count]; i++)
                        {
                            arrow_list.Add(Random.Range(0, 4));
                            ins_arrow_obj.Add(Instantiate(arrow_obj[arrow_list[i]], transform.position, Quaternion.identity));
                            ins_arrow_obj[i].transform.SetParent(arrow_filld.transform);
                        }
                    }
                }
                break;
        }
    }
}