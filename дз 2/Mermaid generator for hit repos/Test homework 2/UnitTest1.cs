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
            File.WriteAllText(xmlFilePath, $"<Config><RepositoryUrl>{expectedUrl}</RepositoryUrl></Config>");

            // Act
            string result = Mermaind.ReadRepoUrlFromXml(xmlFilePath);

            // Assert
            Assert.AreEqual(expectedUrl, result);

            // Cleanup
            File.Delete(xmlFilePath);
        }

        [Test]
        public void CloneRepo_ValidRepoUrl_ClonesSuccessfully()
        {
            // Arrange
            string repoUrl = "https://github.com/Uksus1147/Config-Homework-1.git";
            string localRepoPath = "test_temp_repo";

            // Act
            Mermaind.CloneRepo(repoUrl, localRepoPath);

            // Assert
            Assert.IsTrue(Directory.Exists(localRepoPath));

            // Cleanup
            DirectoryCleaner.ForceDeleteDirectory(localRepoPath);


        }

        [Test]
        public void GetCommits_ValidLocalRepo_ReturnsCommits()
        {
            // Arrange
            string localRepoPath = "test_temp_repo";
            Mermaind.CloneRepo("https://github.com/Uksus1147/Config-Homework-1.git", localRepoPath);

            // Act
            string[] commits = Mermaind.GetCommits(localRepoPath);

            // Assert
            Assert.IsNotNull(commits);
            Assert.IsNotEmpty(commits);

            // Cleanup
            DirectoryCleaner.ForceDeleteDirectory(localRepoPath);
        }
    }
}
