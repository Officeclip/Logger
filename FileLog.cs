using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;

namespace OfficeClip.OpenSource.Logger
{
    public class FileLog : ILog
    {
        public string FilePath { get; private set; }

        private bool isFileLogValid;

        private readonly LogConfig logConfig;

        private int logCount;

        private string DefaultLogFolder
        {
            get
            {
                return $"{LogConstants.GetSetupDir()}\\Log";
            }
        }

        public string LogFolder
        {
            get
            {
                return logConfig.FileLogSource.Folder;
            }
            set
            {
                logConfig.FileLogSource.Folder = value;
            }
        }

        #region ILog

        public bool IsEnabled
        {
            get
            {
                return logConfig.FileLogSource.Supress == 0;
            }
        }

        public void Initialize()
        {
            isFileLogValid = true;
            logCount = 0;
            try
            {
                CreateLogFolderIfNotFound();
                CreateIndexHtmlFile();
                CreateIndexFile();
                CreateLogFile(false);
                Cleanup();
                ExtractEmbeddedResourceFile("devd.exe", true);
                ExtractEmbeddedResourceFile("ViewLog.cmd", true);

            }
            catch (Exception ex)
            {
                isFileLogValid = false;
            }
        }

        private string ConvertStackTrace(List<string> lines)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str in lines)
            {
                stringBuilder.AppendFormat(
                                           "<stl>{0}</stl>",
                                           SecurityElement.Escape(str.Trim()));
            }
            return stringBuilder.ToString();
        }

        public void WriteLog(
            LogState state,
            List<string> stackList,
            string topic,
            string description,
            string category,
            string user,
            string custom)
        {
            if (!isFileLogValid)
            {
                return;
            }
            try
            {
                logCount++;
                if (IsLogDayLimitReached() || IsLogSizeLimitReached())
                {
                    Thread.Sleep(2000);
                    CreateLogFile(true);
                }
                using (StreamWriter streamWriter = 
                        new StreamWriter(
                                File.Open(
                                        FilePath,
                                        FileMode.Open,
                                        FileAccess.ReadWrite))
                    )
                {
                    if (!string.IsNullOrEmpty(logConfig.Attributes))
                    {
                        description = Attributes.ChangeDescription(description, logConfig.Attributes);
                    }
                    streamWriter.BaseStream.Seek(
                                    -("</logs>".Length),
                                    SeekOrigin.End);
                    streamWriter.Write(
                        CreateLogXmlNode(
                            state,
                            stackList,
                            topic,
                            description,
                            category,
                            user,
                            custom) + "</logs>");
                }
                if (logConfig.FileLogSource.ShowStatistics == 1)
                {
                    UpdateIndex(state);
                }
            }
            catch
            {

            }
        }

        private string CreateLogXmlNode(
            LogState state,
            List<string> stackList,
            string topic,
            string description,
            string category,
            string user,
            string custom)
        {
            return string.Format(
                          @"<log><dt>{0}</dt><tm>{1}</tm><st>{2}</st><t>{3}</t><d>{4}</d><s>{5}</s><c>{6}</c><u>{7}</u><cu>{8}</cu><id>{9}</id><th>{10}</th></log>",
                          DateTime.Now.ToString("MMM dd, yyyy"),
                          DateTime.Now.ToString("HH:mm:ss.fff"),
                          ConvertStackTrace(stackList),
                          SecurityElement.Escape(topic),
                          SecurityElement.Escape(description),
                          state,
                          SecurityElement.Escape(category),
                          SecurityElement.Escape(user),
                          SecurityElement.Escape(custom),
                          logCount,
                          Thread.CurrentThread.ManagedThreadId
                          );
        }

        public void Cleanup()
        {
            int purgeDays = logConfig.PurgeDays;
            foreach (string path in Directory.GetFiles(
                                                       LogFolder,
                                                       "*.xml"))
            {
                if (Path.GetFileName(path) == "index.xml")
                {
                    continue;
                }
                bool isDeleteFile = File.GetCreationTime(path).Date < DateTime.Now.AddDays(-1 * purgeDays).Date;
                if (isDeleteFile)
                {
                    File.Delete(path);
                    DeleteNodeFromIndex(Path.GetFileName(path));
                }
            }
        }

        #endregion //ILog

        internal FileLog(LogConfig _logConfig)
        {
            logConfig = _logConfig;
            if (string.IsNullOrWhiteSpace(logConfig.FileLogSource.Folder))
            {
                logConfig.FileLogSource.Folder = DefaultLogFolder;
            }
            if (logConfig.FileLogSource.CurrentLogDayLimit == 0)
            {
                logConfig.FileLogSource.CurrentLogDayLimit = 1;
            }
            if (logConfig.FileLogSource.CurrentLogSizeLimitKb == 0)
            {
                logConfig.FileLogSource.CurrentLogSizeLimitKb = 100;
            }
        }

        private bool IsLogDayLimitReached()
        {
            return (File.GetCreationTime(FilePath).Date.AddDays(logConfig.FileLogSource.CurrentLogDayLimit) <=
                    DateTime.Now.Date);
        }

        private bool IsLogSizeLimitReached()
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            return (fileInfo.Length > (logConfig.FileLogSource.CurrentLogSizeLimitKb * 1024));
        }


        internal void CreateLogFolderIfNotFound()
        {
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }
            LogConstants.ProvideFullAccess(LogFolder);
        }

        private void DeleteNodeFromIndex(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(IndexPath);
            XmlNode node = doc.SelectSingleNode("//name[text()='" + fileName + "']");
            node = node.ParentNode;
            node.ParentNode.RemoveChild(node);
            doc.Save(IndexPath);
        }

        private string CreateFilePath()
        {
            return $@"{LogFolder}\log.{DateTime.Now.ToString(
                                                             "yyyyMMdd")}.{DateTime.Now.ToString(
                                                                                                 "HHmmss")}.xml";
        }

        private string IndexPath => $@"{LogFolder}\index.xml";

        private string IndexHtmlPath => $@"{LogFolder}\index.html";

        private XmlWriterSettings XmlWriterSettings
        {
            get
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                                                      {
                                                          Indent = true,
                                                          NewLineOnAttributes = true
                                                      };
                return xmlWriterSettings;
            }
        }

        private void AddXsltHeaders(XmlWriter logWriter, string styleSheetName)
        {
            string processingInstructionText = $"type=\"text/xsl\" href=\"{styleSheetName}\"";
            logWriter.WriteProcessingInstruction("xml-stylesheet", processingInstructionText);
            //logWriter.WriteDocType(topNode, null, null, "\n<!ATTLIST xsl:stylesheet\n id ID #REQUIRED>\n");
        }

        private void ExtractEmbeddedResourceFile(string fileName, bool IsSkipIfExists = false)
        {
            var filePath = $"{LogFolder}\\{fileName}";
            
            if (File.Exists(filePath))
            {
                if (IsSkipIfExists)
                {
                    return;
                }
                File.Delete(filePath);
            }
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "OfficeClip.OpenSource.Logger.Resource." + fileName;
            using (var input = assembly.GetManifestResourceStream(resourceName))
            {
                using (var output = File.Open(filePath, FileMode.CreateNew))
                {
                    if (input == null) throw new FileNotFoundException(resourceName + ": Embedded resoure file not found");
                    var buffer = new byte[32768];
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                    }
                    output.Flush();
                }
            }
        }

        private string GetXmlEmbeddedResourceContent(string resourceFileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string assemblyName = "OfficeClip.OpenSource.Logger.Resource." + resourceFileName;
            using (Stream stream = assembly.GetManifestResourceStream(assemblyName))
            {
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    reader.MoveToContent();
                    return reader.ReadOuterXml();
                }
            }
        }

        private string GetFileContent(string fileName)
        {
            using (XmlTextReader reader = new XmlTextReader(fileName))
            {
                reader.MoveToContent();
                return reader.ReadOuterXml();
            }
        }

        private string GetIndexXsltContent()
        {
            string applicationPath = LogConstants.GetSetupDir() + "\\";
            string content = (string.IsNullOrWhiteSpace(logConfig.FileLogSource.IndexXslt)) ?
                GetXmlEmbeddedResourceContent("index.xslt") :
                GetFileContent(applicationPath + logConfig.FileLogSource.IndexXslt);
            return AddXmlHeader(content);
        }

        private string GetLogXsltContent()
        {
            string applicationPath = LogConstants.GetSetupDir() + "\\";
            string content = (string.IsNullOrWhiteSpace(logConfig.FileLogSource.LogXslt)) ?
                GetXmlEmbeddedResourceContent("log.xslt") :
                GetFileContent(applicationPath + logConfig.FileLogSource.LogXslt);
            return AddXmlHeader(content);
        }

        private string AddXmlHeader(string content)
        {
            return $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\n{content}";
        }

        internal void CreateIndexHtmlFile()
        {
            if (!IsEnabled)
            {
                return;
            }
            if (!File.Exists(IndexHtmlPath))
            {
                using (StreamWriter streamWriter = File.CreateText(IndexHtmlPath))
                {
                    streamWriter.Write(
                        GetXmlEmbeddedResourceContent("index.html"));
                }
            }
        }

        internal void CreateIndexFile()
        {
            if (!IsEnabled)
            {
                return;
            }
            if (!File.Exists(IndexPath))
            {
                CreateXsltFile("index.xslt", GetIndexXsltContent());
                using (XmlWriter logWriter = XmlWriter.Create(
                                                              IndexPath,
                                                              XmlWriterSettings))
                {
                    AddXsltHeaders(logWriter, "index.xslt");
                    logWriter.WriteStartElement("files");
                    logWriter.WriteEndElement();
                    logWriter.Flush();
                }
            }
        }

        private string GetExistingLogFilePath()
        {
            var files = Directory.GetFiles(
                                        LogFolder,
                                        "log.*.xml")
                                      .Select(f => new FileInfo(f))
                                      .OrderByDescending(f => f.CreationTime);
            if (files.Any())
            {
                FileInfo fileInfo = files.ElementAt(0);
                string dateCaptured = fileInfo.Name.Split(".".ToCharArray())[1];
                if (DateTime.Now.Date.ToString("yyyyMMdd") == dateCaptured)
                {
                    return fileInfo.FullName;
                }
            }
            return null;
        }


        internal void CreateLogFile(bool isForced)
        {
            if (!isForced)
            {
                string existingPath = GetExistingLogFilePath();
                if (!string.IsNullOrEmpty(existingPath))
                {
                    FilePath = existingPath;
                    return;
                }
            }

            CreateXsltFile("log.xslt", GetLogXsltContent());

            FilePath = CreateFilePath();
            using (XmlWriter logWriter = XmlWriter.Create(FilePath, XmlWriterSettings))
            {
                AddXsltHeaders(logWriter, "log.xslt");

                logWriter.WriteStartElement("logs");
                logWriter.WriteFullEndElement();
                logWriter.Flush();
                WriteIndex(Path.GetFileName(FilePath), File.GetCreationTime(FilePath).ToString("G"));
            }
        }

        private void CreateXsltFile(string fileName, string content)
        {
            string indexFilePath = $"{LogFolder}\\{fileName}";
            if (! File.Exists(indexFilePath))
            {
                WriteTextFile(indexFilePath, content);
            }
        }

        private void WriteTextFile(string filePath, string dataStr)
        {
            StreamWriter sr = File.CreateText(filePath);
            sr.Write(dataStr);
            sr.Close();
        }



        public string GetLogStateString(LogState state)
        {
            return state.ToString();
        }

        private void UpdateIndex(LogState state)
        {
            try
            {
                XDocument xDocument = XDocument.Load(IndexPath);
                var query = from c in xDocument.Elements("files").Elements("file")
                            select c;
                foreach (XElement file in query)
                {
                    if (file.Element("name").Value == Path.GetFileName(FilePath))
                    {
                        file.Element("lines").Value =
                            string.IsNullOrEmpty(file.Element("lines").Value)
                                ? "1"
                                : (Convert.ToInt32(file.Element("lines").Value) + 1).ToString();
                        switch (state)
                        {
                            case LogState.FatalError:
                                file.Element("fatal").Value =
                                    string.IsNullOrEmpty(file.Element("fatal").Value)
                                        ? "1"
                                        : (Convert.ToInt32(file.Element("fatal").Value) + 1).ToString();
                                break;
                            case LogState.Error:
                                file.Element("error").Value =
                                    string.IsNullOrEmpty(file.Element("error").Value)
                                        ? "1"
                                        : (Convert.ToInt32(file.Element("error").Value) + 1).ToString();
                                break;
                            case LogState.Warning:
                                file.Element("warning").Value =
                                    string.IsNullOrEmpty(file.Element("warning").Value)
                                        ? "1"
                                        : (Convert.ToInt32(file.Element("warning").Value) + 1).ToString();
                                break;
                        }
                    }
                }
                xDocument.Save(IndexPath);
            }
            catch
            {

            }
        }

        internal void WriteIndex(
            string fileName,
            string displayName
            )
        {
            if (!isFileLogValid)
            {
                return;
            }
            try
            {
                if (!File.Exists(IndexPath))
                {
                    CreateIndexFile();
                }
                XDocument xDocument = XDocument.Load(IndexPath);
                XElement root = xDocument.Element("files");
                root.Add(
                    new XElement(
                             "file",
                             new XElement(
                                 "name",
                                 fileName),
                             new XElement(
                                 "display",
                                 displayName),
                             new XElement(
                                 "lines",
                                 string.Empty),
                             new XElement(
                                 "fatal",
                                 string.Empty),
                             new XElement(
                                 "error",
                                 string.Empty),
                             new XElement(
                                 "warning",
                                 string.Empty)));
                xDocument.Save(IndexPath);
            }
            catch (Exception ex)
            {

            }
        }

    }
}
