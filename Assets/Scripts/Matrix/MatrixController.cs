using UnityEngine;

namespace Matrix
{
    public sealed class MatrixController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MatrixDataHandler matrixData;
        [SerializeField] private MatrixView matrixView;

        private void Awake()
        {
            matrixData.OnDataHandled += (allModelMatrices, solutionIndexes) =>
            {
                matrixView.Draw(allModelMatrices, solutionIndexes);
            };
        }
    }
}