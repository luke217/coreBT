using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TreeSharpPlus;
using UnityEngine.Playables;

public class AdamEvent : MonoBehaviour,IHasBehaviorEvent
{

    #region ENUM
    public enum StoryID : int
    {
        mainstoryarc,
        freeAdam1
    }
    #endregion

    #region Parameters
    public BehaviorEvent Behavior{ get; set; }
    private IEnumerable<IHasBehaviorObject> participants;
    //public CrowdBehaviorEvent<AdamBehaviorTree> Crowd { get; set; }
    //private IEnumerable<AdamBehaviorTree> participants;
    public EventStatus eventstatus;

    public GameObject adam1;
    public GameObject adam2;

 
    public StoryID currentStoryID;
    public bool adam1Freed;
    #endregion

    #region Unity Functions
    // Use this for initialization
    void Start () {
        participants = findBAgent();
        Behavior = new BehaviorEvent((Token toke)=>this.BuildTreeRoot(),this.participants);

        //participants = findBAcrowd();
        //Crowd = new CrowdBehaviorEvent<AdamBehaviorTree>((AdamBehaviorTree tree, object token)=>this.BuildTreeRoot(),this.participants);

        
        currentStoryID = StoryID.mainstoryarc;
        adam1Freed = false;
    }
	
	// Update is called once per frame
	void Update () {
        eventstatus = Behavior.Status;
        //eventstatus = Crowd.Status;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Behavior.StartEvent(1f);
            //Crowd.StartEvent(1f);
        }
        if(Input.GetKeyDown(KeyCode.Q)&&eventstatus==EventStatus.Running)
        {
            adam1Freed = true;
            Behavior.Drop(adam1.GetComponent<AdamBehaviorTree>().Object);
            //Crowd.Drop(adam1.GetComponent<AdamBehaviorTree>().Object);
        }
        if(eventstatus==EventStatus.Finished)
        {
            adam1Freed = false;
            currentStoryID = StoryID.mainstoryarc;
        }
    }
    #endregion

    #region Root
    /// <summary>
    /// Has to be an interactive behavior tree if you want to control agent
    /// </summary>
    /// <returns></returns>
    protected Node BuildTreeRoot()
    {
        Node InteractiveBehaviorTreeRoot =
            new SelectorParallel(
                this.ST_Story(),
                this.ST_MonitorStoryState()
                );
                        
                           
        return InteractiveBehaviorTreeRoot;
    }
    #endregion
   
    #region SubTreeNodes

    #region Affordances
    protected Node ST_PlayGesture(string name, long duration, GameObject adam)
    {
        Val<string> Name = Val.V(() => name);
        Val<long> Duration = Val.V(() => duration);

        return adam.GetComponent<BehaviorMecanim>().ST_PlayHandGesture(Name, Duration);
    }
    #endregion

    #region StoryArc
    protected Node ST_MainStoryArc()
    {
        //Node actor1 = new DecoratorLoop(new SequenceShuffle(this.ST_GoToUpToRadius(wanderpoint1,0.5f, 3f), this.ST_GoToUpToRadius(wanderpoint2,0.5f, 5.0f), this.ST_GoToUpToRadius(wanderpoint3, 1f, 6f)));
        Node actor1 = this.ST_PlayGesture("COWBOY", 3000L, adam1);
        Node actor2 = this.ST_PlayGesture("POINTING", 3000L, adam2);
        Node mainStoryArc = new SequenceParallel(actor1, actor2);
        return mainStoryArc;
    }

    protected Node ST_freeAdam1()
    {
        Node freeAdam1 = new SequenceParallel
                    (
                           this.ST_PlayGesture("POINTING", 3000L, adam2)    

                    );
        return freeAdam1;
    }
    #endregion

    #region Story
    protected Node ST_Story()
    {
        Node Story = new Sequence(
            this.ST_SelectStory(StoryID.mainstoryarc),
            this.ST_SelectStory(StoryID.freeAdam1)
            );
        return Story;
    }
    protected Node ST_SelectStory(StoryID id)
    {

        switch (id)
        {
            case StoryID.mainstoryarc:


                Node SelectmainStory = new SelectorParallel(
                    new DecoratorInvert(new DecoratorLoop(new DecoratorInvert(new LeafAssert(() => currentStoryID != id)))),
                    this.ST_MainStoryArc());

                return SelectmainStory;

            case StoryID.freeAdam1:


                Node SelectfreeAdam1 = new SelectorParallel(
                new DecoratorInvert(new DecoratorLoop(new DecoratorInvert(new LeafAssert(() => currentStoryID != id)))),
                this.ST_freeAdam1());

                return SelectfreeAdam1;

            default:
                return null;
        }
    }
    #endregion

    #region MonitorStoryState
    protected Node ST_MonitorStoryState()
    {
        Node MonitorState = new DecoratorLoop(new Selector(
            this.ST_CheckMainStoryArc(),
            this.ST_CheckfreeAdam1Arc()
            ));
        return MonitorState;
    }
    protected Node ST_CheckMainStoryArc()
    {
        Node checkmainarc = new Sequence(
            new LeafAssert(() => !adam1Freed),
            new LeafInvoke(() => currentStoryID = StoryID.mainstoryarc)
            );
        return checkmainarc;
    }
    protected Node ST_CheckfreeAdam1Arc()
    {
        Node checkfreeAdam1arc = new Sequence(
            new LeafAssert(() => adam1Freed),
            new LeafInvoke(() => currentStoryID = StoryID.freeAdam1)
            );
        return checkfreeAdam1arc;
    }
    #endregion

    #endregion

    #region Helper Function
    private IEnumerable<IHasBehaviorObject> findBAgent()
    {
        yield return this.adam1.GetComponent<AdamBehaviorTree>();
        yield return this.adam2.GetComponent<AdamBehaviorTree>();
    }

    private IEnumerable<AdamBehaviorTree> findBAcrowd() 
    {
        yield return this.adam1.GetComponent<AdamBehaviorTree>();
        yield return this.adam2.GetComponent<AdamBehaviorTree>();
    }
    #endregion
}
