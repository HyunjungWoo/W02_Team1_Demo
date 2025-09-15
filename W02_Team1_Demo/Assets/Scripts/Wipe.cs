using UnityEngine;

public class Wipe : MonoBehaviour
{
    public static Wipe Instance { get; private set; }

    public Animator animator; // 인스펙터에 연결할 Animator

    public Vector3 offset; // 카메라와의 거리 차이
    void Awake()
    {
        // 싱글톤 패턴: 인스턴스가 없을 때만 생성
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        animator = GetComponent<Animator>();
    }


    // 이 함수를 애니메이션 이벤트로 호출
    public void ResetIsDeadParameter()
    {
        animator.SetBool("IsDead", false);
    }
    // 외부에서 호출할 애니메이션 재생 함수
    public void StartCloseWipe(Vector3 pos)
    {
        transform.position = pos;

        if (animator != null)
        {
            animator.SetTrigger("PlayWipe");
        }
    }
}
