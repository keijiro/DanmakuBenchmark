using UnityEngine;

namespace Danmaku {

sealed class Boss : MonoBehaviour
{
    void Update()
    {
        var t = Time.time;
        var x = Mathf.Sin(t * 0.745f) * 0.25f;
        var y = Mathf.Sin(t * 0.614f) * 0.25f;
        transform.position = new Vector3(x, y, -1);
    }
}

} // namespace Danmaku
