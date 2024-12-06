using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    public string enemyName;
    public int maxHp;
    public int nowHp;
    public int atkDmg;
    public int enemyExp;
    public float atkSpeed;
    public float moveSpeed;
    public float atkRange;
    public float fieldOfVision;
    public bool attacked;
    public bool isGrounded;
    public bool tookDamage;

    public GameObject prfHpBar;
    public GameObject canvas;
    RectTransform hpBar;
    public float height = 1.7f;

    public GameObject damageTextPrefab; // ������ ǥ���� �ؽ�Ʈ ������

    public Elf elf;
    Image nowHpbar;
    public Animator enemyAnimator;

    void Start()
    {
        hpBar = Instantiate(prfHpBar, canvas.transform).GetComponent<RectTransform>();
        if (name.Equals("Enemy1"))
        {
            SetEnemyStatus("Enemy1", 100, 10, 2.0f, 2, 0.5f, 7f);
        }
        nowHpbar = hpBar.transform.GetChild(0).GetComponent<Image>();
        SetAttackSpeed(atkSpeed);
    }

    void Update()
    {
        Vector3 _hpBarPos = Camera.main.WorldToScreenPoint
            (new Vector3(transform.position.x, transform.position.y + height, 0));
        hpBar.position = _hpBarPos;
        nowHpbar.fillAmount = (float)nowHp / (float)maxHp;

        if (elf.attacked == true)
            elf.attacked = false;
    }

    public void Die()
    {
        enemyAnimator.SetTrigger("die");            // die �ִϸ��̼� ����
        GetComponent<EnemyAI>().enabled = false;    // ���� ��Ȱ��ȭ
        GetComponent<Collider2D>().enabled = false; // �浹ü ��Ȱ��ȭ
        Destroy(gameObject, 1);                     // 1�� �� ����
        Destroy(hpBar.gameObject, 1);               // 1�� �� ü�¹� ����
        elf.nowExp += enemyExp;
        Debug.Log("���� ����ġ : " + elf.nowExp);
    } 

    void AttackTrue() { attacked = true; }

    void AttackFalse() { attacked = false; }

    private void SetEnemyStatus(string _enemyName, int _maxHp, int _atkDmg, float _atkSpeed, float _moveSpeed, float _atkRange, float _fieldOfVision)
    {
        enemyName = _enemyName;
        maxHp = _maxHp;
        nowHp = _maxHp;
        atkDmg = _atkDmg;
        atkSpeed = _atkSpeed;
        moveSpeed = _moveSpeed;
        atkRange = _atkRange;
        fieldOfVision = _fieldOfVision;
    }

    void SetAttackSpeed(float speed)
    {
        // enemyAnimator.SetFloat("attackSpeed", speed);
    }
}
