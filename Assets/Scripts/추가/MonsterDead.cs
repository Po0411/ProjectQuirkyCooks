using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDead : MonoBehaviour
{
    private Animator animator;

    public static bool isDead = false;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // 테스트를 위해 키 입력으로 Die() 함수를 호출하는 예시
    void Update()
    {
         if (isDead == true)
        {
            animator.SetTrigger("Dead");
        }
    }
}
