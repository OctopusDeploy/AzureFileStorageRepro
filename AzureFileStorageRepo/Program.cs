using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace AzureFileStorageRepo
{
    class Program
    {
        static void Main(string[] args)
        {
            string srcFilePath = args[0];
            string destFilePath = args[1];

            CopyFileAndVerify(srcFilePath, destFilePath);
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
            return  BitConverter.ToString(cryptoServiceProvider.ComputeHash(fileStream));
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