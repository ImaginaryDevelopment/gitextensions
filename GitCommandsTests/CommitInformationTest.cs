#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Category = Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute;
#else
using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
#endif
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GitCommands;

namespace GitCommandsTests
{
    [TestClass]
    public class CommitInformationTest
    {

       

        [TestMethod]
        public void CanCreateCommitInformationFromFormattedDataApostropheInEmail()
        {
          
            var commitGuid = Guid.NewGuid();
            var authorTime = DateTime.UtcNow.AddDays(-3);
            var commitTime = DateTime.UtcNow.AddDays(-2);
            

            Func<string, string, string> generateRawLine = (n, email) =>
             String.Format("{0} <{1}>", n, email);
            var aName = "John D'oe (Acme Inc)";
            var aMail = "John.D'oe@test.com";
            var cName = "Jane D'oe (Acme Inc)";
            var cMail = "Jane.D'oe@test.com";
            var raw = BuildRawForHeader(aName, aMail, authorTime, cName, cMail, commitTime, commitGuid);


            var expectedHeader = "Author:\t\t<a href='mailto:John.D&#39;oe@test.com'>John D&#39;oe (Acme Inc) &lt;John.D&#39;oe@test.com&gt;</a>" + Environment.NewLine +
                                 "Author date:\t3 days ago (" + authorTime.ToLocalTime().ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")" + Environment.NewLine +
                                 "Committer:\t<a href='mailto:Jane.D&#39;oe@test.com'>Jane D&#39;oe (Acme Inc) &lt;Jane.D&#39;oe@test.com&gt;</a>" + Environment.NewLine +
                                 "Commit date:\t2 days ago (" + commitTime.ToLocalTime().ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")" + Environment.NewLine +
                                 "Commit hash:\t" + commitGuid;

            var actualHeader = CommitData.GenerateHeader(aName, generateRawLine(aName, aMail), "3 days ago", authorTime, cName, generateRawLine(cName, cMail), "2 days ago", commitTime, commitGuid);
            AssertTextBlocksEqual(expectedHeader, actualHeader);
        }
       
        [TestMethod]
        public void CanCreateCommitInformationHeaderFromFormattedDataRefactorVsLegacy()
        {
            var commitGuid = Guid.NewGuid();
            var treeGuid = Guid.NewGuid();
            var parentGuid1 = Guid.NewGuid();
            var parentGuid2 = Guid.NewGuid();
            var authorTime = DateTime.UtcNow.AddDays(-3);
            var commitTime = DateTime.UtcNow.AddDays(-2);
            var authorUnixTime = (int)(authorTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var commitUnixTime = (int)(commitTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            Func<string, string, string> generateRawLine = (n, email) =>
               String.Format("{0} <{1}>\n", n, email);
            var aName = "John Doe (Acme Inc)";
            var aMail = "John.Doe@test.com";
            var cName = "Jane Doe (Acme Inc)";
            var cMail = "Jane.Doe@test.com";

            var rawData = commitGuid + "\n" +
                          treeGuid + "\n" +
                          parentGuid1 + " " + parentGuid2 + "\n" +
                           generateRawLine(aName, aMail) +
                          authorUnixTime + "\n" +
                          generateRawLine(cName, cMail) +
                          commitUnixTime + "\n" +
                          "\n" +
                          "\tI made a really neato change.\n\n" +
                          "Notes (p4notes):\n" +
                          "\tP4@547123";


            var expectedHeader = CommitData.CreateFromFormattedData(rawData).GetHeader();
            var actualHeader = CommitData.GenerateHeader("John Doe (Acme Inc)", "John Doe (Acme Inc) <John.Doe@test.com>", "3 days ago", authorTime, "Jane Doe (Acme Inc)", "Jane Doe (Acme Inc) <Jane.Doe@test.com>", "2 days ago", commitTime, commitGuid);

            var expectedLines = expectedHeader.SplitLines();
            var actualLines = actualHeader.SplitLines();
            Assert.AreEqual(expectedLines.Count(), actualLines.Count());
            foreach (var l in expectedLines.Zip(actualLines, (expected, actual) => new { expected, actual }))
            {
                Assert.AreEqual(l.expected.Length, l.actual.Length, l.actual.Replace("\t", "\\t"));
                Assert.AreEqual(l.expected, l.actual);

            }
            Assert.AreEqual(expectedHeader, actualHeader);

        }

        [TestMethod]
        public void CanCreateCommitInformationHeaderFromFormattedDataRefactor()
        {
            var commitGuid = Guid.NewGuid();
            var treeGuid = Guid.NewGuid();
            var parentGuid1 = Guid.NewGuid();
            var parentGuid2 = Guid.NewGuid();
            var authorTime = DateTime.UtcNow.AddDays(-3);
            var commitTime = DateTime.UtcNow.AddDays(-2);
            var authorUnixTime = (int)(authorTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var commitUnixTime = (int)(commitTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            Func<string, string, string> generateRawLine = (n, email) =>
                String.Format("{0} <{1}>\n", n, email);
            var aName = "John Doe (Acme Inc)";
            var aMail = "John.Doe@test.com";
            var cName = "Jane Doe (Acme Inc)";
            var cMail = "Jane.Doe@test.com";

            var rawData = commitGuid + "\n" +
                          treeGuid + "\n" +
                          parentGuid1 + " " + parentGuid2 + "\n" +
                          generateRawLine(aName, aMail) +
                          authorUnixTime + "\n" +
                          generateRawLine(cName,cMail) +
                          commitUnixTime + "\n" +
                          "\n" +
                          "\tI made a really neato change.\n\n" +
                          "Notes (p4notes):\n" +
                          "\tP4@547123";

            var expectedHeader = "Author:\t\t<a href='mailto:John.Doe@test.com'>"+aName+" &lt;John.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Author date:\t3 days ago (" + authorTime.ToLocalTime().ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")" + Environment.NewLine +
                                 "Committer:\t<a href='mailto:Jane.Doe@test.com'>Jane Doe (Acme Inc) &lt;Jane.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Commit date:\t2 days ago (" + commitTime.ToLocalTime().ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")" + Environment.NewLine +
                                 "Commit hash:\t" + commitGuid;



            var header = CommitData.GenerateHeader("John Doe (Acme Inc)", "John Doe (Acme Inc) <John.Doe@test.com>", "3 days ago", authorTime, "Jane Doe (Acme Inc)", "Jane Doe (Acme Inc) <Jane.Doe@test.com>", "2 days ago", commitTime, commitGuid);

            var expectedLines = expectedHeader.SplitLines();
            var actualLines = header.SplitLines();
            Assert.AreEqual(expectedLines.Count(), actualLines.Count());
            foreach (var l in expectedLines.Zip(actualLines, (expected, actual) => new { expected, actual }))
            {
                Assert.AreEqual(l.expected.Length, l.actual.Length, l.actual.Replace("\t", "\\t"));
                Assert.AreEqual(l.expected, l.actual);

            }
            Assert.AreEqual(expectedHeader, header);
            

        }

        [TestMethod]
        public void CanCreateCommitInformationHeaderFromFormattedData()
        {
            var commitGuid = Guid.NewGuid();
            var treeGuid = Guid.NewGuid();
            var parentGuid1 = Guid.NewGuid();
            var parentGuid2 = Guid.NewGuid();
            var authorTime = DateTime.UtcNow.AddDays(-3);
            var commitTime = DateTime.UtcNow.AddDays(-2);
            var authorUnixTime = (int)(authorTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var commitUnixTime = (int)(commitTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            var rawData = commitGuid + "\n" +
                          treeGuid + "\n" +
                          parentGuid1 + " " + parentGuid2 + "\n" +
                          "John Doe (Acme Inc) <John.Doe@test.com>\n" +
                          authorUnixTime + "\n" +
                          "Jane Doe (Acme Inc) <Jane.Doe@test.com>\n" +
                          commitUnixTime + "\n" +
                          "\n" +
                          "\tI made a really neato change.\n\n" +
                          "Notes (p4notes):\n" +
                          "\tP4@547123";

            var expectedHeader = "Author:\t\t<a href='mailto:John.Doe@test.com'>John Doe (Acme Inc) &lt;John.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Author date:\t3 days ago (" + authorTime.ToLocalTime().ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")" + Environment.NewLine +
                                 "Committer:\t<a href='mailto:Jane.Doe@test.com'>Jane Doe (Acme Inc) &lt;Jane.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Commit date:\t2 days ago (" + commitTime.ToLocalTime().ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")" + Environment.NewLine +
                                 "Commit hash:\t" + commitGuid;



            var header = CommitData.GenerateHeader("John Doe (Acme Inc)", "John Doe (Acme Inc) <John.Doe@test.com>", "3 days ago", authorTime, "Jane Doe (Acme Inc)", "Jane Doe (Acme Inc) <Jane.Doe@test.com>", "2 days ago", commitTime, commitGuid);

            var expectedLines = expectedHeader.SplitLines();
            var actualLines = header.SplitLines();
            Assert.AreEqual(expectedLines.Count(), actualLines.Count());
            foreach (var l in expectedLines.Zip(actualLines,(expected,actual)=>new{expected,actual}))
            {
                Assert.AreEqual(l.expected.Length, l.actual.Length,l.actual.Replace("\t","\\t"));
                Assert.AreEqual(l.expected, l.actual);
                
            }
            Assert.AreEqual(expectedHeader, header);
           
        }

        [TestMethod]
        public void CanCreateCommitInformationHeaderFromFormattedDataVsLegacy()
        {
            var commitGuid = Guid.NewGuid();
            var treeGuid = Guid.NewGuid();
            var parentGuid1 = Guid.NewGuid();
            var parentGuid2 = Guid.NewGuid();
            var authorTime = DateTime.UtcNow.AddDays(-3);
            var commitTime = DateTime.UtcNow.AddDays(-2);
            var authorUnixTime = (int)(authorTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var commitUnixTime = (int)(commitTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            var rawData = commitGuid + "\n" +
                          treeGuid + "\n" +
                          parentGuid1 + " " + parentGuid2 + "\n" +
                          "John Doe (Acme Inc) <John.Doe@test.com>\n" +
                          authorUnixTime + "\n" +
                          "Jane Doe (Acme Inc) <Jane.Doe@test.com>\n" +
                          commitUnixTime + "\n" +
                          "\n" +
                          "\tI made a really neato change.\n\n" +
                          "Notes (p4notes):\n" +
                          "\tP4@547123";

           
            var expectedHeader = CommitData.CreateFromFormattedData(rawData).GetHeader();
            var actualHeader = CommitData.GenerateHeader("John Doe (Acme Inc)", "John Doe (Acme Inc) <John.Doe@test.com>", "3 days ago", authorTime, "Jane Doe (Acme Inc)", "Jane Doe (Acme Inc) <Jane.Doe@test.com>", "2 days ago", commitTime, commitGuid);

            var expectedLines = expectedHeader.SplitLines();
            var actualLines = actualHeader.SplitLines();
            Assert.AreEqual(expectedLines.Count(), actualLines.Count());
            foreach (var l in expectedLines.Zip(actualLines, (expected, actual) => new { expected, actual }))
            {
                Assert.AreEqual(l.expected.Length, l.actual.Length, l.actual.Replace("\t", "\\t"));
                Assert.AreEqual(l.expected, l.actual);

            }
            Assert.AreEqual(expectedHeader, actualHeader);

        }
        [TestMethod]
        public void CanCreateCommitInformationFromFormattedData()
        {
            var commitGuid = Guid.NewGuid();
            var treeGuid = Guid.NewGuid();
            var parentGuid1 = Guid.NewGuid();
            var parentGuid2 = Guid.NewGuid();
            var authorTime = DateTime.UtcNow.AddDays(-3);
            var commitTime = DateTime.UtcNow.AddDays(-2);
            var authorUnixTime = (int)(authorTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var commitUnixTime = (int)(commitTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            var rawData = commitGuid + "\n" +
                          treeGuid + "\n" +
                          parentGuid1 + " " + parentGuid2 + "\n" +
                          "John Doe (Acme Inc) <John.Doe@test.com>\n" + 
                          authorUnixTime + "\n" +
                          "Jane Doe (Acme Inc) <Jane.Doe@test.com>\n" +
                          commitUnixTime + "\n" +
                          "\n" +
                          "\tI made a really neato change.\n\n" +
                          "Notes (p4notes):\n" +
                          "\tP4@547123";

            var expectedHeader = "Author:\t\t<a href='mailto:John.Doe@test.com'>John Doe (Acme Inc) &lt;John.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Author date:\t3 days ago (" + authorTime.ToLocalTime().ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")" + Environment.NewLine +
                                 "Committer:\t<a href='mailto:Jane.Doe@test.com'>Jane Doe (Acme Inc) &lt;Jane.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Commit date:\t2 days ago (" + commitTime.ToLocalTime().ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")" + Environment.NewLine +
                                 "Commit hash:\t" + commitGuid;

            var expectedBody = "\n\nI made a really neato change." + Environment.NewLine + Environment.NewLine +
                               "Notes (p4notes):" + Environment.NewLine +
                               "\tP4@547123\n\n";

            var commitData = CommitData.CreateFromFormattedData(rawData);
            var commitInformation = CommitInformation.GetCommitInfo(commitData);
            
            Assert.AreEqual(expectedHeader,commitInformation.Header);
            Assert.AreEqual(expectedBody, commitInformation.Body);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanCreateCommitInformationFromFormatedDataThrowsException()
        {
            CommitInformation.GetCommitInfo(data: null);
        }

        [TestMethod]
        public void GetCommitInfoTestWhenDataIsNull()
        {
            var actualResult = CommitInformation.GetCommitInfo(new GitModule(""), "fakesha1");
            Assert.AreEqual("Cannot find commit fakesha1", actualResult.Header);
        }

        [TestMethod]
        public void GetAllBranchesWhichContainGivenCommitTestReturnsEmptyList()
        {
            var actualResult = CommitInformation.GetAllBranchesWhichContainGivenCommit("fakesha1", false, false);

            Assert.IsNotNull(actualResult);
            Assert.IsTrue(!actualResult.Any());
        }

        void AssertTextBlocksEqual(string expectedBlock, string actualBlock)
        {
            var expectedLines = expectedBlock.SplitLines();
            var actualLines = actualBlock.SplitLines();
            Assert.AreEqual(expectedLines.Count(), actualLines.Count());
            foreach (var l in expectedLines.Zip(actualLines, (expected, actual) => new { expected, actual }).Select((r, lineNumber) => new { lineNumber, r.actual, r.expected }))
            {
                Assert.AreEqual(l.expected.Length, l.actual.Length, l.actual.Replace("\t", "\\t"));
                for (int i = 0; i < l.actual.Length; i++)
                {
                    Assert.AreEqual(l.expected[i], l.actual[i], "Mismatch at {0},{1} Expected:{2},Actual:{3}", l.lineNumber, i, l.expected[i], l.actual[i]);
                }
                Assert.AreEqual(l.expected, l.actual, l.actual.Replace("\t", "\\t"));
            }
            Assert.AreEqual(expectedBlock, actualBlock);
        }

        string BuildRawForHeader(string aName, string aMail, DateTime aTime, string cName, string cMail, DateTime cTime, Guid commitGuid)
        {
            var aUnixTime = (int)(aTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var cUnixTime = (int)(cTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            Func<string, string, string> generateRawLine = (n, email) =>
             String.Format("{0} <{1}>", n, email);
            var rawData = commitGuid + "\n" +
                         Guid.NewGuid() + "\n" +
                         Guid.NewGuid() + " " + Guid.NewGuid() + Environment.NewLine +
                          generateRawLine(aName, aMail) + Environment.NewLine +
                         aUnixTime + "\n" +
                         generateRawLine(cName, cMail) + Environment.NewLine +
                         cUnixTime + "\n" +
                         "\n" +
                         "\tI made a really neato change.\n\n" +
                         "Notes (p4notes):\n" +
                         "\tP4@547123";
            return rawData;
        }

    }
}
