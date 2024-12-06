using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    public GameObject levelUpPanel;  // 레벨업 창 (Panel)
    public Text levelText;
    public Button closeButton;       // 창을 닫는 버튼
    public Elf elf;

    void Start() 
    {
        // 시작할 때는 레벨업 창을 비활성화해둠
        levelUpPanel.SetActive(false);

        // 버튼 클릭 시 창을 닫는 함수를 연결
        closeButton.onClick.AddListener(CloseLevelUpPanel);
    }

    // 레벨업 창을 열고 메시지를 설정하는 함수
    public void ShowLevelUpPanel(int newLevel)
    {
        levelUpPanel.SetActive(true);  // 레벨업 패널 활성화

        // 레벨 정보를 표시하는 텍스트 UI 요소를 찾고 업데이트
        levelText.text = "레벨 " + newLevel + "로 레벨업!";
    }


    // 레벨업 창을 닫는 함수
    void CloseLevelUpPanel()
    {
        levelUpPanel.SetActive(false);
    }
}
