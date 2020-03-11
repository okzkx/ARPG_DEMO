/**********************************************************************
* 角色行为控制器
* 
* 控制角色行为,
* 控制角色的逻辑状态机
* 同步动画状态机
* 
* 状态机同步策略:
* Signals 信号量方法
* 1.在每一帧做下面操作
*   1.1 更新角色相应的信号量 UpdateSignals
*   1.2 根据信号量进行状态切换 SyncState
*       有时需要传递Trigger到动画状态机
*   1.3 同步必要的信号量到动画状态机,
*       并由动画状态机进行状态切换 SyncAnimator
*   1.4 根据角色的状态进行相应的角色行为控制 UpdateState
* 2.在动画状态机的事件触发后,也会改变相应的信号量
* 
* 移动策略:
* 为了角色能够任意地形自由行走,起跳,掉落
* 使用了下面这套角色移动控制算法,
* 1.只有在有键盘移动输入时触发
* 2.角色前方向插值到摄像机前方向 transform.forward -sl> camera_f_2d
* 3.由输入获取角色自身原始2d速度 velocity_2d_local
* 4.velocity_2d_local 同步进动画状态机地面混合树状态
* 5.由 charactor_right_world, ground_normal_warld, cross后得到 charactor_forward_world,
*   三个构造出 角色前进坐标系 的三维矩阵 FCS
* 6.变换原始速度到世界速度 FCS * velocity_2d_local = velocity_world
***********************************************************************/
using OKZKX.UnityTool;
using UnityEngine;

public enum CharactorState
{
    Move,
    Air,
    Attack,
    GetAttack
}

public class CharactorController : MonoBehaviour
{
    //Components
    [AutoSet("Main Camera", SetBy.SceneObject)] FreeLookCamera FreeLookCamera;
    [AutoSet] new Rigidbody rigidbody;
    [AutoSet] Animator animator;
    [AutoSet] CharactorAttackController CharactorAttackController;

    //Setting
    public float EnterFallTime;
    public LayerMask groundLayer;

    //Signals
    public CharactorState CharactorState;
    public Vector3 groundNormal;
    public bool IsGround = true;//在地面
    public bool IsJumpUp = false;//起跳中(控制不重复起跳)
    public bool IsAttack = false;//攻击中(控制攻击时移动)
    public bool IsFall = false;//掉落中
    public bool IsAir = false;//在空中(和在地面有一小段重合时间)
    public bool IsGetAttack = false;//受击打中

    Timer airTimer;
    bool HasMovingInput => InputManager.Instance.AxisRaw != Vector2.zero;

    void Awake()
    {
        AutoSetTool.SetFields(this);
        airTimer = new Timer();
    }

    private void Start()
    {
        FreeLookCamera.Target = transform;

        StateEventTool.ForEach(animator, (stateEvent, name) =>
        {
            switch (name)
            {
                case "Jump"://控制起跳中信号
                    stateEvent.OnStateExitEvent += () => IsJumpUp = false;
                    break;
                case "Attack"://控制攻击中信号
                    stateEvent.OnStateMachineEnterEvent += () => IsAttack = true;
                    stateEvent.OnStateMachineExitEvent += () => IsAttack = false;
                    break;
                case "Ground"://清空 Trigger
                    stateEvent.OnStateEnterEvent += EnterGoundState;
                    break;
                case "GetAttack":
                    stateEvent.OnStateExitEvent += () => IsGetAttack = false;
                    break;
                default:
                    break;
            }
        });

        CharactorAttackController.OnAttackEndEvent += () => animator.SetBool("KeepAttack", false);
        CharactorAttackController.OnAttackReceiveEvent += OnGetAttack;
    }

    void Update()
    {
        UpdateSignals();

        SyncAnimator();

        SyncState();

        UpdateState();
    }

    #region Event Action

    private void OnGetAttack(object arg1, AttackSender arg2)
    {
        IsGetAttack = true;
        animator.SetTrigger("GetAttack");
    }

    private void EnterGoundState() { }

    #endregion

    #region Update

    private void UpdateSignals()
    {
        //Update IsGround
        groundNormal = GetGroundNormal();
        IsGround = groundNormal != default;
        if (!IsJumpUp && IsGround) IsAir = false;

        //Update IsFall
        if (IsGround) airTimer.Reset();
        IsFall = airTimer.Time > EnterFallTime;
        if (IsFall) IsAir = true;
    }

    private void SyncState()
    {
        switch (CharactorState)
        {
            case CharactorState.Move:
                if (IsAir)
                {
                    CharactorState = CharactorState.Air;
                }
                if (IsAttack)
                {
                    CharactorState = CharactorState.Attack;
                }
                break;
            case CharactorState.Air:
                if (!IsAir)
                {
                    CharactorState = CharactorState.Move;
                }
                break;
            case CharactorState.Attack:
                if (!IsAttack)
                {
                    CharactorState = CharactorState.Move;
                }
                break;
            case CharactorState.GetAttack:
                break;
            default:
                break;
        }
    }

    private void SyncAnimator()
    {
        animator.SetBool("IsFall", IsFall);
        animator.SetBool("IsAir", IsAir);
        animator.SetBool("IsGetAttack", IsGetAttack);
    }

    private void UpdateState()
    {
        switch (CharactorState)
        {
            case CharactorState.Move:
                //Rotate
                if (HasMovingInput)
                {
                    Vector3 camera_z_w_2d = Vector3.Scale(Vector3.right + Vector3.forward,
                        FreeLookCamera.transform.forward);
                    transform.forward = Vector3.Slerp(transform.forward, camera_z_w_2d, 0.2f);
                }

                //Move
                bool IsRun = Input.GetKey(KeyCode.LeftShift);
                float speedMul = (IsRun ? 1f : 0.5f);
                Vector2 axis = InputManager.Instance.Axis;

                Vector3 speed_local = new Vector3(axis.x, 0, axis.y) * speedMul;
                animator.SetFloat("SpeedX", speed_local.x);
                animator.SetFloat("SpeedZ", speed_local.z);

                Vector3 forward_world = Vector3.Cross(transform.right, groundNormal);
                Matrix4x4 matrix4X4 = new Matrix4x4(
                    transform.right, groundNormal, forward_world, Vector3.zero);
                Vector3 speed_world = matrix4X4.MultiplyVector(speed_local);
                rigidbody.velocity = speed_world * CharactorData.Instance.Speed;

                //Attack
                if (IsGround && Input.GetMouseButton(0))
                {
                    animator.SetBool("KeepAttack", true);
                    CharactorState = CharactorState.Attack;
                }

                //Jump
                if (Input.GetKey(KeyCode.Space) && !IsJumpUp)
                {
                    IsJumpUp = true;
                    IsAir = true;
                    rigidbody.velocity += transform.up * CharactorData.Instance.JumpSpeed;
                }
                break;
            case CharactorState.Air:
                break;
            case CharactorState.Attack:
                if (Input.GetMouseButton(0))
                {
                    animator.SetBool("KeepAttack", true);
                }
                break;
            case CharactorState.GetAttack:
                break;
            default:
                break;
        }
    }

    #endregion

    #region Others

    private Vector3 GetGroundNormal()
    {
        Ray ray = new Ray(transform.position + transform.up * 0.01f, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, CharactorData.Instance.GroundCheckDistance, groundLayer))
        {
            return hitInfo.normal;
        }
        else
        {
            return default;
        }

    }

    private void OnAnimatorMove()
    {
        if (IsAttack)
        {
            transform.position += animator.deltaPosition;
        }
    }

    #endregion
}
