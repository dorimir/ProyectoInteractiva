#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MazeSceneSetup
{
    const string SpriteFolder = "Assets/Sprites/Maze/";

    [MenuItem("Tools/Laberinto/Crear escenas")]
    public static void CreateScenes()
    {
        foreach (var n in new[] { "Suelo", "Inicio", "Agujero", "Muro", "Meta", "Jugador" })
            ConfigureSprite(SpriteFolder + n + ".png");

        CreateScene(Technique.OSLA, "Pr2.2", "Pr22_OSLA");
        CreateScene(Technique.QLearning, "LAB04", "LAB04_QLearning");

        EditorSceneManager.OpenScene("Assets/Scenes/LAB04/LAB04_QLearning.unity");
        Debug.Log("[Laberinto] Escenas creadas: Pr2.2/Pr22_OSLA y LAB04/LAB04_QLearning");
    }

    static void CreateScene(Technique technique, string folder, string sceneName)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes/" + folder))
            AssetDatabase.CreateFolder("Assets/Scenes", folder);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.16f, 0.17f, 0.2f);
        camGo.transform.position = new Vector3(0, 0, -10);

        var go = new GameObject("Laberinto_" + technique);
        var runner = go.AddComponent<MazeRunner>();
        runner.technique = technique;
        runner.floorSprite = Load("Suelo");
        runner.startSprite = Load("Inicio");
        runner.holeSprite = Load("Agujero");
        runner.wallSprite = Load("Muro");
        runner.goalSprite = Load("Meta");
        runner.playerSprite = Load("Jugador");
        runner.BuildBoard();
        AddUI(runner);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/" + folder + "/" + sceneName + ".unity");
    }

    [MenuItem("Tools/Laberinto/Redibujar laberinto (vista previa)")]
    public static void Rebuild()
    {
        var r = Object.FindFirstObjectByType<MazeRunner>();
        if (r != null) { r.BuildBoard(); EditorSceneManager.MarkSceneDirty(r.gameObject.scene); }
    }

    [MenuItem("Tools/Laberinto/Probar técnicas en consola")]
    public static void TestInConsole()
    {
        foreach (Technique t in System.Enum.GetValues(typeof(Technique)))
        {
            MazeAI.Player p;
            string iter = "";
            if (t == Technique.QLearning)
            {
                string qtable_filename = System.IO.Path.Combine(Application.dataPath, "..", "Data", "qtable.csv");
                var st = new MazeAI.GameState();
                st.Reset(new MazeAI.MazeOriginal(), 100);
                var ql = new MazeAI.QLearner();
                ql.SetHeuristic(new MazeAI.HeuristicRafael());
                MazeAI.QLearner.WriteQtableToFile(ql.Learn(st, 500), qtable_filename);
                p = new MazeAI.QLPlayer(qtable_filename);
                iter = "  iteraciones=" + ql.first_goal_episode;
            }
            else p = new MazeAI.OSLAPlayer();

            var result = new MazeAI.Game().Run(new MazeAI.MazeOriginal(), p, 1f, 100, false);
            Debug.Log("[" + t + "] meta=" + result.reached_goal + "  pasos=" + result.iterations + iter);
        }
    }

    [MenuItem("Tools/Laberinto/Crear UI (Canvas) en la escena actual")]
    public static void CreateUIInCurrentScene()
    {
        var runner = Object.FindFirstObjectByType<MazeRunner>();
        if (runner == null) { Debug.LogWarning("[Laberinto] No hay ningún MazeRunner en la escena."); return; }
        AddUI(runner);
        EditorSceneManager.MarkSceneDirty(runner.gameObject.scene);
        EditorSceneManager.SaveScene(runner.gameObject.scene);
        Debug.Log("[Laberinto] UI creada en la escena " + runner.gameObject.scene.name);
    }

    static void AddUI(MazeRunner runner)
    {
        var old = GameObject.Find("UI");
        if (old != null) Object.DestroyImmediate(old);

        var canvasGo = new GameObject("UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        int row = 0;
        runner.iterationsText = null;
        if (runner.technique == Technique.QLearning)
            runner.iterationsText = CreateText(canvasGo.transform, "IteracionesText", "Iteraciones: 0", row++);
        runner.stepsText = CreateText(canvasGo.transform, "PasosText", "Pasos: 0", row);

        EditorUtility.SetDirty(runner);
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string text, int row)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(30, -30 - row * 55);
        rt.sizeDelta = new Vector2(700, 50);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = 40;
        t.fontStyle = FontStyles.Bold;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.TopLeft;
        return t;
    }

    static Sprite Load(string n) => AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + n + ".png");

    static void ConfigureSprite(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogWarning("[Laberinto] No encuentro " + path); return; }
        if (imp.textureType == TextureImporterType.Sprite) return;
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = 64;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.SaveAndReimport();
    }
}
#endif
