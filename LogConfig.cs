using System;
using System.IO;
using System.Xml.Serialization;

namespace OfficeClip.OpenSource.Logger
{
    [Serializable()]
    [XmlRoot("logConfig")]
    public class LogConfig
    {
        [XmlElement(ElementName = "purgeDays", DataType = "int")]
        public int PurgeDays { get; set; }

        /// <summary>
        /// Output stack line will be shown if this string is present. Multiple strings can
        /// be used with comma demiliter
        /// </summary>
        [XmlElement("stackLineMatchPattern")]
        public string StackLineMatchPattern { get; set; }

        /// <summary>
        /// Output stack line NOT will be shown if this string is present. Multiple strings can
        /// be used with comma demiliter
        /// </summary>
        [XmlElement("stackLineRejectPattern")]
        public string StackLineRejectPattern { get; set; }

        [XmlElement("attributes")]
        public string Attributes { get; set; }

        [XmlElement("fileLogSource")] 
        public FileLogSource FileLogSource;

        [XmlElement("databaseLogSource")]
        public DatabaseLogSource DatabaseLogSource;

        [XmlElement("eventLogSource")]
        public EventLogSource EventLogSource;

        [XmlArray("loggers")]
        [XmlArrayItem("logger")]
        public Logger[] Loggers;
    }

    [Serializable()]
    public class EventLogSource
    {
        [XmlElement(ElementName = "supress", DataType = "int")]
        public int Supress { get; set; }

        [XmlElement("source")]
        public string Source { get; set; }
    }

    [Serializable()]
    public class DatabaseLogSource
    {
        [XmlElement(ElementName = "supress", DataType = "int")]
        public int Supress { get; set; }

        [XmlElement("connectionString")]
        public string ConnectionString { get; set; }

        [XmlElement("tableName")]
        public string TableName { get; set; }

        [XmlElement("dateTimeField")]
        public string DateTimeField { get; set; }

        [XmlElement("loggerNameField")]
        public string LoggerNameField { get; set; }

        [XmlElement("logStateField")]
        public string LogStateField { get; set; }

        [XmlElement("stackInfoField")]
        public string StackInfoField { get; set; }

        [XmlElement("topicField")]
        public string TopicField { get; set; }

        [XmlElement("descriptionField")]
        public string DescriptionField { get; set; }
    }

    [Serializable()]
    public class FileLogSource
    {
    [XmlElement(ElementName = "supress", DataType = "int")]
    public int Supress { get; set; }

    [XmlElement("folder")]
    public string Folder { get; set; }

    [XmlElement("logXslt", IsNullable = true)]
    public string LogXslt { get; set; }

    [XmlElement("indexXslt", IsNullable = true)]
    public string IndexXslt { get; set; }

    [XmlElement(ElementName = "currentLogDayLimit", DataType = "int")]
    public int CurrentLogDayLimit { get; set; }

    [XmlElement(ElementName = "currentLogSizeLimitKb", DataType = "int")]
    public int CurrentLogSizeLimitKb { get; set; }

    [XmlElement(ElementName = "showStatistics", DataType = "int")]
    public int ShowStatistics { get; set; }
    }

    [Serializable()]
    public class Logger
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement(ElementName = "level", DataType = "int")]
        public int Level { get; set; }

        public Logger()
        {
        }

        public Logger(
            string name,
            string description,
            int level)
        {
            Name = name;
            Description = description;
            Level = level;
        }
    }

}
