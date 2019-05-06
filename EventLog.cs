using System.Collections.Generic;

namespace OfficeClip.OpenSource.Logger
{
    public class EventLog : ILog
    {
        public bool IsEnabled
        {
            get
            {
                return false;
            }
        }

        public void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public void WriteLog(
            LogState state,
            List<string> stackList,
            string topic,
            string description,
            string user,
            string category,
            string custom)
        {
            throw new System.NotImplementedException();
        }

        public void Cleanup()
        {
            throw new System.NotImplementedException();
        }
    }
}