using UnityEngine;
using UnityEngine.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Danmaku {

static class MeshBuilderNew
{
    public static void Build(NativeSlice<Bullet> bullets, float size, Mesh mesh)
    {
        var bulletCount = bullets.Length;
        var vertexCount = bulletCount * 4;

        mesh.Clear();

        // Vertex buffer
        mesh.SetVertexBufferParams
          (vertexCount,
           new VertexAttributeDescriptor(VertexAttribute.Position,
                                         VertexAttributeFormat.Float32, 2),
           new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                                         VertexAttributeFormat.Float32, 2));

        using (var varray = CreateVertexArray(bullets))
            mesh.SetVertexBufferData(varray, 0, 0, bulletCount);

        // Index buffer
        mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt32);

        using (var iarray = CreateIndexArray(vertexCount))
            mesh.SetIndexBufferData(iarray, 0, 0, vertexCount);

        // Mesh construction
        var meshDesc = new SubMeshDescriptor(0, vertexCount, MeshTopology.Quads);
        mesh.SetSubMesh(0, meshDesc, MeshUpdateFlags.DontRecalculateBounds);
    }

    static NativeArray<Quad> CreateVertexArray(NativeSlice<Bullet> bullets)
    {
        var array = new NativeArray<Quad>
          (bullets.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        new VertexConstructJob(bullets, array).Schedule(bullets.Length, 64).Complete();
        return array;
    }

    static NativeArray<uint> CreateIndexArray(int vcount)
    {
        var array = new NativeArray<uint>
          (vcount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        new IndexConstructJob(array).Schedule(vcount, 64).Complete();
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

}

} // namespace Danmaku
