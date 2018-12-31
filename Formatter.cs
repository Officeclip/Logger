using System.Collections;
using System.Data;
using System.Text;

namespace OfficeClip.OpenSource.Logger
{
    /// <summary>
    /// From: http://stackoverflow.com/a/19683749
    /// </summary>
    public class Formatter
    {
        public static string ToPrint(DataTable dt)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine(@"<table style=""width:inherit"">");
            //add header row
            html.AppendLine("<tr>");
            for (int i = 0; i < dt.Columns.Count; i++)
                html.AppendLine("<td><b>" + dt.Columns[i].ColumnName + "</b></td>");
            html.AppendLine("</tr>");
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html.AppendLine("<tr>");
                for (int j = 0; j < dt.Columns.Count; j++)
                    html.AppendLine("<td>" + dt.Rows[i][j] + "</td>");
                html.AppendLine("</tr>");
            }
            html.AppendLine("</table>");
            return html.ToString();
        }

        public static string ToPrint(DataSet dataSet)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index=0; index < dataSet.Tables.Count; index++)
            {
                stringBuilder.Append($"Table: {dataSet.Tables[index].TableName}<br/>");
                stringBuilder.Append(ToPrint(dataSet.Tables[index]));
            }
            return stringBuilder.ToString();
        }

        public static string ToPrint(Hashtable hashTable)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine(@"<table style=""width:inherit"">");

            foreach (DictionaryEntry entry in hashTable)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{entry.Key}</td>");
                html.AppendLine($"<td>{entry.Value}</td>");
                html.AppendLine("</tr>");
            }
            html.AppendLine("</table>");
            return html.ToString();
        }
    }
}
