using UnityEngine;

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class DynamicBoneMT {
    class Particle {
        public Transform m_Transform = null;

        public int m_ParentIndex = -1;
        public float m_Damping = 0;
        public float m_Elasticity = 0;
        public float m_Stiffness = 0;
        public float m_Inert = 0;
        public float m_Radius = 0;
        public float m_BoneLength = 0;

        public Vector3 m_Position = Vector3.zero;
        public Vector3 m_PrevPosition = Vector3.zero;
        public Vector3 m_EndOffset = Vector3.zero;
        public Vector3 m_InitLocalPosition = Vector3.zero;
        public Quaternion m_InitLocalRotation = Quaternion.identity;
    }

    public Transform m_Root = null;
    public float m_UpdateRate = 60.0f;
    public float m_EndLength = 0;
    public Vector3 m_EndOffset = Vector3.zero;
    public Vector3 m_Gravity = Vector3.zero;
    public Vector3 m_Force = Vector3.zero;
}

public class DynamicBoneMTMgr : Singleton<DynamicBoneMTMgr> {

    //public void AddParticles(int nID, Particle)

    private Thread thread = null;
    public void StartThread() {
        thread = new Thread(DynamicBoneThread);

        thread.Start();
    }

    private List<DynamicBoneMT> lstDynamicBone = new List<DynamicBoneMT>();

    public void DynamicBoneThread() {
        while (true) {

        }
    }

}
