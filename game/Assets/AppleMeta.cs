using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleMeta : MonoBehaviour
{
    public int number;
    public bool isOn = true;
    public bool isSelected = false;

    private Transform selectZone;

    private void Start()
    {
        TextMesh _txt = gameObject.GetComponentInChildren<TextMesh>();
        _txt.text = System.Convert.ToString(number);
        selectZone = transform.Find("SelectZone");
    }

    private void Update()
    {
        if (isOn)
            transform.gameObject.SetActive(true);
        else
            transform.gameObject.SetActive(false);

        if (isSelected)
            selectZone.gameObject.SetActive(true);
        else
            selectZone.gameObject.SetActive(false);
    }
}
