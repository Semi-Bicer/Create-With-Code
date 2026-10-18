using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
/*
 * This is a custom Editor-only utility. Its goal is to let you rotate any die in the Scene view so a specific number
 * points up, right-click the component, and have Unity calculate, clean up, and save that face's vector directly into
 * your DiceFaceTable asset.
 * The class itself compiles in builds so a die that still carries it does not log a missing script; only the
 * recording logic is editor-only.
 */

public class DiceCalibrator : MonoBehaviour
{
    public DiceFaceTable targetTable;
    public int valueToRecord;

#if UNITY_EDITOR
    [ContextMenu("Record Face")]
    public void RecordFace()
    {
        if (targetTable == null)
        {
            Debug.LogWarning("Assign a Target Table asset first!");
            return;
        }

        Vector3 rawLocalDir = Quaternion.Inverse(transform.rotation) * Vector3.up;
        rawLocalDir = rawLocalDir.normalized;

        Vector3 snappedDir = new Vector3(
            SnapValue(rawLocalDir.x),
            SnapValue(rawLocalDir.y),
            SnapValue(rawLocalDir.z)
        );


        if (targetTable.faces == null)
        {
            targetTable.faces = new DiceFaceTable.face[0];
        }
        bool found = false;
        for (int i = 0; i < targetTable.faces.Length; i++)
        {
            if (targetTable.faces[i].faceValue == valueToRecord)
            {
                targetTable.faces[i].faceValueDirection = snappedDir;
                found = true;
                break;
            }
        }

        if (!found)
        {
            List<DiceFaceTable.face> tempList = new List<DiceFaceTable.face>(targetTable.faces);
            tempList.Add(new DiceFaceTable.face { faceValue = valueToRecord, faceValueDirection = snappedDir});
            targetTable.faces = tempList.ToArray();
        }
        
        EditorUtility.SetDirty(targetTable);
        AssetDatabase.SaveAssets();
        Debug.Log($"Recorded Face {valueToRecord} : {snappedDir}");
    }

    private float SnapValue(float val)
    {
        float tolerance = 0.01f;
        if (Math.Abs(val) < tolerance) return 0f;
        if (Math.Abs(val - 1f) < tolerance) return 1f;
        if (Math.Abs(val + 1f) < tolerance) return -1f;
        return val;
    }
#endif
}
