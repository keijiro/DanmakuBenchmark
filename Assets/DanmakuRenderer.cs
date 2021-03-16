using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System.Collections.Generic;

namespace Danmaku {

public class DanmakuRenderer : System.IDisposable
{
    Mesh _mesh;
    List<Vector3> _vertices;
    List<Vector2> _uvs;
    List<int> _indices;

    public DanmakuRenderer(int maxBulletCount)
    {
        _mesh = new Mesh();
        _mesh.indexFormat = IndexFormat.UInt32;

        _vertices = new List<Vector3>();
        _uvs = new List<Vector2>();
        _indices = new List<int>();
    }

    public void Dispose()
    {
        Object.Destroy(_mesh);
    }

    public void ConstructMesh(NativeSlice<Bullet> bullets)
    {
        _mesh.Clear();
        _vertices.Clear();
        _uvs.Clear();
        _indices.Clear();

        for (var i = 0; i < bullets.Length; i++)
        {
            var p = bullets[i].Position;

            var dx = 0.01f;
            var dy = 0.01f;

            _vertices.Add(new Vector3(p.x - dx, p.y - dy, 0));
            _vertices.Add(new Vector3(p.x + dx, p.y - dy, 0));
            _vertices.Add(new Vector3(p.x - dx, p.y + dy, 0));
            _vertices.Add(new Vector3(p.x + dx, p.y + dy, 0));

            _uvs.Add(new Vector2(0, 0));
            _uvs.Add(new Vector2(1, 0));
            _uvs.Add(new Vector2(0, 1));
            _uvs.Add(new Vector2(1, 1));

            _indices.Add(i * 4 + 2);
            _indices.Add(i * 4 + 3);
            _indices.Add(i * 4 + 1);
            _indices.Add(i * 4);
        }

        _mesh.SetVertices(_vertices);
        _mesh.SetUVs(0, _uvs);
        _mesh.SetIndices(_indices, MeshTopology.Quads, 0);
    }

    public void DrawMesh(Material material)
      => Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, material, 0);
}

} // namespace Danmaku
