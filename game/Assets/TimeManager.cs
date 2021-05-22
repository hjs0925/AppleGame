using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    Slider slTimer;
    public float fSliderBarTime;
    public bool isEnd = false;

    private float maxBarTime;

    // Start is called before the first frame update
    void Start()
    {
        slTimer = GetComponent<Slider>();
        maxBarTime = fSliderBarTime;
    }

    // Update is called once per frame
    void Update()
    {
        fSliderBarTime -= Time.deltaTime;
        if (fSliderBarTime < 0)
        {
            isEnd = true;
            fSliderBarTime = 0f;
        }

        slTimer.value = fSliderBarTime / maxBarTime;

    }
}
