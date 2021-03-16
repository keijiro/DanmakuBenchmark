using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace Danmaku {

[BurstCompile]
struct BulletUpdateJob : IJobParallelFor
{
    NativeArray<Bullet> _bullets;
    float _deltaTime;

    public BulletUpdateJob(NativeArray<Bullet> bullets, float deltaTime)
      => (_bullets, _deltaTime) = (bullets, deltaTime);

    public void Execute(int i)
      => _bullets[i] = _bullets[i].NextFrame(_deltaTime);
}

[BurstCompile]
struct BulletSweepJob : IJob
{
    NativeArray<Bullet> _bullets;
    NativeArray<BulletGroupInfo> _info;
    float _screenAspect;

    public BulletSweepJob(NativeArray<Bullet> bullets,
                          NativeArray<BulletGroupInfo> info,
                          float screenAspect)
      => (_bullets, _info, _screenAspect) = (bullets, info, screenAspect);

    public void Execute()
    {
        var bound1 = math.float2(-_screenAspect, -1);
        var bound2 = math.float2(+_screenAspect, +1);

        var actives = _info[0].ActiveCount;
        var written = 0;

        for (var i = 0; i < actives; i++)
        {
            var p = _bullets[i].Position;

            if (math.any(p < bound1)) continue;
            if (math.any(p > bound2)) continue;

            if (i != written) _bullets[written] = _bullets[i];
            written++;
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
      => (_bullets, _info, _spawnCount) = (bullets, info, spawnCount);

    public void Execute()
    {
        var seed = _info[0].SpawnCount + 1;

        var actives = _info[0].ActiveCount;
        var spawns = math.min(_bullets.Length - actives, _spawnCount);

        for (var i = 0; i < spawns; i++)
            _bullets[actives + i] = Bullet.Spawn(seed + i);

        _info[0] = BulletGroupInfo.AddActiveAndSpawnCount(_info[0], spawns);
    }
}

} // namespace Danmaku
