using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace Prog
{
    public static class Mermaind
    {
        static void Main(string[] args)
        {
            // Чтение XML файла
            string xmlFilePath = "config.xml"; // Путь к XML файлу
            string repoUrl = ReadRepoUrlFromXml(xmlFilePath);

            if (string.IsNullOrEmpty(repoUrl))
            {
                Console.WriteLine("Ссылка на репозиторий не найдена в XML.");
                return;
            }

            // Локальная папка для клонирования репозитория
            string localRepoPath = "temp_repo";

            // Клонирование репозитория
            if (Directory.Exists(localRepoPath))
            {

                try
                {
                    DirectoryCleaner.ForceDeleteDirectory(localRepoPath);
                    Console.WriteLine("Директория успешно удалена.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при удалении директории: {ex.Message}");
                }

            }

            CloneRepo(repoUrl, localRepoPath);

            // Получение коммитов и их визуализация
            string[] commits = GetCommits(localRepoPath);
            if (commits != null)
            {
                VisualizeCommits(commits);
            }

            Console.ReadLine();
        }

        // Метод для чтения URL репозитория из XML файла
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

        // Метод для клонирования репозитория через команду git
        public static void CloneRepo(string repoUrl, string localRepoPath)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("git", $"clone {repoUrl} {localRepoPath}");
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;

            using (Process process = Process.Start(processInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine("Ошибка при клонировании репозитория: " + error);
                }
                else
                {
                    Console.WriteLine(output);
                }
            }
        }

        // Метод для получения списка коммитов через команду git
        public static string[] GetCommits(string localRepoPath)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("git", "log --pretty=format:\"%h %s\"")
            {
                WorkingDirectory = localRepoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using (Process process = Process.Start(processInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine("Ошибка при получении коммитов: " + error);
                    return null;
                }
                else
                {
                    return output.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
        }

        // Метод для визуализации коммитов в формате Mermaid
        public static void VisualizeCommits(string[] commits)
        {
            Console.WriteLine("flowchart TD");

            string previousCommit = null;
            foreach (var commit in commits)
            {
                string[] parts = commit.Split(new[] { ' ' }, 2);
                string commitId = parts[0]; // Короткий хеш коммита
                string commitMessage = parts[1].Replace("\n", " "); // Убираем переносы строк

                Console.WriteLine($"    {commitId}[\"{commitMessage}\"]");

                if (previousCommit != null)
                {
                    Console.WriteLine($"    {commitId} --> {previousCommit}");
                }

                previousCommit = commitId;
            }
        }
    }

    public static class DirectoryCleaner
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeleteFile(string lpFileName);

        public static void ForceDeleteDirectory(string path)
        {
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal); // Снимаем атрибуты "только чтение"
                DeleteFile(file); // Используем WinAPI для удаления файлов
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                ForceDeleteDirectory(dir); // Рекурсивно удаляем подкаталоги
            }

            Directory.Delete(path, true); // Удаляем саму директорию
        }
    }
}
