using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStates { HitPlayer, HitEnemy, Hitnothing }

    private Rigidbody rb;
    public RockStates rockStates;

    [Header("Basic Settings")]
    public float force;
    public int damage;

    public GameObject target;
    private Vector3 direction;

    public GameObject breakEffect;
    public float timer=0f;

    /*void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude < 0.5f)
        {
            rockStates = RockStates.Hitnothing;
        }
    }*/
   private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 3f)
        {
            timer= 0f;
            Destroy(gameObject, 2f);
        }
    }


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        rockStates = RockStates.HitPlayer;
        FlyToTarget();
    }
    public void FlyToTarget()
    {
        if (target == null)
        {
            target = FindObjectOfType<PlayerController>().gameObject;
        }
        direction = (target.transform.position - transform.position+Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        switch (rockStates)
        { 
            case RockStates.HitPlayer:
                if (other.gameObject.CompareTag("Player"))
                { 
                    other.gameObject.GetComponent<NavMeshAgent>().isStopped= true;
                    other.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;

                    other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    other.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, other.gameObject.GetComponent<CharacterStats>());
                    //Instantiate(breakEffect, transform.position, Quaternion.identity);
                    rockStates = RockStates.Hitnothing;
                    //Destroy(gameObject);
                }
                break;

            case RockStates.HitEnemy:
                if (other.gameObject.GetComponent<Golem>())
                {
                    var otherStats = other.gameObject.GetComponent<CharacterStats>();
                    otherStats.TakeDamage(damage, otherStats);
                    Destroy(gameObject);
                }
                break;
        }
    }
}
