using OKZKX.UnityTool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Move,
    Attack,
    GetAttack
}

public class ChomperMove : AutoSetBehaviour
{
    //Setting
    [SerializeField] EnemyState state;
    [SerializeField] float stepDistance = 8;
    [SerializeField] bool patroling = false;
    [SerializeField] float restTime = 1.5f;

    //Compnents
    [AutoSet] Animator animator;
    [AutoSet] EnemyAttackController EnemyAttack;
    [AutoSet] NavMeshAgent navMeshAgent;
    [AutoSet] EnemyData enemyData;
    [AutoSet] new Rigidbody rigidbody;

    //States
    [SerializeField] bool IsAttack = false;
    [SerializeField] bool IsGetAttack = false;

    //Privates
    Transform seekedTarget;
    Vector3 target;
    Timer noPatrolTimer;
    AttackSender AttackSender;
    AttackMessage AttackMessage;
    bool ArrivedTarget => navMeshAgent.remainingDistance < 1f;
    bool ArraiedSeekingTarget => Vector3.Distance(transform.position, seekedTarget.position) < 2;
    bool IsSeeking => seekedTarget != null;

    void Start()
    {
        navMeshAgent.speed = enemyData.Speed;
        navMeshAgent.angularSpeed = enemyData.AngularSpeed;
        noPatrolTimer = new Timer();

        EnemyAttack.OnAttackReceiveEvent += OnAttackReceive;
        StateEventTool.ForEach(animator, (stateEvent, name) =>
        {
            switch (name)
            {
                case "Attack":
                    stateEvent.OnStateExitEvent += () => IsAttack = false;
                    break;
                case "GetAttack":
                    stateEvent.OnStateExitEvent += () => IsGetAttack = false;
                    break;
                default:
                    break;
            }
        });

    }
    void Update()
    {

        SyncAnimator();
        SyncState();
        UpdateState();
    }

    #region Events Receiver

    private void OnAttackReceive(object attackMessage, AttackSender attackSender)
    {
        AttackMessage = (AttackMessage)attackMessage;
        AttackSender = attackSender;

        seekedTarget = attackSender.transform;
        IsGetAttack = true;
    }

    void EnterGroundState()
    {
        state = EnemyState.Move;
        animator.ResetTrigger("GetAttack");
        animator.ResetTrigger("Attack");
    }

    #endregion

    #region Update

    private void SyncAnimator()
    {
        SetMoveAnim();
    }

    private void SyncState()
    {
        switch (state)
        {
            case EnemyState.Move:
                if (IsAttack)
                {
                    state = EnemyState.Attack;
                    animator.SetTrigger("Attack");
                }

                TryEnterGetAttackState();

                break;
            case EnemyState.Attack:
                if (!IsAttack)
                {
                    state = EnemyState.Move;
                }

                TryEnterGetAttackState();

                break;
            case EnemyState.GetAttack:
                if (!IsGetAttack)
                {
                    state = EnemyState.Move;
                    rigidbody.velocity = Vector3.zero;
                }
                break;
            default:
                break;
        }

    }

    void UpdateState()
    {
        switch (state)
        {
            case EnemyState.Move:
                if (IsSeeking)
                {
                    //追寻目标
                    target = seekedTarget.position;

                    if (ArraiedSeekingTarget)
                    {
                        //追到目标
                        DontMove();

                        Quaternion preQuaternion = transform.rotation;
                        transform.LookAt(seekedTarget);
                        Quaternion targetQuaternion = transform.rotation;
                        float angle = Quaternion.Angle(preQuaternion, targetQuaternion);
                        if (angle > 45)
                        {
                            transform.rotation = Quaternion.RotateTowards(preQuaternion, targetQuaternion, 30 / Time.deltaTime);
                            Vector3 rawEular = transform.rotation.eulerAngles;
                            rawEular = Vector3.Scale(Vector3.up, rawEular);
                            transform.rotation = Quaternion.Euler(rawEular);

                        }
                        else
                        {
                            transform.rotation = preQuaternion;
                            IsAttack = true;
                        }
                    }
                    else
                    {
                        //没追到目标
                        Move();
                    }
                }
                else
                {
                    //休息检测
                    if (patroling)
                    {
                        //巡逻
                        Move();

                        //巡逻到达目的,开始休息
                        if (ArrivedTarget)
                        {
                            patroling = false;
                            noPatrolTimer.Reset();
                        }
                    }
                    else
                    {
                        //休息
                        DontMove();

                        //休息结束,重新设定随机移动目标
                        if (noPatrolTimer > restTime)
                        {
                            Vector3 offset = Vector3.right * UnityEngine.Random.Range(-1f, 1f)
                            + Vector3.forward * UnityEngine.Random.Range(-1f, 1f);
                            offset = Vector3.Normalize(offset) * stepDistance;
                            target = transform.position + offset;

                            patroling = true;
                        }
                    }
                }
                break;
            case EnemyState.Attack:
                DontMove();
                break;
            case EnemyState.GetAttack:
                DontMove();
                break;
            default:
                break;
        }
    }

    #endregion

    #region Check Enter States

    private void TryEnterGetAttackState()
    {
        if (IsGetAttack)
        {
            state = EnemyState.GetAttack;
            animator.SetTrigger("GetAttack");
            AddForce();
        }
    }

    #endregion

    void Move()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(target);
    }

    private void DontMove()
    {
        navMeshAgent.isStopped = true;
    }

    private void SetMoveAnim()
    {
        Vector3 velocity_local = transform.InverseTransformVector(navMeshAgent.velocity);

        animator.SetFloat("SpeedY", velocity_local.z / enemyData.Speed);
        animator.SetFloat("SpeedX", velocity_local.x / enemyData.Speed);
    }

    void AddForce()
    {
        Vector3 forceDirect = Vector3.Normalize(transform.position - AttackSender.transform.position);
        Vector3 force = forceDirect * AttackMessage.Damege * 500;
        force.y = 100;
        rigidbody.AddForce(force);
    }

    private void OnAnimatorMove()
    {
        if (IsAttack)
        {
            transform.position += animator.deltaPosition;
        }
    }

}
