using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OfficeClip.OpenSource.Logger
{
    public class Attributes
    {
        internal static string ChangeDescription(string description, string attributes)
        {
            if (attributes.Contains("msbuild"))
            {
                // find and flag warnings
                description = Regex.Replace(description, @"(warning\s\w+)",
                                @": <span style=""background-color:yellow"">$1</span>:",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline);
                // successful build
                description = Regex.Replace(description, @"(Build succeeded\.)",
                                @"<span style=""background-color:lightgreen"">$1</span>",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline);
                // find and flag error
                description = Regex.Replace(description, @"(error\s\w+)",
                                @": <span style=""background-color:lightpink"">$1</span>:",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline);
                // failed build
                description = Regex.Replace(description, @"(Build FAILED\.)",
                                @"<span style=""background-color:violet"">$1</span>",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline);
                // start and end of the build
                description = Regex.Replace(description, @"(Build started|Time Elapsed)",
                                @"<span style=""background-color:khaki"">$1</span>",
                                RegexOptions.IgnoreCase | RegexOptions.Multiline);
                // project
                description = Regex.Replace(description, @"(Project|Done\sBuilding\sProject|Skipping target)\s""",
                                @"<span style=""background-color:lightgrey"">$1</span> """,
                                RegexOptions.IgnoreCase | RegexOptions.Multiline);
                // Start and end of task
                description = Regex.Replace(description, @"(\+{3,}\s+.+?\s+\+{3,})",
                                @"<span style=""background-color:lightblue"">$1</span> """,
                                RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }
            return description;
        }
    }
}
