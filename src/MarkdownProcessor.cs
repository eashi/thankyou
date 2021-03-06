using System;
using System.Collections.Generic;
using System.Linq;

namespace ThankYou
{
    public static class MarkdownProcessor
    {
        public static Dictionary<string, string> userServiceList = new Dictionary<string, string>(){
            {"github", "https://github.com"},
            {"twitch", "https://twitch.com"},
            {"twitter", "https://twitter.com"}
        };
        public static IEnumerable<string> AddContributorsToMarkdownFile(IEnumerable<string> inputLines, IEnumerable<Contributor> contributorsToday)
        {
            bool foundThankYouBlock = false;
            bool startedACodeBlock = false;
            List<string> newContributorLines = new List<string>();
            List<string> existingContributorLines = new List<string>();

            // This is a state machine with three states: Before contributors, contributors, and after contributors
            // Since the Before and After states behave the same way, I'm cheating and using a boolean and reducing
            // it down to two states. Hopefully it still makes sense.
            foreach (var line in inputLines)
            {
                if (line.Equals("```"))
                {
                    startedACodeBlock = !startedACodeBlock;
                }
                if (startedACodeBlock)
                {
                    yield return line;
                    continue;
                }
                else if (!startedACodeBlock && line.Equals("[//]: # (ThankYouBlockStart)") )
                {
                    // Found the start of the thank you block, so start collecting contributors
                    foundThankYouBlock = true;
                    yield return line;
                }
                else if (line.Equals("[//]: # (ThankYouBlockEnd)") && foundThankYouBlock)
                {
                    // Work out which ones to add
                    var contributorsToOutput = newContributorLines.Except(existingContributorLines, StringComparer.OrdinalIgnoreCase);
                    // Add them to the end
                    foreach (var contributor in contributorsToOutput)
                    {
                        yield return contributor;
                    }
                    // Now add our closing block again
                    yield return line;
                    // Finally, turn off collection of contributors
                    foundThankYouBlock = false;
                }
                else if (line.StartsWith("[//]: # \"ThankYouTemplate:") && foundThankYouBlock)
                {
                    // found the template, so now we can calculate the new lines
                    yield return line;

                    foreach (var contributor in contributorsToday)
                    {
                        //if contributor already exists

                        var thankYouLine = line.Replace("[//]: # \"ThankYouTemplate:", "").Replace("@name", contributor.Name).Replace("@serviceUrl", contributor.PreferredUserService);
                        newContributorLines.Add(thankYouLine.Substring(0, thankYouLine.Length - 1));
                    }
                }
                else if (foundThankYouBlock)
                {
                    // A "normal" line inside the thank you block is a contributor
                    existingContributorLines.Add(line);
                    yield return line;
                }
                else
                {
                    // A normal line, just add it
                    yield return line;
                } //!thanks @voiceOfApollo
            }
        }
    }
}
