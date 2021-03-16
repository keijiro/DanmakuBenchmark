using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Danmaku {

public readonly struct Bullet
{
    public float2 Position { get; }
    public float2 Velocity { get; }

    public Bullet(float2 position, float2 velocity)
    {
        Position = position;
        Velocity = velocity;
    }

    public static Bullet Spawn(int id)
    {
        var hash = new Klak.Math.XXHash((uint)id);
        var angle = hash.Float(math.PI * 2, 0u);
        var speed = hash.Float(0.05f, 0.2f, 1u);
        return new Bullet(math.float2(0, 0),
                          math.float2(math.cos(angle), math.sin(angle)) * speed);
    }

    public Bullet NextFrame(float delta)
      => new Bullet(Position + Velocity * delta, Velocity);

    public bool IsOnScreen
      => math.all(Position > -1) && math.all(Position < 1);
}

public readonly struct BulletGroupInfo
{
    public int ActiveCount { get; }
    public int SpawnCount { get; }

    public BulletGroupInfo(int activeCount, int spawnCount)
      => (ActiveCount, SpawnCount) = (activeCount, spawnCount);

    public static BulletGroupInfo InitialState
      => new BulletGroupInfo(0, 0);

    public static BulletGroupInfo
      ChangeActiveCount(in BulletGroupInfo orig, int count)
      => new BulletGroupInfo(count, orig.SpawnCount);

    public static BulletGroupInfo
      AddActiveAndSpawnCount(in BulletGroupInfo orig, int add)
      => new BulletGroupInfo(orig.ActiveCount + add, orig.SpawnCount + add);
}

} // namespace Danmaku
