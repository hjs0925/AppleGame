using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AddScore : MonoBehaviour
{
    public float moveSpeed;
    public float destroyTime;
    private TextMeshProUGUI textMeshPro;
    private Vector2 spawnPos;

    private void Awake()
    {
        textMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void print(int _score, int combo=0, bool isMany=false)
    {
        string _text = string.Format("+{0}", _score);

        if (combo != 0)
            _text += string.Format("\nCombo! +{0}", combo*5);

        if (isMany)
            _text += "\nMulti! +10";

        textMeshPro.text = _text;
    }

    // Update is called once per frame
    void Update()
    {
        spawnPos.Set(gameObject.transform.position.x,
            gameObject.transform.position.y + (moveSpeed + Time.deltaTime));

        gameObject.transform.position = spawnPos;

        destroyTime -= Time.deltaTime;

        if (destroyTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}
