using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    public GameObject levelUpPanel;  // ������ â (Panel)
    public Text levelText;
    public Button closeButton;       // â�� �ݴ� ��ư
    public Elf elf;

    void Start() 
    {
        // ������ ���� ������ â�� ��Ȱ��ȭ�ص�
        levelUpPanel.SetActive(false);

        // ��ư Ŭ�� �� â�� �ݴ� �Լ��� ����
        closeButton.onClick.AddListener(CloseLevelUpPanel);
    }

    // ������ â�� ���� �޽����� �����ϴ� �Լ�
    public void ShowLevelUpPanel(int newLevel)
    {
        levelUpPanel.SetActive(true);  // ������ �г� Ȱ��ȭ

        // ���� ������ ǥ���ϴ� �ؽ�Ʈ UI ��Ҹ� ã�� ������Ʈ
        levelText.text = "���� " + newLevel + "�� ������!";
    }


    // ������ â�� �ݴ� �Լ�
    void CloseLevelUpPanel()
    {
        levelUpPanel.SetActive(false);
    }
}
