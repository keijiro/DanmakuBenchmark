using UnityEngine;

namespace Danmaku {

public class AppConfig : MonoBehaviour
{
    void Start()
      => Application.targetFrameRate = 60;
}

} // namespace Danmaku
