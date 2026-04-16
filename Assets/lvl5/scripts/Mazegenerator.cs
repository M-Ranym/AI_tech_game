using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UPersian.Components;

public class MazeGenerator : MonoBehaviour
{
    private static readonly int[] LEVEL_SIZES = { 2, 3, 4, 5 };

    // Robot pixel size per level
    private static readonly float[] LEVEL_ROBOT_SIZES = { 200f, 150f, 100f, 90f };

    private static readonly Vector2Int[] LEVEL_GOALS = {
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(2, 1),
        new Vector2Int(3, 3),
    };

    private int currentLevel = 0;

    [Header("References")]
    public RectTransform mazePanel;
    public RectTransform robotTransform;

    [Header("Colors")]
    public Color wallColor = Color.white;
    public Color floorColor = new Color(0.3f, 0.55f, 1f, 0.12f);
    public Color goalColor = Color.yellow;

    [Header("Padding")]
    public float padding = 15f;

    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("UI")]
    public GameObject resultPanel;
    public RtlText resultMessageLabel;
    public ProgramPanel programPanel;
    public RtlText levelLabel;

    [Header("Visual")]
    public float wallThicknessPx = 16f;

    private readonly string msgWall = "اصطدم الروبوت بجدار";
    private readonly string msgNoGoal = "لم يصل الروبوت الى الهدف";
    private readonly string msgNext = "ممتاز! المستوى التالي";
    private readonly string msgWinAll = "رائع! اكملت جميع المستويات";

    private int[,] mazeGrid;
    private int rows, cols, width, height;

    private int startGCol, startGRow;
    private int goalGCol, goalGRow;

    private float corridorPx;
    private float[] colPos, rowPos;
    private float wt;

    private int robotCol, robotRow;
    private bool isRunning;
    private Vector2 robotStartPos;
    private bool lastRunFailed = false;

    void Start() => LoadLevel(0);

    void LoadLevel(int index)
    {
        currentLevel = Mathf.Clamp(index, 0, LEVEL_SIZES.Length - 1);
        width = height = LEVEL_SIZES[currentLevel];

        UpdateLevelLabel();
        ClearMazeCells();
        GenerateMaze();

        rows = mazeGrid.GetLength(0);
        cols = mazeGrid.GetLength(1);

        StopAllCoroutines();
        StartCoroutine(InitAfterLayout());
    }

    void UpdateLevelLabel()
    {
        if (levelLabel == null) return;
        levelLabel.text = "المستوى " + (currentLevel + 1);
        var tmp = levelLabel.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp != null) tmp.alignment = TMPro.TextAlignmentOptions.Center;
    }

    void ClearMazeCells()
    {
        var list = new List<Transform>();
        foreach (Transform t in mazePanel)
            if (t != robotTransform) list.Add(t);
        foreach (var t in list) Destroy(t.gameObject);
    }

    void GenerateMaze()
    {
        if (currentLevel == 3) { GenerateLevel4Maze(); return; }

        Random.InitState(currentLevel);

        int w = width * 2 + 1;
        int h = height * 2 + 1;

        mazeGrid = new int[h, w];
        for (int r = 0; r < h; r++)
            for (int c = 0; c < w; c++)
                mazeGrid[r, c] = 1;

        var stack = new Stack<Vector2Int>();
        var seed = new Vector2Int(1, 1);
        mazeGrid[seed.y, seed.x] = 0;
        stack.Push(seed);

        Vector2Int[] dirs = {
            new Vector2Int(0,  2), new Vector2Int(2,  0),
            new Vector2Int(0, -2), new Vector2Int(-2,  0)
        };

        while (stack.Count > 0)
        {
            var cur = stack.Peek();
            var neighbors = new List<Vector2Int>();
            foreach (var d in dirs)
            {
                var n = cur + d;
                if (n.x > 0 && n.x < w - 1 && n.y > 0 && n.y < h - 1 && mazeGrid[n.y, n.x] == 1)
                    neighbors.Add(n);
            }
            if (neighbors.Count > 0)
            {
                var next = neighbors[Random.Range(0, neighbors.Count)];
                var wall = (cur + next) / 2;
                mazeGrid[next.y, next.x] = 0;
                mazeGrid[wall.y, wall.x] = 0;
                stack.Push(next);
            }
            else stack.Pop();
        }

        startGCol = 1;
        startGRow = h - 2;

        Vector2Int g = LEVEL_GOALS[currentLevel];
        goalGCol = g.x * 2 + 1;
        goalGRow = g.y * 2 + 1;
    }

    void GenerateLevel4Maze()
    {
        int[,] grid = new int[11, 11]
        {
            {  1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            {  1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1 },
            {  1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1 },
            {  1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1 },
            {  1, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1 },
            {  1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1 },
            {  1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1 },
            {  1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 },
            {  1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            {  1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            {  1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        };

        mazeGrid = grid;
        startGCol = 1;
        startGRow = 9;
        goalGCol = 5;
        goalGRow = 1;
    }

    IEnumerator InitAfterLayout()
    {
        yield return null;
        yield return null;

        float availW = mazePanel.rect.width - padding * 2;
        float availH = mazePanel.rect.height - padding * 2;

        int nWC = (cols + 1) / 2;
        int nCC = cols / 2;

        wt = wallThicknessPx;

        corridorPx = Mathf.Min(
            (availW - nWC * wt) / nCC,
            (availH - nWC * wt) / nCC
        );

        colPos = BuildPositions(cols);
        rowPos = BuildPositions(rows);

        DrawMaze();

        // Per-level robot size
        float rSize = LEVEL_ROBOT_SIZES[currentLevel];
        robotTransform.SetParent(mazePanel, false);
        robotTransform.sizeDelta = new Vector2(rSize, rSize);

        robotCol = startGCol;
        robotRow = startGRow;
        robotStartPos = CellPos(robotCol, robotRow);
        robotTransform.anchoredPosition = robotStartPos;
        robotTransform.SetAsLastSibling();

        if (resultPanel) resultPanel.SetActive(false);
    }

    float[] BuildPositions(int count)
    {
        float[] pos = new float[count];
        float cursor = 0;
        for (int i = 0; i < count; i++)
        {
            float size = (i % 2 == 0) ? wt : corridorPx;
            pos[i] = cursor + size / 2;
            cursor += size;
        }
        float total = cursor;
        for (int i = 0; i < count; i++) pos[i] -= total / 2;
        return pos;
    }

    Vector2 CellPos(int c, int r) => new Vector2(colPos[c], -rowPos[r]);

    void DrawMaze()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (mazeGrid[r, c] == 1)
                {
                    var go = new GameObject("Wall");
                    go.transform.SetParent(mazePanel, false);
                    var rt = go.AddComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(
                        (c % 2 == 0) ? wt : corridorPx,
                        (r % 2 == 0) ? wt : corridorPx
                    );
                    rt.anchoredPosition = CellPos(c, r);
                    go.AddComponent<Image>().color = wallColor;
                }
            }
        }

        var goal = new GameObject("Goal");
        goal.transform.SetParent(mazePanel, false);
        var gr = goal.AddComponent<RectTransform>();
        gr.sizeDelta = new Vector2(corridorPx * 0.6f, corridorPx * 0.6f);
        gr.anchoredPosition = CellPos(goalGCol, goalGRow);
        goal.AddComponent<Image>().color = goalColor;
    }

    // ── Button logic ──────────────────────────────────────────────────────────
    public void OnPlayResetPressed()
    {
        if (isRunning)
        {
            StopAllCoroutines();
            isRunning = false;
            ResetRobotToStart();
            if (resultPanel) resultPanel.SetActive(false);
            FindObjectOfType<UIManager>()?.SetPlayLabel();
            return;
        }

        if (resultPanel != null && resultPanel.activeSelf)
        {
            resultPanel.SetActive(false);
            lastRunFailed = false;
            ResetRobotToStart();
            FindObjectOfType<UIManager>()?.SetPlayLabel();
            return;
        }

        var prog = programPanel.GetProgram();
        if (prog.Count == 0) { ShowResult(msgNoGoal, failed: true); return; }
        StartCoroutine(ExecuteProgram(prog));
    }

    void ResetRobotToStart()
    {
        robotCol = startGCol;
        robotRow = startGRow;
        robotTransform.anchoredPosition = robotStartPos;
        robotTransform.SetAsLastSibling();
    }

    IEnumerator ExecuteProgram(List<BlockType> prog)
    {
        isRunning = true;
        FindObjectOfType<UIManager>()?.SetStopLabel();

        foreach (var block in prog)
        {
            Vector2Int dir = Vector2Int.zero;
            if (block == BlockType.MoveUp) dir = Vector2Int.up;
            if (block == BlockType.MoveDown) dir = Vector2Int.down;
            if (block == BlockType.MoveLeft) dir = Vector2Int.left;
            if (block == BlockType.MoveRight) dir = Vector2Int.right;

            if (!TryMove(dir))
            {
                ShowResult(msgWall, failed: true);
                isRunning = false;
                FindObjectOfType<UIManager>()?.SetRepeatLabel();
                yield break;
            }

            yield return SlideRobot(CellPos(robotCol, robotRow));

            // ── Passing through the goal mid-sequence does NOT end the level ──
            // The robot simply continues executing remaining blocks.
        }

        // ── All blocks finished ───────────────────────────────────────────────
        isRunning = false;

        // Win only if the robot's final position is the goal
        if (robotCol == goalGCol && robotRow == goalGRow)
        {
            if (currentLevel + 1 < LEVEL_SIZES.Length)
            {
                ShowResult(msgNext, failed: false);
                FindObjectOfType<UIManager>()?.SetPlayLabel();
                yield return new WaitForSeconds(1.5f);
                if (resultPanel) resultPanel.SetActive(false);
                programPanel.ClearProgram();
                LoadLevel(currentLevel + 1);
            }
            else
            {
                ShowResult(msgWinAll, failed: false);
                FindObjectOfType<UIManager>()?.SetPlayLabel();
                var returnToMap = FindObjectOfType<GameFinishTrigger>();
                if (returnToMap != null) returnToMap.FinishGame();
            }
        }
        else
        {
            // Robot did not end on the goal
            ShowResult(msgNoGoal, failed: true);
            FindObjectOfType<UIManager>()?.SetRepeatLabel();
        }
    }

    bool TryMove(Vector2Int dir)
    {
        int nc = robotCol, nr = robotRow;
        bool moved = false;
        while (true)
        {
            int tc = nc + dir.x, tr = nr - dir.y;
            if (tc < 0 || tc >= cols || tr < 0 || tr >= rows) break;
            if (mazeGrid[tr, tc] == 1) break;
            nc = tc; nr = tr; moved = true;
        }
        if (!moved) return false;
        robotCol = nc; robotRow = nr;
        return true;
    }

    IEnumerator SlideRobot(Vector2 target)
    {
        Vector2 start = robotTransform.anchoredPosition;
        for (float t = 0; t < 1; t += Time.deltaTime * moveSpeed)
        {
            robotTransform.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }
        robotTransform.anchoredPosition = target;
    }

    void ShowResult(string msg, bool failed)
    {
        lastRunFailed = failed;
        if (resultMessageLabel) resultMessageLabel.text = msg;
        if (resultPanel) resultPanel.SetActive(true);
    }

    public bool IsResultShowing() => resultPanel != null && resultPanel.activeSelf;
    public bool IsRunning() => isRunning;
    public bool LastRunFailed() => lastRunFailed;
}