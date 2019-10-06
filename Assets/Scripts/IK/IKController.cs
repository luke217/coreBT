using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour {
    #region Inspector
    public Vector3 m_TargetObj;
    #endregion

    private Animator m_Anim;

    private float m_MixWeight = 0.0f;

    //parameter provided for the Behaviour Mechanim script
    [HideInInspector]
    public float TargetMixWeight = 0.0f;


    void Awake()
    {
        m_Anim = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update () {
        if (TargetMixWeight != m_MixWeight)
        {
            if (Mathf.Abs(TargetMixWeight - m_MixWeight) < 0.0001f)
            {
                m_MixWeight = TargetMixWeight;
            }
            else
            {
                m_MixWeight = Mathf.Lerp(m_MixWeight, TargetMixWeight, 0.05f);
            }
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        //Hand IK
        m_Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, m_MixWeight);
        

        m_Anim.SetIKPosition(AvatarIKGoal.RightHand, m_TargetObj);

    }
}
