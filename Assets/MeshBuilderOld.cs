using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections.Generic;

namespace Danmaku {

static class MeshBuilderOld
{
    static bool _allocated;
    static List<Vector3> _vertices;
    static List<Vector2> _uvs;
    static List<int> _indices;

    public static void Build(NativeSlice<Bullet> bullets, float size, Mesh mesh)
    {
        if (!_allocated)
        {
            // Buffer allocation
            _vertices = new List<Vector3>();
            _uvs = new List<Vector2>();
            _indices = new List<int>();
            _allocated = true;
        }

        // Position
        _vertices.Clear();

        for (var i = 0; i < bullets.Length; i++)
        {
            var p = bullets[i].Position;
            _vertices.Add(new Vector3(p.x - size, p.y - size, 0));
            _vertices.Add(new Vector3(p.x - size, p.y + size, 0));
            _vertices.Add(new Vector3(p.x + size, p.y + size, 0));
            _vertices.Add(new Vector3(p.x + size, p.y - size, 0));
        }

        // UV
        _uvs.Clear();

        var uv0 = new Vector2(0, 0);
        var uv1 = new Vector2(0, 1);
        var uv2 = new Vector2(1, 1);
        var uv3 = new Vector2(1, 0);

        for (var i = 0; i < bullets.Length; i++)
        {
            _uvs.Add(uv0);
            _uvs.Add(uv1);
            _uvs.Add(uv2);
            _uvs.Add(uv3);
        }

        // Index
        _indices.Clear();

        for (var i = 0; i < bullets.Length * 4; i++)
            _indices.Add(i);

        // Mesh construction
        mesh.Clear();
        mesh.SetVertices(_vertices);
        mesh.SetUVs(0, _uvs);
        mesh.SetIndices(_indices, MeshTopology.Quads, 0);
    }
}

} // namespace Danmaku
