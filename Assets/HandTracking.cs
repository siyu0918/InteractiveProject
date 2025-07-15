using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // For TryParse

public class HandTracking : MonoBehaviour
{
    public UDPReceive udpReceive;
    public GameObject[] handPoints;

    public int currentGestureId = -1; // 此变量仍用于判断是否有手部数据

    public Attractor mainAttractor; // 引用 Attractor 脚本
    private bool isClickEffectActive = false; // 防止点击效果重复触发
    private int lastActiveClusteringGestureId = -1; // 新增：记录上一个激活群聚的手势ID (1或2)

    void Start()
    {
        // ... (现有 Start 方法，确保 handPoints 和 udpReceive 以及 mainAttractor 被赋值的检查) ...
        if (handPoints == null || handPoints.Length != 21)
        {
            Debug.LogError("handPoints 数组必须包含 21 个 GameObject！");
            enabled = false;
            return;
        }

        if (udpReceive == null)
        {
            Debug.LogError("UDPReceive 组件未赋值！请在 Inspector 中拖拽 UDPReceive GameObject 到此字段。");
            enabled = false;
            return;
        }

        if (mainAttractor == null)
        {
            Debug.LogError("Attractor 组件未赋值！请在 Inspector 中拖拽 Attractor GameObject 到此字段。");
            enabled = false;
            return;
        }

        if (Spawner.S == null) { Debug.LogError("Spawner.S not found! Make sure a Spawner GameObject exists in the scene."); enabled = false; return; }
    }

    void Update()
    {
        string receivedData = udpReceive.data;

        // 如果没有接收到有效数据，或者手部未被检测到
        if (string.IsNullOrEmpty(receivedData) || !receivedData.Contains(";"))
        {
            currentGestureId = -1; // 视为无手部数据
            // 当没有手部时，将 Attractor 移到默认位置（例如，场景中心）
            if (mainAttractor != null)
            {
                mainAttractor.SetAttractorPosition(Vector3.zero);
            }
            // 可选：重置 handPoints 的位置，使其消失
            foreach (GameObject point in handPoints)
            {
                point.transform.localPosition = Vector3.zero;
            }
            HandleGestureEffects(); // Still call to reset Spawner params
            return;
        }

        string[] parts = receivedData.Split(';');
        if (parts.Length != 2)
        {
            Debug.LogWarning("接收到无效数据格式: " + receivedData + ". 预期 'ID;coords'.");
            currentGestureId = -1; // 格式错误，也视为无效
            if (mainAttractor != null) mainAttractor.SetAttractorPosition(Vector3.zero); // 移回默认位置
            HandleGestureEffects(); // Still call to reset Spawner params
            return;
        }

        // 尝试解析手势ID (虽然这里不再根据ID切换行为，但可以用于判断是否有手)
        if (int.TryParse(parts[0], out int gestureId))
        {
            currentGestureId = gestureId;
        }
        else
        {
            Debug.LogWarning("无法解析手势ID: " + parts[0]);
            currentGestureId = -1; // 解析失败
            if (mainAttractor != null) mainAttractor.SetAttractorPosition(Vector3.zero); // 移回默认位置
            return;
        }

        string coordData = parts[1];
        string[] coordStrings = coordData.Split(',');

        if (coordStrings.Length != 63)
        {
            Debug.LogWarning("接收到坐标数据数量不正确: " + coordStrings.Length + " 预期 63. Data: " + receivedData);
            currentGestureId = -1; // 坐标数据不全，也视为无效
            if (mainAttractor != null) mainAttractor.SetAttractorPosition(Vector3.zero); // 移回默认位置
            HandleGestureEffects();
            return;
        }

        // 更新手部点的局部位置
        for (int i = 0; i < 21; i++)
        {
            if (float.TryParse(coordStrings[i * 3], out float x) &&
                float.TryParse(coordStrings[i * 3 + 1], out float y) &&
                float.TryParse(coordStrings[i * 3 + 2], out float z))
            {
                // 确保你的坐标映射在 Unity 场景中是合适的
                // 这些值是关键，请根据你的场景和相机进行调试
                float unityX = x / 90f - 10f;
                float unityY = y / 90f - 4f;
                float unityZ = z * 100f; // 深度通常需要放大

                handPoints[i].transform.localPosition = new Vector3(unityX, unityY, unityZ);
            }
            else
            {
                Debug.LogWarning($"无法解析点 {i} 的坐标: {coordStrings[i * 3]}, {coordStrings[i * 3 + 1]}, {coordStrings[i * 3 + 2]}");
                currentGestureId = -1; // 坐标解析失败，视为无效手部数据
                if (mainAttractor != null) mainAttractor.SetAttractorPosition(Vector3.zero); // 移回默认位置
                HandleGestureEffects();
                return; // 提前返回，避免使用无效坐标
            }
        }

        // 只要有有效的手部数据（currentGestureId 不为 -1），就让 Attractor 跟随食指指尖
        HandleAttractorControl();
        HandleGestureEffects(); // **重要：在这里调用，处理手势特效**
    }

    private void HandleAttractorControl()
    {
        if (mainAttractor == null) return;

        // 如果 currentGestureId 不为 -1 (表示有手部数据)，Attractor 就跟随食指指尖
        if (currentGestureId != -1)
        {
            // 让 Attractor 跟随食指指尖 (Point 8) 的位置
            // 确保 handPoints[8] 存在且已更新位置
            if (handPoints.Length > 8 && handPoints[8] != null)
            {
                mainAttractor.SetAttractorPosition(handPoints[8].transform.position);
            }
        }
        else // 如果 currentGestureId 是 -1 (表示没有手部数据)
        {
            // 将 Attractor 移到默认位置（例如，场景中心）
            mainAttractor.SetAttractorPosition(Vector3.zero);
        }
    }

    private void HandleGestureEffects()
    {
        if (Spawner.S == null) return; // 确保 Spawner 单例已加载

        // 定义 Boids 在特定手势下的参数
        float activeAttractPull = 4f;   // 增加吸引力，让 Boids 更紧密
        float activeNeighborDist = 0.5f; // 减小邻居范围，使群聚更紧密
        float activeCollDist = 0.05f;      // 减小碰撞距离，使碰撞避免更积极，队形更紧凑

        // 定义 Boids 在没有特定手势时的默认参数 (从 Spawner.cs 中获取)
        // 注意：这里使用 Spawner.S.velocity 作为默认值，因为 Spawner 的 velocity 变量就是 Boids 的基础速度
        float defaultVelocity = 5f; // 或使用 Spawner.S.velocity 的初始默认值
        float defaultAttractPull = 1f;
        float defaultNeighborDist = 2f;
        float defaultCollDist = 1f;
        float defaultAttractPush = 1f; // 默认推斥力也是 1f

        switch (currentGestureId)
        {
            case 1: // 顺时针手势
            case 2: // 逆时针手势
                    // 当手势 ID 为 1 或 2 时，激活群聚效果
                Spawner.S.attractPull = activeAttractPull;
                Spawner.S.neighborDist = activeNeighborDist;
                Spawner.S.collDist = activeCollDist;
                Spawner.S.attractPush = defaultAttractPush; // 确保推斥力恢复默认，不干扰旋转群聚

                // 恢复 Boids 速度到默认，因为这不是点击手势
                Spawner.S.velocity = defaultVelocity;

                // 记录当前激活的群聚手势ID
                lastActiveClusteringGestureId = currentGestureId;
                // 确保点击效果不会在旋转手势激活时运行
                if (isClickEffectActive)
                {
                    StopCoroutine("ClickEffectCoroutine");
                    isClickEffectActive = false;
                    // 当协程被中断时，需要手动恢复参数，避免被协程未完成的修改卡住
                    Spawner.S.attractPull = defaultAttractPull;
                    Spawner.S.neighborDist = defaultNeighborDist;
                    Spawner.S.collDist = defaultCollDist;
                    Spawner.S.attractPush = defaultAttractPush;
                    Spawner.S.velocity = defaultVelocity;
                }
                break;

            case 4: // 敲击手势 (新的！)
                // 重置群聚手势ID，避免在点击后立即触发推散
                lastActiveClusteringGestureId = -1;
                // 只有当点击效果未激活时才触发
                if (!isClickEffectActive)
                {
                    StartCoroutine(ClickEffectCoroutine());
                }
                // 注意：点击是瞬间效果，Boids参数不保持，会立即恢复默认
                break;

            default: // 没有识别到 ID 1 或 2 的手势时 (包括无手势时 currentGestureId = -1)
                     // 恢复 Boids 参数为默认值，使其散开或恢复正常行为
                Spawner.S.attractPull = defaultAttractPull;
                Spawner.S.neighborDist = defaultNeighborDist;
                Spawner.S.collDist = defaultCollDist;
                Spawner.S.attractPush = defaultAttractPush;
                Spawner.S.velocity = defaultVelocity; // 恢复 Boids 速度到默认
                                                      // Debug.Log($"手势结束，Boids 恢复正常。"); // 调试用

                // --- 新增逻辑：如果刚从群聚手势(ID 1或2)切换到默认状态 ---
                if (lastActiveClusteringGestureId != -1)
                {
                    // 启动一个临时的推斥协程，帮助Boids分散
                    // 可以复用 ClickEffectCoroutine 的逻辑，或者创建一个新的轻量级协程
                    StartCoroutine(TemporaryDisperseCoroutine(
                        defaultAttractPull, defaultAttractPush, defaultNeighborDist, defaultCollDist, defaultVelocity
                    ));
                    lastActiveClusteringGestureId = -1; // 重置，只触发一次
                }

                // 确保点击效果停止并在手势消失时恢复参数
                if (isClickEffectActive)
                {
                    StopCoroutine("ClickEffectCoroutine");
                    isClickEffectActive = false;
                }
                break;
        }
    }

    // 协程：处理点击手势的“先聚集后弹开再恢复”效果
    IEnumerator ClickEffectCoroutine()
    {
        isClickEffectActive = true; // 标记效果正在激活

        // --- 阶段 1: 瞬间聚集 ---
        // 存储当前 Spawner 参数，以便之后恢复
        float originalAttractPull = Spawner.S.attractPull;
        float originalAttractPush = Spawner.S.attractPush;
        float originalNeighborDist = Spawner.S.neighborDist;
        float originalCollDist = Spawner.S.collDist;
        float originalVelocity = Spawner.S.velocity; // 新增：存储原始速度

        // 设置极高的吸引力，极小的邻居范围和碰撞距离，强制 Boids 瞬间聚集
        Spawner.S.attractPull = Spawner.S.clickGatherPullStrength;
        Spawner.S.attractPush = originalAttractPush; // 聚集时不进行推斥
        Spawner.S.neighborDist = Spawner.S.collDist * 0.2f; // 甚至可以更小，让它们挤在一起
        Spawner.S.collDist = 0.1f; // 更小的碰撞距离，允许更紧密
        // Debug.Log("Click Effect: Phase 1 - Gathering"); // 调试用
        Spawner.S.velocity = Spawner.S.clickEffectVelocity; // 新增：加速 Boids

        yield return new WaitForSeconds(Spawner.S.clickGatherDuration); // 等待聚集完成

        // --- 阶段 2: 猛地弹开 ---
        // 恢复吸引力，同时施加强大的推斥力
        Spawner.S.attractPull = originalAttractPull; // 恢复吸引力，避免和推斥力冲突
        Spawner.S.attractPush = Spawner.S.clickRepulsionStrength; // 应用强大的推斥力
        Spawner.S.neighborDist = originalNeighborDist; // 恢复邻居范围
        Spawner.S.collDist = originalCollDist;         // 恢复碰撞距离
        // Boids 速度在弹开阶段保持不变，因为它应该还在快速飞行中                               // Debug.Log("Click Effect: Phase 2 - Repelling"); // 调试用

        yield return new WaitForSeconds(Spawner.S.clickRepulsionDuration); // 等待弹开完成

        // --- 阶段 3: 恢复正常 ---
        // 恢复所有 Spawner 参数到点击前的状态
        Spawner.S.attractPull = originalAttractPull;
        Spawner.S.attractPush = originalAttractPush;
        Spawner.S.neighborDist = originalNeighborDist;
        Spawner.S.collDist = originalCollDist;
        Spawner.S.velocity = originalVelocity; // 新增：恢复 Boids 速度
        // Debug.Log("Click Effect: Phase 3 - Restoring to Normal"); // 调试用

        isClickEffectActive = false; // 标记效果结束
    }

    // 新协程：在从群聚手势恢复时，临时施加推斥力
    IEnumerator TemporaryDisperseCoroutine(
        float restoreAttractPull, float restoreAttractPush,
        float restoreNeighborDist, float restoreCollDist,
        float restoreVelocity)
    {
        // 施加临时推斥力
        Spawner.S.attractPush = Spawner.S.clickRepulsionStrength * 0.5f; // 可以用点击斥力的一半，或者根据需要调整
                                                                         // Spawner.S.velocity = Spawner.S.clickEffectVelocity; // 可以短暂加速帮助分散，或者保持默认速度

        yield return new WaitForSeconds(0.2f); // 推斥持续时间，可以根据效果调整

        // 恢复所有参数到期望的默认值
        Spawner.S.attractPull = restoreAttractPull;
        Spawner.S.attractPush = restoreAttractPush;
        Spawner.S.neighborDist = restoreNeighborDist;
        Spawner.S.collDist = restoreCollDist;
        Spawner.S.velocity = restoreVelocity;

        // Debug.Log("Boids: Temporary disperse finished, restored to default."); // 调试用
    }
}