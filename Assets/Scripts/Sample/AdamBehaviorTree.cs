using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TreeSharpPlus;
using UnityEngine.Playables;
using System;

public class AdamBehaviorTree : MonoBehaviour,IHasBehaviorObject {

    #region Parameters
    private BehaviorAgent behaviorAgent;
    public Transform wanderpoint1;
    public Transform wanderpoint2;
    public Transform wanderpoint3;
    public Transform item;

    public GameObject adam1;

    
    /// <summary>
    /// set speed by speed parameter instead of behavior tree setting speed if true
    /// </summary>
    public bool lockSpeed = false;
    public float speed=3.5f;

    public BehaviorObject Object
    {
        get
        {
            return this.behaviorAgent;
        }
    }

    public BehaviorStatus behaviorStatus;
    #endregion

    #region Unity Functions
    public void Awake()
    {
        behaviorAgent = new BehaviorAgent(this.BuildTreeRoot());
    }
    // Use this for initialization
    void Start()
    {
        behaviorAgent.StartBehavior();
    }
    private void Update()
    {
        behaviorStatus = Object.Status;
        if (lockSpeed)
            adam1.GetComponent<SteeringController>().maxSpeed = speed;
        if(Input.GetKeyDown(KeyCode.L))
        {
            lockSpeed = !lockSpeed;
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            behaviorAgent.StopBehavior();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            behaviorAgent.StartBehavior();
        }
    }
    #endregion

    #region behavior tree

    #region Root
    /// <summary>
    /// This is what you create, using control nodes(sequence, selector, etc.) and leaf nodes
    /// and pass it to the Behavior Agent. Each time the root ticks, it will tick its children, 
    /// if any, and the ticks continue down the tree until reaching a leaf to execute. This process
    /// is implemented using Closures and Enumerators in C#.
    /// </summary>
    /// <returns></returns>
    protected Node BuildTreeRoot()
    {
        //Node actor1 = new DecoratorLoop(new SequenceShuffle(this.ST_GoToUpToRadius(wanderpoint1,0.5f, 3f), this.ST_GoToUpToRadius(wanderpoint2,0.5f, 5.0f), this.ST_GoToUpToRadius(wanderpoint3, 1f, 6f)));
        Node actor1 = new DecoratorLoop(
                                 new Sequence(
                                                this.ST_Routes(wanderpoint1,wanderpoint2, 3f,2f,6f,3f, wanderpoint3),
                                                this.ST_Grab(item)
                                             )
                                        );
        Node actor2 = new Sequence() ;
        Node mainStoryArc = new SequenceParallel(actor1, actor2);
        return mainStoryArc;
    }
    #endregion

    #region Subtree
    protected Node ST_Routes(params object[] parameters)
    {
        List<Val<Vector3>> wanderPositions= new List<Val<Vector3>>();
        List<Val<float>> speeds = new List<Val<float>>();
        foreach (object parameter in parameters)
        {
            
            Transform temp = parameter as Transform;
            if(temp!=null)
            {
                wanderPositions.Add(Val.V(() => temp.position));
            }
            else
            {
                speeds.Add(Val.V(() => (float)parameter));
            }
        }

    
        //make sure #targets is as much as #speeds
        if (speeds.Count==0)
        {
            //Debug.Log("wanderPosition has #" + wanderPositions.Count+ " speeds has #"+speeds.Count);
            for (int i=0;i<wanderPositions.Count;i++)
            {
                speeds.Add(Val.V(() => 3.5f));
            }
        }
        else if(speeds.Count<wanderPositions.Count)
        {
            //Debug.Log("wanderPosition has #" + wanderPositions.Count + " speeds has #" + speeds.Count);
            while (speeds.Count < wanderPositions.Count)
            {
                speeds.Add(speeds[speeds.Count-1]);
            }
        }
        else if(speeds.Count>wanderPositions.Count)
        {
            //Debug.Log("wanderPosition has #" + wanderPositions.Count + " speeds has #" + speeds.Count);
            while (speeds.Count > wanderPositions.Count)
            {
                speeds.Remove(speeds[speeds.Count - 1]);
            }
        }

        return adam1.GetComponent<BehaviorMecanim>().Node_GoAlongPoints(wanderPositions.ToArray(),speeds.ToArray());
    }

    protected Node ST_GoTo(Transform wanderPoint, float speed=3.5f)
    {
        Val<Vector3> wanderPosition = Val.V(() => wanderPoint.position);
        Val<float> wanderSpeed = Val.V(()=>speed);

        return adam1.GetComponent<BehaviorMecanim>().Node_GoTo(wanderPosition,wanderSpeed);
    }

    protected Node ST_GoToUpToRadius(Transform wanderPoint, float distance, float speed)
    {
        Val<Vector3> wanderPosition = Val.V(() => wanderPoint.position);
        //distance has to be adjusted according to the speed
        Val<float> dist = Val.V(()=> distance+Mathf.Clamp(speed,1f,6f)/3f);
        Val<float> wanderSpeed = Val.V(() => speed);

        return adam1.GetComponent<BehaviorMecanim>().Node_GoToUpToRadius(wanderPosition, dist, wanderSpeed);
    }
    
    protected Node ST_Grab(Transform item)
    {
        Val<Vector3> itemPosition = Val.V(() => item.position);

        return adam1.GetComponent<BehaviorMecanim>().Node_Grab(itemPosition);
    }

    #endregion

    #endregion
}
