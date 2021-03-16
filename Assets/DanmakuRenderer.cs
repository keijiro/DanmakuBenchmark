using UnityEngine;
using UnityEngine.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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
            _mesh.SetVertexBufferData(varray, 0, 0, bullets.Length);

        _mesh.SetIndexBufferParams(vcount, IndexFormat.UInt32);

        using (var iarray = CreateIndexArray(vcount))
            _mesh.SetIndexBufferData(iarray, 0, 0, vcount);

        _mesh.SetSubMesh(0, new SubMeshDescriptor(0, vcount, MeshTopology.Quads), MeshUpdateFlags.DontRecalculateBounds);
    }

    NativeArray<Quad> CreateVertexArray(NativeSlice<Bullet> bullets)
    {
        var array = new NativeArray<Quad>
          (bullets.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        new VertexConstructJob(bullets, array).Schedule(bullets.Length, 64).Complete();

        return array;
    }

    readonly struct Quad
    {
        readonly float4 _v0, _v1, _v2, _v3;

        public Quad(float2 center, float extent)
        {
            _v0 = math.float4(center.x - extent, center.y + extent, 0, 1);
            _v1 = math.float4(center.x + extent, center.y + extent, 1, 1);
            _v2 = math.float4(center.x + extent, center.y - extent, 1, 0);
            _v3 = math.float4(center.x - extent, center.y - extent, 0, 0);
        }
    }

    [BurstCompile]
    struct VertexConstructJob : IJobParallelFor
    {
        [ReadOnly] NativeSlice<Bullet> _bullets;
        [WriteOnly] NativeArray<Quad> _output;

        public VertexConstructJob
          (NativeSlice<Bullet> bullets, NativeArray<Quad> output)
          => (_bullets, _output) = (bullets, output);

        public void Execute(int i)
          => _output[i] = new Quad(_bullets[i].Position, 0.01f);
    }

    [BurstCompile]
    struct IndexConstructJob : IJobParallelFor
    {
        [WriteOnly] NativeArray<uint> _output;

        public IndexConstructJob(NativeArray<uint> output)
          => _output = output;

        public void Execute(int i)
          => _output[i] = (uint)i;
    }

    NativeArray<uint> CreateIndexArray(int vcount)
    {
        var array = new NativeArray<uint>
          (vcount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        new IndexConstructJob(array).Schedule(vcount, 64).Complete();

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
