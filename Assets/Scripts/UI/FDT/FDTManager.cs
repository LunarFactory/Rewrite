using UnityEngine;
using System.Collections.Generic;

public class FDTManager : MonoBehaviour
{
    public static FDTManager Instance { get; private set; }

    [Header("Settings")]
    public FDTObject fdtPrefab; // TextMeshPro가 붙은 FDTObject 프리팹
    public float defaultDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ── 공개 API ──────────────────────────────────────────────────

    /// <summary>
    /// 지정된 위치에서 지정된 피해량을 표시하는 FDT를 생성합니다.
    /// </summary>
    public void SpawnText(Vector3 position, int damage, Color color)
    {
        if (fdtPrefab == null)
        {
            Debug.LogError("FDTManager: fdtPrefab이 할당되지 않았습니다!");
            return;
        }

        // 1. FDT 객체 생성 (실제로는 Object Pool을 쓰는 것이 좋습니다.)
        FDTObject newFdt = Instantiate(fdtPrefab, position, Quaternion.identity, transform);

        // 2. 색상 결정 및 초기화
        Color damageColor = color;
        newFdt.Initialize(damage, defaultDuration, damageColor);
    }
    public void SpawnText(Vector3 position, string text, Color color)
    {
        if (fdtPrefab == null)
        {
            Debug.LogError("FDTManager: fdtPrefab이 할당되지 않았습니다!");
            return;
        }

        // 1. FDT 객체 생성 (실제로는 Object Pool을 쓰는 것이 좋습니다.)
        FDTObject newFdt = Instantiate(fdtPrefab, position, Quaternion.identity, transform);

        // 2. 색상 결정 및 초기화
        Color damageColor = color;
        newFdt.Initialize(text, defaultDuration, damageColor);
    }
}