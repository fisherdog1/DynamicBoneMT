using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DynamicBoneMT {
    /// <summary>
    /// self Transform properties
    /// </summary>
    public TransformMT transform;

    /// <summary>
    /// public field from DynamicBone
    /// <summary>
    public TransformMT m_Root = null;

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
    public List<DynamicBoneColliderMT> m_Colliders = null;  

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

    public class Particle {
        public TransformMT m_Transform = null;
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
                this.m_Transform = null;
            }
            else {
                this.m_Transform = new TransformMT(p.m_Transform);
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

    public List<Particle> m_Particles = new List<Particle>();

    public DynamicBoneMT(DynamicBone bone) {
        //component self
        this.transform = new TransformMT(bone.transform);

        //m_root
        this.m_Root = new TransformMT(bone.m_Root);

        //m_collider
        if (bone.m_Colliders != null) {
            this.m_Colliders = new List<DynamicBoneColliderMT>(bone.m_Colliders.Count);
            for (int i = 0; i < bone.m_Colliders.Count; ++i) {
                this.m_Colliders.Add(new DynamicBoneColliderMT(bone.m_Colliders[i]));
            }
        }
        
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
        this.transform.InitTransform(trans);

        // m_root
        this.m_Root.InitTransform(bone.m_Root);

        // collider
        if (bone.m_Colliders != null) {
            for (int i = 0; i < m_Colliders.Count; ++i) {
                m_Colliders[i].InitColliderTransform(bone.m_Colliders[i]);
            }
        }

        for (int i = 0; i < m_Particles.Count; ++i) {
            trans = bone.m_Particles[i].m_Transform;
            if (trans != null && m_Particles[i] != null) {
                m_Particles[i].m_Transform.InitTransform(trans);
            }
        }
    }

    public void UpdateDynamicBones(float t) {
        //if (m_Root == null)
        //    return;

        m_ObjectScale = Mathf.Abs(transform.lossyScale.x);
        m_ObjectMove = transform.position - m_ObjectPrevPosition;
        m_ObjectPrevPosition = transform.position;

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
        Vector3 rf = m_Root.TransformDirection(m_LocalGravity);
        //Vector3 rf = m_rootLocal2World * m_LocalGravity;
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
                p.m_Position = p.m_Transform.position;
            }
        }
    }

    void UpdateParticles2() {
        Plane movePlane = new Plane();

        for (int i = 1; i < m_Particles.Count; ++i) {
            Particle p = m_Particles[i];
            Particle p0 = m_Particles[p.m_ParentIndex];

            float restLen;
            if (p.m_Transform != null)
                restLen = (p0.m_Transform.position - p.m_Transform.position).magnitude;
            else
                restLen = p0.m_Transform.localToWorldMatrix.MultiplyVector(p.m_EndOffset).magnitude;

            // keep shape
            float stiffness = Mathf.Lerp(1.0f, p.m_Stiffness, m_Weight);
            if (stiffness > 0 || p.m_Elasticity > 0) {
                Matrix4x4 m0 = p0.m_Transform.localToWorldMatrix;
                m0.SetColumn(3, p0.m_Position);
                Vector3 restPos;
                if (p.m_Transform != null)
                    restPos = m0.MultiplyPoint3x4(p.m_Transform.localPosition);
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


            ///////////////////////////////////////////////////////////////////////
            // todo 
            ///////////////////////////////////////////////////////////////////////
            // collide  
            
            if (m_Colliders != null) {
                float particleRadius = p.m_Radius * m_ObjectScale;
                for (int j = 0; j < m_Colliders.Count; ++j) {
                    DynamicBoneColliderMT c = m_Colliders[j];
                    if (c != null && c.enabled)
                        c.Collide(ref p.m_Position, particleRadius);
                }
            }
            

            // freeze axis, project to plane 
            if (m_FreezeAxis != FreezeAxis.None) {
                switch (m_FreezeAxis) {
                    case FreezeAxis.X:
                        movePlane.SetNormalAndPosition(p0.m_Transform.right, p0.m_Position);
                        break;
                    case FreezeAxis.Y:
                        movePlane.SetNormalAndPosition(p0.m_Transform.up, p0.m_Position);
                        break;
                    case FreezeAxis.Z:
                        movePlane.SetNormalAndPosition(p0.m_Transform.forward, p0.m_Position);
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
                if (p.m_Transform != null)
                    restLen = (p0.m_Transform.position - p.m_Transform.position).magnitude;
                else
                    restLen = p0.m_Transform.localToWorldMatrix.MultiplyVector(p.m_EndOffset).magnitude;

                // keep shape
                float stiffness = Mathf.Lerp(1.0f, p.m_Stiffness, m_Weight);
                if (stiffness > 0) {
                    Matrix4x4 m0 = p0.m_Transform.localToWorldMatrix;
                    m0.SetColumn(3, p0.m_Position);
                    Vector3 restPos;
                    if (p.m_Transform != null)
                        restPos = m0.MultiplyPoint3x4(p.m_Transform.localPosition);
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
                p.m_Position = p.m_Transform.position;
            }
        }
    }
}