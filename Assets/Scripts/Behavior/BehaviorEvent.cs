using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TreeSharpPlus;

public enum EventStatus
{
    Instantiated, //< The event has been instantiated and is waiting to begin
    Initializing, //< The event is logging as "pending" with all participants
    Pending,      //< The event is pending and is now acquiring participants
    Running,      //< The event is running and actively ticking
    Terminating,  //< The event is in the process of terminating
    Detaching,    //< The event is detaching from agents and cleaning up
    Finished,     //< The event has ended (success or failure)
    // TODO: Maybe a TerminateSuccess and TerminateFailure? - AS
}



public class BehaviorEvent : IBehaviorUpdate
{
    #region Parameters
    public string Name = null;

    public delegate void StatusChangedEventHandler(BehaviorEvent sender, EventStatus newStatus);
    public event StatusChangedEventHandler StatusChanged;

    private float priority;
    private EventStatus eventStatus;
    public float Priority { get { return this.priority; } }
    /// <summary>
    /// The status of the event. Use the OnStatusChanged event to be notified
    /// of changes
    /// </summary>
    public EventStatus Status
    {
        get
        {
            return this.eventStatus;
        }

        private set
        {
            this.eventStatus = value;
            if (this.StatusChanged != null)
                this.StatusChanged.Invoke(this, value);
        }
    }


    /// <summary>
    /// User-defined token to attach to this object if necessary
    /// </summary>
    public Token Token { get; set; }
    /// <summary>
    /// Function to produce a tree root
    /// </summary>
    protected Func<Token, Node> treeFactory = null;
    /// <summary>
    /// The tree is final and can't be changed
    /// </summary>
    protected Node treeRoot = null;

    /// <summary>
    /// The objects we will have in this event
    /// </summary>
    protected List<BehaviorObject> participants;
    /// <summary>
    /// Returns an enumeration of all the participants
    /// </summary>
    public IEnumerable<BehaviorObject> Participants
    {
        get
        {
            foreach (BehaviorObject obj in this.participants)
                yield return obj;
            yield break;
        }
    }

    #endregion

    #region Constructor
    /// <summary>
    /// Block off the empty constructor
    /// </summary>
    private BehaviorEvent()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Constructs a BehaviorEvent responsible for maintaining a tree
    /// </summary>
    /// <param name="root">The root node of the tree</param>
    /// <param name="priority">The event's priority</param>
    /// <param name="statusChanged">An OnStatusChanged delegate for receiving 
    /// status change events</param>
    /// <param name="involvedAgents">The agents involved</param>
    public BehaviorEvent(Func<Token, Node> bakeFunc, IEnumerable<IHasBehaviorObject> participants)
    {
        this.Token = null;
        this.treeFactory = bakeFunc;
        this.Status = EventStatus.Instantiated;

        this.priority = 0.0f;
        this.participants = new List<BehaviorObject>();
        foreach (IHasBehaviorObject participant in participants)
        {
            //Debug.Log(participant.Object);
            this.participants.Add(participant.Object);
        }

        //Debug.Log("After Constructor, there are "+this.participants.Count+" participants");
    }
    #endregion

    #region Internal Functions
    /// <summary>
    /// Starts the actual event and registers with the BehaviorManager
    /// to start receiving ticks for it
    /// </summary>
    internal virtual void StartEvent(float priority)
    {
        if(this.Status == EventStatus.Instantiated||this.Status==EventStatus.Finished)
        {
            this.priority = priority;
            if (this.treeRoot != null)
            {   //throw new ApplicationException("Starting already started tree");
                this.treeRoot = null;
            }
            this.treeRoot = this.treeFactory(this.Token);
            this.Status = EventStatus.Initializing;
            BehaviorManager.Instance.Register(this);
        }
       
    }

    /// <summary>
    /// Tells the event to stop itself, reporting success or failure
    /// </summary>
    /// <returns>true if the event is finished, false otherwise</returns>
    internal RunStatus StopEvent()
    {
        if (this.Status == EventStatus.Instantiated)
            this.Status = EventStatus.Finished;
        if (this.Status == EventStatus.Finished)
            return RunStatus.Success;

        // We do the actual termination in the behavior update
        if (this.Status == EventStatus.Initializing
            || this.Status == EventStatus.Pending
            || this.Status == EventStatus.Running)
            this.Status = EventStatus.Terminating;
        return RunStatus.Running;
    }

    /// <summary>
    /// if you want to control a behavior agent, use this function to free agent
    /// from event, but after that, you have to call stopbehavior to stop agent's own autonomy.
    /// And most importantly, you have to use an interactive behavior tree as the root of
    /// event behavior tree because nodes related to the controlled agent should change correspondingly. 
    /// With normal tree root, Do not use this Function, otherwise the event BT will mess up with agent's own BT!
    /// </summary>
    /// <param name="obj"></param>
    internal void Drop(BehaviorObject obj)
    {
        if (this.participants.Contains(obj) == true)
        {
            this.Detach(obj);
        }
    }
    #endregion

    #region Protected Functions

    /// <summary>
    /// First checks eligibility of the event, and then registers as a
    /// pending event for each involved agent. If we aren't eligible,
    /// this kills the event.
    /// </summary>
    protected virtual void Initializing()
    {
       // Debug.Log("Before initialization, there are " + this.participants.Count + " participants");
        // if one of the agents is not available, event cannot happen
        if (this.DoForAll(this.CheckEligible) == RunStatus.Failure)
        // No, how shameful -- time to commit Sudoku
        {
            Debug.Log("Initializing(): Terminating event...");
            this.Status = EventStatus.Terminating;
        }

        // If we're still clear to continue, enroll us as 
        // every participant's pending event
        if (this.Status == EventStatus.Initializing)
        {
            RunStatus status = this.DoForAll(this.Enroll);

            if (status == RunStatus.Success)
            {
                this.Status = EventStatus.Pending;
            }
            else if (status == RunStatus.Failure)
            {
                Debug.LogError("Initializing(): Failure during acquisition");
                this.Status = EventStatus.Terminating;
            }
        }

        // We're done with initialzation now. That means for each object, the
        // object's pending event points to us (but it hasn't stopped yet!).
    }

    /// <summary>
    /// Now that we're the pending event for each participant, stop all
    /// participant activity and launch when that's done
    /// </summary>
    protected virtual void Pending()
    {
        if (this.DoForAll(this.CheckEligible) == RunStatus.Failure)
            this.Status = EventStatus.Terminating;

        RunStatus result = this.DoForAll(this.Acquire);
        if (result == RunStatus.Success)
        {
            this.Launch();
        }
        else if (result == RunStatus.Failure)
        {
            Debug.LogError("Failure during launch");
            this.Status = EventStatus.Terminating;
        }

    }

    /// <summary>
    /// Handles ticking the tree, switching to detaching if the tree finishes
    /// </summary>
    private void Running()
    {
        RunStatus result = this.treeRoot.Tick();

        if (result == RunStatus.Success)
        {
            this.Status = EventStatus.Detaching;
        }
        else if (result == RunStatus.Failure)
        {
            // TODO: Better tree failure debug info. - AS
            Debug.LogError("Failure during tree execution");
            this.Status = EventStatus.Terminating;
        }
    }

    /// <summary>
    /// Handles terminating the tree, switching to detaching if the tree finishes
    /// </summary>
    private void Terminating()
    {
        RunStatus result = this.treeRoot.Terminate();

        if (result == RunStatus.Success)
        {
            this.Status = EventStatus.Detaching;
        }
        else if (result == RunStatus.Failure)
        {
            Debug.LogError("Failure during tree termination");
            this.Status = EventStatus.Detaching;
        }
    }

    /// <summary>
    /// Handles detaching from agents. If an agent terminated because of us,
    /// and we're still pending on that agent, this event will stick around long
    /// enough for that termination to finish and for the agent to restart.
    /// </summary>
    private void Detaching()
    {
        RunStatus result = this.DoForAll(this.Detach);
        if (result == RunStatus.Success)
        {
            this.Status = EventStatus.Finished;
        }
        else if (result == RunStatus.Failure)
        {
            Debug.LogError("Failure during detaching");
            this.Status = EventStatus.Finished;
        }
    }
    #endregion

    #region Helper Functions
   

    /// <summary>
    /// Gives up a participating object for use in another event
    /// </summary>
    protected virtual RunStatus Yield(BehaviorObject obj)
    {
        if (this.participants.Contains(obj) == true)
            return this.StopEvent();

        // We don't have that object anyway
        return RunStatus.Success;
    }

    /// <summary>
    /// Helper function to iterate over participants and do something,
    /// priorities output as Failure > Running > Success
    /// </summary>
    protected RunStatus DoForAll(Func<BehaviorObject, RunStatus> func)
    {
        //Debug.Log("In DoForAll, there are " + this.participants.Count + " participants");
        RunStatus result = RunStatus.Success;
        foreach (BehaviorAgent obj in this.participants)
        {
            //Debug.Log(obj);
            RunStatus status = func(obj);
            if (status == RunStatus.Failure)
                return RunStatus.Failure;
            if (result == RunStatus.Success)
                result = status;
        }
        return result;
    }

    /// <summary>
    /// Checks whether we're eligible for a given objects
    /// </summary>
    protected RunStatus CheckEligible(BehaviorObject obj)
    {
        return obj.IsElegible(this);
    }

    /// <summary>
    /// Registers this event as an object's pending event
    /// </summary>
    private RunStatus Enroll(BehaviorObject obj)
    {
        // Are we already enrolled?
        if (obj.PendingEvent == this)
            return RunStatus.Success;

        // Kill any pending event and just replace it
        RunStatus result = RunStatus.Success;
        if (obj.PendingEvent != null)
            result = obj.PendingEvent.Yield(obj);
        if (result == RunStatus.Success)
            obj.PendingEvent = this;
        return result;
    }

    /// <summary>
    /// Terminates the object's current activity and gets it ready to execute
    /// </summary>
    private RunStatus Acquire(BehaviorObject obj)
    {
        // Reality check, make sure we're still the pending event
        if (obj.PendingEvent != this)
            return RunStatus.Failure;

        // Either kill the active event or suspend the agent
        RunStatus result;
        if (obj.CurrentEvent != null)
            result = obj.CurrentEvent.Yield(obj);
        else
            result = obj.StopBehavior();

        // If the current event is finished, clear it out
        if (result == RunStatus.Success)
            obj.ClearEvent();
        return result;
    }

    /// <summary>
    /// Removes this event from the object, restoring autonomy if appropriate
    /// </summary>
    /// <returns>true if the event is successfully detached</returns>
    private RunStatus Detach(BehaviorObject obj)
    {
        // If we were the object's current event, restore autonomy (even if the
        // object has another pending event -- that event will just stop it)
        if (obj.CurrentEvent == this)
        {
            obj.FinishEvent();
            obj.StartBehavior();
        }

        // If we were a pending event, then the response depends
        if (obj.PendingEvent == this)
        {
            // Was the object terminating because of us? If so, wait until it's
            // done terminating, and then restart it
            if (obj.Status == BehaviorStatus.Terminating)
                return RunStatus.Running;

            // If the object isn't terminating (anymore), restart it if it's
            // idle and then clear the pending event
            if (obj.Status == BehaviorStatus.Idle)
                obj.StartBehavior();
            obj.PendingEvent = null;
            // Don't worry if another pending event swoops in and replaces us,
            // it'll handle the object whether its terminating, running, or idle
        }

        return RunStatus.Success;
    }

    /// <summary>
    /// Launches the event for every participant
    /// </summary>
    private void Launch()
    {
        foreach (BehaviorObject agent in this.participants)
            if (agent.PendingEvent == this)
                agent.LaunchEvent();
            else
                throw new ApplicationException(
                    this + ".Pending(): Starting a non-pending event");

        this.treeRoot.Start();
        this.Status = EventStatus.Running;
    }

    #endregion

    /// <summary>
    /// Regularly updates the event behavior
    /// </summary>
    RunStatus IBehaviorUpdate.BehaviorUpdate(float deltaTime)
    {
        switch (this.Status)
        {
            case EventStatus.Initializing:
                this.Initializing();
                break;
            case EventStatus.Pending:
                this.Pending();
                break;
            case EventStatus.Running:
                this.Running();
                break;
            case EventStatus.Terminating:
                this.Terminating();
                break;
            case EventStatus.Detaching:
                this.Detaching();
                break;
        }

        if (this.Status == EventStatus.Finished)
            return RunStatus.Success;
        return RunStatus.Running;
    }

    #region Utility Function
    public static int ComparePriority(BehaviorEvent a, BehaviorEvent b)
    {
        return Comparer<float>.Default.Compare(a.Priority, b.Priority);
    }
    #endregion
}
