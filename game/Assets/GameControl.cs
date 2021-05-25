using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameControl: MonoBehaviour
{
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

    [Header("Game Settings")]
    public int horizontalLength;
    public int verticalLength;
    public float comboTime;

    [Header("Only for Read")]
    public int nothing;
    public int score = 0;
    public float comboDelta = 0f;
    public int comboCnt;

    private Vector3 zeroPos;
    private Vector2 startpos = Vector2.zero;
    private Rect selectRect = new Rect();

    private List<AppleMeta> applesList;
    private List<AppleMeta> selectList;

    private TimeManager TM;

    private int[,] mapApple;
    public List<Vector4> hints;    // x, y : 힌트 시작 좌표 z, w : 힌트 끝 좌표
    private List<Vector2Int> answerCoors;

    private void Awake()
    {
        zeroPos = apple.transform.position;
        applesList = new List<AppleMeta>();
        selectList = new List<AppleMeta>();
        mapApple = new int[verticalLength, horizontalLength];
        hints = new List<Vector4>();

        for (int i = 0; i< verticalLength; i++)
        {
            for (int j = 0; j< horizontalLength; j++)
            {
                Vector3 newPos = new Vector3 (zeroPos.x + j, zeroPos.y + i, zeroPos.z);
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

    // Start is called before the first frame update
    void Start()
    {
        foreach (AppleMeta _apple in applesList)
        {
            _apple.gameObject.SetActive(true);
            _apple.isOn = true;
        }

        TM = timeSlider.GetComponent<TimeManager>();
        gameOver.SetActive(false);

        FindHints();

    }

    // Update is called once per frame
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

        int sum = 0;
        int cnt = 0;
        foreach(AppleMeta selectedApple in selectList)
        {
            sum += selectedApple.number;
            // 선택된 사과들의 좌표 모음
            answerCoors.Add(selectedApple.coor);
            cnt += 1;
        }

        if (sum == 10)
        {
            hintBox.gameObject.SetActive(false);

            #region 추가기능 #3 - 제한 시간 안에 연속적으로 성공하면 콤보
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
            #endregion

            #region 추가기능 #4 - 3개 이상의 사과들을 묶어서 터뜨린 경우 점수 +10
            if (cnt >= 3)
            {
                animateCorrect(cnt, comboCnt, true);
                score += cnt + 10 + comboCnt*5;
            }
            else
            {
                animateCorrect(cnt, comboCnt, false);
                score += cnt + comboCnt*5;
            }
            #endregion

            /* 추가기능 #1 */
            // 정답 노드의 값을 0으로 만들고 힌트를 새로 탐색
            foreach(Vector2Int answerCoor in answerCoors)
            {
                mapApple[answerCoor.x, answerCoor.y] = 0;
            }
            hints.Clear();
            FindHints();
        }
        selectList.Clear();
        answerCoors.Clear();
    }

    private void animateCorrect(int _score, int combo, bool isMany)
    {
        foreach(AppleMeta selectedApple in selectList)
        {
            // 사과의 성공적 제거
            selectedApple.isAnimated = true;
        }

        Vector2 midPos = new Vector2(selectBox.position.x + selectBox.rect.center.x, selectBox.position.y + selectBox.rect.center.y);
        TextMeshProUGUI addScore = Instantiate(addScoreText, midPos, Quaternion.identity);
        addScore.transform.SetParent(canvas.transform);
        addScore.gameObject.SetActive(true);
        AddScore AS = addScore.GetComponent<AddScore>();
        AS.print(_score, combo, isMany);
    }

    private void SelectSystem()
    {
        // 선택을 마쳤을 때, select flag가 켜져있는 사과는 selectList에 추가
        if (Input.GetMouseButtonUp(0))
        {
            foreach (AppleMeta everyApple in applesList)
            {
                if (everyApple.isSelected) selectList.Add(everyApple);
            }
        }

        // 선택을 마친게 아니면, select flag 초기화
        else
        {
            foreach(AppleMeta everyApple in applesList)
            {
                everyApple.isSelected = false;
            }
        }

        // 선택박스가 켜져있는 동안, 박스 내의 사과의 select flag를 켜준다.
        if (selectBox.gameObject.activeSelf)
        {
            // 모든 사과의 선택을 초기화
            foreach (AppleMeta everyApple in applesList)
            {
                everyApple.isSelected = false;
            }

            // 콜라이더로 선택을 설정
            Collider2D[] hit = Physics2D.OverlapBoxAll(collideBox.anchoredPosition, collideBox.sizeDelta, 0f);
            foreach (Collider2D i in hit)
            {
                if (!i.CompareTag("apple"))
                    continue;

                AppleMeta hitApple = i.gameObject.GetComponent<AppleMeta>();

                // collide한 사과가 이미 새로 영역에 들어온 사과면,
                if (!hitApple.isSelected)
                {
                    hitApple.isSelected = true;
                }
            }
        }
    }

    private void DragSystem()
    {
        #region 처음 클릭한 좌표 저장
        if (Input.GetMouseButtonDown(0))
        {
            selectBox.gameObject.SetActive(true);
            startpos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            selectBox.gameObject.SetActive(false);
        }
        #endregion

        #region 드래그 중일 때
        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition.x > startpos.x)
            {
                // 처음 클릭한 곳에서 오른쪽으로 드래그할 경우
                selectRect.xMin = startpos.x;
                selectRect.xMax = Input.mousePosition.x;
            }

            else
            {
                // 처음 클릭한 곳에서 왼쪽으로 드래그할 경우
                selectRect.xMin = Input.mousePosition.x;
                selectRect.xMax = startpos.x;
            }

            if (Input.mousePosition.y > startpos.y)
            {
                // 처음 클릭한 곳에서 위쪽으로 드래그할 경우
                selectRect.yMin = startpos.y;
                selectRect.yMax = Input.mousePosition.y;
            }

            else
            {
                // 처음 클릭한 곳에서 아래쪽으로 드래그할 경우
                selectRect.yMin = Input.mousePosition.y;
                selectRect.yMax = startpos.y;
            }
        }
        #endregion

        #region Safe Area
        if (selectRect.xMin < 0) selectRect.xMin = 0;
        if (selectRect.yMin < 0) selectRect.yMin = 0;
        if (selectRect.xMax > Screen.width) selectRect.xMax = Screen.width;
        if (selectRect.yMax > Screen.height) selectRect.yMax = Screen.height;
        #endregion

        // 사이즈 반영
        selectBox.offsetMin = selectRect.min;
        selectBox.offsetMax = selectRect.max;

        collideBox.offsetMin = Camera.main.ScreenToWorldPoint(selectRect.min);
        collideBox.offsetMax = Camera.main.ScreenToWorldPoint(selectRect.max);
    }

    private void FindHints()
    {
        answerCoors = new List<Vector2Int>();
        Vector4 vector = new Vector4(); // 힌트 시작사과좌표, 끝사과좌표
        int sum = 0;
        for (int x = 0; x < verticalLength; x++)
        {
            for (int y = 0; y < horizontalLength; y++)
            {
                // sum을 현재 노드로 초기화
                sum = 0;
                // 가로로 탐색
                for (int k = y; k < horizontalLength; k++)
                {
                    sum += mapApple[x, k];
                    if (sum > 10) break;
                    if (sum == 10)
                    {
                        vector.Set(x, y, x, k);
                        hints.Add(vector);
                    }
                }
                sum = 0;
                // 세로로 탐색
                for (int k = x; k < verticalLength; k++)
                {
                    sum += mapApple[k, y];
                    if (sum > 10) break;
                    if (sum == 10)
                    {
                        vector.Set(x, y, k, y);
                        hints.Add(vector);
                    }
                }
            }
        }
    }

    #region 버튼 액션
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
    #endregion
}
