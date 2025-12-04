using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TutorialSlideManager : MonoBehaviour
{

    [SerializeField] private GameObject tutorialPanel; // Panel chứa Text và nút Next
    [SerializeField] private Image tutorialImage; // Ảnh minh họa
    [SerializeField] private TextMeshProUGUI contentText; // Nơi hiện chữ
    [SerializeField] private Button nextButton; // Nút Next
    [SerializeField] private Button skipButton; // Nút Skip (tùy chọn)

    [Header("Data")]
    // Danh sách các bước hướng dẫn
    [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

    private int currentStepIndex = -1;

    private void Start()
    {
      
        // Gán sự kiện cho nút
        nextButton.onClick.AddListener(NextStep);

        if (skipButton != null)
            skipButton.onClick.AddListener(EndTutorial);

        // Bắt đầu ngay khi vào game (hoặc bạn có thể gọi hàm này từ nơi khác)
        StartTutorial();

    }

    public void StartTutorial()
    {
        if (steps.Count == 0) return;

        tutorialPanel.SetActive(true);
        currentStepIndex = -1;
        NextStep(); // Chạy bước đầu tiên
    }

    public void NextStep()
    {
        currentStepIndex++;

        // Kiểm tra xem đã hết các bước chưa
        if (currentStepIndex >= steps.Count)
        {
            EndTutorial();
            return;
        }

        // Lấy dữ liệu bước hiện tại
        TutorialStep currentStep = steps[currentStepIndex];

        // 1. Cập nhật nội dung chữ
        contentText.text = currentStep.instructionText;
        // 2. Cập nhật ảnh
        tutorialImage.sprite = currentStep.image;

    }

    public void EndTutorial()
    {
        Debug.Log("Tutorial Finished!");
        tutorialPanel.SetActive(false);
      

        // Lưu lại là người chơi đã xem hướng dẫn xong (để lần sau không hiện nữa)
        PlayerPrefs.SetInt("HasPlayedTutorial", 1);
    }
}



[System.Serializable]
public class TutorialStep
{
    [Tooltip("Ghi chú cho dễ nhớ (ví dụ: Bước 1 - Nút Mua)")]
    public string note;

    [Tooltip("Nội dung hướng dẫn hiển thị")]
    [TextArea(3, 5)]
    public string instructionText;
    [Tooltip("Ảnh")]
    public Sprite image;
}