using UnityEngine;
using UnityEngine.UI;

namespace UI.DDA
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIRadarChart : Graphic
    {
        public float[] values = new float[3] { 0.5f, 0.5f, 0.5f }; // APM, 명중률, 회피율 (0~1)
        public float radius = 100f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            // 중앙 꼭짓점
            vertex.position = Vector2.zero;
            vh.AddVert(vertex);

            // 삼각형 꼭짓점 3개
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f + 90f; // 90도(위)부터 시작
                float rad = angle * Mathf.Deg2Rad;
                float val = Mathf.Clamp01(values[i]);

                vertex.position = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * (radius * val);
                vh.AddVert(vertex);
            }

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);
            vh.AddTriangle(0, 3, 1);
        }

        public void UpdateValues(float apm, float accuracy, float evasion)
        {
            values[0] = apm; values[1] = accuracy; values[2] = evasion;
            SetVerticesDirty();
        }
    }
}
