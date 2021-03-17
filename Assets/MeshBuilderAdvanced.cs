using UnityEngine;
using UnityEngine.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Danmaku {

//
// Danmaku mesh builder with the advanced mesh API & C# job system
//
static class MeshBuilderAdvanced
{
    public static void Build(NativeSlice<Bullet> bullets, float size, Mesh mesh)
    {
        var bulletCount = bullets.Length;
        var vertexCount = bulletCount * 4;

        mesh.Clear();

        // Vertex/index buffer allocation
        var varray = new NativeArray<Quad>(bulletCount, Allocator.TempJob,
                                           NativeArrayOptions.UninitializedMemory);

        var iarray = new NativeArray<uint>(vertexCount, Allocator.TempJob,
                                           NativeArrayOptions.UninitializedMemory);

        // Vertex/index array construction
        var vjob = new VertexConstructionJob(bullets, size, varray);
        var ijob = new IndexConstructionJob(iarray);

        var handle = vjob.Schedule(bulletCount, 64);
        handle = ijob.Schedule(vertexCount, 64, handle);

        handle.Complete();

        // Vertex buffer
        mesh.SetVertexBufferParams
          (vertexCount,
           new VertexAttributeDescriptor(VertexAttribute.Position,
                                         VertexAttributeFormat.Float32, 2),
           new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                                         VertexAttributeFormat.Float32, 2));
        mesh.SetVertexBufferData(varray, 0, 0, bulletCount);

        // Index buffer
        mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt32);
        mesh.SetIndexBufferData(iarray, 0, 0, vertexCount);

        // Submesh definition
        var meshDesc = new SubMeshDescriptor(0, vertexCount, MeshTopology.Quads);
        mesh.SetSubMesh(0, meshDesc, MeshUpdateFlags.DontRecalculateBounds);

        // Cleanup
        varray.Dispose();
        iarray.Dispose();
    }

    // Quad vertex data structure
    readonly struct Quad
    {
        readonly float4 _v0, _v1, _v2, _v3;

        public Quad(float2 center, float size)
        {
            _v0 = math.float4(center.x - size, center.y + size, 0, 1);
            _v1 = math.float4(center.x + size, center.y + size, 1, 1);
            _v2 = math.float4(center.x + size, center.y - size, 1, 0);
            _v3 = math.float4(center.x - size, center.y - size, 0, 0);
        }
    }

    //  Vertex array construction job data
    [BurstCompile]
    struct VertexConstructionJob : IJobParallelFor
    {
        [ReadOnly] NativeSlice<Bullet> _bullets;
        [WriteOnly] NativeArray<Quad> _output;
        float _size;

        public VertexConstructionJob(NativeSlice<Bullet> bullets, float size,
                                     NativeArray<Quad> output)
          => (_bullets, _output, _size) = (bullets, output, size);

        public void Execute(int i)
          => _output[i] = new Quad(_bullets[i].Position, _size);
    }

    // Index array construction job data
    [BurstCompile]
    struct IndexConstructionJob : IJobParallelFor
    {
        [WriteOnly] NativeArray<uint> _output;

        public IndexConstructionJob(NativeArray<uint> output)
          => _output = output;

        public void Execute(int i)
          => _output[i] = (uint)i;
    }
}

} // namespace Danmaku
