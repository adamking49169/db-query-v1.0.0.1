using Deedle;
using Models;  // Add this to reference your DataRow class
using System;
using System.Collections.Generic;

namespace db_query_v1._9_0._0_1.DataProcessing
{
    public class DataProcessor
    {
        public List<DataRow> ProcessCsvData(string filePath)
        {
            var df = Frame.ReadCsv(filePath);
            var processedData = new List<DataRow>();

            int rowCount = 0;
            foreach (var row in df.Rows.Values)
            {
                if (rowCount >= 5) break; // Process first 5 rows
                processedData.Add(new DataRow
                {
                    ColumnName = "Sample",
                    Value = row.ToString() // Ensure this is the value you want to store
                });
                rowCount++;
            }

            return processedData;
        }
    }
}
