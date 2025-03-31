using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public float fSliderBarTime;
    public bool isEnd = false;
    private float maxBarTime;

    public Image clockImage; // 시계 이미지 연결

    void Start()
    {
        maxBarTime = fSliderBarTime;
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
        clockImage.fillAmount = ratio;
    }
}
