//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading;
//using System.Xml.Linq;

//namespace Prog
//{
//    public static class Mermaind
//    {
//        static void Main(string[] args)
//        {
//            // Чтение XML файла
//            string xmlFilePath = "config.xml"; // Путь к XML файлу
//            string repoUrl = ReadRepoUrlFromXml(xmlFilePath);

//            if (string.IsNullOrEmpty(repoUrl))
//            {
//                Console.WriteLine("Ссылка на репозиторий не найдена в XML.");
//                return;
//            }

//            // Локальная папка для клонирования репозитория
//            string localRepoPath = "temp_repo";

//            // Клонирование репозитория
//            if (Directory.Exists(localRepoPath))
//            {

//                try
//                {
//                    DirectoryCleaner.ForceDeleteDirectory(localRepoPath);
//                    Console.WriteLine("Директория успешно удалена.");
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Ошибка при удалении директории: {ex.Message}");
//                }

//            }

//            CloneRepo(repoUrl, localRepoPath);

//            // Получение коммитов и их визуализация
//            string[] commits = GetCommits(localRepoPath);
//            if (commits != null)
//            {
//                VisualizeCommits(commits);
//            }

//            Console.ReadLine();
//        }

//        // Метод для чтения URL репозитория из XML файла
//        public static string ReadRepoUrlFromXml(string xmlFilePath)
//        {
//            try
//            {
//                XDocument xmlDoc = XDocument.Load(xmlFilePath);
//                XElement urlElement = xmlDoc.Root.Element("RepositoryUrl");
//                return urlElement?.Value;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Ошибка при чтении XML: {ex.Message}");
//                return null;
//            }
//        }

//        // Метод для клонирования репозитория через команду git
//        public static void CloneRepo(string repoUrl, string localRepoPath)
//        {
//            ProcessStartInfo processInfo = new ProcessStartInfo("git", $"clone {repoUrl} {localRepoPath}");
//            processInfo.RedirectStandardOutput = true;
//            processInfo.RedirectStandardError = true;
//            processInfo.UseShellExecute = false;
//            processInfo.CreateNoWindow = true;

//            using (Process process = Process.Start(processInfo))
//            {
//                string output = process.StandardOutput.ReadToEnd();
//                string error = process.StandardError.ReadToEnd();
//                process.WaitForExit();

//                if (process.ExitCode != 0)
//                {
//                    Console.WriteLine("Ошибка при клонировании репозитория: " + error);
//                }
//                else
//                {
//                    Console.WriteLine(output);
//                }
//            }
//        }

//        // Метод для получения списка коммитов через команду git
//        public static string[] GetCommits(string localRepoPath)
//        {
//            ProcessStartInfo processInfo = new ProcessStartInfo("git", "log --pretty=format:\"%h %s\"")
//            {
//                WorkingDirectory = localRepoPath,
//                RedirectStandardOutput = true,
//                RedirectStandardError = true,
//                UseShellExecute = false,
//                CreateNoWindow = true,
//                StandardOutputEncoding = Encoding.UTF8
//            };

//            using (Process process = Process.Start(processInfo))
//            {
//                string output = process.StandardOutput.ReadToEnd();
//                string error = process.StandardError.ReadToEnd();
//                process.WaitForExit();

//                if (process.ExitCode != 0)
//                {
//                    Console.WriteLine("Ошибка при получении коммитов: " + error);
//                    return null;
//                }
//                else
//                {
//                    return output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
//                }
//            }
//        }

//        // Метод для визуализации коммитов в формате Mermaid
//        public static void VisualizeCommits(string[] commits)
//        {
//            Console.WriteLine("flowchart TD");

//            string previousCommit = null;
//            foreach (var commit in commits)
//            {
//                string[] parts = commit.Split(new[] { ' ' }, 2);
//                string commitId = parts[0]; // Короткий хеш коммита
//                string commitMessage = parts[1].Replace("\n", " "); // Убираем переносы строк

//                Console.WriteLine($"    {commitId}[\"{commitMessage}\"]");

//                if (previousCommit != null)
//                {
//                    Console.WriteLine($"    {commitId} --> {previousCommit}");
//                }

//                previousCommit = commitId;
//            }
//        }
//    }

//    public static class DirectoryCleaner
//    {
//        [DllImport("kernel32.dll", SetLastError = true)]
//        private static extern bool DeleteFile(string lpFileName);

//        public static void ForceDeleteDirectory(string path)
//        {
//            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
//            {
//                File.SetAttributes(file, FileAttributes.Normal); // Снимаем атрибуты "только чтение"
//                DeleteFile(file); // Используем WinAPI для удаления файлов
//            }

//            foreach (var dir in Directory.GetDirectories(path))
//            {
//                ForceDeleteDirectory(dir); // Рекурсивно удаляем подкаталоги
//            }

//            Directory.Delete(path, true); // Удаляем саму директорию
//        }
//    }
//}
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Prog
{
    public static class Mermaind
    {
        static void Main(string[] args)
        {
            string xmlFilePath = "config.xml"; // Путь к XML файлу
            string repoPath = ReadRepoUrlFromXml(xmlFilePath);

            if (string.IsNullOrEmpty(repoPath) || !Directory.Exists(repoPath))
            {
                Console.WriteLine("Локальный путь к репозиторию не найден или указан неверно.");
                return;
            }

            // Получение коммитов и их визуализация
            string[] commits = GetCommits(repoPath);
            if (commits != null)
            {
                VisualizeCommits(commits);
            }

            Console.ReadLine();
        }

        public static string ReadRepoUrlFromXml(string xmlFilePath)
        {
            try
            {
                XDocument xmlDoc = XDocument.Load(xmlFilePath);
                XElement urlElement = xmlDoc.Root.Element("RepositoryUrl");
                return urlElement?.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении XML: {ex.Message}");
                return null;
            }
        }

        public static string[] GetCommits(string repoPath)
        {
            string headFilePath = Path.Combine(repoPath, ".git", "HEAD");
            if (!File.Exists(headFilePath))
            {
                Console.WriteLine("HEAD файл не найден. Возможно, это не репозиторий git.");
                return null;
            }

            string headContent = File.ReadAllText(headFilePath).Trim();
            string branchPath = headContent.Split(' ')[1];
            string branchFilePath = Path.Combine(repoPath, ".git", branchPath);

            if (!File.Exists(branchFilePath))
            {
                Console.WriteLine("Файл ветки не найден. Возможно, это не репозиторий git.");
                return null;
            }

            string lastCommitHash = File.ReadAllText(branchFilePath).Trim();
            List<string> commits = new List<string>();

            while (!string.IsNullOrEmpty(lastCommitHash))
            {
                string commitPath = Path.Combine(repoPath, ".git", "objects", lastCommitHash.Substring(0, 2), lastCommitHash.Substring(2));

                if (!File.Exists(commitPath))
                {
                    Console.WriteLine("Файл коммита не найден: " + commitPath);
                    break;
                }

                // Читаем и разжимаем данные коммита
                byte[] commitData = File.ReadAllBytes(commitPath);
                string commitContent = Decompress(commitData);
                string commitMessage = ExtractCommitMessage(commitContent);
                commits.Add($"{lastCommitHash.Substring(0, 7)} {commitMessage}");

                lastCommitHash = GetParentCommitHash(commitContent);
            }

            return commits.ToArray();
        }

        private static string Decompress(byte[] data)
        {
            // Удаляем первые два байта, специфичные для Git-объектов
            using (var compressedStream = new MemoryStream(data, 2, data.Length - 2))
            using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                deflateStream.CopyTo(resultStream);
                return Encoding.UTF8.GetString(resultStream.ToArray());
            }
        }

        private static string ExtractCommitMessage(string commitContent)
        {
            int messageStart = commitContent.IndexOf("\n\n") + 2;
            return commitContent.Substring(messageStart).Trim();
        }

        private static string GetParentCommitHash(string commitContent)
        {
            int parentIndex = commitContent.IndexOf("parent ");
            if (parentIndex == -1) return null;

            int start = parentIndex + "parent ".Length;
            int end = commitContent.IndexOf('\n', start);
            return commitContent.Substring(start, end - start);
        }

        public static void VisualizeCommits(string[] commits)
        {
            Console.WriteLine("flowchart TD");

            string previousCommit = null;
            foreach (var commit in commits)
            {
                string[] parts = commit.Split(new[] { ' ' }, 2);
                string commitId = parts[0];
                string commitMessage = parts[1].Replace("\n", " ");

                Console.WriteLine($"    {commitId}[\"{commitMessage}\"]");

                if (previousCommit != null)
                {
                    Console.WriteLine($"    {commitId} --> {previousCommit}");
                }

                previousCommit = commitId;
            }
        }
    }
}
