using System.Collections.Generic;
using Enemy;
using UnityEngine;

public class GeminiA : EnemyAI
{
    [Header("Gemini Settings")]
    public GameObject geminiBPrefab;
    public float targetDistance = 3f; // 유지하려는 짧은 거리
    public float shootRange = 5f;
    public float fireInterval = 1.5f;

    private GeminiB _geminiB;
    private float _fireTimer;
    private bool _isPhase2Triggered = false;

    protected override void Awake()
    {
        base.Awake();
        SpawnGeminiB();
    }

    private void SpawnGeminiB()
    {
        GameObject bGo = Instantiate(
            geminiBPrefab,
            transform.position + Vector3.right,
            Quaternion.identity
        );
        _geminiB = bGo.GetComponent<GeminiB>();
        _geminiB.Init(this); // A의 참조를 넘겨줌
    }

    protected override void ExecuteBehavior()
    {
        if (playerStat == null)
            return;

        HandleMovement();
        HandleAttack();
        CheckPhase2();
    }

    private void HandleMovement()
    {
        float dist = Vector2.Distance(transform.position, playerStat.transform.position);
        Vector2 dir = (playerStat.transform.position - transform.position).normalized;

        // 짧은 거리 유지 (가까우면 멀어지고, 멀면 다가감)
        if (dist > targetDistance + 0.5f)
            rb.velocity = dir * stats.MoveSpeed.GetValue();
        else if (dist < targetDistance - 0.5f)
            rb.velocity = -dir * stats.MoveSpeed.GetValue();
        else
            rb.velocity = Vector2.zero;
    }

    private void HandleAttack()
    {
        _fireTimer -= Time.deltaTime;
        if (
            _fireTimer <= 0
            && Vector2.Distance(transform.position, playerStat.transform.position) <= shootRange
        )
        {
            Fire3Way();
            _fireTimer = _isPhase2Triggered ? fireInterval * 0.7f : fireInterval;
        }
    }

    private void Fire3Way()
    {
        Vector2 dir = (playerStat.transform.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float[] angles = { baseAngle - 15f, baseAngle, baseAngle + 15f };
        foreach (float angle in angles)
        {
            // 탄환 생성 및 Initialize 로직 (기존 Shoot 방식 활용)
        }
    }

    private void CheckPhase2()
    {
        if (!_isPhase2Triggered && stats.currentHealth <= stats.maxHealth * 0.5f)
        {
            _isPhase2Triggered = true;
            InstallLaserDevice();
        }
    }

    private void InstallLaserDevice()
    {
        // 제자리에 레이저 장치 설치 및 B에게도 명령
        _geminiB.TriggerPhase2();
        // 레이저 렌더러 활성화 로직
    }
}
