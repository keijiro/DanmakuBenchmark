using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;

namespace Danmaku {

class DanmakuDriver : MonoBehaviour
{
    enum ApiType { OldApi, NewApi }

    [SerializeField] ApiType _apiType = ApiType.OldApi;
    [SerializeField] Material _material = null;
    [SerializeField] UnityEngine.UI.Text _uiText = null;

    const int MaxBulletCount = 0x200000;

    NativeArray<Bullet> _bullets;
    NativeArray<BulletGroupInfo> _info;
    Mesh _mesh;

    NativeSlice<Bullet> ActiveBulletSlice
      => new NativeSlice<Bullet>(_bullets, 0, _info[0].ActiveCount);

    void Start()
    {
        _bullets = new NativeArray<Bullet>(MaxBulletCount, Allocator.Persistent);

        _info = new NativeArray<BulletGroupInfo>(1, Allocator.Persistent);
        _info[0] = BulletGroupInfo.InitialState;

        _mesh = new Mesh();
        _mesh.indexFormat = IndexFormat.UInt32;
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
        var actives = _info[0].ActiveCount;

        var toSpawn = Time.deltaTime < 1.0f / 50 ? 400 : 20;

        var handle = new BulletUpdateJob(_bullets, dt).Schedule(actives, 64);
        handle = new BulletSweepJob(_bullets, _info).Schedule(handle);
        handle = new BulletSpawnJob(_bullets, _info, toSpawn).Schedule(handle);
        handle.Complete();

        if (_apiType == ApiType.OldApi)
            MeshBuilderOld.Build(ActiveBulletSlice, 0.02f, _mesh);
        else
            MeshBuilderNew.Build(ActiveBulletSlice, 0.02f, _mesh);

        Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);

        _uiText.text = $"{actives:n0} bullets";
    }
}

} // namespace Danmaku
