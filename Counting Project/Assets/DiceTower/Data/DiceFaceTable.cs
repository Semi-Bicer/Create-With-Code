using UnityEngine;

[CreateAssetMenu(fileName = "DiceFaceTable", menuName = "Scriptable Objects/DiceFaceTable")]
public class DiceFaceTable : ScriptableObject
{
    [System.Serializable]public struct face
    {
        public int faceValue;
        public Vector3 faceValueDirection;
    }

    public face[] faces = new face[0]; // null safety check

    public float cockedThreshold = 0.9f;
    public bool readsDownwardFace = false;
    public int sides;

    void OnValidate()
    {
        for ( int i=0; i < faces.Length; i++ )
        {
            faces[i].faceValueDirection = faces[i].faceValueDirection.normalized;
        }
    }
}
