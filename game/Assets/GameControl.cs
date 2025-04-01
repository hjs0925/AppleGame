using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

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
    public TextMeshProUGUI addScoreText;
    public RectTransform hintBox;
    public TextMeshProUGUI modeText;
    public TimeManager TM;
    public Button Hint;
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
    private int[,] mapApple;
    public List<Vector4> hints;
    private List<Vector2Int> answerCoors;
    private Dictionary<GameMode, BoardState> savedStates = new Dictionary<GameMode, BoardState>();

    

    private void Awake()
{
    zeroPos = apple.transform.position;
    applesList = new List<AppleMeta>();
    selectList = new List<AppleMeta>();
    mapApple = new int[verticalLength, horizontalLength];
    
    hints = new List<Vector4>();

    // Subtract 모드일 때 가로 길이 홀수면 1 줄이기
    if (currentMode == GameMode.Subtract && horizontalLength % 2 != 0)
    {
        horizontalLength -= 1;
        mapApple = new int[verticalLength, horizontalLength]; // 배열 다시 초기화
    }

    InitBoard();
}

    private void InitBoard()
{
    applesList.Clear();
    selectList.Clear();
    mapApple = new int[verticalLength, horizontalLength];

    if (currentMode == GameMode.Subtract)
{
    // 1. 숫자쌍 미리 생성
    List<int> numbers = new List<int>();
    int pairCount = (horizontalLength * verticalLength) / 2;

    for (int i = 0; i < pairCount; i++)
    {
        int baseNum = Random.Range(11, 20);
        int secondNum = baseNum - 10;

        // 순서 랜덤하게
        if (Random.value < 0.5f)
        {
            numbers.Add(baseNum);
            numbers.Add(secondNum);
        }
        else
        {
            numbers.Add(secondNum);
            numbers.Add(baseNum);
        }
    }

    // 2. 좌표 전부 뽑아서 셔플
    List<Vector2Int> coords = new List<Vector2Int>();
    for (int i = 0; i < verticalLength; i++)
    {
        for (int j = 0; j < horizontalLength; j++)
        {
            coords.Add(new Vector2Int(i, j));
        }
    }

    // 좌표 랜덤 섞기
    for (int i = 0; i < coords.Count; i++)
    {
        Vector2Int temp = coords[i];
        int randIndex = Random.Range(i, coords.Count);
        coords[i] = coords[randIndex];
        coords[randIndex] = temp;
    }

    // 3. 숫자들 보드에 랜덤하게 배치
    for (int i = 0; i < coords.Count && i < numbers.Count; i++)
    {
        Vector2Int coor = coords[i];
        Vector3 newPos = new Vector3(zeroPos.x + coor.y, zeroPos.y + coor.x, zeroPos.z);
        GameObject newApple = Instantiate(apple, newPos, Quaternion.identity);

        AppleMeta am = newApple.GetComponent<AppleMeta>();
        int num = numbers[i];

        am.number = num;
        am.coor = coor;
        am.isOn = true;

        newApple.name = $"Apple ({coor.x}, {coor.y})";
        newApple.SetActive(true);

        mapApple[coor.x, coor.y] = num;
        applesList.Add(am);
    }

    // 4. 남는 칸이 홀수면 빈 칸 추가
    if (numbers.Count < coords.Count)
    {
        Vector2Int coor = coords[coords.Count - 1];
        Vector3 newPos = new Vector3(zeroPos.x + coor.y, zeroPos.y + coor.x, zeroPos.z);
        GameObject newApple = Instantiate(apple, newPos, Quaternion.identity);

        AppleMeta am = newApple.GetComponent<AppleMeta>();
        am.number = Random.Range(1, 10);
        am.coor = coor;
        am.isOn = false; // 선택 안 되도록

        newApple.name = $"Apple ({coor.x}, {coor.y})";
        newApple.SetActive(true);

        mapApple[coor.x, coor.y] = am.number;
        applesList.Add(am);
    }
}

    else // Add 모드
    {
        for (int i = 0; i < verticalLength; i++)
        {
            for (int j = 0; j < horizontalLength; j++)
            {
                Vector3 newPos = new Vector3(zeroPos.x + j, zeroPos.y + i, zeroPos.z);
                GameObject newApple = Instantiate(apple, newPos, Quaternion.identity);

                AppleMeta am = newApple.GetComponent<AppleMeta>();
                int randNum = Random.Range(1, 10);

                am.number = randNum;
                am.coor = new Vector2Int(i, j);
                am.isOn = true;

                newApple.name = $"Apple ({i}, {j})";
                newApple.SetActive(true);

                mapApple[i, j] = randNum;
                applesList.Add(am);
            }
        }
    }

    FindHints(); // 힌트 갱신
}

    void Start()
    {
        Screen.SetResolution(1920, 1080, true);

        foreach (AppleMeta _apple in applesList)
        {
            _apple.gameObject.SetActive(true);
            _apple.isOn = true;
        }
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

    int cnt = selectList.Count;
    int result = 0;

    if (currentMode == GameMode.Add)
    {
        foreach (AppleMeta apple in selectList)
        {
            result += apple.number;
        }
        if (result == 10)
        {
            OnCorrectAnswer(cnt);
        }
    }
    else if (currentMode == GameMode.Subtract && selectList.Count > 1)
    {
        AppleMeta maxApple = selectList[0];
        foreach (AppleMeta apple in selectList)
        {
            if (apple.number > maxApple.number)
                maxApple = apple;
        }

        result = maxApple.number;
        foreach (AppleMeta apple in selectList)
        {
            if (apple != maxApple)
                result -= apple.number;
        }

        if (result == 0)
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
    // 현재 상태 저장
    SaveCurrentModeState();

    // 기존 오브젝트 제거
    foreach (AppleMeta apple in applesList)
    {
        if (apple != null)
            Destroy(apple.gameObject);
    }

    applesList.Clear();

    // 모드 전환
    currentMode = (currentMode == GameMode.Add) ? GameMode.Subtract : GameMode.Add;

    // 상태 불러오기 또는 새로 만들기
    LoadOrInitModeState();
    UpdateModeUI();
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

        Vector2 worldMin = Camera.main.ScreenToWorldPoint(selectRect.min);
        Vector2 worldMax = Camera.main.ScreenToWorldPoint(selectRect.max);
        collideBox.position = (worldMin + worldMax) / 2f;
        collideBox.sizeDelta = worldMax - worldMin;

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

    private AppleMeta GetAppleAt(int x, int y)
{
    return applesList.FirstOrDefault(a => a.coor.x == x && a.coor.y == y && a.isOn);
}

private void FindHints()
{
    answerCoors = new List<Vector2Int>();
    hints.Clear();
    Vector4 vector = new Vector4();

    for (int x = 0; x < verticalLength; x++)
    {
        for (int y = 0; y < horizontalLength; y++)
        {
            // 가로 방향
            for (int length = 2; length <= horizontalLength - y; length++)
            {
                List<int> values = new List<int>();
                bool hasEmpty = false;

                for (int k = 0; k < length; k++)
                {
                    AppleMeta apple = GetAppleAt(x, y + k);
                    if (apple == null)
                    {
                        hasEmpty = true;
                        break;
                    }
                    values.Add(apple.number);
                }

                if (hasEmpty) continue;

                bool isValid = false;
                if (currentMode == GameMode.Add)
                {
                    isValid = values.Sum() == 10;
                }
                else if (currentMode == GameMode.Subtract)
                {
                    isValid = IsValidSubtractCombo(values);
                }

                if (isValid)
                {
                    vector.Set(x, y, x, y + length - 1);
                    hints.Add(vector);
                }
            }

            // 세로 방향
            for (int length = 2; length <= verticalLength - x; length++)
            {
                List<int> values = new List<int>();
                bool hasEmpty = false;

                for (int k = 0; k < length; k++)
                {
                    AppleMeta apple = GetAppleAt(x + k, y);
                    if (apple == null)
                    {
                        hasEmpty = true;
                        break;
                    }
                    values.Add(apple.number);
                }

                if (hasEmpty) continue;

                bool isValid = false;
                if (currentMode == GameMode.Add)
                {
                    isValid = values.Sum() == 10;
                }
                else if (currentMode == GameMode.Subtract)
                {
                    isValid = IsValidSubtractCombo(values);
                }

                if (isValid)
                {
                    vector.Set(x, y, x + length - 1, y);
                    hints.Add(vector);
                }
            }
        }
    }
}


    private bool IsValidSubtractCombo(List<int> values)
{
    if (values.Count < 2)
        return false;

    int max = values.Max();
    values.Remove(max);
    values.Sort((a, b) => b.CompareTo(a)); // 내림차순

    int result = max;
    foreach (int val in values)
    {
        result -= val;
    }

    return result == 0;
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
        Hint.gameObject.SetActive(false);
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
    private void SaveCurrentModeState()
{
    if (!savedStates.ContainsKey(currentMode))
        savedStates.Add(currentMode, new BoardState(applesList, mapApple));
    else
        savedStates[currentMode] = new BoardState(applesList, mapApple);
}
    private void LoadOrInitModeState()
{
    if (savedStates.ContainsKey(currentMode))
    {
        BoardState state = savedStates[currentMode];
        applesList = new List<AppleMeta>();
        mapApple = (int[,])state.map.Clone();

        foreach (var meta in state.apples)
        {
            GameObject newApple = Instantiate(apple, meta.position, Quaternion.identity);
            AppleMeta am = newApple.GetComponent<AppleMeta>();
            am.number = meta.number;
            am.coor = meta.coor;
            am.isOn = meta.isOn;

            newApple.name = $"Apple ({meta.coor.x}, {meta.coor.y})";
            newApple.SetActive(meta.isOn);
            applesList.Add(am);
        }

        FindHints();
    }
    else
    {
        InitBoard(); // 저장된 상태 없으면 새로 생성
    }
}

    [System.Serializable]
    public class AppleMetaData
{
    public int number;
    public Vector2Int coor;
    public Vector3 position;
    public bool isOn;
}

    [System.Serializable]
    public class BoardState
{
    public List<AppleMetaData> apples;
    public int[,] map;

    public BoardState(List<AppleMeta> appleList, int[,] mapApple)
    {
        apples = new List<AppleMetaData>();
        foreach (var am in appleList)
        {
            apples.Add(new AppleMetaData
            {
                number = am.number,
                coor = am.coor,
                position = am.transform.position,
                isOn = am.isOn
            });
        }

        map = (int[,])mapApple.Clone();
    }
}

}