using System;
using System.IO;
using NUnit.Framework;
using Prog;

namespace Prog
{
    [TestFixture]
    public class MermaindTests
    {
        [Test]
        public void ReadRepoUrlFromXml_ValidXml_ReturnsCorrectUrl()
        {
            // Arrange
            string xmlFilePath = "config.xml";
            string expectedUrl = "https://github.com/Uksus1147/Config-Homework-1.git";
            File.WriteAllText(xmlFilePath, $"<Configuration><RepositoryUrl>{expectedUrl}</RepositoryUrl></Configuration>");

            // Act
            string result = Mermaind.ReadRepoUrlFromXml(xmlFilePath);

            // Assert
            Assert.AreEqual(expectedUrl, result);

            // Cleanup
            File.Delete(xmlFilePath);
        }

        [Test]
        public void GetCommits_ValidLocalRepo_ReturnsCommits()
        {
            // Arrange
            string localRepoPath = "test_temp_repo";
            string testRepoPath = "C:\\Users\\”ксус147\\Documents\\GitHub\\Config-Homework-1"; // ѕуть к существующему репозиторию

            // Act
            string[] commits = Mermaind.GetCommits(testRepoPath);

            // Assert
            Assert.IsNotNull(commits);
            Assert.IsNotEmpty(commits);
        }

        [Test]
        public void GetCommits_InvalidLocalRepo_ReturnsNull()
        {
            // Arrange
            string invalidRepoPath = "C:\\Invalid\\Path\\To\\Repo";

            // Act
            string[] commits = Mermaind.GetCommits(invalidRepoPath);

            // Assert
            Assert.IsNull(commits);
        }
    }
}
