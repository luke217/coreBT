using UnityEngine;
using System.Collections;
using UnityEngine.AI;

/// <summary>
/// Animator Controller:
/// Use agent information to setup animator
/// rotate transform when animator rotate(apply root rotation)
/// </summary>
public class AnimatorController : MonoBehaviour {

    private NavMeshAgent agent;
    private Animator animator;
    protected AnimatorLocomotion locomotion;


    [HideInInspector]
    public Quaternion desiredOrientation { get; set; }

    private float angleDiff;

    void Start()
    {
        this.Initialize();
    }

    public void Initialize()
    {
        agent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();
        desiredOrientation = transform.rotation;


        locomotion = new AnimatorLocomotion(animator);

    }


    public bool AgentDone()
    {
        return !agent.pathPending && AgentStopping();
    }

    protected bool AgentStopping()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    void Update()
    {
        SetupAgentLocomotion();
    }

    protected void SetupAgentLocomotion()
    {
        if (AgentDone())
        {
            agent.ResetPath();

            //handle animation
            locomotion.Do(0, angleDiff);
        }
        else
        {
            float speed = agent.desiredVelocity.magnitude;

            Vector3 velocity = Quaternion.Inverse(transform.rotation) * agent.desiredVelocity;

            float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

            //handle animation
            locomotion.Do(speed, angle);
        }
    }
    void OnAnimatorMove()
    {
        agent.velocity = animator.deltaPosition / Time.deltaTime;
        transform.rotation = animator.rootRotation;
        // get a "forward vector" for each rotation
        var forwardA = transform.rotation * Vector3.forward;
        var forwardB = desiredOrientation * Vector3.forward;
        // get a numeric angle for each vector, on the X-Z plane (relative to world forward)
        var angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
        var angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;
        // get the signed difference in these angles
        angleDiff = Mathf.DeltaAngle(angleA, angleB);
    }
}
