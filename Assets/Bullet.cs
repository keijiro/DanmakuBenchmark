using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Danmaku {

//
// Simple linear motion bullet structure
//
readonly struct Bullet
{
    public float2 Position { get; }
    public float2 Velocity { get; }

    public Bullet(float2 position, float2 velocity)
    {
        Position = position;
        Velocity = velocity;
    }

    public static Bullet Spawn(float2 position, int seed)
    {
        var hash = new Klak.Math.XXHash((uint)seed);
        var angle = hash.Float(math.PI * 2, 0u);
        var speed = hash.Float(0.05f, 0.2f, 1u);
        return new Bullet(position,
                          math.float2(math.cos(angle),
                                      math.sin(angle)) * speed);
    }

    public Bullet NextFrame(float delta)
      => new Bullet(Position + Velocity * delta, Velocity);
}

//
// Bullet group shared information structure
//
readonly struct BulletGroupInfo
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
