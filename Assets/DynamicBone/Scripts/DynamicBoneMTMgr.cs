using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class DynamicBoneMT {
    /// <summary>
    /// self Transform properties
    /// </summary>
    public Vector3 m_selfPosition = Vector3.zero;
    public Vector3 m_selfScale = Vector3.zero;
    public Quaternion m_selfRotation = Quaternion.identity;
    
    /// <summary>
    /// public field from DynamicBone
    /// <summary>
    public Matrix4x4 m_rootLocal2World = Matrix4x4.identity;
    //public Transform m_Root = null;  discard
    
    public float m_UpdateRate = 60.0f;

    //[Range(0, 1)]
    public float m_Damping = 0.1f;
    //public AnimationCurve m_DampingDistrib = null;  discard

    //[Range(0, 1)]
    public float m_Elasticity = 0.1f;
    //public AnimationCurve m_ElasticityDistrib = null;  discard

    //[Range(0, 1)]
    public float m_Stiffness = 0.1f;
    //public AnimationCurve m_StiffnessDistrib = null;  discard

    //[Range(0, 1)]
    public float m_Inert = 0;
    //public AnimationCurve m_InertDistrib = null;  discard

    public float m_Radius = 0;
    //public AnimationCurve m_RadiusDistrib = null;  discard

    public float m_EndLength = 0;
    public Vector3 m_EndOffset = Vector3.zero;
    public Vector3 m_Gravity = Vector3.zero;
    
    public Vector3 m_Force = Vector3.zero;

    /// <summary>
    /// discard
    /// </summary>
    //public List<DynamicBoneCollider> m_Colliders = null;  
    
    /// <summary>
    /// discard
    /// </summary>
    //public List<Transform> m_Exclusions = null;

    public enum FreezeAxis {
        None, X, Y, Z
    }
    public FreezeAxis m_FreezeAxis = FreezeAxis.None;
    public bool m_DistantDisable = false;

    /// <summary>
    /// discard
    /// </summary>
    //public Transform m_ReferenceObject = null;
    public float m_DistanceToObject = 20;

    /// <summary>
    /// private field from DynamicBone
    /// </summary>
    Vector3 m_LocalGravity = Vector3.zero;
    Vector3 m_ObjectMove = Vector3.zero;
    Vector3 m_ObjectPrevPosition = Vector3.zero;
    float m_BoneTotalLength = 0;
    float m_ObjectScale = 1.0f;
    float m_Time = 0;
    float m_Weight = 1.0f;
    bool m_DistantDisabled = false;

    class Particle {
        /// <summary>
        /// discard
        /// </summary>
        //public Transform m_Transform = null;

        public bool m_bTransIsNull = false;
        public Vector3 m_transPosition = Vector3.zero;
        public Vector3 m_transScale = Vector3.zero;
        public Quaternion m_transRotation = Quaternion.identity;

        public Matrix4x4 m_transLocal2World = Matrix4x4.identity;

        public Vector3 m_transLocalPosition = Vector3.zero;
        public Quaternion m_transLocalRotation = Quaternion.identity;

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

        public Particle(DynamicBone.Particle p) {
            if (p.m_Transform == null) {
                this.m_bTransIsNull = true;
            }
            else {
                this.m_bTransIsNull = false;
            }

            this.m_ParentIndex = p.m_ParentIndex;
            this.m_Damping = p.m_Damping;
            this.m_Elasticity = p.m_Elasticity;
            this.m_Stiffness = p.m_Stiffness;
            this.m_Inert = p.m_Inert;
            this.m_Radius = p.m_Radius;
            this.m_BoneLength = p.m_BoneLength;

            this.m_Position = p.m_Position;
            this.m_PrevPosition = p.m_PrevPosition;
            this.m_EndOffset = p.m_EndOffset;
            this.m_InitLocalPosition = p.m_InitLocalPosition;
            this.m_InitLocalRotation = p.m_InitLocalRotation;
        }
    }

    private List<Particle> m_Particles = new List<Particle>();

    public DynamicBoneMT(DynamicBone bone) {
        this.m_selfPosition = bone.transform.position;
        this.m_selfScale = bone.transform.lossyScale;
        this.m_selfRotation = bone.transform.rotation;

        this.m_UpdateRate = bone.m_UpdateRate;
        this.m_Elasticity = bone.m_Elasticity;
        this.m_Stiffness = bone.m_Stiffness;
        this.m_Inert = bone.m_Inert;
        this.m_Radius = bone.m_Radius;
        this.m_EndLength = bone.m_EndLength;
        this.m_EndOffset = bone.m_EndOffset;
        this.m_Gravity = bone.m_Gravity;
        this.m_Force = bone.m_Force;
        this.m_FreezeAxis = (FreezeAxis)((int)bone.m_FreezeAxis);
        this.m_DistantDisable = bone.m_DistantDisable;
        this.m_DistanceToObject = bone.m_DistanceToObject;

        for (int i = 0; i < bone.m_Particles.Count; ++i) {
            Particle p = new Particle(bone.m_Particles[i]);
            m_Particles.Add(p);
        }
    }

    public void InitTransform(DynamicBone bone) {
        Transform trans = bone.transform;
        
        // component self
        this.m_selfPosition = trans.position;
        this.m_selfScale = trans.lossyScale;
        this.m_selfRotation = trans.rotation;

        // m_root
        this.m_rootLocal2World = bone.m_Root.worldToLocalMatrix;

        for (int i = 0; i < m_Particles.Count; ++i) {
            trans = bone.m_Particles[i].m_Transform;
            if (trans != null) {
                m_Particles[i].m_transPosition = trans.position;
                m_Particles[i].m_transScale = trans.lossyScale;
                m_Particles[i].m_transRotation = trans.rotation;

                m_Particles[i].m_transLocal2World = trans.localToWorldMatrix;

                m_Particles[i].m_transLocalPosition = trans.localPosition;
                m_Particles[i].m_transLocalRotation = trans.localRotation;
            }
        }
    }

    public void UpdateDynamicBones(float t) {
        //if (m_Root == null)
        //    return;

        m_ObjectScale = Mathf.Abs(m_selfScale.x);
        m_ObjectMove = m_selfPosition - m_ObjectPrevPosition;
        m_ObjectPrevPosition = m_selfPosition;

        int loop = 1;
        if (m_UpdateRate > 0) {
            float dt = 1.0f / m_UpdateRate;
            m_Time += t;
            loop = 0;

            while (m_Time >= dt) {
                m_Time -= dt;
                if (++loop >= 3) {
                    m_Time = 0;
                    break;
                }
            }
        }

        if (loop > 0) {
            for (int i = 0; i < loop; ++i) {
                UpdateParticles1();
                UpdateParticles2();
                m_ObjectMove = Vector3.zero;
            }
        }
        else {
            SkipUpdateParticles();
        }
    }

    void UpdateParticles1() {
        Vector3 force = m_Gravity;
        Vector3 fdir = m_Gravity.normalized;
        //Vector3 rf = m_Root.TransformDirection(m_LocalGravity);
        Vector3 rf = m_rootLocal2World * m_LocalGravity;
        Vector3 pf = fdir * Mathf.Max(Vector3.Dot(rf, fdir), 0);	// project current gravity to rest gravity
        force -= pf;	// remove projected gravity
        force = (force + m_Force) * m_ObjectScale;

        for (int i = 0; i < m_Particles.Count; ++i) {
            Particle p = m_Particles[i];
            if (p.m_ParentIndex >= 0) {
                // verlet integration
                Vector3 v = p.m_Position - p.m_PrevPosition;
                Vector3 rmove = m_ObjectMove * p.m_Inert;
                p.m_PrevPosition = p.m_Position + rmove;
                p.m_Position += v * (1 - p.m_Damping) + force + rmove;
            }
            else {
                p.m_PrevPosition = p.m_Position;
                p.m_Position = p.m_transPosition;
            }
        }
    }

    void UpdateParticles2() {
        Plane movePlane = new Plane();

        for (int i = 1; i < m_Particles.Count; ++i) {
            Particle p = m_Particles[i];
            Particle p0 = m_Particles[p.m_ParentIndex];

            float restLen;
            if (!p.m_bTransIsNull)
                restLen = (p0.m_transPosition - p.m_transPosition).magnitude;
            else
                restLen = p0.m_transLocal2World.MultiplyVector(p.m_EndOffset).magnitude;

            // keep shape
            float stiffness = Mathf.Lerp(1.0f, p.m_Stiffness, m_Weight);
            if (stiffness > 0 || p.m_Elasticity > 0) {
                Matrix4x4 m0 = p0.m_transLocal2World;
                m0.SetColumn(3, p0.m_Position);
                Vector3 restPos;
                if (!p.m_bTransIsNull)
                    restPos = m0.MultiplyPoint3x4(p.m_transLocalPosition);
                else
                    restPos = m0.MultiplyPoint3x4(p.m_EndOffset);

                Vector3 d = restPos - p.m_Position;
                p.m_Position += d * p.m_Elasticity;

                if (stiffness > 0) {
                    d = restPos - p.m_Position;
                    float len = d.magnitude;
                    float maxlen = restLen * (1 - stiffness) * 2;
                    if (len > maxlen)
                        p.m_Position += d * ((len - maxlen) / len);
                }
            }
            
            // collide
            /*
            if (m_Colliders != null) {
                float particleRadius = p.m_Radius * m_ObjectScale;
                for (int j = 0; j < m_Colliders.Count; ++j) {
                    DynamicBoneCollider c = m_Colliders[j];
                    if (c != null && c.enabled)
                        c.Collide(ref p.m_Position, particleRadius);
                }
            }
            */

            // freeze axis, project to plane 
            if (m_FreezeAxis != FreezeAxis.None) {
                switch (m_FreezeAxis) {
                    case FreezeAxis.X:
                        //movePlane.SetNormalAndPosition(p0.m_Transform.right, p0.m_Position);
                        movePlane.SetNormalAndPosition(p0.m_transLocal2World.GetColumn(0), p0.m_Position);
                        break;
                    case FreezeAxis.Y:
                        //movePlane.SetNormalAndPosition(p0.m_Transform.up, p0.m_Position);
                        movePlane.SetNormalAndPosition(p0.m_transLocal2World.GetColumn(1), p0.m_Position);
                        break;
                    case FreezeAxis.Z:
                        //movePlane.SetNormalAndPosition(p0.m_Transform.forward, p0.m_Position);
                        movePlane.SetNormalAndPosition(p0.m_transLocal2World.GetColumn(2), p0.m_Position);
                        break;
                }
                p.m_Position -= movePlane.normal * movePlane.GetDistanceToPoint(p.m_Position);
            }

            // keep length
            Vector3 dd = p0.m_Position - p.m_Position;
            float leng = dd.magnitude;
            if (leng > 0)
                p.m_Position += dd * ((leng - restLen) / leng);
        }
    }

    void SkipUpdateParticles() {
        for (int i = 0; i < m_Particles.Count; ++i) {
            Particle p = m_Particles[i];
            if (p.m_ParentIndex >= 0) {
                p.m_PrevPosition += m_ObjectMove;
                p.m_Position += m_ObjectMove;

                Particle p0 = m_Particles[p.m_ParentIndex];

                float restLen;
                if (!p.m_bTransIsNull)
                    restLen = (p0.m_transPosition - p.m_transPosition).magnitude;
                else
                    restLen = p0.m_transLocal2World.MultiplyVector(p.m_EndOffset).magnitude;

                // keep shape
                float stiffness = Mathf.Lerp(1.0f, p.m_Stiffness, m_Weight);
                if (stiffness > 0) {
                    Matrix4x4 m0 = p0.m_transLocal2World;
                    m0.SetColumn(3, p0.m_Position);
                    Vector3 restPos;
                    if (!p.m_bTransIsNull)
                        restPos = m0.MultiplyPoint3x4(p.m_transLocalPosition);
                    else
                        restPos = m0.MultiplyPoint3x4(p.m_EndOffset);

                    Vector3 d = restPos - p.m_Position;
                    float len = d.magnitude;
                    float maxlen = restLen * (1 - stiffness) * 2;
                    if (len > maxlen)
                        p.m_Position += d * ((len - maxlen) / len);
                }

                // keep length
                Vector3 dd = p0.m_Position - p.m_Position;
                float leng = dd.magnitude;
                if (leng > 0)
                    p.m_Position += dd * ((leng - restLen) / leng);
            }
            else {
                p.m_PrevPosition = p.m_Position;
                p.m_Position = p.m_transPosition;
            }
        }
    }
}

public class DynamicBoneMTMgr : Singleton<DynamicBoneMTMgr> {

    //public void AddParticles(int nID, Particle)
    public bool bMultiThread = true;
    private Thread thread = null;
    private EventWaitHandle hCalculateEvent = new AutoResetEvent(false);
    private System.Object objLock = new System.Object();


    private float fDeltaTime = 0.0f;
    public void StartThread() {
        thread = new Thread(DynamicBoneThread);
        thread.Start();
    }

    //private List<DynamicBoneMT> lstDynamicBone = new List<DynamicBoneMT>();
    private Dictionary<int, DynamicBoneMT> dictID2Bone = new Dictionary<int, DynamicBoneMT>();

    private List<int> lstToCalculateID = new List<int>();

    public void SetUpDynamicBone(DynamicBone bone) {
        int nID = bone.GetInstanceID();

        if (dictID2Bone.ContainsKey(nID)) {
            return;
        }

        dictID2Bone.Add(nID, new DynamicBoneMT(bone));
    }

    public void InitBoneTransform(DynamicBone bone) {
        int nID = bone.GetInstanceID();

        DynamicBoneMT boneMT = null;
        if (!dictID2Bone.TryGetValue(nID, out boneMT)) {
            Debug.LogFormat("{0} bone is not exist in DictId2Bone", bone.name);
            return;
        }

        if (IsLastFrameDone()) {
            lock (objLock) {
                if (!lstToCalculateID.Contains(nID)) {
                    boneMT.InitTransform(bone);
                    lstToCalculateID.Add(nID);
                }
            }

            //通知线程开始计算
            if (lstToCalculateID.Count == dictID2Bone.Count) {
                hCalculateEvent.Set();
            }
        }
    }

    public void Update(float fTime) {
        fDeltaTime = fTime;
    }

    public void LateUpdate() {

    }

    private int m_nCurFrame = 0;
    private int m_nLastFrame = 0;

    public bool IsLastFrameDone() {
        return m_nCurFrame == m_nLastFrame;
    }


    public void DynamicBoneThread() {
        while (true) {
            DynamicBoneMT boneMT = null;

            //wait event
            bool bRet = hCalculateEvent.WaitOne();
            ++m_nCurFrame;

            if (bRet) {
                for (int i = 0; i < lstToCalculateID.Count; ++i) {
                    int nID = lstToCalculateID[i];

                    if (dictID2Bone.TryGetValue(nID, out boneMT)) {
                        boneMT.UpdateDynamicBones(fDeltaTime);
                    }
                }

                lock (objLock) {
                    lstToCalculateID.Clear();
                }
            }
            else {
                
            }

            ++m_nLastFrame;
        }
    }

}
