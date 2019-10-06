using System;
using System.Collections.Generic;
using TreeSharpPlus;
using System.Linq;
//For Debug.
using UnityEngine;

/// <summary>
/// A special BehaviorEvent that locally uses the ForEach node and can thus free up individual
/// objects instead of terminating the entire tree.
/// </summary>
public class CrowdBehaviorEvent<T> : BehaviorEvent where T : IHasBehaviorObject
{
    #region Parameters
    private Func<T, object, Node> nodeFactory;
    private Dictionary<BehaviorObject, T> behaviorObjToParticipant;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs a CrowdBehaviorEvent responsible for maintaining a ForEach node.
    /// </summary>
    public CrowdBehaviorEvent(Func<T, object, Node> participantFunc, IEnumerable<T> participants) : base(null, participants.Cast<IHasBehaviorObject>())
    {
        this.nodeFactory = participantFunc;
        this.behaviorObjToParticipant = new Dictionary<BehaviorObject, T>();
        foreach (T participant in participants)
            this.behaviorObjToParticipant.Add(participant.Object, participant);
        this.treeFactory = this.RootFactory;
    }
    #endregion

    #region Protected Functions
    protected override void Initializing()
    {
        //Remove all ineligible objects from the node, before doing the
        //real initialization. As the node has not been started yet, removing
        //a child from the ForEach<> node can be done immediately.
        this.RemoveIneligible();
        base.Initializing();
    }

    protected override void Pending()
    {
        //Remove all ineligible objects from the node, before doing the
        //real initialization. As the node has not been started yet, removing
        //a child from the ForEach<> node can be done immediately.
        this.RemoveIneligible();
        base.Pending();
    }
    protected override RunStatus Yield(BehaviorObject obj)
    {
        Debug.Log("CrowdBehaviorEvent: Yielding " + obj.ToString());
        if (this.participants.Contains(obj))
        {
            ForEach<T> root = (ForEach<T>)this.treeRoot;
            return root.RemoveParticipant(this.behaviorObjToParticipant[obj]);
        }
        return RunStatus.Success;
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// The function which returns the root of this BehaviorEvent.
    /// </summary>
    private ForEach<T> RootFactory(object token)
    {
        return new ForEach<T>(
            (T participant) => this.nodeFactory.Invoke(participant, token),
            this.behaviorObjToParticipant.Values);
    }

    /// <summary>
    /// Removes all the ineligible objects from the Event. This must be done before
    /// the node is actually started, else it can lead to wrong behavior.
    /// </summary>
    private void RemoveIneligible()
    {
        this.DoForAll((BehaviorObject obj) =>
        {
            if (this.CheckEligible(obj) == RunStatus.Failure)
            {
                this.Yield(obj);
                this.participants.Remove(obj);
                ((ForEach<T>)this.treeRoot).RemoveParticipant(behaviorObjToParticipant[obj]);
            }
            return RunStatus.Success;
        });
    }
    #endregion

}
