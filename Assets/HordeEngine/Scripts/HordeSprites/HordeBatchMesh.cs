using System;
using UnityEngine;

namespace HordeEngine
{
    /// <summary>
    /// Fixed size mesh. Triangles that are not in use will be rendered as degenerate (empty) by setting all vertices to 0.
    /// </summary>
    public class HordeBatchMesh
    {
        [NonSerialized] public Mesh Mesh;
        [NonSerialized] public int Capacity;
        [NonSerialized] public int ActiveQuadCount;

        Vector3[] vertices_;
        Vector2[] UV_;
        int[] indices_;
        Color32[] colors_;

        int idxAlreadyZeroed_;

        const int VerticesPerQuad = 4;
        const int IndicesPerQuad = 6;

        public HordeBatchMesh(int capacity)
        {
            Capacity = capacity;

            Mesh = new Mesh();
            Mesh.MarkDynamic();
            Mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

            vertices_ = new Vector3[capacity * VerticesPerQuad];
            UV_ = new Vector2[capacity * VerticesPerQuad];
            colors_ = new Color32[capacity * VerticesPerQuad];
            indices_ = new int[capacity * IndicesPerQuad];
            InitializeIndices();

            // All vertices are zeroed to begin with
            idxAlreadyZeroed_ = 0;

            ApplyChanges();

            // Triangles are set only once
            Mesh.SetTriangles(indices_, 0, false);

            Mesh.UploadMeshData(false);
        }

        void InitializeIndices()
        {
            int vert0 = 0;
            for (int i = 0; i < indices_.Length; i += IndicesPerQuad)
            {
                indices_[i + 0] = vert0 + 0;
                indices_[i + 1] = vert0 + 1;
                indices_[i + 2] = vert0 + 3;

                indices_[i + 3] = vert0 + 1;
                indices_[i + 4] = vert0 + 2;
                indices_[i + 5] = vert0 + 3;

                vert0 += 4;
            }
        }

        public void Clear()
        {
            ActiveQuadCount = 0;
        }

        public void ApplyChanges()
        {
            ZeroVertices(ActiveQuadCount, idxAlreadyZeroed_ - 1);
            idxAlreadyZeroed_ = ActiveQuadCount;
            Mesh.RecalculateBounds(); // TODO: What be nice to get rid of this
            Mesh.vertices = vertices_;
            Mesh.uv = UV_;
            Mesh.colors32 = colors_;

            Mesh.UploadMeshData(false);
        }

        void ZeroVertices(int firstQuad, int lastQuad)
        {
            for (int i = firstQuad; i <= lastQuad; ++i)
            {
                int vert0 = i * 4;
                vertices_[vert0 + 0].x = 0.0f;
                vertices_[vert0 + 0].y = 0.0f;
                vertices_[vert0 + 0].z = 0.0f;

                vertices_[vert0 + 1].x = 0.0f;
                vertices_[vert0 + 1].y = 0.0f;
                vertices_[vert0 + 1].z = 0.0f;

                vertices_[vert0 + 2].x = 0.0f;
                vertices_[vert0 + 2].y = 0.0f;
                vertices_[vert0 + 2].z = 0.0f;

                vertices_[vert0 + 3].x = 0.0f;
                vertices_[vert0 + 3].y = 0.0f;
                vertices_[vert0 + 3].z = 0.0f;
            }
        }

        public void AddQuad(Vector3 center, Vector2 size, float rotationDegrees, float zSkew, Color32 color)
        {
            AddQuad(center, size, rotationDegrees, zSkew, Vector2.up, Vector2.one, color);
        }

        public void AddQuad(Vector3 center, Vector2 size, float rotationDegrees, float zSkew, Vector2 uvTopLeft, Vector2 uvSize, Color32 color)
        {
            // 0---1
            // | / | = [0, 1, 3] and [1, 2, 3]
            // 3---2

            float halfW = size.x * 0.5f;
            float halfH = size.y * 0.5f;

            float sin = Mathf.Sin(-rotationDegrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(-rotationDegrees * Mathf.Deg2Rad);
            float sinX = halfW * sin;
            float cosX = halfW * cos;
            float sinY = halfH * sin;
            float cosY = halfH * cos;

            int vert0 = ActiveQuadCount * 4;
            vertices_[vert0 + 0].x = (-halfW * cos -  halfH * sin) + center.x;
            vertices_[vert0 + 1].x = ( halfW * cos -  halfH * sin) + center.x;
            vertices_[vert0 + 2].x = ( halfW * cos - -halfH * sin) + center.x;
            vertices_[vert0 + 3].x = (-halfW * cos - -halfH * sin) + center.x;

            vertices_[vert0 + 0].y = (-halfW * sin +  halfH * cos) + center.y;
            vertices_[vert0 + 1].y = ( halfW * sin +  halfH * cos) + center.y;
            vertices_[vert0 + 2].y = ( halfW * sin + -halfH * cos) + center.y;
            vertices_[vert0 + 3].y = (-halfW * sin + -halfH * cos) + center.y;

            vertices_[vert0 + 0].z = center.z - zSkew;
            vertices_[vert0 + 1].z = center.z - zSkew;
            vertices_[vert0 + 2].z = center.z;
            vertices_[vert0 + 3].z = center.z;

            UV_[vert0 + 0].x = uvTopLeft.x;
            UV_[vert0 + 0].y = uvTopLeft.y;

            UV_[vert0 + 1].x = uvTopLeft.x + uvSize.x;
            UV_[vert0 + 1].y = uvTopLeft.y;

            UV_[vert0 + 2].x = uvTopLeft.x + uvSize.x;
            UV_[vert0 + 2].y = uvTopLeft.y - uvSize.y;

            UV_[vert0 + 3].x = uvTopLeft.x;
            UV_[vert0 + 3].y = uvTopLeft.y - uvSize.y;

            colors_[vert0 + 0] = color;
            colors_[vert0 + 1] = color;
            colors_[vert0 + 2] = color;
            colors_[vert0 + 3] = color;

            ActiveQuadCount++;
        }
    }
}
