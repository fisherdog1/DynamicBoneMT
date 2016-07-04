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

            if (m_nCurFrame != 0) {
                boneMT.ApplyParticlesToTransforms(bone);
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
