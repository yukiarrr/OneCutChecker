using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScreenController : MonoBehaviour
{
    [HideInInspector]
    public List<Texture2D> Texture2Ds = new List<Texture2D>();
    [HideInInspector]
    public int FramePosition;

    void Start()
    {
    }

    void Update()
    {
        if (!EditorApplication.isPaused)
        {
            FramePosition = -1;

            if (Texture2Ds.Count > 0)
            {
                Texture2Ds.Clear();
            }

            return;
        }

        StartCoroutine(CaptureScreen());
    }

    void OnGUI()
    {
        if (!EditorApplication.isPaused || Texture2Ds.Count == 0)
        {
            return;
        }

        if (FramePosition + 1 != Texture2Ds.Count)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2Ds[FramePosition], ScaleMode.ScaleToFit, false);
        }
    }

    IEnumerator CaptureScreen()
    {
        FramePosition = Mathf.Max(Texture2Ds.Count - 1, 0);

        yield return new WaitForEndOfFrame();

        var texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        texture2D.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture2D.Apply();
        Texture2Ds.Add(texture2D);

        FramePosition = Texture2Ds.Count - 1;
    }
}