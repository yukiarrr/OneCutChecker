using System.Threading;
#if NET_4_6
using System.Threading.Tasks;
#endif
using UnityEditor;
using UnityEngine;

public class OneCutChecker : EditorWindow
{
    bool isPlayingLoop;
    int speed;

    static OneCutChecker oneCutChecker;
    static ScreenController screenController;
    static EditorWindow gameView;
    static bool useShortcutKey = true;

    [MenuItem("Window/One Cut Checker")]
    static void ShowWindow()
    {
        if (oneCutChecker == null)
        {
            oneCutChecker = GetWindow<OneCutChecker>();
            oneCutChecker.titleContent = new GUIContent("One Cut Checker");
            oneCutChecker.Show();
        }
    }

    [MenuItem("Edit/Pause _p")]
    static void Pause()
    {
        if (!useShortcutKey || !EditorApplication.isPlaying)
        {
            if (focusedWindow != null)
            {
                focusedWindow.SendEvent(Event.KeyboardEvent("p"));
            }

            return;
        }

        EditorApplication.isPaused = !EditorApplication.isPaused;
    }

    [MenuItem("Edit/Previous frame _LEFT")]
    static void PrevFrame()
    {
        if (!useShortcutKey || !EditorApplication.isPlaying)
        {
            if (focusedWindow != null)
            {
                focusedWindow.SendEvent(Event.KeyboardEvent("LEFT"));
            }

            return;
        }

        if (screenController != null && screenController.FramePosition > 0)
        {
            screenController.FramePosition--;
            gameView.Repaint();
        }
    }

    [MenuItem("Edit/Next frame _RIGHT")]
    static void NextFrame()
    {
        if (!useShortcutKey || !EditorApplication.isPlaying)
        {
            if (focusedWindow != null)
            {
                focusedWindow.SendEvent(Event.KeyboardEvent("RIGHT"));
            }

            return;
        }

        if (screenController != null && screenController.FramePosition < screenController.Texture2Ds.Count - 1)
        {
            screenController.FramePosition++;
            gameView.Repaint();
        }
        else
        {
            EditorApplication.Step();
        }
    }

    void OnGUI()
    {
        if (speed == 0)
        {
            speed = Application.targetFrameRate == -1 ? 60 : Application.targetFrameRate;
        }

        if (gameView == null)
        {
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            gameView = GetWindow(type);
        }

        DoToolbar();

        EditorGUILayout.LabelField("Shortcut key");

        EditorGUILayout.BeginVertical("box");

        var style = new GUIStyle(GUI.skin.label);
        style.wordWrap = true;
        EditorGUILayout.LabelField("p  : Pause/Resume\n← : Previous frame\n→ : Next frame", style);

        EditorGUILayout.EndVertical();

        useShortcutKey = EditorGUILayout.Toggle("Use shortcut key", useShortcutKey);

        if (!EditorApplication.isPaused || screenController == null)
        {
            return;
        }

        if (screenController.Texture2Ds.Count > 1)
        {
            EditorGUILayout.Space();

            screenController.FramePosition = EditorGUILayout.IntSlider("Frame position", screenController.FramePosition, 0, screenController.Texture2Ds.Count - 1);
            gameView.Repaint();

            EditorGUILayout.Space();

            speed = EditorGUILayout.IntSlider("Speed (FPS)", speed, 1, 150);
            gameView.Repaint();

            EditorGUILayout.Space();

            if (GUILayout.Button("Play"))
            {
                isPlayingLoop = false;

                PlayAsync();
            }

            if (GUILayout.Button(isPlayingLoop ? "Pause loop" : "Play loop"))
            {
                isPlayingLoop = !isPlayingLoop;

                if (isPlayingLoop)
                {
                    PlayLoopAsync();
                }
            }
        }
    }

    void Update()
    {
        if (!EditorApplication.isPlaying)
        {
            screenController = null;
            isPlayingLoop = false;

            return;
        }

        if (screenController == null)
        {
            screenController = new GameObject("Screen Controller").AddComponent<ScreenController>();
        }

        Repaint();

        if (!EditorApplication.isPaused)
        {
            isPlayingLoop = false;

            return;
        }
    }

    void DoToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    void PlayAsync()
    {
        screenController.FramePosition = 0;

#if NET_4_6
        Task.Run(() =>
        {
            Play();
        });
#else
        var thread = new Thread(() =>
        {
            Play();
        });
        thread.Start();
#endif
    }

    void Play()
    {
        while (!isPlayingLoop)
        {
            if (screenController.FramePosition == screenController.Texture2Ds.Count - 1)
            {
                return;
            }
            else
            {
                screenController.FramePosition++;
            }

            Thread.Sleep(Mathf.RoundToInt(1.0f / speed * 1000.0f));
        }
    }

    void PlayLoopAsync()
    {
#if NET_4_6
        Task.Run(() =>
        {
            PlayLoop();
        });
#else
        var thread = new Thread(() =>
        {
            PlayLoop();
        });
        thread.Start();
#endif
    }

    void PlayLoop()
    {
        while (isPlayingLoop)
        {
            if (screenController.FramePosition == screenController.Texture2Ds.Count - 1)
            {
                screenController.FramePosition = 0;
            }
            else
            {
                screenController.FramePosition++;
            }

            Thread.Sleep(Mathf.RoundToInt(1.0f / speed * 1000.0f));
        }
    }
}