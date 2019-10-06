using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TreeSharpPlus;

/// <summary>
/// This is where subtrees of high level behaviors are implemented,
/// using the provided methods in CharacterMecanim. Here you can find high level action
/// nodes that can be used as leaf nodes in your tree (Node_GoTo, ST_PlayGesture)
/// </summary>
public enum AnimationLayer
{
    Hand,
    Body,
    Face,
}

public class BehaviorMecanim :MonoBehaviour
{
    [HideInInspector]
    public CharacterMecanim Character = null;

    [HideInInspector]
    public IKController IK = null;
    

    void Awake() { this.Initialize(); }

    protected void Initialize()
    {
        if(this.GetComponent<CharacterMecanim>() == null)
        {
            this.gameObject.AddComponent<CharacterMecanim>();
            this.gameObject.AddComponent<IKController>();
        }
        this.Character = this.GetComponent<CharacterMecanim>();
        this.IK = this.GetComponent<IKController>();
    }

    protected void StartTree(
        Node root,
        BehaviorObject.StatusChangedEventHandler statusChanged = null)
    {
    }

    #region Helper Nodes

    #region Navigation
    /// <summary>
    /// Approaches a target
    /// </summary>
    public Node Node_GoTo(Val<Vector3> targ)
    {
        return new LeafInvoke(
            () => this.Character.NavGoTo(targ),
            () => this.Character.NavStop());
    }

    /// <summary>
    /// Approaches a target with a certain speed
    /// speed has to be in the range of (1f,6f)
    /// </summary>
    public Node Node_GoTo(Val<Vector3> targ, Val<float> speed)
    {
        return new Sequence(
            new LeafInvoke(() => this.Character.SetSpeed(speed)),
            new LeafInvoke(() => this.Character.NavGoTo(targ), () => this.Character.NavStop())
            );
    }
    /// <summary>
    /// Approaches a target at a given radius
    /// </summary>
    public Node Node_GoToUpToRadius(Val<Vector3> targ, Val<float> dist)
    {
        Func<RunStatus> GoUpToRadius =
            delegate ()
            {
                Vector3 targPos = targ.Value;
                Vector3 curPos = this.transform.position;
                if ((targPos - curPos).magnitude < dist.Value)
                {
                    this.Character.NavStop();
                    return RunStatus.Success;
                }
                return this.Character.NavGoTo(targ);
            };

        return new LeafInvoke(
            GoUpToRadius,
            () => this.Character.NavStop());
    }
    /// <summary>
    /// Approaches a target at a given radius
    /// speed has to be in the range of (1f,6f)
    /// </summary>
    public Node Node_GoToUpToRadius(Val<Vector3> targ, Val<float> dist, Val<float> speed)
    {
        Func<RunStatus> GoUpToRadius =
            delegate ()
            {
                Vector3 targPos = targ.Value;
                Vector3 curPos = this.transform.position;
                if ((targPos - curPos).magnitude < dist.Value)
                {
                    this.Character.NavStop();
                    //Debug.Log("Stoped!");
                    return RunStatus.Success;
                }
                this.Character.SetSpeed(speed);
                return this.Character.NavGoTo(targ);
            };

        return new LeafInvoke(
            GoUpToRadius,
            () => this.Character.NavStop());
    }

    /// <summary>
    /// Go along Points with certain speed when reaching each point
    /// </summary>
    /// <param name="targs"></param>
    /// <param name="speeds"></param>
    /// <returns></returns>
    public Node Node_GoAlongPoints(Val<Vector3>[] targs, Val<float>[] speeds)
    {

        IEnumerable<Node> nodes = GetNodes(targs,speeds);
        List<Node> nodesList = new List<Node>();
        foreach (Node node in nodes)
        {
            nodesList.Add(node);
        }

        return new Sequence(nodesList.ToArray());  
    }

    
    /// <summary>
    /// Helper function for GoAlongPoints
    /// </summary>
    /// <param name="targs"></param>
    /// <param name="speeds"></param>
    /// <returns></returns>
    private IEnumerable<Node> GetNodes(Val<Vector3>[] targs, Val<float>[] speeds)
    {
        if(targs.Length!=speeds.Length)
        {
            Debug.Log("#targets should be as much as #speeds");
            throw new NotImplementedException();
        }
        for (int i=0;i<targs.Length;i++)
        {
            yield return this.Node_GoToUpToRadius(targs[i], new Val<float>(1f), speeds[i]);
        }
    }


    /// <summary>
    /// Orient towards a target position
    /// </summary>
    /// <param name="targ"></param>
    /// <returns></returns>
    public Node Node_OrientTowards(Val<Vector3> targ)
    {
        return new LeafInvoke(
            () => this.Character.NavTurn(targ),
            () => this.Character.NavOrientBehavior(
                OrientationBehavior.LookForward));
    }

    /// <summary>
    /// Orient towards a target position
    /// </summary>
    /// <param name="targ"></param>
    /// <returns></returns>
    public Node Node_Orient(Val<Quaternion> direction)
    {
        return new LeafInvoke(
            () => this.Character.NavTurn(direction),
            () => this.Character.NavOrientBehavior(
                OrientationBehavior.LookForward));
    }

    public Node Node_NudgeTo(Val<Vector3> targ)
    {
        return new LeafInvoke(
            () => this.Character.NavNudgeTo(targ),
            () => this.Character.NavStop());
    }
    #endregion


    #region IK
    public Node Node_Grab(Val<Vector3> item)
    {
        return new SequenceParallel(
                   this.ST_PlayGesture("GRAB", AnimationLayer.Hand, 3000),
                   new Sequence(
                       new LeafWait(1000),
                       new LeafInvoke(()=> { IK.m_TargetObj = item.Value; IK.TargetMixWeight = 1.0f; }),
                       new LeafWait(1000),
                       new LeafInvoke(()=> { IK.TargetMixWeight = 0.0f; })
                       )
                   );
    }
    #endregion



    #region Animation
    /// <summary>
    /// A Hand animation is started if the bool is true, the hand animation 
    /// is stopped if the bool is false
    /// </summary>
    public Node Node_HandAnimation(Val<string> gestureName, Val<bool> start)
    {

        return new LeafInvoke(
            () => this.Character.HandAnimation(gestureName, start),
            () => this.Character.HandAnimation(gestureName, false));
    }

    /// <summary>
    /// A Face animation is started if the bool is true, the face animation 
    /// is stopped if the bool is false
    /// </summary>
    public Node Node_FaceAnimation(Val<string> gestureName, Val<bool> start)
    {
        return new LeafInvoke(
            () => this.Character.FaceAnimation(gestureName, start),
            () => this.Character.FaceAnimation(gestureName, false));
    }

    public Node Node_BodyAnimation(Val<string> gestureName, Val<bool> start)
    {
        return new LeafInvoke(
            () => this.Character.BodyAnimation(gestureName, start),
            () => this.Character.BodyAnimation(gestureName, false));
    }


    #endregion

    #endregion

    #region Helper Subtrees

    /// <summary>
    /// Plays a gesture of a determined type for a given duration
    /// </summary>
    public Node ST_PlayGesture(
        Val<string> gestureName,
        Val<AnimationLayer> layer,
        Val<long> duration)
    {
        switch (layer.Value)
        {
            case AnimationLayer.Hand:
                return this.ST_PlayHandGesture(gestureName, duration);
            case AnimationLayer.Body:
                return this.ST_PlayBodyGesture(gestureName, duration);
            case AnimationLayer.Face:
                return this.ST_PlayFaceGesture(gestureName, duration);
        }
        return null;
    }

    /// <summary>
    /// Plays a hand gesture for a duration in miliseconds
    /// </summary>
    public Node ST_PlayHandGesture(
        Val<string> gestureName, Val<long> duration)
    {
        return new DecoratorCatch(
            () => this.Character.HandAnimation(gestureName, false),
            new Sequence(
                Node_HandAnimation(gestureName, true),
                new LeafWait(duration),
                Node_HandAnimation(gestureName, false)));
    }

    /// <summary>
    /// Plays a body gesture for a duration in miliseconds
    /// </summary>
    public Node ST_PlayBodyGesture(
        Val<string> gestureName, Val<long> duration)
    {
        return new DecoratorCatch(
            () => this.Character.BodyAnimation(gestureName, false),
            new Sequence(
            this.Node_BodyAnimation(gestureName, true),
            new LeafWait(duration),
            this.Node_BodyAnimation(gestureName, false)));
    }

    /// <summary>
    /// Plays a face gesture for a duration in miliseconds
    /// </summary>
    public Node ST_PlayFaceGesture(
        Val<string> gestureName, Val<long> duration)
    {
        return new DecoratorCatch(
            () => this.Character.FaceAnimation(gestureName, false),
            new Sequence(
                Node_FaceAnimation(gestureName, true),
                new LeafWait(duration),
                Node_FaceAnimation(gestureName, false)));
    }

    /// <summary>
    /// Turns to face a target position
    /// </summary>
    public Node ST_TurnToFace(Val<Vector3> target)
    {
        Func<RunStatus> turn =
            () => this.Character.NavTurn(target);

        Func<RunStatus> stopTurning =
            () => this.Character.NavOrientBehavior(
                OrientationBehavior.LookForward);

        return
            new Sequence(
                new LeafInvoke(turn, stopTurning));
    }
    #endregion
}
