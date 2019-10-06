using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SteeringState
{
    Stopped,
    Navigating,
    Arriving
}

public enum OrientationQuality
{
    Low,
    High
}

public enum OrientationBehavior
{
    LookForward,
    LookAtTarget,
    None
}

/// <summary>
/// Template for agent steering
/// </summary>
public abstract class SteeringTemplate : MonoBehaviour
{
    
    /// <summary>
    /// return a vector pointing from the projection of v on normal to the vector v
    /// </summary>
    /// <param name="v"></param>
    /// <param name="normal"></param>
    /// <returns></returns>
    public static Vector3 ProjectOnPlane(Vector3 v, Vector3 normal)
    {
        return v - Vector3.Project(v, normal);
    }

    protected static readonly float TURN_EPSILON = 0.9995f;
    protected static readonly float STOP_EPSILON = 0.01f;
    protected static readonly float TURN_ANGLE = 30f;


    protected Vector3 lastPosition = Vector3.zero;
    protected Vector3 target = Vector3.zero;
    public abstract Vector3 Target { get; set; }

    /// <summary>
    /// Whether we're attached to the navmesh
    /// </summary>
    protected bool attached = true;
    public abstract bool Attached { get; set; }

    [HideInInspector]
    public Quaternion desiredOrientation = Quaternion.identity;

    // Steering Parameters
    public float YOffset = 0.0f;
    public float radius = 0.6f;
    public float height = 2.0f;

    [Tooltip("When a character is within this radius from the target, it will completely stop.")]
    [Range(0.7f, 1f)]
    public float stoppingRadius = 0.7f;

    [Tooltip("The radius at which the character will begin to slow down before stopping (this is addedto the stopping radius). This feature is toggled with the Slow Arrival parameter.")]
    [Range(1f, 2f)]
    public float arrivingRadius = 1f;

    public float acceleration = 2.0f;

    [Range(1f, 6f)]
    public float maxSpeed = 2.2f;

    [Range(1f, 2f)]
    public float minSpeed = 1f;

    public bool SlowArrival = true;

    public bool ShowDragGizmo = false;
    public bool ShowAgentRadiusGizmo = false;
    public bool ShowTargetRadiusGizmo = false;

    // Orientation Parameters
    [Tooltip("The speed at which we will turn to reorient while walking/running.")]
    public float driveSpeed = 120.0f;

    public float dragRadius = 0.5f;
    public bool planar = true;

    [Tooltip("Turns on or off the ability to turn to face a desired orientation.")]
    public bool driveOrientation = true;

    public OrientationQuality orientationQuality = OrientationQuality.High;
    [Tooltip("Currently we have three options to compute orientation while walking/running. " +
        "Setting this to “None” allows an external object to set the desired orientation, " +
        "while any other setting means that the controller will set a desired orientation internally " +
        "to either look forward or look at the objective.")]
    public OrientationBehavior orientationBehavior = OrientationBehavior.LookForward;

    public abstract bool IsAtTarget();
    public abstract bool IsStopped();
    public abstract bool HasArrived();
    public abstract bool CanReach(Vector3 target);
    public abstract void Stop();
    public abstract void Warp(Vector3 target);

    /// <summary>
    /// check whether the agent can face the target without rotating to it
    /// </summary>
    /// <returns></returns>
    public bool IsFacing()
    {
        Quaternion orientation = transform.rotation;
        Quaternion desired = this.desiredOrientation;

        if (Mathf.Abs(Quaternion.Angle(desired, orientation)) < TURN_ANGLE)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// Force to rotate to the desired orientation in a snap
    /// </summary>
    public void FacingSnap()
    {
        Quaternion desired = this.desiredOrientation;
        this.transform.rotation = desired;
    }

    public void SetDesiredOrientation(Vector3 target)
    {
        Vector3 difference = ProjectOnPlane(target - transform.position, Vector3.up);

        this.desiredOrientation = Quaternion.LookRotation(difference, Vector3.up);
    }
}
