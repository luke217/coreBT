using UnityEngine;
using System;
using System.Collections;
using TreeSharpPlus;

/// <summary>
/// very basic motor skills(Navigation, steering, IK) performed by the character. 
/// These actions include no preconditions
/// and can fail if executed in impossible/nonsensical situations. 
/// In this case, the functions will usually try their best.
/// </summary>
public class BodyMecanim : MonoBehaviour
{
 
    private Animator animator = null;
    private SteeringController steering = null;

    private const float IN_RANGE = 2.0f;
    private const float DIRECTION_DAMP_TIME = .25f;

    private bool resettingHandLayerWeight;
    private bool resettingFaceLayerWeight;

    private Interpolator<Vector3> nudge = null;

    public SteeringController Steering
    {
        get
        {
            if (this.steering == null)
                throw new ApplicationException(
                    this.gameObject.name + ": No SteeringController found!");
            return this.steering;
        }
    }

    void Awake()
    {
        this.steering = this.gameObject.GetComponent<SteeringController>();
        if (this.steering == null)
        {
            this.steering = transform.gameObject.AddComponent<SteeringController>();
        }

        this.animator = this.gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (resettingHandLayerWeight)
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0, Time.deltaTime));
        }
        if (resettingFaceLayerWeight)
        {
            animator.SetLayerWeight(2, Mathf.Lerp(animator.GetLayerWeight(2), 0, Time.deltaTime));
        }
        if (this.nudge != null)
        {
            transform.position = this.nudge.Value;
        }
    }


    #region Animation Commands
    /// <summary>
    /// Commands for face animations. 
    /// </summary>
    public void FaceAnimation(string gestureName, bool isActive)
    {
        if (isActive == true)
            this.ResetAnimation();
        this.animator.SetBool("FaceAnimation", isActive);

        // Layout 2: "Face Animation Layer". Weight needed in order to 
        // run the animation
        if (isActive)
        {
            this.animator.SetLayerWeight(2, 1);
            resettingFaceLayerWeight = false;
        }
        else
        {
            resettingFaceLayerWeight = true;
        }

        switch (gestureName.ToUpper())
        {
            case "DRINK": 
                this.animator.SetBool("F_Drink", isActive); 
                break;
            case "EAT": 
                this.animator.SetBool("F_Eat", isActive); 
                break;
            case "ROAR": 
                this.animator.SetBool("F_Roar", isActive); 
                break;
            case "SAD": 
                this.animator.SetBool("F_Sad", isActive);
                break;
        }
    }

    /// <summary>
    /// Commands for hand animations. 
    /// </summary>
    public void HandAnimation(string gestureName, bool isActive)
    {

        if (isActive == true)
            this.ResetAnimation();
        this.animator.SetBool("HandAnimation", isActive);

        // Layout 1: "Hand Animation Layer". Weight needed in order to 
        // run the animation
        if (isActive)
        {
            this.animator.SetLayerWeight(1, 1);
            resettingHandLayerWeight = false;
        }
        else
        {
            resettingHandLayerWeight = true;
        }

        switch (gestureName.ToUpper())
        {
            case "GRAB":
                this.animator.SetBool("H_Grab", isActive);
                break;
            case "APPLEPICK": 
                this.animator.SetBool("H_ApplePick", isActive); 
                break;
            case "CHEER": 
                this.animator.SetBool("H_Cheer", isActive); 
                break;
            case "COWBOY": 
                this.animator.SetBool("H_CowBoy", isActive); 
                break;
            case "FISHING": 
                this.animator.SetBool("H_Fishing", isActive); 
                break;
            case "CROWDPUMP": 
                this.animator.SetBool("H_CrowdPump", isActive); 
                break;
            case "POINTING": 
                this.animator.SetBool("H_Pointing", isActive); 
                break;
            case "WONDERFUL": 
                this.animator.SetBool("H_Wonderful", isActive); 
                break;
            case "CUTTHROAT": 
                this.animator.SetBool("H_CutThroat", isActive); 
                break;
			case "LOOKUP":
				this.animator.SetBool("H_LookUp", isActive);
				break;
            case "SATNIGHTFEVER":
                this.animator.SetBool("H_SatNightFever", isActive);
                break;
            case "CHESTPUMPSALUTE":
                this.animator.SetBool("H_ChestPumpSalute", isActive);
                break;
			case "BEINGCOCKY":
				this.animator.SetBool("H_BeingCocky", isActive);
				break;
            case "CRY":
                if (isActive)
                    this.animator.SetTrigger("H_Cry");
                break;
            case "YAWN":
                if (isActive)
                    this.animator.SetTrigger("H_Yawn");
                break;
            case "THINK":
                if (isActive)
                    this.animator.SetTrigger("H_Think");
                break;
            case "SURPRISED":
                if (isActive)
                    this.animator.SetTrigger("H_Surprised");
                break;
            case "WAVE":
                if (isActive)
                    this.animator.SetTrigger("H_Wave");
                break;
            case "TEXTING":
                if (isActive)
                    this.animator.SetTrigger("H_Texting");
                break;
            case "CLAP":
                this.animator.SetBool("H_Clap", isActive);
                break;
            case "CALLOVER":
                if (isActive)
                    this.animator.SetTrigger("H_CallOver");
                break;
            case "STAYAWAY":
                if (isActive)
                    this.animator.SetTrigger("H_StayAway");
                break;
            case "MOUTHWIPE":
                if (isActive)
                    this.animator.SetTrigger("H_MouthWipe");
                break;
            case "SHOCK":
                if (isActive)
                    this.animator.SetTrigger("H_Shock");
                break;
            case "HITSTEALTH":
                this.animator.SetBool("H_HitStealth", isActive);
                break;
     
        }
    }

    /// <summary>
    /// Commands for body animations.
    /// </summary>
	public void BodyAnimation(string gestureName, bool isActive)
	{
		
		if (isActive == true)
			this.ResetAnimation();

		switch (gestureName.ToUpper())
		{
		
		case "STEPBACK": 
            if (isActive)
			    this.animator.SetTrigger("B_StepBackTrigger");
			break;
        case "PICKUPRIGHT":
            if (isActive)
                this.animator.SetTrigger("B_PickupRight");
            break;
        case "PICKUPLEFT":
            if (isActive)
                this.animator.SetTrigger("B_PickupLeft");
            break;
        case "TALKING ON PHONE":
            if (isActive)
                this.animator.SetTrigger("B_Talking_On_Phone");
            break;
     
		}
	}

    /// <summary>
    /// Resets all currently running animations
    /// </summary>
    public void ResetAnimation()
    {
        this.animator.SetBool("F_Drink", false);
        this.animator.SetBool("F_Eat", false);
        this.animator.SetBool("F_Roar", false);
        this.animator.SetBool("F_Sad", false);

        this.animator.SetBool("H_Grab", false);
        this.animator.SetBool("H_ApplePick", false);
        this.animator.SetBool("H_Cheer", false);
        this.animator.SetBool("H_CowBoy", false);
        this.animator.SetBool("H_Fishing", false);
        this.animator.SetBool("H_CrowdPump", false);
        this.animator.SetBool("H_Pointing", false);
        this.animator.SetBool("H_Wonderful", false);
        this.animator.SetBool("H_CutThroat", false);
		this.animator.SetBool ("H_LookUp", false);

        this.animator.SetBool("FaceAnimation", false);
        this.animator.SetBool("HandAnimation", false);
    }
    #endregion

    #region Sitting Commands
    /// <summary>
    /// Sits the character down. Note that this will not interrupt
    /// the character's navigation if the character is still walking
    /// somewhere.
    /// </summary>
    public void SitDown()
    {
		this.animator.SetBool ("B_Sitting", true);
    }

    /// <summary>
    /// Stands the character up. Note that this will not interrupt
    /// the character's navigation if the character is still walking
    /// somewhere.
    /// </summary>
    public void StandUp()
    {
		this.animator.SetBool ("B_Sitting", false);

	}
	
	/// <summary>
    /// Returns true if and only if the character is definitely sitting.
    /// </summary>
    public bool IsSitting()
    {
		return this.animator.GetBool ("B_Sitting");
    }

    /// <summary>
    /// Returns true if and only if the character is definitely standing.
    /// </summary>
    public bool IsStanding()
    {
		return !this.animator.GetBool ("B_Sitting");
	}
    #endregion

    #region Navigation Commands
    public float NavStopRadius
    {
        get { return this.Steering.stoppingRadius; }
        set { this.Steering.stoppingRadius = value; }
    }

    public float NavArriveRadius
    {
        get { return this.Steering.arrivingRadius; }
        set { this.Steering.arrivingRadius = value; }
    }

    /// <summary>
    /// Sets the navigation target for a character Note that this 
    /// will move the character even if the character is sitting.
    /// </summary>
    public void NavGoTo(Vector3 target)
    {
        this.Steering.Target = target;
    }

    /// <summary>
    /// Stops the character while navigating.
    /// </summary>
    public void NavStop()
    {
        this.Steering.Stop();
    }

    /// <summary>
    /// Returns true if and only if the character is below a very
    /// small velocity.
    /// </summary>
    public bool NavIsStopped()
    {
        bool stopped =  this.Steering.IsStopped();
        //Debug.Log("Nav is stopped: " + stopped);
        return stopped;
    }

    /// <summary>
    /// Returns true if and only if the character is very close to the goal.
    /// </summary>
    public bool NavIsAtTarget()
    {
        return this.Steering.IsAtTarget();
    }

    /// <summary>
    /// Combines IsStopped and IsAtTarget.
    /// </summary>
    public bool NavHasArrived()
    {
        bool arrived = this.Steering.HasArrived();
        //Debug.Log("Nav has arrived: " + arrived);
        return arrived;
    }

    /// <summary>
    /// Queries a path to a given navigation target.
    /// </summary>
    public bool NavCanReach(Vector3 target)
    {
        return this.Steering.CanReach(target);
    }

    /// <summary>
    /// Returns the current navigation target.
    /// </summary>
    public Vector3 NavTarget()
    {
        return this.Steering.Target;
    }

    /// <summary>
    /// set character moving speed.
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed(float speed)
    {
        //Debug.Log("Set speed: "+speed);
        this.steering.maxSpeed = Mathf.Clamp(speed,1f,6f);
    }

    /// <summary>
    /// Starts a nudge towards a desired position
    /// </summary>
    public void NavNudge(Vector3 target, float time)
    {
        this.NavSetOrientationBehavior(OrientationBehavior.None);
        this.nudge =
            new Interpolator<Vector3>(
                transform.position,
                target,
                Vector3.Lerp);
        this.nudge.ForceMin();
        this.nudge.ToMax(time);
    }

    /// <summary>
    /// Starts a nudge towards a desired position
    /// </summary>
    public void NavNudgeStop()
    {
        this.nudge = null;
        this.NavSetOrientationBehavior(OrientationBehavior.LookForward);
    }

    /// <summary>
    /// Returns true iff we're done nudging. Returns null if we weren't
    /// nudging in the first place.
    /// </summary>
    /// <returns></returns>
    public bool? NavDoneNudge()
    {
        if (this.nudge == null)
            return null;
        return this.nudge.State == InterpolationState.Max;
    }

    /// <summary>
    /// Warps the character.
    /// </summary>
    public void NavWarp(Vector3 target)
    {
        this.Steering.Warp(target);
    }

    /// <summary>
    /// Returns true if and only if we are facing our desired orientation.
    /// </summary>
    public bool NavIsFacingDesired()
    {
        return this.Steering.IsFacing();
    }

    /// <summary>
    /// Snaps the orientation to the desired orientation
    /// </summary>
    public void NavFacingSnap()
    {
        this.Steering.FacingSnap();
    }

    /// <summary>
    /// Sets a goal orientation to face while walking. Note that this
    /// will only take effect if the orientation behavior is set to
    /// OrientationBehavior.None
    /// </summary>
    public void NavSetDesiredOrientation(Vector3 target)
    {
        this.Steering.SetDesiredOrientation(target);
    }

    /// <summary>
    /// Sets a goal orientation to face while walking. Note that this
    /// will only take effect if the orientation behavior is set to
    /// OrientationBehavior.None
    /// </summary>
    public void NavSetDesiredOrientation(Quaternion desired)
    {
        this.Steering.desiredOrientation = desired;
    }

    /// <summary>
    /// Allows you to configure the automatic orientation behavior, if any.
    /// </summary>
    public void NavSetOrientationBehavior(OrientationBehavior behavior)
    {
        this.Steering.orientationBehavior = behavior;
    }

    /// <summary>
    /// Attaches or detaches the character from the navmesh. Only 
    /// use this if you know what you're doing.
    /// </summary>
    public void NavSetAttached(bool value)
    {
        this.Steering.Attached = value;
    }
    #endregion

            
}
