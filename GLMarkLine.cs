using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class GLMarkLine {

    [Serializable]
    public struct LineSettings {
        public Transform[] paths;
        public Color color;
        public float width;
        public float edgeMarkLength;
        public float insideMarkLength;
        public int intervalCount;


        public LineSettings(Transform[] paths,
                            Color color,
                            float width,
                            float edgeMarkLength,
                            float insideMarkLength,
                            int intervalCount) {
            this.paths = paths;
            this.color = color;
            this.width = width;
            this.edgeMarkLength = edgeMarkLength;
            this.insideMarkLength = insideMarkLength;
            this.intervalCount = intervalCount;
        }
    }


    public LineSettings settings;
    private LineSettings _settings;
    private IReadOnlyList<Vector3> _paths = new List<Vector3>();
    private List<Vector3> currentPaths = new List<Vector3>();
    private Material lineMaterial;
    private float relativeWidth;
    public bool enabled = true;

    private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
    private static readonly int Cull = Shader.PropertyToID("_Cull");
    private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");


    private void InitMaterial() {
        if (!lineMaterial) {
            lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt(SrcBlend, (int) BlendMode.SrcAlpha);
            lineMaterial.SetInt(DstBlend, (int) BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt(Cull, (int) CullMode.Off);
            lineMaterial.SetInt(ZWrite, 0);
        }
    }


    public void Draw(LineSettings currentSetting) {
        settings = currentSetting;
        if (!enabled || settings.paths.Length <= 0 || settings.paths.Any(t => t == null)) return;
        IReadOnlyList<Vector3> vecPaths = settings.paths.Select(t => t.position).ToList();


        InitMaterial();
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        {
            GL.Begin(GL.QUADS);
            {
                GL.Color(settings.color);

                if (_settings.Equals(settings) && _paths.SequenceEqual(vecPaths)) {
                    Debug.Log("nonUpdated");

                    DotVertexes(currentPaths.ToArray());
                }
                else {
                    Debug.Log("Updated");

                    currentPaths.Clear();
                    _settings = settings;
                    _paths = vecPaths;

                    Vector3 v0, v1, o;
                    relativeWidth = 1.0f / Screen.width * settings.width * 0.5f;

                    for (int index = 0; index < _paths.Count - 1; index++) {
                        v0 = _paths[index];
                        v1 = _paths[index + 1];
                        o = (new Vector3(v1.y, v0.x, 0.0f) - new Vector3(v0.y, v1.x, 0.0f)).normalized;

                        DrawLine2D(v0, v1, o);
                        DrawMark2D(v0, v1, o);
                    }
                }
            }
            GL.End();
        }
        GL.PopMatrix();
    }


    void DrawLine2D(Vector3 v0, Vector3 v1, Vector3 o) {
        Vector3 n = o * relativeWidth;
        Vector3[] vertex = new[] {
            new Vector3(v0.x - n.x, v0.y - n.y, 0.0f),
            new Vector3(v0.x + n.x, v0.y + n.y, 0.0f),
            new Vector3(v1.x + n.x, v1.y + n.y, 0.0f),
            new Vector3(v1.x - n.x, v1.y - n.y, 0.0f),
        };

        DotVertexes(vertex);

        foreach (Vector3 v in vertex) {
            currentPaths.Add(v);
        }
    }


    void DrawMark2D(Vector3 v0, Vector3 v1, Vector3 o) {
        Vector3 markLength, _v0, _v1, _o;
        Vector3 _unitVec = (v1 - v0) / settings.intervalCount;
        List<Vector3> _pos = new List<Vector3>();

        for (int i = 0; i < settings.intervalCount + 1; i++) {
            _pos.Add(v0 + _unitVec * i);
        }

        for (int i = 0; i < _pos.Count; i++) {
            Vector3 vec = _pos[i];
            float length = (i == 0 || i == _pos.Count - 1 ? settings.edgeMarkLength : settings.insideMarkLength);

            markLength = o * length;
            _v0 = new Vector3(vec.x - markLength.x, vec.y - markLength.y, 0.0f);
            _v1 = new Vector3(vec.x + markLength.x, vec.y + markLength.y, 0.0f);
            _o = (new Vector3(_v1.y, _v0.x, 0.0f) - new Vector3(_v0.y, _v1.x, 0.0f)).normalized;

            DrawLine2D(_v0, _v1, _o);
        }
    }


    void DotVertexes(Vector3[] pos) {
        foreach (Vector3 v in pos) {
            GL.Vertex3(v.x, v.y, v.z);
        }
    }
}
