using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections.Generic;

namespace Danmaku {

public class DanmakuRenderer : System.IDisposable
{
    Mesh _mesh;

    public DanmakuRenderer(int maxBulletCount)
    {
        _mesh = new Mesh();
        _mesh.indexFormat = IndexFormat.UInt32;
    }

    public void Dispose()
    {
        Object.Destroy(_mesh);
    }

    public void DrawMesh(Material material)
      => Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, material, 0);

    public void ConstructMesh(NativeSlice<Bullet> bullets)
    {
        _mesh.Clear();

        var vcount = bullets.Length * 4;

        _mesh.SetVertexBufferParams
          (vcount,
           new VertexAttributeDescriptor
             (VertexAttribute.Position, VertexAttributeFormat.Float32, 2),
           new VertexAttributeDescriptor
             (VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));

        using (var varray = CreateVertexArray(bullets))
            _mesh.SetVertexBufferData(varray, 0, 0, vcount);

        _mesh.SetIndexBufferParams(vcount, IndexFormat.UInt32);

        using (var iarray = CreateIndexArray(vcount))
            _mesh.SetIndexBufferData(iarray, 0, 0, vcount);

        _mesh.SetSubMesh(0, new SubMeshDescriptor(0, vcount, MeshTopology.Quads), MeshUpdateFlags.DontRecalculateBounds);
    }

    NativeArray<float4> CreateVertexArray(NativeSlice<Bullet> bullets)
    {
        var array = new NativeArray<float4>
          (bullets.Length * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        var offs = 0;

        for (var i = 0; i < bullets.Length; i++)
        {
            var p = bullets[i].Position;

            var dx = 0.01f;
            var dy = 0.01f;

            array[offs++] = math.float4(p.x - dx, p.y + dy, 0, 1);
            array[offs++] = math.float4(p.x + dx, p.y + dy, 1, 1);
            array[offs++] = math.float4(p.x + dx, p.y - dy, 1, 0);
            array[offs++] = math.float4(p.x - dx, p.y - dy, 0, 0);
        }

        return array;
    }

    NativeArray<uint> CreateIndexArray(int vcount)
    {
        var array = new NativeArray<uint>
          (vcount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        for (var i = 0; i < vcount; i++)
            array[i] = (uint)i;

        return array;
    }

#if NOT_USED

    List<Vector3> _vertices;
    List<Vector2> _uvs;
    List<int> _indices;

    public void ConstructMesh(NativeSlice<Bullet> bullets)
    {
        if (_vertices == null) _vertices = new List<Vector3>();
        if (_uvs == null) _uvs = new List<Vector2>();
        if (_indices == null) _indices = new List<int>();

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

#endif
}

} // namespace Danmaku
