// Authors="Daniel Jonas Møller, Anders Eggers-Krag" License="New BSD License http://sqline.codeplex.com/license"
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sqline.ClientFramework.ProviderModel;

namespace Sqline.ClientFramework {
    public class OperationBatch<T> where T : BaseDataItem {
        public List<T> FOperations = new List<T>();
        public List<T> FCurrentBatch = new List<T>();
        private int FBatchSize = 100;
        public string LastSql { get; set; }

        public OperationBatch() {
        }

        public OperationBatch(int batchSize) {
            FBatchSize = batchSize;
        }

        public void Add(T operation) {
            lock (FOperations)
                FOperations.Add(operation);
        }

        private bool IsAllInserts() {
            if (FCurrentBatch.Count == 0) {
                return false;
            }
            String OName = FCurrentBatch[0].GetType().FullName;
            foreach (T OOperation in FCurrentBatch) {
                if (OOperation.GetType().FullName != OName) {
                    throw new Exception("All operations added to OperationBatch<T> must be of the same type: " + OOperation.GetType().FullName + " != " + OName);
                }
                if (!(OOperation is InsertDataItem)) {
                    return false;
                }
            }
            return true;
        }

        public int Execute() {
            var result = 0;
            while (FOperations.Count > 0) {
                var amount = Math.Min(FBatchSize, FOperations.Count);
                FCurrentBatch = FOperations.Take(amount).ToList();
                FOperations.RemoveRange(0, amount);
                if (FCurrentBatch.Count == 0) return 0;
                using (IDbConnection OConnection = Provider.Current.GetConnection(FCurrentBatch[0].GetSqlineConfig().ConnectionString)) {
                    OConnection.Open();
                    result += Execute(OConnection, null);
                }
            }
            return result;
        }

        private int Execute(IDbConnection connection, IDbTransaction transaction) {
            using (IDbCommand OCommand = connection.CreateCommand()) {
                OCommand.Transaction = transaction;

                int OParameterIndex = 0;
                foreach (T OOperation in FCurrentBatch) {
                    OOperation.SetParameterIndex(OParameterIndex);
                    OOperation.PreExecute();
                    OParameterIndex = OOperation.GetParameterIndex();
                    foreach (IBaseParam OParam in OOperation.GetParameters()) {
                        OParam.AddParameter(OCommand);
                    }
                }
                String OSql = LastSql = IsAllInserts() ? PrepareAllInsertStatement() : PrepareAppendedStatement();

                OCommand.CommandText = OSql;
                int OResult = OCommand.ExecuteNonQuery();

                foreach (T OOperation in FCurrentBatch) {
                    OOperation.PostExecute(-1);
                }
                return OResult;
            }
        }

        private string PrepareAllInsertStatement() {
            InsertDataItem OFirstItem = FCurrentBatch[0] as InsertDataItem;
            StringBuilder OSql = new StringBuilder();
            OSql.Append("INSERT INTO ");
            OSql.Append(Provider.Current.GetSafeTableName(OFirstItem.GetSchemaName(), OFirstItem.GetTableName()));
            OSql.Append(" (");
            OSql.Append(OFirstItem.GetSqlColumns());
            OSql.Append(") VALUES ");
            for (int i = 0; i < FCurrentBatch.Count; i++) {
                InsertDataItem OInsertItem = FCurrentBatch[i] as InsertDataItem;
                if (i > 0) {
                    OSql.Append(",\r\n");
                }
                OSql.Append("(");
                OSql.Append(OInsertItem.GetSqlValues());
                OSql.Append(")");
            }
            return OSql.ToString();
        }

        private string PrepareAppendedStatement() {
            StringBuilder OSql = new StringBuilder();
            foreach (BaseDataItem OItem in FCurrentBatch) {
                OSql.Append(OItem.PrepareStatement());
                OSql.Append(";\r\n");
            }
            return OSql.ToString();
        }
    }
}