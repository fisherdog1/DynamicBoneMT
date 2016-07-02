using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameController : MonoBehaviour
{
    public GameObject m_Player;

    void Start() {
        DynamicBoneMTMgr.Instance().StartThread();
    }

    void Update()
    {
        m_Player.transform.Rotate(new Vector3(0, Input.GetAxis("Horizontal") * Time.deltaTime * 200, 0));
        m_Player.transform.Translate(transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * 4);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(50, 50, 200, 20), "Press arrow key to move");
        Animation a = m_Player.GetComponentInChildren<Animation>();
        a.enabled = GUI.Toggle(new Rect(50, 70, 200, 20), a.enabled, "Play Animation");

        DynamicBone[] db = m_Player.GetComponents<DynamicBone>();
        GUI.Label(new Rect(50, 100, 200, 20), "Choose dynamic bone:");
        db[0].enabled = db[1].enabled = GUI.Toggle(new Rect(50, 120, 100, 20), db[0].enabled, "Breasts");
        db[2].enabled = GUI.Toggle(new Rect(50, 140, 100, 20), db[2].enabled, "Tail");

        DynamicBoneMTMgr.Instance().bMultiThread = GUI.Toggle(new Rect(50, 160, 100, 20), DynamicBoneMTMgr.Instance().bMultiThread, "MultiThread");
    }


#if UNITY_EDITOR
    [MenuItem("Test/GetMatrix")]
    public static void GetMatrix() {
        GameObject selectObj = Selection.activeGameObject;

        if (selectObj == null) {
            return;
        }

        Transform trans = selectObj.transform;
        Matrix4x4 matrix = trans.localToWorldMatrix;

        Vector3 right = trans.right;
        Vector3 rightMatrix = matrix.GetColumn(0);

        Vector3 forward = trans.forward;
        Vector3 forwardMatrix = matrix.GetColumn(2);

        Vector3 up = trans.up;
        Vector3 upMatrix = matrix.GetColumn(1);

    }
#endif

}

