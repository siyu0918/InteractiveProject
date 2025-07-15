using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // For TryParse

public class HandTracking : MonoBehaviour
{
    public UDPReceive udpReceive;
    public GameObject[] handPoints;

    public int currentGestureId = -1; // �˱����������ж��Ƿ����ֲ�����

    public Attractor mainAttractor; // ���� Attractor �ű�
    private bool isClickEffectActive = false; // ��ֹ���Ч���ظ�����
    private int lastActiveClusteringGestureId = -1; // ��������¼��һ������Ⱥ�۵�����ID (1��2)

    void Start()
    {
        // ... (���� Start ������ȷ�� handPoints �� udpReceive �Լ� mainAttractor ����ֵ�ļ��) ...
        if (handPoints == null || handPoints.Length != 21)
        {
            Debug.LogError("handPoints ���������� 21 �� GameObject��");
            enabled = false;
            return;
        }

        if (udpReceive == null)
        {
            Debug.LogError("UDPReceive ���δ��ֵ������ Inspector ����ק UDPReceive GameObject �����ֶΡ�");
            enabled = false;
            return;
        }

        if (mainAttractor == null)
        {
            Debug.LogError("Attractor ���δ��ֵ������ Inspector ����ק Attractor GameObject �����ֶΡ�");
            enabled = false;
            return;
        }

        if (Spawner.S == null) { Debug.LogError("Spawner.S not found! Make sure a Spawner GameObject exists in the scene."); enabled = false; return; }
    }

    void Update()
    {
        string receivedData = udpReceive.data;

        // ���û�н��յ���Ч���ݣ������ֲ�δ����⵽
        if (string.IsNullOrEmpty(receivedData) || !receivedData.Contains(";"))
        {
            currentGestureId = -1; // ��Ϊ���ֲ�����
            // ��û���ֲ�ʱ���� Attractor �Ƶ�Ĭ��λ�ã����磬�������ģ�
            if (mainAttractor != null)
            {
                mainAttractor.SetAttractorPosition(Vector3.zero);
            }
            // ��ѡ������ handPoints ��λ�ã�ʹ����ʧ
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
            Debug.LogWarning("���յ���Ч���ݸ�ʽ: " + receivedData + ". Ԥ�� 'ID;coords'.");
            currentGestureId = -1; // ��ʽ����Ҳ��Ϊ��Ч
            if (mainAttractor != null) mainAttractor.SetAttractorPosition(Vector3.zero); // �ƻ�Ĭ��λ��
            HandleGestureEffects(); // Still call to reset Spawner params
            return;
        }

        // ���Խ�������ID (��Ȼ���ﲻ�ٸ���ID�л���Ϊ�������������ж��Ƿ�����)
        if (int.TryParse(parts[0], out int gestureId))
        {
            currentGestureId = gestureId;
        }
        else
        {
            Debug.LogWarning("�޷���������ID: " + parts[0]);
            currentGestureId = -1; // ����ʧ��
            if (mainAttractor != null) mainAttractor.SetAttractorPosition(Vector3.zero); // �ƻ�Ĭ��λ��
            return;
        }

        string coordData = parts[1];
        string[] coordStrings = coordData.Split(',');

        if (coordStrings.Length != 63)
        {
            Debug.LogWarning("���յ�����������������ȷ: " + coordStrings.Length + " Ԥ�� 63. Data: " + receivedData);
            currentGestureId = -1; // �������ݲ�ȫ��Ҳ��Ϊ��Ч
            if (mainAttractor != null) mainAttractor.SetAttractorPosition(Vector3.zero); // �ƻ�Ĭ��λ��
            HandleGestureEffects();
            return;
        }

        // �����ֲ���ľֲ�λ��
        for (int i = 0; i < 21; i++)
        {
            if (float.TryParse(coordStrings[i * 3], out float x) &&
                float.TryParse(coordStrings[i * 3 + 1], out float y) &&
                float.TryParse(coordStrings[i * 3 + 2], out float z))
            {
                // ȷ���������ӳ���� Unity �������Ǻ��ʵ�
                // ��Щֵ�ǹؼ����������ĳ�����������е���
                float unityX = x / 90f - 10f;
                float unityY = y / 90f - 4f;
                float unityZ = z * 100f; // ���ͨ����Ҫ�Ŵ�

                handPoints[i].transform.localPosition = new Vector3(unityX, unityY, unityZ);
            }
            else
            {
                Debug.LogWarning($"�޷������� {i} ������: {coordStrings[i * 3]}, {coordStrings[i * 3 + 1]}, {coordStrings[i * 3 + 2]}");
                currentGestureId = -1; // �������ʧ�ܣ���Ϊ��Ч�ֲ�����
                if (mainAttractor != null) mainAttractor.SetAttractorPosition(Vector3.zero); // �ƻ�Ĭ��λ��
                HandleGestureEffects();
                return; // ��ǰ���أ�����ʹ����Ч����
            }
        }

        // ֻҪ����Ч���ֲ����ݣ�currentGestureId ��Ϊ -1�������� Attractor ����ʳָָ��
        HandleAttractorControl();
        HandleGestureEffects(); // **��Ҫ����������ã�����������Ч**
    }

    private void HandleAttractorControl()
    {
        if (mainAttractor == null) return;

        // ��� currentGestureId ��Ϊ -1 (��ʾ���ֲ�����)��Attractor �͸���ʳָָ��
        if (currentGestureId != -1)
        {
            // �� Attractor ����ʳָָ�� (Point 8) ��λ��
            // ȷ�� handPoints[8] �������Ѹ���λ��
            if (handPoints.Length > 8 && handPoints[8] != null)
            {
                mainAttractor.SetAttractorPosition(handPoints[8].transform.position);
            }
        }
        else // ��� currentGestureId �� -1 (��ʾû���ֲ�����)
        {
            // �� Attractor �Ƶ�Ĭ��λ�ã����磬�������ģ�
            mainAttractor.SetAttractorPosition(Vector3.zero);
        }
    }

    private void HandleGestureEffects()
    {
        if (Spawner.S == null) return; // ȷ�� Spawner �����Ѽ���

        // ���� Boids ���ض������µĲ���
        float activeAttractPull = 4f;   // �������������� Boids ������
        float activeNeighborDist = 0.5f; // ��С�ھӷ�Χ��ʹȺ�۸�����
        float activeCollDist = 0.05f;      // ��С��ײ���룬ʹ��ײ��������������θ�����

        // ���� Boids ��û���ض�����ʱ��Ĭ�ϲ��� (�� Spawner.cs �л�ȡ)
        // ע�⣺����ʹ�� Spawner.S.velocity ��ΪĬ��ֵ����Ϊ Spawner �� velocity �������� Boids �Ļ����ٶ�
        float defaultVelocity = 5f; // ��ʹ�� Spawner.S.velocity �ĳ�ʼĬ��ֵ
        float defaultAttractPull = 1f;
        float defaultNeighborDist = 2f;
        float defaultCollDist = 1f;
        float defaultAttractPush = 1f; // Ĭ���Ƴ���Ҳ�� 1f

        switch (currentGestureId)
        {
            case 1: // ˳ʱ������
            case 2: // ��ʱ������
                    // ������ ID Ϊ 1 �� 2 ʱ������Ⱥ��Ч��
                Spawner.S.attractPull = activeAttractPull;
                Spawner.S.neighborDist = activeNeighborDist;
                Spawner.S.collDist = activeCollDist;
                Spawner.S.attractPush = defaultAttractPush; // ȷ���Ƴ����ָ�Ĭ�ϣ���������תȺ��

                // �ָ� Boids �ٶȵ�Ĭ�ϣ���Ϊ�ⲻ�ǵ������
                Spawner.S.velocity = defaultVelocity;

                // ��¼��ǰ�����Ⱥ������ID
                lastActiveClusteringGestureId = currentGestureId;
                // ȷ�����Ч����������ת���Ƽ���ʱ����
                if (isClickEffectActive)
                {
                    StopCoroutine("ClickEffectCoroutine");
                    isClickEffectActive = false;
                    // ��Э�̱��ж�ʱ����Ҫ�ֶ��ָ����������ⱻЭ��δ��ɵ��޸Ŀ�ס
                    Spawner.S.attractPull = defaultAttractPull;
                    Spawner.S.neighborDist = defaultNeighborDist;
                    Spawner.S.collDist = defaultCollDist;
                    Spawner.S.attractPush = defaultAttractPush;
                    Spawner.S.velocity = defaultVelocity;
                }
                break;

            case 4: // �û����� (�µģ�)
                // ����Ⱥ������ID�������ڵ��������������ɢ
                lastActiveClusteringGestureId = -1;
                // ֻ�е����Ч��δ����ʱ�Ŵ���
                if (!isClickEffectActive)
                {
                    StartCoroutine(ClickEffectCoroutine());
                }
                // ע�⣺�����˲��Ч����Boids���������֣��������ָ�Ĭ��
                break;

            default: // û��ʶ�� ID 1 �� 2 ������ʱ (����������ʱ currentGestureId = -1)
                     // �ָ� Boids ����ΪĬ��ֵ��ʹ��ɢ����ָ�������Ϊ
                Spawner.S.attractPull = defaultAttractPull;
                Spawner.S.neighborDist = defaultNeighborDist;
                Spawner.S.collDist = defaultCollDist;
                Spawner.S.attractPush = defaultAttractPush;
                Spawner.S.velocity = defaultVelocity; // �ָ� Boids �ٶȵ�Ĭ��
                                                      // Debug.Log($"���ƽ�����Boids �ָ�������"); // ������

                // --- �����߼�������մ�Ⱥ������(ID 1��2)�л���Ĭ��״̬ ---
                if (lastActiveClusteringGestureId != -1)
                {
                    // ����һ����ʱ���Ƴ�Э�̣�����Boids��ɢ
                    // ���Ը��� ClickEffectCoroutine ���߼������ߴ���һ���µ�������Э��
                    StartCoroutine(TemporaryDisperseCoroutine(
                        defaultAttractPull, defaultAttractPush, defaultNeighborDist, defaultCollDist, defaultVelocity
                    ));
                    lastActiveClusteringGestureId = -1; // ���ã�ֻ����һ��
                }

                // ȷ�����Ч��ֹͣ����������ʧʱ�ָ�����
                if (isClickEffectActive)
                {
                    StopCoroutine("ClickEffectCoroutine");
                    isClickEffectActive = false;
                }
                break;
        }
    }

    // Э�̣����������Ƶġ��Ⱦۼ��󵯿��ٻָ���Ч��
    IEnumerator ClickEffectCoroutine()
    {
        isClickEffectActive = true; // ���Ч�����ڼ���

        // --- �׶� 1: ˲��ۼ� ---
        // �洢��ǰ Spawner �������Ա�֮��ָ�
        float originalAttractPull = Spawner.S.attractPull;
        float originalAttractPush = Spawner.S.attractPush;
        float originalNeighborDist = Spawner.S.neighborDist;
        float originalCollDist = Spawner.S.collDist;
        float originalVelocity = Spawner.S.velocity; // �������洢ԭʼ�ٶ�

        // ���ü��ߵ�����������С���ھӷ�Χ����ײ���룬ǿ�� Boids ˲��ۼ�
        Spawner.S.attractPull = Spawner.S.clickGatherPullStrength;
        Spawner.S.attractPush = originalAttractPush; // �ۼ�ʱ�������Ƴ�
        Spawner.S.neighborDist = Spawner.S.collDist * 0.2f; // �������Ը�С�������Ǽ���һ��
        Spawner.S.collDist = 0.1f; // ��С����ײ���룬���������
        // Debug.Log("Click Effect: Phase 1 - Gathering"); // ������
        Spawner.S.velocity = Spawner.S.clickEffectVelocity; // ���������� Boids

        yield return new WaitForSeconds(Spawner.S.clickGatherDuration); // �ȴ��ۼ����

        // --- �׶� 2: �͵ص��� ---
        // �ָ���������ͬʱʩ��ǿ����Ƴ���
        Spawner.S.attractPull = originalAttractPull; // �ָ���������������Ƴ�����ͻ
        Spawner.S.attractPush = Spawner.S.clickRepulsionStrength; // Ӧ��ǿ����Ƴ���
        Spawner.S.neighborDist = originalNeighborDist; // �ָ��ھӷ�Χ
        Spawner.S.collDist = originalCollDist;         // �ָ���ײ����
        // Boids �ٶ��ڵ����׶α��ֲ��䣬��Ϊ��Ӧ�û��ڿ��ٷ�����                               // Debug.Log("Click Effect: Phase 2 - Repelling"); // ������

        yield return new WaitForSeconds(Spawner.S.clickRepulsionDuration); // �ȴ��������

        // --- �׶� 3: �ָ����� ---
        // �ָ����� Spawner ���������ǰ��״̬
        Spawner.S.attractPull = originalAttractPull;
        Spawner.S.attractPush = originalAttractPush;
        Spawner.S.neighborDist = originalNeighborDist;
        Spawner.S.collDist = originalCollDist;
        Spawner.S.velocity = originalVelocity; // �������ָ� Boids �ٶ�
        // Debug.Log("Click Effect: Phase 3 - Restoring to Normal"); // ������

        isClickEffectActive = false; // ���Ч������
    }

    // ��Э�̣��ڴ�Ⱥ�����ƻָ�ʱ����ʱʩ���Ƴ���
    IEnumerator TemporaryDisperseCoroutine(
        float restoreAttractPull, float restoreAttractPush,
        float restoreNeighborDist, float restoreCollDist,
        float restoreVelocity)
    {
        // ʩ����ʱ�Ƴ���
        Spawner.S.attractPush = Spawner.S.clickRepulsionStrength * 0.5f; // �����õ��������һ�룬���߸�����Ҫ����
                                                                         // Spawner.S.velocity = Spawner.S.clickEffectVelocity; // ���Զ��ݼ��ٰ�����ɢ�����߱���Ĭ���ٶ�

        yield return new WaitForSeconds(0.2f); // �Ƴ����ʱ�䣬���Ը���Ч������

        // �ָ����в�����������Ĭ��ֵ
        Spawner.S.attractPull = restoreAttractPull;
        Spawner.S.attractPush = restoreAttractPush;
        Spawner.S.neighborDist = restoreNeighborDist;
        Spawner.S.collDist = restoreCollDist;
        Spawner.S.velocity = restoreVelocity;

        // Debug.Log("Boids: Temporary disperse finished, restored to default."); // ������
    }
}