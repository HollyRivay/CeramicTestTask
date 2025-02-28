using UnityEngine;

namespace Matrix
{
    public sealed class MatrixView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Material matchMaterial;
        
        public void Draw(Matrix4x4[] allModels, int[] solutionIndexes)
        {
            for (int i = 0; i < allModels.Length; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Transform cubeTransform = cube.transform;
                
                cubeTransform.SetPositionAndRotation(allModels[i].GetPosition(), allModels[i].rotation);
                cubeTransform.localScale = allModels[i].lossyScale;

                for (int j = 0; j < solutionIndexes.Length; j++)
                {
                    if (i != j)
                        continue;
                    
                    cube.GetComponent<MeshRenderer>().sharedMaterial = matchMaterial;
                    break;
                }
            }
        }
    }
}