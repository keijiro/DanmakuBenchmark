using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace Danmaku {

public class DanmakuDriver : MonoBehaviour
{
    [SerializeField] Material _material = null;
    [SerializeField] UnityEngine.UI.Text _uiText = null;

    const int MaxBulletCount = 0x100000;

    NativeArray<Bullet> _bullets;
    NativeArray<BulletGroupInfo> _info;

    DanmakuRenderer _renderer;

    NativeSlice<Bullet> ActiveBulletSlice
      => new NativeSlice<Bullet>(_bullets, 0, _info[0].ActiveCount);

    void Start()
    {
        _bullets = new NativeArray<Bullet>(MaxBulletCount, Allocator.Persistent);

        _info = new NativeArray<BulletGroupInfo>(1, Allocator.Persistent);
        _info[0] = BulletGroupInfo.InitialState;

        _renderer = new DanmakuRenderer(MaxBulletCount);
    }

    void OnDisable()
    {
        _bullets.Dispose();
        _info.Dispose();
        _renderer.Dispose();
    }

    void Update()
    {
        var dt = 1.0f / 60;
        var actives = _info[0].ActiveCount;

        var toSpawn = Time.deltaTime < 1.0f / 50 ? 200 : 10;

        var handle = new BulletUpdateJob(_bullets, dt).Schedule(actives, 64);
        handle = new BulletSweepJob(_bullets, _info).Schedule(handle);
        handle = new BulletSpawnJob(_bullets, _info, toSpawn).Schedule(handle);
        handle.Complete();

        _renderer.ConstructMesh(ActiveBulletSlice);
        _renderer.DrawMesh(_material);

        _uiText.text = $"{actives:n0} bullets";
    }
}

} // namespace Danmaku
