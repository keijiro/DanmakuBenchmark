using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace Danmaku {

[BurstCompile]
struct BulletUpdateJob : IJobParallelFor
{
    NativeArray<Bullet> _bullets;
    float _delta;

    public BulletUpdateJob(NativeArray<Bullet> bullets, float delta)
    {
        _bullets = bullets;
        _delta = delta;
    }

    public void Execute(int i)
      => _bullets[i] = _bullets[i].NextFrame(_delta);
}

[BurstCompile]
struct BulletSweepJob : IJob
{
    NativeArray<Bullet> _bullets;
    NativeArray<BulletGroupInfo> _info;

    public BulletSweepJob(NativeArray<Bullet> bullets,
                          NativeArray<BulletGroupInfo> info)
    {
        _bullets = bullets;
        _info = info;
    }

    public void Execute()
    {
        var count = _info[0].ActiveCount;
        var written = 0;
        for (var i = 0; i < count; i++)
        {
            if (_bullets[i].IsOnScreen)
            {
                if (i != written) _bullets[written] = _bullets[i];
                written++;
            }
        }
        _info[0] = BulletGroupInfo.ChangeActiveCount(_info[0], written);
    }
}

[BurstCompile]
struct BulletSpawnJob : IJob
{
    NativeArray<Bullet> _bullets;
    NativeArray<BulletGroupInfo> _info;
    int _spawnCount;

    public BulletSpawnJob(NativeArray<Bullet> bullets,
                          NativeArray<BulletGroupInfo> info,
                          int spawnCount)
    {
        _bullets = bullets;
        _info = info;
        _spawnCount = spawnCount;
    }

    public void Execute()
    {
        var count = _info[0].ActiveCount;
        var id = _info[0].SpawnCount;
        var toSpawn = math.min(_bullets.Length - count, _spawnCount);
        for (var i = 0; i < toSpawn; i++)
            _bullets[count + i] = Bullet.Spawn(id + i);
        _info[0] = BulletGroupInfo.AddActiveAndSpawnCount(_info[0], toSpawn);
    }
}

} // namespace Danmaku
