﻿using StorybrewEditor.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace StorybrewEditor
{
    public class Builder
    {
        public static void Build()
        {
            var archiveName = $"storybrew.{Program.Version.Major}.{Program.Version.Minor}.zip";
            var appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            try
            {
                buildReleaseZip(archiveName, appDirectory);
            }
            catch (Exception e)
            {
                MessageBox.Show($"\nBuild failed:\n\n{e}", Program.FullName);
                return;
            }

            try
            {
                testUpdate(archiveName);
            }
            catch (Exception e)
            {
                MessageBox.Show($"\nUpdate test failed:\n\n{e}", Program.FullName);
                return;
            }

            Trace.WriteLine($"\nOpening {appDirectory}");
            Process.Start(appDirectory);
        }

        private static void buildReleaseZip(string archiveName, string appDirectory)
        {
            Trace.WriteLine($"\n\nBuilding {archiveName}\n");

            var scriptsDirectory = Path.GetFullPath(Path.Combine(appDirectory, "../../../scripts"));

            using (var stream = new FileStream(archiveName, FileMode.Create, FileAccess.ReadWrite))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                addFile(archive, "StorybrewEditor.exe", appDirectory);
                addFile(archive, "StorybrewEditor.exe.config", appDirectory);
                foreach (var path in Directory.EnumerateFiles(appDirectory, "*.dll", SearchOption.TopDirectoryOnly))
                    addFile(archive, path, appDirectory);
                foreach (var path in Directory.EnumerateFiles(scriptsDirectory, "*.cs", SearchOption.TopDirectoryOnly))
                    addFile(archive, path, scriptsDirectory, "scripts");
                archive.CreateEntry(Updater.FirstRunPath);
            }
        }

        private static void testUpdate(string archiveName)
        {
            var previousVersion = $"{Program.Version.Major}.{Program.Version.Minor - 1}";
            var previousArchiveName = $"storybrew.{previousVersion}.zip";
            if (!File.Exists(previousArchiveName))
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("user-agent", Program.Name);
                    webClient.DownloadFile($"https://github.com/{Program.Repository}/releases/download/{previousVersion}/{previousArchiveName}", previousArchiveName);
                }

            var updateTestPath = Path.GetFullPath("updatetest");
            var updateFolderPath = Path.GetFullPath(Path.Combine(updateTestPath, Updater.UpdateFolderPath));
            var executablePath = Path.GetFullPath(Path.Combine(updateFolderPath, "StorybrewEditor.exe"));

            if (Directory.Exists(updateTestPath))
            {
                foreach (var filename in Directory.GetFiles(updateTestPath, "*", SearchOption.AllDirectories))
                    File.SetAttributes(filename, FileAttributes.Normal);
                Directory.Delete(updateTestPath, true);
            }
            Directory.CreateDirectory(updateTestPath);

            ZipFile.ExtractToDirectory(previousArchiveName, updateTestPath);
            ZipFile.ExtractToDirectory(archiveName, updateFolderPath);

            Process.Start(new ProcessStartInfo(executablePath, $"update \"{updateTestPath}\" {previousVersion}")
            {
                WorkingDirectory = updateFolderPath,
            });
        }

        private static void addFile(ZipArchive archive, string path, string sourceDirectory, string targetPath = null)
        {
            path = Path.GetFullPath(path);

            var entryName = PathHelper.GetRelativePath(sourceDirectory, path);
            if (targetPath != null)
            {
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);
                entryName = Path.Combine(targetPath, entryName);
            }

            Trace.WriteLine($"  Adding {path} -> {entryName}");
            var entry = archive.CreateEntryFromFile(path, entryName, CompressionLevel.Optimal);
        }
    }
}