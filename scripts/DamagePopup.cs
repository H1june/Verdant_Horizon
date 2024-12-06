using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed;     // �ؽ�Ʈ �̵� �ӵ�
    public float alphaSpeed;    // ���� ��ȭ �ӵ�

    public Elf elf;
    public int damage;          // ������ ��

    private TextMeshProUGUI text; // TextMeshProUGUI ������Ʈ
    
    private Color alpha;

    void Start()
    {
        elf = FindObjectOfType<Elf>();  // Elf�� ã��
                                        // ������ ���� ���� ������ elf.atkDmg�� ���
        if (damage == 0 && elf != null)
        {
            damage = elf.atkDmg;
        }

        text = GetComponent<TextMeshProUGUI>();
        text.text = damage.ToString();  // ������ �� �ؽ�Ʈ�� ����
        alpha = text.color;
    }

    void Update()
    {
        // �ؽ�Ʈ ���� �̵�
        transform.Translate(new Vector3(0, moveSpeed * Time.deltaTime, 0));

        // ���İ� ���� (���� ����)
        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaSpeed);
        text.color = new Color(alpha.r, alpha.g, alpha.b, alpha.a); // ���� ����
    }

    void DestroyObject()
    {
        Destroy(gameObject);  // ��ü ����
    }
}
