using UnityEngine;

namespace Danmaku {

sealed class AppConfig : MonoBehaviour
{
    void Start()
      => Application.targetFrameRate = 60;
}

} // namespace Danmaku
