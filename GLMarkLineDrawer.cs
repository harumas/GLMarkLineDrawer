using System;
using UnityEngine;

[ExecuteInEditMode]
public class GLMarkLineDrawer : MonoBehaviour {

    [SerializeField]
    private GLMarkLine.LineSettings settings =
        new GLMarkLine.LineSettings(new Transform[0], Color.white, 100f, 2f, 1f, 10);

    private GLMarkLine line = new GLMarkLine();


    private void OnRenderObject() {
        line.Draw(settings);
    }
}
