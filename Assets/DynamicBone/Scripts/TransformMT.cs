using UnityEngine;
using System.Collections;

public class TransformMT {
    public Vector3 localPosition;
    public Vector3 localScale;
    public Quaternion localRotation;

    public Vector3 position;
    public Vector3 lossyScale;
    public Quaternion rotation;

    public Matrix4x4 localToWorldMatrix;

    public Vector3 right;
    public Vector3 up;
    public Vector3 forward;
    public int childCount;

    // discard default construction
    private TransformMT() {

    }

    public TransformMT(Transform trans) {
        InitTransform(trans);
    }

    public void InitTransform(Transform trans) {
        if (trans == null) {
            return;
        }
        this.localPosition = trans.localPosition;
        this.localScale = trans.localScale;
        this.localRotation = trans.localRotation;

        this.position = trans.position;
        this.lossyScale = trans.lossyScale;
        this.rotation = trans.rotation;

        this.localToWorldMatrix = trans.localToWorldMatrix;

        this.right = trans.right;
        this.up = trans.up;
        this.forward = trans.forward;

        this.childCount = trans.childCount;
    }

    public Vector3 TransformDirection(Vector3 direction) {
        return localToWorldMatrix * direction;
    }

    public Vector3 TransformPoint(Vector3 v) {
        return localToWorldMatrix.MultiplyPoint(v);
    }
}
