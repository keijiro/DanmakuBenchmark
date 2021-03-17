using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Danmaku {

//
// A top-class manager class for driving a group of bullets
//
sealed class DanmakuDriver : MonoBehaviour
{
    #region Editable attributes

    enum MethodType { Simple, Advanced }

    [SerializeField] MethodType _methodType = MethodType.Advanced;
    [SerializeField] Material _material = null;
    [SerializeField] float _bulletSize = 0.02f;
    [SerializeField] UnityEngine.UI.Text _uiText = null;

    #endregion

    #region Private memebers

    const int MaxBulletCount = 0x300000;

    // Fixed length array for managing bullets
    // The actual length of the array is stored in _info.
    NativeArray<Bullet> _bullets;

    // Single-element array of BulletGroupInfo
    // We must use an array to make it modifiable from C# jobs.
    NativeArray<BulletGroupInfo> _info;

    // Mesh object for rendering bullets
    Mesh _mesh;

    #endregion

    #region Private utility properties

    int ActiveBulletCount
      => _info[0].ActiveCount;

    NativeSlice<Bullet> ActiveBulletSlice
      => new NativeSlice<Bullet>(_bullets, 0, _info[0].ActiveCount);

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _bullets = new NativeArray<Bullet>(MaxBulletCount, Allocator.Persistent);

        _info = new NativeArray<BulletGroupInfo>(1, Allocator.Persistent);
        _info[0] = BulletGroupInfo.InitialState;

        _mesh = new Mesh();
        _mesh.indexFormat = IndexFormat.UInt32;
        _mesh.MarkDynamic();
    }

    void OnDisable()
    {
        _bullets.Dispose();
        _info.Dispose();
    }

    void OnDestroy()
      => Destroy(_mesh);

    void Update()
    {
        var dt = 1.0f / 60;
        var aspect = (float)Screen.width / Screen.height;
        var pos = math.float3(transform.position).xy;
        var spawn = Time.deltaTime < 1.0f / 58 ? 400 : 20;

        // Bullet update job chain
        var handle = new BulletUpdateJob(_bullets, dt).Schedule(ActiveBulletCount, 64);
        handle = new BulletSweepJob(_bullets, _info, aspect).Schedule(handle);
        handle = new BulletSpawnJob(_bullets, _info, pos, spawn).Schedule(handle);
        handle.Complete();

        // Mesh construction
        if (_methodType == MethodType.Simple)
            MeshBuilderSimple.Build(ActiveBulletSlice, _bulletSize, _mesh);
        else
            MeshBuilderAdvanced.Build(ActiveBulletSlice, _bulletSize, _mesh);

        // Draw call
        Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);

        // UI update
        _uiText.text = $"{ActiveBulletCount:n0} bullets";
    }

    #endregion
}

} // namespace Danmaku
