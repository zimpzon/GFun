using System.Collections.Generic;
using UnityEngine;

namespace HordeEngine
{
    public class DynamicQuadMesh
    {
        Mesh mesh_;

        List<Vector3> vertices_;
        List<Vector2> UV_;
        List<int> indices_;
        List<Color> colors_;

        public DynamicQuadMesh(int initialCapacity)
        {
            mesh_ = new Mesh();
            mesh_.MarkDynamic();

            const int VerticesPerQuad = 4;
            const int IndicesPerQuad = 6;
            vertices_ = new List<Vector3>(initialCapacity * VerticesPerQuad);
            UV_ = new List<Vector2>(initialCapacity * VerticesPerQuad);
            colors_ = new List<Color>(initialCapacity * VerticesPerQuad);
            indices_ = new List<int>(initialCapacity * IndicesPerQuad);
        }

        public int QuadCount()
        {
            return vertices_.Count / 4;
        }

        public void Clear()
        {
            vertices_.Clear();
            UV_.Clear();
            colors_.Clear();
            indices_.Clear();
        }

        public void ApplyChanges()
        {
            mesh_.Clear();
            mesh_.SetVertices(vertices_);
            mesh_.SetUVs(0, UV_);
            mesh_.SetColors(colors_);
            mesh_.SetTriangles(indices_, 0, false);
            mesh_.UploadMeshData(false);
        }

        public Mesh GetMesh()
        {
            return mesh_;
        }

        public void AddQuad(Vector3 center, Vector2 size, float rotationDegrees, float zSkew, Color color)
        {
            AddQuad(center, size, rotationDegrees, zSkew, Vector2.up, Vector2.one, color);
        }

        public void AddQuad(Vector3 center, Vector2 size, float rotationDegrees, float zSkew, Vector2 UVTopLeft, Vector2 uvSize, Color color)
        {
            // 0---1
            // | / | = [0, 1, 3] and [1, 2, 3]
            // 3---2

            float halfW = size.x * 0.5f;
            float halfH = size.y * 0.5f;
            float halfSkew = zSkew * 0.5f;
            vertices_.Add(new Vector3(center.x - halfW, center.y + halfH, center.z - halfSkew));
            vertices_.Add(new Vector3(center.x + halfW, center.y + halfH, center.z - halfSkew));
            vertices_.Add(new Vector3(center.x + halfW, center.y - halfH, center.z + halfSkew));
            vertices_.Add(new Vector3(center.x - halfW, center.y - halfH, center.z + halfSkew));

            UV_.Add(UVTopLeft);
            UV_.Add(new Vector2(UVTopLeft.x + uvSize.x, UVTopLeft.y));
            UV_.Add(new Vector2(UVTopLeft.x + uvSize.x, UVTopLeft.y - uvSize.y));
            UV_.Add(new Vector2(UVTopLeft.x, UVTopLeft.y - uvSize.y));

            colors_.Add(color);
            colors_.Add(color);
            colors_.Add(color);
            colors_.Add(color);

            int idx0 = vertices_.Count - 4;
            indices_.Add(idx0 + 0);
            indices_.Add(idx0 + 1);
            indices_.Add(idx0 + 3);

            indices_.Add(idx0 + 1);
            indices_.Add(idx0 + 2);
            indices_.Add(idx0 + 3);
        }
    }
}
