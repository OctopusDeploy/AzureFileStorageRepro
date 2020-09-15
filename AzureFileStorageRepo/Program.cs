using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace AzureFileStorageRepo
{
    class Program
    {
        private const int _bytesIn10MB = 1024 * 1024 * 10;

        static void Main(string[] args)
        {
            var command = args[0];
            var rootPath = args[1];

            if (command == "fill")
            {
                FillStorage(rootPath);
            }
            else if (command == "repro")
            {
                ExceedQuota(rootPath);
            }
            else
            {
                throw new Exception("Command must be 'fill' or 'repro', and the path to the file share");
            }

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        private static void ExceedQuota(string rootPath)
        {
            var fileName = "ReproFile";
            var srcPath = Path.GetFullPath(fileName);
            File.WriteAllBytes(srcPath, Generate10MBFile());

            CopyFileAndVerify(srcPath, Path.Combine(rootPath, fileName));
        }

        private static void FillStorage(string rootPath)
        {
            var pathForDummyFiles = Path.Combine(rootPath, "dummy");
            var bytesInTotal = 1024 * 1024 * 1024;

            var filesNeeded = bytesInTotal / _bytesIn10MB;

            Directory.CreateDirectory(pathForDummyFiles);
            foreach (var fileNumber in Enumerable.Range(1, filesNeeded))
            {
                var fileName = Path.Combine(pathForDummyFiles, "dummy" + fileNumber);
                if (File.Exists(fileName))
                {
                    Console.WriteLine("Dummy file already exists: " + fileName);
                }
                else
                {
                    Console.WriteLine("Creating dummy file: " + fileName);
                    File.WriteAllBytes(fileName, Generate10MBFile());
                }
            }
        }

        private static byte[] Generate10MBFile()
        {
            return Enumerable.Repeat((byte) 65, _bytesIn10MB).ToArray();
        }

        private static void CopyFileAndVerify(string srcFilePath, string destFilePath)
        {
            PrintFileStats(srcFilePath);

            //This should throw if the file takes us over the quota
            File.Copy(srcFilePath, destFilePath);

            PrintFileStats(destFilePath);
        }

        private static void PrintFileStats(string srcFilePath)
        {
            var fileInfoLength = GetFileSizeFromFileInfo(srcFilePath);
            var streamLength = GetFileSizeFromStream(srcFilePath);
            var hash = GetFileHash(srcFilePath);

            Console.WriteLine("File stats for:      " + srcFilePath);
            Console.WriteLine("- FileInfo length:   " + fileInfoLength);
            Console.WriteLine("- FileStream length: " + streamLength);
            Console.WriteLine("- Hash:              " + hash);
        }

        private static string GetFileHash(string filePath)
        {
            using var fileStream = File.OpenRead(filePath);
            using SHA1CryptoServiceProvider cryptoServiceProvider = new SHA1CryptoServiceProvider();
            return BitConverter.ToString(cryptoServiceProvider.ComputeHash(fileStream));
        }

        private static long GetFileSizeFromStream(string filePath)
        {
            //fileSystem.GetFileSize() which calls FileInfo.Length can give the wrong result if the file gets truncated in Azure Storage
            using var fileStream = File.OpenRead(filePath);
            return fileStream.Length;
        }

        private static long GetFileSizeFromFileInfo(string filePath)
        {
            return new FileInfo(filePath).Length;
        }
    }
}