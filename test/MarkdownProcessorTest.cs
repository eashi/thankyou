using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ThankYou.Test
{
    public class MarkdownProcessorTest
    {
        [Fact]
        public void FilesWithNoTemplateAreNotChanged()
        {
            var inputLines = new[] {
                "# Secret App",
                "",
                "This is a secret app with no contributors."
            };
            var contributors = new[] { "new" };

            var outputLines = MarkdownProcessor.AddContributorsToMarkdownFile(inputLines, contributors).ToArray();

            AssertCollectionsAreEqual(inputLines, outputLines);
        }

        [Fact]
        public void ContributorsAreAppendedForAWellFormedContributorsTemplate()
        {
            var inputLines = new[] {
                "# Awesome App",
                "",
                "This app is lovingly crafted by lots of awesome folks!",
                "",
                "## Contributors",
                "",
                "[//]: # (ThankYouBlockStart)",
                "[//]: # (ThankYouTemplate:- @name)",
                "- gandalf",
                "[//]: # (ThankYouBlockEnd)"
            };
            var contributors = new[] { "boromir" };

            var outputLines = MarkdownProcessor.AddContributorsToMarkdownFile(inputLines, contributors).ToArray();

            var expectedOutputLines = new[] {
                "# Awesome App",
                "",
                "This app is lovingly crafted by lots of awesome folks!",
                "",
                "## Contributors",
                "",
                "[//]: # (ThankYouBlockStart)",
                "[//]: # (ThankYouTemplate:- @name)",
                "- gandalf",
                "- boromir",
                "[//]: # (ThankYouBlockEnd)"
            };
            AssertCollectionsAreEqual(expectedOutputLines, outputLines);
        }

        [Fact]
        public void DuplicateContributorsAreNotAdded()
        {
            var inputLines = new[] {
                "# Awesome App",
                "",
                "This app is lovingly crafted by lots of awesome folks!",
                "",
                "## Contributors",
                "",
                "[//]: # (ThankYouBlockStart)",
                "[//]: # (ThankYouTemplate:- @name)",
                "- frodo",
                "[//]: # (ThankYouBlockEnd)"
            };
            var contributors = new[] { "frodo" };

            var outputLines = MarkdownProcessor.AddContributorsToMarkdownFile(inputLines, contributors).ToArray();

            AssertCollectionsAreEqual(inputLines, outputLines);
        }

        [Fact]
        public void ContributorsThatDifferOnlyInCaseAreConsideredDuplicates()
        {
            var inputLines = new[] {
                "# Awesome App",
                "",
                "This app is lovingly crafted by lots of awesome folks!",
                "",
                "## Contributors",
                "",
                "[//]: # (ThankYouBlockStart)",
                "[//]: # (ThankYouTemplate:- @name)",
                "- frodo",
                "[//]: # (ThankYouBlockEnd)"
            };
            var contributors = new[] { "Frodo", "froDo" };

            var outputLines = MarkdownProcessor.AddContributorsToMarkdownFile(inputLines, contributors).ToArray();

            AssertCollectionsAreEqual(inputLines, outputLines);
        }

        private static void AssertCollectionsAreEqual<T>(IList<T> expected, IList<T> actual)
        {
            // The collection equality assertion in XUnit Assert.Equal doesn't print a great
            // error message if the list is large. It truncates the list and prints ellipses.
            //
            // This method asserts the equality of each item one by one, providing better error
            // messages.

            Assert.All(
                expected.Zip(actual),
                ((T expected, T actual) pair) => Assert.Equal(pair.expected, pair.actual));
            Assert.Equal(expected.Count, actual.Count);
        }
    }
}
