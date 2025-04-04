using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public float fSliderBarTime = 120f;
    public bool isEnd = false;
    public GameControl gameControl;

    private float maxBarTime;

    public Image clockImage; 
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI comboTimeText;

    void Start()
    {
        maxBarTime = fSliderBarTime;

        if (clockImage != null)
            clockImage.fillAmount = 1f;

        if (timeText != null)
            timeText.text = Mathf.CeilToInt(fSliderBarTime).ToString();
            
        if (comboTimeText != null)
            comboTimeText.text = "";
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
            
        if (comboTimeText != null && gameControl != null)
        {
            float remainingComboTime = gameControl.comboTime - (Time.time - gameControl.comboDelta);
            if (remainingComboTime > 0 && gameControl.comboCnt > 0)
            {
                comboTimeText.text = $"콤보 {gameControl.comboCnt} - {Mathf.CeilToInt(remainingComboTime)}초";
            }
            else
            {
                comboTimeText.text = "";
            }
        }
    }
}
