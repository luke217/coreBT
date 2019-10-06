using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// This is the object responsible for ticking all the trees, overall clock generator.
/// run the update method of behavior manager.
/// </summary>
public class BehaviorUpdater : MonoBehaviour
{
    public float updateTime = 0.05f;
    protected float nextUpdate = 0.0f;
    public bool updated = false;

    private static BehaviorUpdater instance = null;

    void OnEnable()
    {
        if (instance != null)
            throw new ApplicationException("Multiple BehaviorUpdaters found");
        instance = this;
    }

    void Start()
    {
        this.nextUpdate = Time.time + this.updateTime;
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            updated = !updated;
        if(!updated)
        //if (Time.time > this.nextUpdate)
        //{
            BehaviorManager.Instance.Update(this.updateTime);
          //  this.nextUpdate += this.updateTime;
        //}
    }
}