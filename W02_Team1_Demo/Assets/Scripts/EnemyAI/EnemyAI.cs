using JetBrains.Annotations;
using Pathfinding;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform target;

    [Header("이동/물리")]
    public float speed = 200f; //AddForce에 곱해줄 값
    public float nextWaypointDistance = 3f; // 웨이포인트 전환 거리

    public Transform enemyGFX;

    [Header("추격 조건(히스테리시스)")]
    public float chaseStartRange = 6f; //추격 시작 거리
    public float chaseStopRange = 9f; //추격 종료 거리
    public float repathInterval = 0.5f; //추격 중 경로 갱신 주기

    [Header("리쉬(선택)")]
    public float maxChaseDistanceFromSpawn = 0f; // 0이면 제한 없음. 스폰 지점에서 이 거리 넘으면 추격 해제


    Pathfinding.Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb;

    bool isChasing = false;
    Vector2 spawnPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();


        InvokeRepeating("UpdatePath", 0f, .5f);

        // 경로 갱신 타이머 (추격 중일 때만 StartPath를 호출하도록 UpdatePath 내부에서 체크)
        InvokeRepeating(nameof(UpdatePath), 0f, repathInterval);
    }
    void Update()
    {
        if (target == null)
        {
            isChasing = false; return;
        }

        float distToPlayer = Vector2.Distance(rb.position, target.position);

        // 시작 조건
        if (!isChasing && distToPlayer <= chaseStartRange)
            isChasing = true;

        // 종료 조건
        if (isChasing && distToPlayer >= chaseStopRange)
            isChasing = false;

        // 리쉬(선택): 스폰 지점에서 너무 멀어지면 추격 해제
        if (isChasing && maxChaseDistanceFromSpawn > 0f)
        {
            float distFromSpawn = Vector2.Distance(spawnPos, rb.position);
            if (distFromSpawn > maxChaseDistanceFromSpawn)
                isChasing = false;
        }
    }

        void UpdatePath()
        {
            // 추격 중일 때만 경로 요청
            if (!isChasing || target == null) return;

            if (seeker.IsDone())
                seeker.StartPath(rb.position, target.position, OnPathComplete);
        }

        void OnPathComplete(Pathfinding.Path p_)
        {
            if (!p_.error)
            {
                path = p_;
                currentWaypoint = 0;
            }
        }

        void FixedUpdate()
        {
            // 추격이 아니라면 속도 서서히 줄이고 리턴
            if (!isChasing || path == null)
            {
                // 수평 속도만 부드럽게 감속 (멈춰 서있는 느낌)
                rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0f, 0.15f), rb.linearVelocity.y);
                return;
            }

            if (currentWaypoint >= path.vectorPath.Count)
            {
                reachedEndOfPath = true;
                return;
            }
            else
            {
                reachedEndOfPath = false;
            }
            // 다음 웨이포인트 방향으로 힘 가하기
            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            Vector2 force = direction * speed * Time.deltaTime;

            rb.AddForce(force);

            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

            if (distance < nextWaypointDistance)
            {
                currentWaypoint++;
            }

            if (force.x >= 0.01f)
            {
                enemyGFX.localScale = new Vector3(-1f, 1f, 1f);
            }
            else if (force.x <= -0.01f)
            {
                enemyGFX.localScale = new Vector3(1f, 1f, 1f);
            }

        }
    }

