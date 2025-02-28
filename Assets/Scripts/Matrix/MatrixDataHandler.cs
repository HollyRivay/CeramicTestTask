using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Matrix.Serialization;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Matrix
{
    public sealed class MatrixDataHandler : MonoBehaviour
    {
        public event Action<Matrix4x4[], int[]> OnDataHandled;
        
        private void Start()
        {
            Debug.Log("Calculations Is Started");
            
            Handle().Forget();
        }

        private async UniTaskVoid Handle()
        {
            string dataPath = Path.Combine(Application.dataPath, "Data");
            
            if (!Directory.Exists(dataPath))
                throw new DirectoryNotFoundException("Data directory doesn't exist");

            string modelDataPath = Path.Combine(dataPath, "model.json");
            string spaceDataPath = Path.Combine(dataPath, "space.json");
            
            if (!File.Exists(modelDataPath) || !File.Exists(spaceDataPath))
                throw new FileNotFoundException("One or more required files are missing");

            string modelText = await File.ReadAllTextAsync(modelDataPath, destroyCancellationToken);
            string spaceText = await File.ReadAllTextAsync(spaceDataPath, destroyCancellationToken);

            Matrix4x4[] modelDefaultMatrices = JsonConvert.DeserializeObject<Matrix4x4[]>(modelText);
            Matrix4x4[] spaceDefaultMatrices = JsonConvert.DeserializeObject<Matrix4x4[]>(spaceText);

            NativeArray<float4x4> modelMatrices = new(modelDefaultMatrices.Length, Allocator.TempJob);
            NativeArray<float4x4> spaceMatrices = new(spaceDefaultMatrices.Length, Allocator.TempJob);
            NativeList<float4x4> solutions = new(Allocator.TempJob);
            NativeList<int> solutionIndexes = new(Allocator.TempJob);

            for (int i = 0; i < modelDefaultMatrices.Length; i++)
                modelMatrices[i] = ConvertMatrix(modelDefaultMatrices[i]);

            for (int i = 0; i < spaceDefaultMatrices.Length; i++)
                spaceMatrices[i] = ConvertMatrix(spaceDefaultMatrices[i]);

            FindOffsetJob findOffsetJob = new()
            {
                ModelMatrices = modelMatrices,
                SpaceMatrices = spaceMatrices,
                Solutions = solutions,
                SolutionIndexes = solutionIndexes 
            };

            JobHandle jobHandle = findOffsetJob.Schedule();
            jobHandle.Complete();
            
            Debug.Log("Calculations Is Finished");
            Debug.Log("View is start working");
            
            int solutionsAmount = solutions.Length;
            
            int[] solutionIndexesArray = new int[solutionsAmount];
            for (int i = 0; i < solutionsAmount; i++)
            {
                solutionIndexesArray[i] = solutionIndexes[i];
            }
            
            OnDataHandled?.Invoke(modelDefaultMatrices, solutionIndexesArray);

            Debug.Log($"Total unique solutions: {solutionsAmount}");
            Debug.Log("Solutions:");

            foreach (float4x4 solutionMatrix in solutions)
            {
                Debug.Log(solutionMatrix);
            }

            MatrixSerializationData[] solutionsArray = new MatrixSerializationData[solutionsAmount];
            for (int i = 0; i < solutionsAmount; i++)
            {
                solutionsArray[i] = new MatrixSerializationData();

                Matrix4x4 solutionMatrix = ConvertMatrix(solutions[i]);

                for (int j = 0; j < solutionsArray[i].columns.Length; j++)
                {
                    solutionsArray[i].columns[j].SetRow(solutionMatrix.GetRow(j));
                }
            }
            string solutionsText = JsonConvert.SerializeObject(solutionsArray);
            
            string solutionsDataPath = Path.Combine(dataPath, "solutions.json");
            if (!File.Exists(solutionsDataPath))
                await File.Create(solutionsDataPath).DisposeAsync();
            
            await File.WriteAllTextAsync(solutionsDataPath, solutionsText, destroyCancellationToken);
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif

            modelMatrices.Dispose();
            spaceMatrices.Dispose();
            solutions.Dispose();
            solutionIndexes.Dispose();
        }
        
        private static float4x4 ConvertMatrix(Matrix4x4 m)
        {
            return new float4x4(
                new float4(m.m00, m.m10, m.m20, m.m30),
                new float4(m.m01, m.m11, m.m21, m.m31),
                new float4(m.m02, m.m12, m.m22, m.m32),
                new float4(m.m03, m.m13, m.m23, m.m33)
            );
        }
        
        private static Matrix4x4 ConvertMatrix(float4x4 m)
        {
            return new Matrix4x4(
                new Vector4(m.c0.x, m.c0.y, m.c0.z, m.c0.w),
                new Vector4(m.c1.x, m.c1.y, m.c1.z, m.c1.w),
                new Vector4(m.c2.x, m.c2.y, m.c2.z, m.c2.w),
                new Vector4(m.c3.x, m.c3.y, m.c3.z, m.c3.w)
            );
        }
    }
}