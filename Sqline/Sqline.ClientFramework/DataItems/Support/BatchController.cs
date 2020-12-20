// Authors="Daniel Jonas Møller, Anders Eggers-Krag" License="New BSD License http://sqline.codeplex.com/license"
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Sqline.ClientFramework.ProviderModel;

namespace Sqline.ClientFramework {
    public class BatchController {

        public SortedDictionary<string, OperationBatch> Batches = new SortedDictionary<string, OperationBatch>();

        public void Initialize<T>(int batchSize) where T : BaseDataItem {
            var type = typeof(T).Name;
            if (!Batches.TryGetValue(type, out var _)) {
                Batches.Add(type, new OperationBatch<T>(batchSize));
            }
        }

        public void Add<T>(T operation) where T : BaseDataItem {
            var type = typeof(T).Name;
            if (!Batches.TryGetValue(type, out var operationlist)) {
                Batches.Add(type, new OperationBatch<T>());
            }

            ((OperationBatch<T>)operationlist).Add(operation);
        
        }

        public void Execute() {
            foreach (var item in Batches) {
                item.Value.Execute();
            }
        }
    }
}