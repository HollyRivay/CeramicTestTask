using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Matrix
{
    [BurstCompile]
    public struct FindOffsetJob : IJob
    {
        [ReadOnly] public NativeArray<float4x4> ModelMatrices;
        [ReadOnly] public NativeArray<float4x4> SpaceMatrices;
        
        public NativeList<float4x4> Solutions;
        public NativeList<int> SolutionIndexes;

        private const float MaxDelta = 0.0001f;
        
        public void Execute()
        {
            for (int i = 0; i < ModelMatrices.Length; i++)
            {
                float4x4 modelMatrix = ModelMatrices[i];
                
                if (math.abs(math.determinant(modelMatrix)) < MaxDelta)
                    continue;

                float4x4 modelInvertedMatrix = math.inverse(modelMatrix);

                for (int j = 0; j < SpaceMatrices.Length; j++)
                {
                    float4x4 offset = math.mul(SpaceMatrices[j], modelInvertedMatrix);

                    bool isValid = true;
                    for (int k = 0; k < ModelMatrices.Length; k++)
                    {
                        float4x4 candidate = math.mul(offset, ModelMatrices[k]);
                        
                        if (!CandidateIsInSpace(candidate))
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid && !ContainsDuplicate(offset))
                    {
                        Solutions.Add(offset);
                        SolutionIndexes.Add(i);
                    }
                }
            }
        }
        
        private bool CandidateIsInSpace(float4x4 candidate)
        {
            for (int i = 0; i < SpaceMatrices.Length; i++)
            {
                if (MatrixApproximatelyEqual(candidate, SpaceMatrices[i]))
                    return true;
            }
            return false;
        }

        private bool ContainsDuplicate(float4x4 newMatrix)
        {
            for (int i = 0; i < Solutions.Length; i++)
            {
                if (MatrixApproximatelyEqual(Solutions[i], newMatrix))
                    return true;
            }
            
            return false;
        }

        private static bool MatrixApproximatelyEqual(float4x4 a, float4x4 b)
        {
            return math.all(math.abs(a.c0 - b.c0) < MaxDelta) &&
                   math.all(math.abs(a.c1 - b.c1) < MaxDelta) &&
                   math.all(math.abs(a.c2 - b.c2) < MaxDelta) &&
                   math.all(math.abs(a.c3 - b.c3) < MaxDelta);
        }
    }
}