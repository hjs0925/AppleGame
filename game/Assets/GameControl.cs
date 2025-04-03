// GameControl.cs (최적화 + 힌트 없음 수정 포함)
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
    public TextMeshProUGUI noHintText;

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
    private List<AppleMeta> applesList = new List<AppleMeta>();
    private List<AppleMeta> selectList = new List<AppleMeta>();
    private int[,] mapApple;
    public List<Vector4> hints = new List<Vector4>();
    private List<Vector2Int> answerCoors = new List<Vector2Int>();
    private Dictionary<GameMode, BoardState> savedStates = new();
    private bool usedHintInAdd = false;
    private bool usedHintInSubtract = false;

    private void Awake()
    {
        zeroPos = apple.transform.position;

        if (currentMode == GameMode.Subtract && horizontalLength % 2 != 0)
            horizontalLength--;

        mapApple = new int[verticalLength, horizontalLength];
        InitBoard();
    }

    private void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        foreach (AppleMeta a in applesList) { a.gameObject.SetActive(true); a.isOn = true; }
        gameOver.SetActive(false);
        FindHints();
        UpdateModeUI();
    }

    private void Update()
    {
        if (TM.isEnd)
        {
            if (!gameOver.activeSelf)
            {
                gameOver.SetActive(true);
                clearGame();
            }
            return;
        }

        DragSystem();
        SelectSystem();
        calculateAnswer();
        scoreText.text = $"Score: {score}";
    }

    private void clearGame()
    {
        hintBox.gameObject.SetActive(false);
        foreach (var apple in applesList) apple.isOn = false;
        gameOverText.text = $"Game Over\n\nYour Score: {score}";
    }

    private void calculateAnswer()
    {
        if (!Input.GetMouseButtonUp(0)) return;

        int cnt = selectList.Count;
        int result = 0;

        if (currentMode == GameMode.Add)
        {
            result = selectList.Sum(a => a.number);
            if (result == 10) OnCorrectAnswer(cnt);
        }
        else if (cnt > 1)
        {
            var maxApple = selectList.OrderByDescending(a => a.number).First();
            result = maxApple.number - selectList.Where(a => a != maxApple).Sum(a => a.number);
            if (result == 0) OnCorrectAnswer(cnt);
        }

        selectList.Clear();
        answerCoors.Clear();
    }

    private void OnCorrectAnswer(int cnt)
    {
        hintBox.gameObject.SetActive(false);
        comboCnt = (Time.time - comboDelta < comboTime && comboDelta != 0) ? comboCnt + 1 : 0;
        comboDelta = Time.time;

        score += cnt + (cnt >= 3 ? 10 : 0) + comboCnt * 5;
        animateCorrect(cnt, comboCnt, cnt >= 3);

        foreach (Vector2Int coor in answerCoors)
            mapApple[coor.x, coor.y] = 0;

        hints.Clear();
        FindHints();
    }

    private void animateCorrect(int scoreValue, int combo, bool isMany)
    {
        foreach (var a in selectList) a.isAnimated = true;
        Vector2 midPos = (Vector2)selectBox.position + selectBox.rect.center;
        var addScore = Instantiate(addScoreText, midPos, Quaternion.identity, canvas.transform);
        addScore.gameObject.SetActive(true);
        addScore.GetComponent<AddScore>().print(scoreValue, combo, isMany);
    }

    public void OnToggleModeButton()
    {
        SaveCurrentModeState();
        foreach (var a in applesList) Destroy(a.gameObject);
        applesList.Clear();

        currentMode = (currentMode == GameMode.Add) ? GameMode.Subtract : GameMode.Add;
        LoadOrInitModeState();
        UpdateModeUI();
        Hint.gameObject.SetActive(true);
        hintBox.gameObject.SetActive(false);
        UpdateHintButtonVisibility();
    }

    private void UpdateHintButtonVisibility()
    {
        Hint.gameObject.SetActive(!(currentMode == GameMode.Add && usedHintInAdd || currentMode == GameMode.Subtract && usedHintInSubtract));
        hintBox.gameObject.SetActive(false);
    }

    private void UpdateModeUI()
    {
        if (modeText != null)
            modeText.text = currentMode == GameMode.Add ? "현재 모드: 더하기" : "현재 모드: 빼기";
    }

    private void DragSystem()
    {
        if (Input.GetMouseButtonDown(0)) { selectBox.gameObject.SetActive(true); startpos = Input.mousePosition; }
        if (Input.GetMouseButtonUp(0)) { selectBox.gameObject.SetActive(false); }

        if (Input.GetMouseButton(0))
        {
            selectRect = Rect.MinMaxRect(
                Mathf.Clamp(Mathf.Min(Input.mousePosition.x, startpos.x), 0, Screen.width),
                Mathf.Clamp(Mathf.Min(Input.mousePosition.y, startpos.y), 0, Screen.height),
                Mathf.Clamp(Mathf.Max(Input.mousePosition.x, startpos.x), 0, Screen.width),
                Mathf.Clamp(Mathf.Max(Input.mousePosition.y, startpos.y), 0, Screen.height));
        }

        selectBox.offsetMin = selectRect.min;
        selectBox.offsetMax = selectRect.max;

        Vector2 worldMin = Camera.main.ScreenToWorldPoint(selectRect.min);
        Vector2 worldMax = Camera.main.ScreenToWorldPoint(selectRect.max);
        Vector2 padding = new Vector2(0.5f, 0.5f);
        collideBox.position = (worldMin + worldMax) / 2f;
        collideBox.sizeDelta = worldMax - worldMin + padding;
    }

    private void SelectSystem()
    {
        if (Input.GetMouseButtonUp(0))
        {
            foreach (var a in applesList) if (a.isSelected) selectList.Add(a);
        }
        else
        {
            foreach (var a in applesList) a.isSelected = false;
        }

        if (selectBox.gameObject.activeSelf)
        {
            foreach (var a in applesList) a.isSelected = false;
            var hit = Physics2D.OverlapBoxAll(collideBox.anchoredPosition, collideBox.sizeDelta, 0f);
            foreach (var i in hit)
            {
                if (!i.CompareTag("apple")) continue;
                var hitApple = i.GetComponent<AppleMeta>();
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
        hints.Clear();
        for (int x = 0; x < verticalLength; x++)
        {
            for (int y = 0; y < horizontalLength; y++)
            {
                CheckHintLine(x, y, 1, 0, horizontalLength - y);
                CheckHintLine(x, y, 0, 1, verticalLength - x);
            }
        }
    }

    private void CheckHintLine(int sx, int sy, int dx, int dy, int maxLength)
    {
        for (int len = 2; len <= maxLength; len++)
        {
            List<int> values = new();
            bool hasEmpty = false;

            for (int k = 0; k < len; k++)
            {
                var a = GetAppleAt(sx + k * dx, sy + k * dy);
                if (a == null) { hasEmpty = true; break; }
                values.Add(a.number);
            }

            if (hasEmpty) continue;

            bool isValid = currentMode == GameMode.Add ? values.Sum() == 10 : IsValidSubtractCombo(values);
            if (isValid)
                hints.Add(new Vector4(sx, sy, sx + (len - 1) * dx, sy + (len - 1) * dy));
        }
    }

    private bool IsValidSubtractCombo(List<int> values)
    {
        if (values.Count < 2) return false;
        int max = values.Max();
        values.Remove(max);
        return max - values.Sum() == 0;
    }

    public void OnHintButton()
    {
        if ((currentMode == GameMode.Add && usedHintInAdd) || (currentMode == GameMode.Subtract && usedHintInSubtract)) return;
        if (hints.Count == 0)
        {
            Debug.Log("힌트 없음!");
            hintBox.gameObject.SetActive(false);
            StartCoroutine(ShowNoHintMessage());
            return;
        }

        if (currentMode == GameMode.Add) usedHintInAdd = true;
        else usedHintInSubtract = true;

        Hint.gameObject.SetActive(false);

        Vector4 hint = hints[0];
        Vector2 hintOffsetMin = new Vector2(zeroPos.x + hint.y - 0.5f, zeroPos.y + hint.x - 0.5f);
        Vector2 hintOffsetMax = new Vector2(zeroPos.x + hint.w + 0.5f, zeroPos.y + hint.z + 0.5f);
        hintBox.offsetMin = Camera.main.WorldToScreenPoint(hintOffsetMin);
        hintBox.offsetMax = Camera.main.WorldToScreenPoint(hintOffsetMax);
        hintBox.gameObject.SetActive(true);
    }

    private IEnumerator ShowNoHintMessage()
    {
        noHintText.gameObject.SetActive(true);
        noHintText.text = "힌트 없음!";

        CanvasGroup cg = noHintText.GetComponent<CanvasGroup>();
        if (cg == null) cg = noHintText.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 1f;
        yield return new WaitForSeconds(2f);
        cg.alpha = 0f;
        noHintText.gameObject.SetActive(false);
    }

    public void OnResetButton() => SceneManager.LoadScene("Main");
    public void OnHomeButton() => SceneManager.LoadScene("Title");

    private void SaveCurrentModeState()
    {
        savedStates[currentMode] = new BoardState(applesList, mapApple);
    }

    private void LoadOrInitModeState()
    {
        if (savedStates.TryGetValue(currentMode, out var state))
        {
            applesList = new();
            mapApple = (int[,])state.map.Clone();
            foreach (var meta in state.apples)
            {
                GameObject go = Instantiate(apple, meta.position, Quaternion.identity);
                AppleMeta am = go.GetComponent<AppleMeta>();
                am.number = meta.number;
                am.coor = meta.coor;
                am.isOn = meta.isOn;
                go.name = $"Apple ({meta.coor.x}, {meta.coor.y})";
                go.SetActive(meta.isOn);
                applesList.Add(am);
            }
            FindHints();
        }
        else InitBoard();
    }

    private void InitBoard()
    {
        applesList.Clear();
        selectList.Clear();
        mapApple = new int[verticalLength, horizontalLength];

        if (currentMode == GameMode.Subtract)
        {
            List<int> numbers = new();
            int pairCount = (horizontalLength * verticalLength) / 2;

            for (int i = 0; i < pairCount; i++)
            {
                int baseNum = Random.Range(11, 20);
                int secondNum = baseNum - 10;
                if (Random.value < 0.5f) { numbers.Add(baseNum); numbers.Add(secondNum); }
                else { numbers.Add(secondNum); numbers.Add(baseNum); }
            }

            List<Vector2Int> coords = new();
            for (int i = 0; i < verticalLength; i++)
                for (int j = 0; j < horizontalLength; j++)
                    coords.Add(new(i, j));

            for (int i = 0; i < coords.Count; i++)
            {
                int r = Random.Range(i, coords.Count);
                (coords[i], coords[r]) = (coords[r], coords[i]);
            }

            for (int i = 0; i < coords.Count && i < numbers.Count; i++)
            {
                Vector2Int coor = coords[i];
                Vector3 newPos = new(zeroPos.x + coor.y, zeroPos.y + coor.x, zeroPos.z);
                GameObject go = Instantiate(apple, newPos, Quaternion.identity);
                AppleMeta am = go.GetComponent<AppleMeta>();
                am.number = numbers[i]; am.coor = coor; am.isOn = true;
                go.name = $"Apple ({coor.x}, {coor.y})"; go.SetActive(true);
                mapApple[coor.x, coor.y] = am.number;
                applesList.Add(am);
            }

            if (numbers.Count < coords.Count)
            {
                Vector2Int coor = coords[^1];
                Vector3 newPos = new(zeroPos.x + coor.y, zeroPos.y + coor.x, zeroPos.z);
                GameObject go = Instantiate(apple, newPos, Quaternion.identity);
                AppleMeta am = go.GetComponent<AppleMeta>();
                am.number = Random.Range(1, 10); am.coor = coor; am.isOn = false;
                go.name = $"Apple ({coor.x}, {coor.y})"; go.SetActive(true);
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
                    Vector3 pos = new(zeroPos.x + j, zeroPos.y + i, zeroPos.z);
                    GameObject go = Instantiate(apple, pos, Quaternion.identity);
                    AppleMeta am = go.GetComponent<AppleMeta>();
                    int randNum = Random.Range(1, 10);
                    am.number = randNum; am.coor = new(i, j); am.isOn = true;
                    go.name = $"Apple ({i}, {j})"; go.SetActive(true);
                    mapApple[i, j] = randNum;
                    applesList.Add(am);
                }
            }
        }

        FindHints();
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
            apples = appleList.Select(am => new AppleMetaData
            {
                number = am.number,
                coor = am.coor,
                position = am.transform.position,
                isOn = am.isOn
            }).ToList();
            map = (int[,])mapApple.Clone();
        }
    }
}
