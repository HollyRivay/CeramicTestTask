using UnityEngine;

namespace Matrix.Serialization
{
    [System.Serializable]
    public sealed class MatrixRowData
    {
        public float[] row = new float[4];

        public void SetRow(Vector4 rowVector)
        {
            row[0] = rowVector.x;
            row[1] = rowVector.y;
            row[2] = rowVector.z;
            row[3] = rowVector.w;
        }
    }
}