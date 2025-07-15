//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Attractor : MonoBehaviour
//{
//    static public Vector3 POS = Vector3.zero;

//    [Header("Set in Inspector")]
//    public float radius = 10;
//    public float xPhase = 0.5f;
//    public float yPhase = 0.4f;
//    public float zPhase = 0.1f;

//    // 新增：控制Attractor是否手动控制的开关
//    public bool isManualControl = false;
//    // 每次物理更新都会调用FixedUpdate (例如每秒50次)

//    void FixedUpdate()
//    {
//        if (!isManualControl) // 只有当不是手动控制时才执行自动运动
//        {
//            Vector3 tPos = Vector3.zero;
//            Vector3 scale = transform.localScale;
//            tPos.x = Mathf.Sin(xPhase * Time.time) * radius * scale.x;
//            tPos.y = Mathf.Sin(yPhase * Time.time) * radius * scale.y;
//            tPos.z = Mathf.Sin(zPhase * Time.time) * radius * scale.z;
//            transform.position = tPos;
//            POS = tPos;
//        }
//        else // 如果是手动控制，Attractor.POS 应该和 transform.position 保持一致
//        {
//            POS = transform.position;
//        }
//    }

//    // 新增：公共方法来设置手动控制状态
//    public void SetManualControl(bool enable)
//    {
//        isManualControl = enable;
//    }

//    // 新增：公共方法来直接设置Attractor的位置
//    public void SetAttractorPosition(Vector3 newPos)
//    {
//        transform.position = newPos;
//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    static public Vector3 POS = Vector3.zero;

    // Radius, xPhase, yPhase, zPhase 等字段现在可以移除，因为不再有自动运动

    void FixedUpdate()
    {
        // Attractor 的位置将完全由外部脚本（HandTracking.cs）设置
        // 这里只需确保静态变量 POS 始终与 GameObject 的实际位置同步
        POS = transform.position;
    }

    // 公共方法来直接设置Attractor的位置
    public void SetAttractorPosition(Vector3 newPos)
    {
        transform.position = newPos;
    }

    // SetManualControl 方法也已移除
}