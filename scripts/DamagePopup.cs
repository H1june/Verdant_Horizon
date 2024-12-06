using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed;     // 텍스트 이동 속도
    public float alphaSpeed;    // 투명도 변화 속도

    public Elf elf;
    public int damage;          // 데미지 값

    private TextMeshProUGUI text; // TextMeshProUGUI 컴포넌트
    
    private Color alpha;

    void Start()
    {
        elf = FindObjectOfType<Elf>();  // Elf를 찾음
                                        // 데미지 값을 받지 않으면 elf.atkDmg을 사용
        if (damage == 0 && elf != null)
        {
            damage = elf.atkDmg;
        }

        text = GetComponent<TextMeshProUGUI>();
        text.text = damage.ToString();  // 데미지 값 텍스트로 설정
        alpha = text.color;
    }

    void Update()
    {
        // 텍스트 위로 이동
        transform.Translate(new Vector3(0, moveSpeed * Time.deltaTime, 0));

        // 알파값 감소 (투명도 조정)
        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaSpeed);
        text.color = new Color(alpha.r, alpha.g, alpha.b, alpha.a); // 투명도 변경
    }

    void DestroyObject()
    {
        Destroy(gameObject);  // 객체 삭제
    }
}
