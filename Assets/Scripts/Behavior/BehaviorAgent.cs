﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TreeSharpPlus;

/// <summary>
/// This is one of the objects that can hold your tree (your tree root).
/// This object handles high level operations on the tree it owns.
/// This is basically just a wrapper around your tree.
/// </summary>
public sealed class BehaviorAgent : BehaviorObject, IBehaviorUpdate
{
    #region Parameters
    /// <summary>
    /// The tree is final and can't be changed
    /// </summary>
    private readonly Node treeRoot = null;
    #endregion

    #region Constructor
    /// <summary>
    /// Block off the empty constructor
    /// </summary>
    private BehaviorAgent()
    {
        throw new NotImplementedException();  
    }

    /// <summary>
    /// Constructs a new BehaviorAgent responsible for taking care of a tree
    /// </summary>
    /// <param name="root">The root node of the tree</param>
    /// <param name="statusChanged">An OnStatusChanged delegate for receiving 
    /// status change events</param>
    public BehaviorAgent(Node root)
        : base()
    {
        this.treeRoot = root;
    }
    #endregion

    #region Internal Functions
    /// <summary>
    /// External command for resuming autonomy and registers with the 
    /// BehaviorManager to start receiving ticks for it, and calls the 
    /// TreeStart function. Startbehavior will always succeed
    /// unless an error is thrown (i.e. resuming while in an event
    /// or terminating)
    /// </summary>
    internal override void StartBehavior()
    {
        switch (this.Status)
        {
            case BehaviorStatus.Terminating:
                this.Status = BehaviorStatus.Restarting;
                break;
            case BehaviorStatus.InEvent:
                Debug.LogWarning(
                    this + ".StartBehavior() ignored: Agent is in an event!");
                break;
            case BehaviorStatus.Idle:
                BehaviorManager.Instance.Register(this);
                this.TreeStart();
                this.Status = BehaviorStatus.Running;
                break;
            default: break;
        }
    }

    /// <summary>
    /// Tells the agent to suspend itself, reporting success or failure
    /// </summary>
    /// <returns>true if the agent is idle, false otherwise</returns>
    internal override RunStatus StopBehavior()
    {
        switch (this.Status)
        {
            case BehaviorStatus.Idle:
                return RunStatus.Success;
            case BehaviorStatus.InEvent:
                Debug.LogWarning(
                    this + ".StopBehavior() ignored: Agent is in an event!");
                return RunStatus.Success;
            case BehaviorStatus.Running:
                this.Status = BehaviorStatus.Terminating;
                break;
            case BehaviorStatus.Restarting:
                this.Status = BehaviorStatus.Terminating; // Nevermind then!
                break;
            default: break;
        }

        // We do the actual termination in the behavior update to keep
        // everything in sync with the central heartbeat ticks
        return RunStatus.Running;
    }
    #endregion


    #region Private Functions

    /// <summary>
    /// Activates the personal behavior tree, used in startbehavior
    /// </summary>
    private void TreeStart()
    {
        this.treeRoot.Start();
    }

    /// <summary>
    /// Terminates the personal behavior tree, used in behaviorupdate
    /// </summary>
    private RunStatus TreeTerminate()
    {
        // TODO: This doesn't handle termination failure very well, since we'll
        // report failure once and then switch to Idle and then report success
        // - AS

        // If we finish terminating, switch our state to Idle
        RunStatus result = this.treeRoot.Terminate();

        if (result == RunStatus.Failure)
            Debug.LogWarning(this + ".Terminate() failed");
        return result;
    }
    #endregion

    /// <summary>
    /// By default, ticks the internal tree if it's running
    /// </summary>
    RunStatus IBehaviorUpdate.BehaviorUpdate(float deltaTime) 
    {
		if (this.Status == BehaviorStatus.Running)
        {
            this.treeRoot.Tick();
        }
        else if (this.Status == BehaviorStatus.Terminating 
            || this.Status == BehaviorStatus.Restarting)
        {
            RunStatus result = this.TreeTerminate();

            // TODO: Handle failure to terminate - AS
            if (result != RunStatus.Running)
            {
                if (this.Status == BehaviorStatus.Restarting)
                {
                    this.Status = BehaviorStatus.Idle;
                    this.StartBehavior();
                }
                else
                {
                    this.Status = BehaviorStatus.Idle;
                }
            }
        }

        // TODO: We could make this more efficient by forgetting about agents
        // when they're in events and remembering them when they're idle. - AS
        return RunStatus.Running;
    }

}
