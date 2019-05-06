using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace OfficeClip.OpenSource.Logger
{
    class Program
    {
        static void Main(string[] args)
        {
            //string file = GetLogFilePath();
            Log log = new Log(GetLogConfig())
            {
                DefaultCategory = "Category-10",
                DefaultUser = "email1@email.com"
            };
            log.WriteInfo("Info Title", "Build succeeded.");
            log.WriteInfo("Info Title", "warning 2136");
            log.WriteInfo("Info Title", "error: 2345");
            log.WriteError("Error Title", "Error Description");
            log.WriteWarning("Warning Title", "Warning Description");
            log.WriteDebug("Debug Title", "Debug Description");
            log.WriteFatalError("Fatal Title", "Fatal Description");
            log.WriteInfo("DataSet Sample", Formatter.ToPrint(GetDataSet()));
            log.WriteInfo("Hashtable Sample", Formatter.ToPrint(GetHashTable()));
            log.WriteError("Error Title", "Error Description");
            log.WriteWarning("Warning Title", "Warning Description");
            log.WriteDebug("Debug Title", "Debug Description");
            log.WriteFatalError("Fatal Title", "Fatal Description");
            log.WriteInfo("Info Title", "Info Description");
            log.WriteError("Error Title", "Error Description");
            log.WriteWarning("Warning Title", "Warning Description");
            log.WriteDebug("Debug Title", "Debug Description");
            log.WriteFatalError("Fatal Title", "Fatal Description");
            log.WriteInfo("Info Title", "Info Description");

            log.DefaultCategory = "Category-2";
            log.DefaultUser = "email2@email.com";

            log.WriteError("Error Title", "Error Description");
            log.WriteWarning("Warning Title", "Warning Description");
            log.WriteDebug("Debug Title", "Debug Description");
            log.WriteFatalError("Fatal Title", "Fatal Description");
            log.WriteInfo("Info Title", "Info Description");
            log.WriteError("Error Title", "Error Description");
            log.WriteWarning("Warning Title", "Warning Description");
            log.WriteDebug("Debug Title", "Debug Description");
            log.WriteFatalError("Fatal Title", "Fatal Description");
            log.WriteInfo("Info Title", "Info Description");
            log.WriteError("Error Title", "Error Description");
            log.WriteWarning("Warning Title", "Warning Description");
            log.WriteDebug("Debug Title", "Debug Description");
            log.WriteFatalError("Fatal Title", "Fatal Description");
            log.WriteInfo("Info Title", "Info Description");
        }

        private static LogConfig GetLogConfig()
        {
            string path = LogConstants.GetSetupDir() + @"\logger.xml";
            return LogConstants.DeserializeLogConfig(path);
        }

        private static DataSet GetDataSet()
        {
            DataSet dataSet = new DataSet();
            // Here we create a DataTable with four columns.
            DataTable table = new DataTable();
            table.Columns.Add("Dosage", typeof(int));
            table.Columns.Add("Drug", typeof(string));
            table.Columns.Add("Patient", typeof(string));
            table.Columns.Add("Date", typeof(DateTime));

            // Here we add five DataRows.
            table.Rows.Add(25, "Indocin", "David", DateTime.Now);
            table.Rows.Add(50, "Enebrel", "Sam", DateTime.Now);
            table.Rows.Add(10, "Hydralazine", "Christoff", DateTime.Now);
            table.Rows.Add(21, "Combivent", "Janet", DateTime.Now);
            table.Rows.Add(100, "Dilantin", "Melanie", DateTime.Now);
            dataSet.Tables.Add(table);
            table = new DataTable();
            dataSet.Tables.Add(table);
            return dataSet;
        }

        private static Hashtable GetHashTable()
        {
            Hashtable hashtable = new Hashtable
                                  {
                                      [1] = "One",
                                      [2] = "Two",
                                      [13] = "Thirteen"
                                  };
            return hashtable;
        }
    }
}
