using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Targets;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace RopeSnake.UI.Common
{
    public class MemoryTarget : TargetWithLayout
    {
        public ObservableCollection<LogEventInfo> Logs { get; }
        private object _lockObj = new object();

        public MemoryTarget()
        {
            Logs = new ObservableCollection<LogEventInfo>();
            BindingOperations.EnableCollectionSynchronization(Logs, _lockObj);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            Logs.Add(logEvent);
        }
    }
}
