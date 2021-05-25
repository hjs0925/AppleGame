using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleMeta : MonoBehaviour
{
    public int number;
    public bool isOn;
    public bool isSelected;
    public bool isAnimated;
    public Vector2Int coor;

    private Transform selectZone;   // 선택되었을 때, 하이라이트
    private Rigidbody2D RB;

    private List<Vector2> throwForces;

    Vector2 throwForce1 = new Vector2(3.6f, 4.6f);
    Vector2 throwForce2 = new Vector2(3f, 5f);
    Vector2 throwForce3 = new Vector2(-3.6f, 4.6f);
    Vector2 throwForce4 = new Vector2(-3f, 5f);

    private void Start()
    {
        RB = gameObject.GetComponent<Rigidbody2D>();

        TextMesh _txt = gameObject.GetComponentInChildren<TextMesh>();
        _txt.text = System.Convert.ToString(number);

        selectZone = transform.Find("SelectZone");

        isOn = true;
        isSelected = false;
        isAnimated = false;

        throwForces = new List<Vector2>();
        throwForces.Add(throwForce1);
        throwForces.Add(throwForce2);
        throwForces.Add(throwForce3);
        throwForces.Add(throwForce4);
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

        if (isAnimated)
        {
            isAnimated = false;

            Vector3 nowPos = gameObject.transform.position;
            Vector3 comeForward = new Vector3(0f, 0f, -1f);
            gameObject.transform.position = nowPos + comeForward;

            RB.WakeUp();
            RB.bodyType = RigidbodyType2D.Dynamic;
            RB.gravityScale = 2f;
            RB.AddForce(throwForces[Random.Range(0, throwForces.Count)], ForceMode2D.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Collector"))
        {
            isOn = false;
        }
    }
}
