namespace Matrix.Serialization
{
    [System.Serializable]
    public class MatrixSerializationData
    {
        public MatrixRowData[] columns = new MatrixRowData[RowsAmount];

        private const int RowsAmount = 4;

        public MatrixSerializationData()
        {
            for (int i = 0; i < RowsAmount; i++)
            {
                columns[i] = new MatrixRowData();
            }
        }
    }
}