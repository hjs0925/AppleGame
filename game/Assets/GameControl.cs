using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameControl : MonoBehaviour
{
    public enum GameMode { Add, Subtract }

    [Header("Editor Setting")]
    public GameObject canvas;
    public GameObject apple;
    public RectTransform selectBox;
    public RectTransform collideBox;
    public TextMeshProUGUI scoreText;
    public GameObject gameOver;
    public TextMeshProUGUI gameOverText;
    public GameObject timeSlider;
    public TextMeshProUGUI addScoreText;
    public RectTransform hintBox;
    public TextMeshProUGUI modeText;

    [Header("Game Settings")]
    public int horizontalLength;
    public int verticalLength;
    public float comboTime;

    [Header("Only for Read")]
    public int nothing;
    public int score = 0;
    public float comboDelta = 0f;
    public int comboCnt;

    public GameMode currentMode = GameMode.Add;

    private Vector3 zeroPos;
    private Vector2 startpos = Vector2.zero;
    private Rect selectRect = new Rect();

    private List<AppleMeta> applesList;
    private List<AppleMeta> selectList;

    private TimeManager TM;

    private int[,] mapApple;
    public List<Vector4> hints;
    private List<Vector2Int> answerCoors;

    private void Awake()
    {
        zeroPos = apple.transform.position;
        applesList = new List<AppleMeta>();
        selectList = new List<AppleMeta>();
        mapApple = new int[verticalLength, horizontalLength];
        hints = new List<Vector4>();

        for (int i = 0; i < verticalLength; i++)
        {
            for (int j = 0; j < horizontalLength; j++)
            {
                Vector3 newPos = new Vector3(zeroPos.x + j, zeroPos.y + i, zeroPos.z);
                GameObject newApple = Instantiate(apple, newPos, Quaternion.identity);
                AppleMeta _am = newApple.GetComponent<AppleMeta>();
                newApple.name = string.Format("Apple ({0}, {1})", i, j);
                int _number = Random.Range(1, 10);
                _am.number = _number;
                _am.coor = new Vector2Int(i, j);
                mapApple[i, j] = _number;
                applesList.Add(_am);
            }
        }
    }

    void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        foreach (AppleMeta _apple in applesList)
        {
            _apple.gameObject.SetActive(true);
            _apple.isOn = true;
        }

        TM = timeSlider.GetComponent<TimeManager>();
        gameOver.SetActive(false);

        FindHints();
        UpdateModeUI();
    }

    void Update()
    {
        if (TM.isEnd)
        {
            gameOver.SetActive(true);
            clearGame();
        }
        else
        {
            DragSystem();
            SelectSystem();
            calculateAnswer();
            scoreText.text = string.Format("Score: {0}", score);
        }
    }

    private void clearGame()
    {
        hintBox.gameObject.SetActive(false);
        foreach (AppleMeta everyApple in applesList)
        {
            everyApple.isOn = false;
        }
        gameOverText.text = string.Format("Game Over\n\nYour Score: {0}", score);
    }

    private void calculateAnswer()
    {
        if (!Input.GetMouseButtonUp(0)) return;

        int result = 0;
        int cnt = 0;
        foreach (AppleMeta selectedApple in selectList)
        {
            answerCoors.Add(selectedApple.coor);
            cnt += 1;
        }

        if (currentMode == GameMode.Add)
        {
            foreach (AppleMeta selectedApple in selectList)
            {
                result += selectedApple.number;
            }
            if (result == 10)
            {
                OnCorrectAnswer(cnt);
            }
        }
        else if (currentMode == GameMode.Subtract && selectList.Count > 0)
        {
            result = selectList[0].number;
            for (int i = 1; i < selectList.Count; i++)
            {
                result -= selectList[i].number;
            }
            if (result == 10)
            {
                OnCorrectAnswer(cnt);
            }
        }

        selectList.Clear();
        answerCoors.Clear();
    }

    private void OnCorrectAnswer(int cnt)
    {
        hintBox.gameObject.SetActive(false);

        if (Time.time - comboDelta < comboTime && comboDelta != 0)
        {
            comboCnt += 1;
            comboDelta = Time.time;
        }
        else
        {
            comboCnt = 0;
            comboDelta = Time.time;
        }

        if (cnt >= 3)
        {
            animateCorrect(cnt, comboCnt, true);
            score += cnt + 10 + comboCnt * 5;
        }
        else
        {
            animateCorrect(cnt, comboCnt, false);
            score += cnt + comboCnt * 5;
        }

        foreach (Vector2Int answerCoor in answerCoors)
        {
            mapApple[answerCoor.x, answerCoor.y] = 0;
        }
        hints.Clear();
        FindHints();
    }

    private void animateCorrect(int _score, int combo, bool isMany)
    {
        foreach (AppleMeta selectedApple in selectList)
        {
            selectedApple.isAnimated = true;
        }

        Vector2 midPos = new Vector2(selectBox.position.x + selectBox.rect.center.x, selectBox.position.y + selectBox.rect.center.y);
        TextMeshProUGUI addScore = Instantiate(addScoreText, midPos, Quaternion.identity);
        addScore.transform.SetParent(canvas.transform);
        addScore.gameObject.SetActive(true);
        AddScore AS = addScore.GetComponent<AddScore>();
        AS.print(_score, combo, isMany);
    }

    public void OnToggleModeButton()
    {
        if (currentMode == GameMode.Add)
            currentMode = GameMode.Subtract;
        else
            currentMode = GameMode.Add;

        UpdateModeUI();
        FindHints();
    }

    private void UpdateModeUI()
    {
        if (modeText == null) return;

        if (currentMode == GameMode.Add)
            modeText.text = "현재 모드: 더하기";
        else
            modeText.text = "현재 모드: 빼기";
    }

    private void DragSystem()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectBox.gameObject.SetActive(true);
            startpos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            selectBox.gameObject.SetActive(false);
        }

        if (Input.GetMouseButton(0))
        {
            selectRect.xMin = Mathf.Min(Input.mousePosition.x, startpos.x);
            selectRect.xMax = Mathf.Max(Input.mousePosition.x, startpos.x);
            selectRect.yMin = Mathf.Min(Input.mousePosition.y, startpos.y);
            selectRect.yMax = Mathf.Max(Input.mousePosition.y, startpos.y);
        }

        selectRect.xMin = Mathf.Clamp(selectRect.xMin, 0, Screen.width);
        selectRect.yMin = Mathf.Clamp(selectRect.yMin, 0, Screen.height);
        selectRect.xMax = Mathf.Clamp(selectRect.xMax, 0, Screen.width);
        selectRect.yMax = Mathf.Clamp(selectRect.yMax, 0, Screen.height);

        selectBox.offsetMin = selectRect.min;
        selectBox.offsetMax = selectRect.max;

        collideBox.offsetMin = Camera.main.ScreenToWorldPoint(selectRect.min);
        collideBox.offsetMax = Camera.main.ScreenToWorldPoint(selectRect.max);
    }

    private void SelectSystem()
    {
        if (Input.GetMouseButtonUp(0))
        {
            foreach (AppleMeta everyApple in applesList)
            {
                if (everyApple.isSelected) selectList.Add(everyApple);
            }
        }
        else
        {
            foreach (AppleMeta everyApple in applesList)
            {
                everyApple.isSelected = false;
            }
        }

        if (selectBox.gameObject.activeSelf)
        {
            foreach (AppleMeta everyApple in applesList)
            {
                everyApple.isSelected = false;
            }

            Collider2D[] hit = Physics2D.OverlapBoxAll(collideBox.anchoredPosition, collideBox.sizeDelta, 0f);
            foreach (Collider2D i in hit)
            {
                if (!i.CompareTag("apple")) continue;

                AppleMeta hitApple = i.gameObject.GetComponent<AppleMeta>();
                hitApple.isSelected = true;
            }
        }
    }

    private void FindHints()
    {
        answerCoors = new List<Vector2Int>();
        hints.Clear();
        Vector4 vector = new Vector4();
        int sum = 0;

        for (int x = 0; x < verticalLength; x++)
        {
            for (int y = 0; y < horizontalLength; y++)
            {
                sum = 0;

                for (int k = y; k < horizontalLength; k++)
                {
                    if (currentMode == GameMode.Add)
                    {
                        sum += mapApple[x, k];
                        if (sum > 10) break;
                        if (sum == 10)
                        {
                            vector.Set(x, y, x, k);
                            hints.Add(vector);
                        }
                    }
                    else if (currentMode == GameMode.Subtract)
                    {
                        int result = mapApple[x, y];
                        for (int j = y + 1; j <= k; j++)
                        {
                            result -= mapApple[x, j];
                        }
                        if (result == 10)
                        {
                            vector.Set(x, y, x, k);
                            hints.Add(vector);
                        }
                    }
                }

                sum = 0;

                for (int k = x; k < verticalLength; k++)
                {
                    if (currentMode == GameMode.Add)
                    {
                        sum += mapApple[k, y];
                        if (sum > 10) break;
                        if (sum == 10)
                        {
                            vector.Set(x, y, k, y);
                            hints.Add(vector);
                        }
                    }
                    else if (currentMode == GameMode.Subtract)
                    {
                        int result = mapApple[x, y];
                        for (int j = x + 1; j <= k; j++)
                        {
                            result -= mapApple[j, y];
                        }
                        if (result == 10)
                        {
                            vector.Set(x, y, k, y);
                            hints.Add(vector);
                        }
                    }
                }
            }
        }
    }

    public void OnResetButton()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnHomeButton()
    {
        SceneManager.LoadScene("Title");
    }

    public void OnHintButton()
    {
        if (hints.Count == 0)
        {
            Debug.Log("No more Hints!");
        }
        else
        {
            Vector4 hint = hints[0];
            Vector2 hintOffsetMin = new Vector2(zeroPos.x + hint.y - 0.5f, zeroPos.y + hint.x - 0.5f);
            Vector2 hintOffsetMax = new Vector2(zeroPos.x + hint.w + 0.5f, zeroPos.y + hint.z + 0.5f);
            hintBox.offsetMin = Camera.main.WorldToScreenPoint(hintOffsetMin);
            hintBox.offsetMax = Camera.main.WorldToScreenPoint(hintOffsetMax);
            hintBox.gameObject.SetActive(true);
        }
    }
}