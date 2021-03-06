﻿using System.Collections.Generic;

namespace OfficeClip.OpenSource.Logger
{
    interface ILog
    {
        bool IsEnabled
        {
            get;
        }
        void Initialize();
        void WriteLog(
            LogState state,
            List<string> stackList,
            string topic,
            string description,
            string user,
            string category,
            string custom);
        void Cleanup();
    }
}
