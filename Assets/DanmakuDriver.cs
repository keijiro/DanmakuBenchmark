using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;

namespace Danmaku {

class DanmakuDriver : MonoBehaviour
{
    #region Editable sttributes

    enum ApiType { OldApi, NewApi }

    [SerializeField] ApiType _apiType = ApiType.OldApi;
    [SerializeField] Material _material = null;
    [SerializeField] float _bulletSize = 0.02f;
    [SerializeField] UnityEngine.UI.Text _uiText = null;

    #endregion

    #region Private memebers

    const int MaxBulletCount = 0x200000;

    NativeArray<Bullet> _bullets;
    NativeArray<BulletGroupInfo> _info;
    Mesh _mesh;

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
        var toSpawn = Time.deltaTime < 1.0f / 50 ? 400 : 20;

        // Bullet update job chain
        var handle = new BulletUpdateJob(_bullets, dt).Schedule(ActiveBulletCount, 64);
        handle = new BulletSweepJob(_bullets, _info, aspect).Schedule(handle);
        handle = new BulletSpawnJob(_bullets, _info, toSpawn).Schedule(handle);
        handle.Complete();

        // Mesh construction
        if (_apiType == ApiType.OldApi)
            MeshBuilderOld.Build(ActiveBulletSlice, _bulletSize, _mesh);
        else
            MeshBuilderNew.Build(ActiveBulletSlice, _bulletSize, _mesh);

        // Draw call
        Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);

        // UI update
        _uiText.text = $"{ActiveBulletCount:n0} bullets";
    }

    #endregion
}

} // namespace Danmaku
