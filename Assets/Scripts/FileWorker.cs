using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class FileWorker
{
    private const string AndroidDocumentsScheme = "android-documents://";

    private static string EnsureDirectory(string folder)
    {
        string path = Path.Combine(Application.persistentDataPath, folder);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }

    public static string CreateFile(string fileExtension, string folder, string subfolderName = null)
    {
        string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "." + fileExtension;
        string relativePath = BuildAppRelativePath(folder, subfolderName, fileName);

#if UNITY_ANDROID && !UNITY_EDITOR
        return AndroidDocumentsScheme + relativePath;
#else
        string directoryPath = EnsureDirectory(Path.GetDirectoryName(relativePath));
        string fullPath = Path.Combine(directoryPath, Path.GetFileName(relativePath));

        File.WriteAllText(fullPath, "");
        return fullPath;
#endif
    }

    private static string BuildAppRelativePath(string folder, string subfolderName, string fileName)
    {
        var parts = new List<string>
        {
            SanitizePathSegment(Application.productName),
            SanitizePathSegment(folder)
        };

        if (!string.IsNullOrWhiteSpace(subfolderName))
        {
            parts.Add(SanitizePathSegment(subfolderName));
        }

        parts.Add(SanitizePathSegment(fileName));
        return string.Join("/", parts);
    }

    private static string SanitizePathSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Unknown";
        }

        var invalidPathChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (char c in value.Trim())
        {
            if (Array.IndexOf(invalidPathChars, c) >= 0 ||
                c == ':' || c == '*' || c == '?' || c == '"' ||
                c == '<' || c == '>' || c == '|' || char.IsControl(c))
            {
                builder.Append('_');
            }
            else
            {
                builder.Append(c);
            }
        }

        string sanitized = builder.ToString().Trim('.', ' ');
        return string.IsNullOrEmpty(sanitized) ? "Unknown" : sanitized;
    }

    public static void CreateFolder(string folder)
    {
        EnsureDirectory(folder);
    }

    public static bool CheckFolderExists(string folder, out string fullPath)
    {
        fullPath = Path.Combine(Application.persistentDataPath, folder);
        return Directory.Exists(fullPath);
    }

    public static bool CheckFileExists(string fullFilePath)
    {
        return File.Exists(fullFilePath);
    }

    public static List<string> GetItemsInFolder(string folder)
    {
        string path = Path.Combine(Application.persistentDataPath, folder);
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Folder not found: {path}");
        }

        return new List<string>(Directory.GetFiles(path));
    }

    public static byte[] GetFile(string folder, string fileName, out string fullPath)
    {
        fullPath = Path.Combine(Application.persistentDataPath, folder, fileName);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {fullPath}");
        }

        return File.ReadAllBytes(fullPath);
    }
}
