using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public enum EnmeyStates{GUARD,PATROL,CHASE,DEAD}
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour,IEndGameObsever
{
    private EnmeyStates enmeyStates;
    private NavMeshAgent agent;
    private Animator anim;
    private Collider coll;
    protected CharacterStats characterStats;

    [Header("Basic Settings")]
    public float sightRadius;
    public bool isGuard;
    public float lookAtTime;

    private float speed;
    protected GameObject attackTarget;
    private float remainLookAtTime;
    private float lastAttackTime;
    private Quaternion guardRoation;

    [Header("PatrolState")]
    public float patrolRange;
    private Vector3 wayPoint;
    private Vector3 guardPos;
    //¶¯»­
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;
    bool playerIsDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        speed = agent.speed;
        guardPos =transform.position;
        guardRoation= transform.rotation;
        remainLookAtTime = lookAtTime;
        coll= GetComponent<Collider>(); 
    }
    private void Start()
    {
        if (isGuard)
        {
            enmeyStates = EnmeyStates.GUARD;
        }
        else 
        {
            enmeyStates = EnmeyStates.PATROL;
            GetNewWayPoint();
        }
        GameManager.Instance.AddObserver(this);
    }
  /* void OnEnable() 
    { 
        GameManager.Instance.AddObserver(this);
    }*/
    void OnDisable()
    {
        if (!GameManager.IsInitalized) return;
        GameManager.Instance.RemoveObserver(this);
    }

    private void Update()
    {
        if (characterStats.CurrentHealth == 0)
        {
            isDead = true;
        }
        if (!playerIsDead)
        {
            SwitchStates();
            SwitchAnimator();
            lastAttackTime -= Time.deltaTime;
        }
    }
   
    
    //×´Ì¬»ú
    void SwitchStates()
    {
        if (isDead == true)
        { 
            enmeyStates = EnmeyStates.DEAD;
        }
        else if (FoundPlayer())
        {
            enmeyStates = EnmeyStates.CHASE;
            

        }
        switch (enmeyStates)
        {
            case EnmeyStates.GUARD:
                isChase = false;
                if (transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped= false;
                    agent.destination= guardPos;
                    if (Vector3.Distance(transform.position, guardPos) < agent.stoppingDistance)
                    { 
                        isWalk= false;
                        transform.rotation = guardRoation;
                    }
                }

                break;
            case EnmeyStates.PATROL:
                isChase = false ;
                agent.speed= speed*0.5f;
                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                    { 
                        remainLookAtTime-=Time.deltaTime;
                    }
                    else 
                        GetNewWayPoint();
                }
                else 
                { 
                    isWalk= true;
                    agent.destination= wayPoint;
                }
                break;
            case EnmeyStates.CHASE:
                isWalk = false;
                isChase = true;
                agent.speed = speed;
                //ÍÑÕ½
                if (!FoundPlayer())
                {
                    isFollow = false;
                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    //»Øµ½ÊØÎÀ»òÑ²Âß×´Ì¬
                    else if (isGuard)
                    {
                        enmeyStates = EnmeyStates.GUARD;
                    }
                    else 
                    { 
                        enmeyStates= EnmeyStates.PATROL;
                    }
                }
                //×·»÷
                else
                {
                    isFollow=true;
                    agent.isStopped= false;
                    agent.destination = attackTarget.transform.position;
                }
                //¹¥»÷
                if (TargetInAttackRange() || TargetInSkillRange())
                { 
                    isFollow=false;
                    agent.isStopped = true;
                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;

                        //±©»÷ÅÐ¶Ï
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        Attack();
                    }
                }
                break;
            case EnmeyStates.DEAD:
                agent.enabled= false;
                coll.enabled= false;
                Destroy(gameObject,2f);
                break;
        }
    }

    bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(transform.position, attackTarget.transform.position) <= characterStats.attackData.attackRange;
        else return false;
    }

    bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(transform.position, attackTarget.transform.position) <= characterStats.attackData.skillRange;
        else return false;
    }

    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
        {
            //½üÉí¹¥»÷¶¯»­
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange())
        {
            //Ô¶³Ì¹¥»÷¶¯»­
            anim.SetTrigger("Skill");
        }
    }

    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                attackTarget = collider.gameObject;
                return true;
            }
        }
        attackTarget= null;
        return false;
    }

    void SwitchAnimator()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Death", isDead);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }
    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;

        float Xrandom = Random.Range(-patrolRange, patrolRange);
        float Zrandom = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPoint = new Vector3(Xrandom + guardPos.x, transform.position.y, Zrandom + guardPos.z);
        NavMeshHit hit;
        wayPoint=NavMesh.SamplePosition(randomPoint, out hit, patrolRange,1)?hit.position:transform.position;
    }
    void Hit()
    {
        if (attackTarget != null&&transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        //»ñÊ¤
        //Í£Ö¹ÒÆ¶¯
        //Í£Ö¹agent
        playerIsDead= true;
        isChase= false;
        isWalk= false;
        attackTarget =null;
        anim.SetBool("Win", true);
    }
}
