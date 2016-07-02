using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

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

    private Dictionary<int, DynamicBoneMT> dictID2Bone = new Dictionary<int, DynamicBoneMT>();

    private List<int> lstToCalculateID = new List<int>();

    public void SetUpDynamicBone(DynamicBone bone) {
        int nID = bone.GetInstanceID();

        lock (objLock) {
            if (!dictID2Bone.ContainsKey(nID)) {
                dictID2Bone.Add(nID, new DynamicBoneMT(bone));
            }
        }
    }

    public void DeleteDynamicBone(DynamicBone bone) {
        int nID = bone.GetInstanceID();

        lock (objLock) {
            if (dictID2Bone.ContainsKey(nID)) {
                dictID2Bone.Remove(nID);
            }
        }
    }

    public void InitBoneTransform(DynamicBone bone) {
        int nID = bone.GetInstanceID();

        DynamicBoneMT boneMT = null;
        if (!dictID2Bone.TryGetValue(nID, out boneMT)) {
            Debug.LogFormat("{0} bone is not exist in DictId2Bone", bone.name);
            return;
        }

        if (IsLastFrameDone()) {

            // set current frame data to boneMT
            lock (objLock) {
                if (!lstToCalculateID.Contains(nID)) {
                    boneMT.InitTransform(bone);
                    lstToCalculateID.Add(nID);
                }
            }

            // get last frame data and apply to transform
            if (m_nCurFrame != 0) {
                for (int i = 1; i < boneMT.m_Particles.Count; ++i) {
                    DynamicBone.Particle p = bone.m_Particles[i];
                    DynamicBone.Particle p0 = bone.m_Particles[p.m_ParentIndex];

                    DynamicBoneMT.Particle pMT = boneMT.m_Particles[i];
                    DynamicBoneMT.Particle p0MT = boneMT.m_Particles[pMT.m_ParentIndex];

                    if (p0.m_Transform.childCount <= 1) {
                        Vector3 v;
                        if (pMT.m_Transform != null) {
                            v = pMT.m_InitLocalPosition;
                        }
                        else {
                            v = pMT.m_EndOffset;
                        }

                        Quaternion rot = Quaternion.FromToRotation(p0.m_Transform.TransformDirection(v), pMT.m_Position - p0MT.m_Position);

                        p0.m_Transform.rotation = rot * p0.m_Transform.rotation;
                    }

                    if (p.m_Transform != null) {
                        p.m_Transform.position = pMT.m_Position;
                    }
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
