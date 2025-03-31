using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public float fSliderBarTime = 120f; // 전체 시간
    public bool isEnd = false;

    private float maxBarTime;

    public Image clockImage; // 시계 UI 이미지
    public TextMeshProUGUI timeText; // 남은 시간 숫자로 표시 (선택)

    void Start()
    {
        maxBarTime = fSliderBarTime;

        // 초기값 설정
        if (clockImage != null)
            clockImage.fillAmount = 1f;

        if (timeText != null)
            timeText.text = Mathf.CeilToInt(fSliderBarTime).ToString();
    }

    void Update()
    {
        fSliderBarTime -= Time.deltaTime;

        if (fSliderBarTime < 0)
        {
            isEnd = true;
            fSliderBarTime = 0f;
        }

        float ratio = fSliderBarTime / maxBarTime;

        if (clockImage != null)
            clockImage.fillAmount = ratio;

        if (timeText != null)
            timeText.text = Mathf.CeilToInt(fSliderBarTime).ToString();
    }
}
