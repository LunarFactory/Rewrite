using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UpgradeUIController : MonoBehaviour
    {
        public static UpgradeUIController Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject mainPanel;

        [Header("Lists")]
        [SerializeField] private Transform activePartsContent; // 왼쪽: 적용된 파츠 목록
        [SerializeField] private Transform shopContent;        // 오른쪽: 구매 가능한 파츠 목록

        private void Awake()
        {
            Instance = this;
            mainPanel.SetActive(false); // 처음엔 꺼둠
        }

        public void Open()
        {
            mainPanel.SetActive(true);

            // 1. 게임 일시정지
            Time.timeScale = 0f;

            // 2. 마우스 커서 활성화
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 3. 리스트 갱신 (기존 아이템 삭제 후 새로 생성하는 로직 권장)
            RefreshAllLists();
        }

        public void Close()
        {
            mainPanel.SetActive(false);

            // 1. 게임 재개
            Time.timeScale = 1f;

            // 2. 마우스 커서 다시 숨기기 (인게임용)
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void RefreshAllLists()
        {
            // 여기에 MetaUpgradeManager에서 데이터를 가져와 
            // 왼쪽(적용된 것)과 오른쪽(상점) UI 요소를 생성/갱신하는 코드를 넣습니다.
            Debug.Log("보급 포트 UI 리스트 갱신 중...");
        }

        // UI 내의 '나가기' 버튼 등에 연결
        public void OnExitButtonClick() => Close();
        
        private void Update()
        {
            if (mainPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }
    }
}