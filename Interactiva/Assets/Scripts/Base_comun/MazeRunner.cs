using System.Collections;
using System.IO;
using UnityEngine;
using TMPro;
using MazeAI;

public enum Technique
{
    OSLA,
    QLearning
}

public class MazeRunner : MonoBehaviour
{
    [Header("Partida")]
    public Technique technique = Technique.OSLA;
    [Tooltip("Tiempo para pensar (budget), igual que en Python")]
    public float budget = 1f;
    [Tooltip("Número máximo de pasos (max_iterations en Python)")]
    public int maxSteps = 100;
    public bool playOnStart = true;
    [Range(0.02f, 2f)] public float stepDelay = 0.3f;
    public bool verbose = false;

    [Header("Q-Learning (solo LAB04)")]
    [Tooltip("Episodios de aprendizaje (n_episodes)")]
    public int qlEpisodes = 500;

    [Header("Sprites (cámbialos cuando quieras)")]
    public Sprite floorSprite;
    public Sprite startSprite;
    public Sprite holeSprite;
    public Sprite wallSprite;
    public Sprite goalSprite;
    public Sprite playerSprite;

    [Header("Aspecto")]
    public float cellSize = 1f;
    public bool showPath = true;
    public Color pathColor = new Color(1f, 0.45f, 0.1f);
    [Range(0.02f, 0.5f)] public float pathWidth = 0.12f;
    public bool fitCamera = true;

    [Header("UI (Canvas)")]
    public TMP_Text stepsText;
    public TMP_Text iterationsText;
    public string stepsFormat = "Pasos: {0}";
    public string iterationsFormat = "Iteraciones: {0}";
    public string iterationsNotReached = "Iteraciones: no llegó a la meta";

    Game game;
    Player player;
    Transform board, playerTf;
    LineRenderer path;
    Coroutine running;
    bool learns;
    int trainingIterations;
    static Sprite fallback;

    void Start()
    {
        if (playOnStart) Play();
        else BuildBoard();
    }

    Player CreatePlayer()
    {
        trainingIterations = 0;
        learns = technique == Technique.QLearning;
        switch (technique)
        {
            case Technique.QLearning:
            {
                string qtable_filename = Path.Combine(Application.dataPath, "..", "Data", "qtable.csv");
                var game_state = new GameState();
                game_state.Reset(new MazeOriginal(), maxSteps);

                var ql = new QLearner();
                ql.SetHeuristic(new HeuristicRafael());
                double[,] qtable = ql.Learn(game_state, qlEpisodes);
                QLearner.WriteQtableToFile(qtable, qtable_filename);

                trainingIterations = ql.first_goal_episode;
                Debug.Log("[QLearning] Llegó a la meta por primera vez en el episodio " +
                          ql.first_goal_episode + " de " + qlEpisodes);
                return new QLPlayer(qtable_filename);
            }
            default:
                return new OSLAPlayer();
        }
    }

    public void Play()
    {
        if (running != null) StopCoroutine(running);
        BuildBoard();
        running = StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        player = CreatePlayer();
        player.Reset();
        game = new Game();
        game.game_state.Reset(new MazeOriginal(), maxSteps);
        UpdateTexts();

        while (!game.game_state.IsTerminal())
        {
            yield return new WaitForSeconds(stepDelay);
            Observation observation = game.game_state.GetObservation();
            if (verbose) observation.PrintMaze();
            Action action = player.Think(observation, budget);
            if (action == null)
                break;
            if (verbose) Debug.Log("Player select: " + action);
            game.forward_model.Play(game.game_state, action);

            Pos p = game.game_state.GetPosition();
            UpdateTexts();
            yield return MovePlayer(CellToWorld(p.row, p.column));
        }

        Pos end = game.game_state.GetPosition();
        string result = game.game_state.ReachedGoal() ? "ha llegado a la meta"
            : game.game_state.maze.IsHole(end.row, end.column) ? "ha caído en un agujero"
            : "no encontró la salida";
        Debug.Log("[" + technique + "] MazeOriginal -> " + player + " " + result +
                  " en " + game.game_state.GetIterationNumber() + " pasos");
        running = null;
    }

    IEnumerator MovePlayer(Vector3 target)
    {
        Vector3 from = playerTf.localPosition;
        if (from == target) yield break;

        int last = path.positionCount;
        path.positionCount = last + 1;

        float t = 0, dur = Mathf.Min(stepDelay * 0.8f, 0.25f);
        while (t < dur)
        {
            t += Time.deltaTime;
            playerTf.localPosition = Vector3.Lerp(from, target, t / dur);
            path.SetPosition(last, playerTf.localPosition);
            yield return null;
        }
        playerTf.localPosition = target;
        path.SetPosition(last, target);
    }

    public void BuildBoard()
    {
        Maze m = new MazeOriginal();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform old = transform.GetChild(i);
            if (old.name != "Board") continue;
            old.SetParent(null);
            if (Application.isPlaying) Destroy(old.gameObject); else DestroyImmediate(old.gameObject);
        }
        board = new GameObject("Board").transform;
        board.SetParent(transform, false);

        SpriteRenderer firstTile = null;
        for (int r = 0; r < m.number_rows; r++)
            for (int c = 0; c < m.number_columns; c++)
            {
                var floor = MakeTile("Cell_" + r + "_" + c, floorSprite, CellToWorld(r, c), 0);
                if (firstTile == null) firstTile = floor;
                Sprite overlay = null;
                if (m.IsStart(r, c)) overlay = startSprite;
                else if (m.IsHole(r, c)) overlay = holeSprite;
                else if (m.IsWall(r, c)) overlay = wallSprite;
                else if (m.IsGoal(r, c)) overlay = goalSprite;
                if (overlay != null)
                    MakeTile("Item_" + r + "_" + c, overlay, CellToWorld(r, c), 1).transform.SetParent(floor.transform, true);
            }

        Pos s = m.GetStartPosition();
        playerTf = MakeTile("Player", playerSprite, CellToWorld(s.row, s.column), 3).transform;

        var pathGo = new GameObject("Path");
        pathGo.transform.SetParent(board, false);
        path = pathGo.AddComponent<LineRenderer>();
        path.useWorldSpace = false;
        path.sharedMaterial = firstTile.sharedMaterial;
        path.startColor = path.endColor = pathColor;
        path.startWidth = path.endWidth = pathWidth * cellSize;
        path.numCornerVertices = 4;
        path.numCapVertices = 4;
        path.sortingOrder = 2;
        path.positionCount = 1;
        path.SetPosition(0, CellToWorld(s.row, s.column));
        path.enabled = showPath;

        if (fitCamera) FitCamera(m);
    }

    SpriteRenderer MakeTile(string name, Sprite sprite, Vector3 pos, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(board, false);
        go.transform.localPosition = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : Fallback();
        sr.sortingOrder = order;
        Vector2 size = sr.sprite.bounds.size;
        float k = cellSize / Mathf.Max(size.x, size.y);
        go.transform.localScale = new Vector3(k, k, 1);
        return sr;
    }

    Vector3 CellToWorld(int row, int column) => new Vector3(column * cellSize, -row * cellSize, 0);

    void FitCamera(Maze m)
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.orthographic = true;
        float w = m.number_columns * cellSize, h = m.number_rows * cellSize;
        Vector3 center = transform.position + new Vector3((w - cellSize) / 2f, -(h - cellSize) / 2f, 0);
        float aspect = cam.aspect > 0 ? cam.aspect : 16f / 9f;
        cam.orthographicSize = Mathf.Max(h / 2f, w / 2f / aspect) + cellSize * 0.75f;
        cam.transform.position = new Vector3(center.x, center.y, -10);
    }

    static Sprite Fallback()
    {
        if (fallback == null)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.magenta);
            tex.Apply();
            fallback = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }
        return fallback;
    }

    void UpdateTexts()
    {
        if (stepsText != null)
            stepsText.text = string.Format(stepsFormat, game != null ? game.game_state.GetIterationNumber() : 0);

        if (iterationsText != null)
        {
            iterationsText.gameObject.SetActive(learns);
            if (learns)
                iterationsText.text = trainingIterations > 0
                    ? string.Format(iterationsFormat, trainingIterations)
                    : iterationsNotReached;
        }
    }
}
