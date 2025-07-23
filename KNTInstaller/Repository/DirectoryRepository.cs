using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNTInstaller.Repository
{
    public class DirectoryRepository
    {
        PowerShellRepository _powerShellRepository;

        public DirectoryRepository(PowerShellRepository powerShellRepository)
        {
            _powerShellRepository = powerShellRepository;
        }

        /// <summary>
        /// Extract zip files and copy to direcotry
        /// </summary>
        /// <param name="zipFile">Example: SMM.zip</param>
        /// <param name="extractPath">Example: C:\Programs\</param>
        public void ExtractZip(string zipFile, string extractPath, bool archiveVersion = true)
        {
            if (archiveVersion)
                ArchiveExistingVersion(extractPath);

            var zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", zipFile);
            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }

        void ArchiveExistingVersion(string fullPath)
        {
            if (!Directory.Exists(fullPath))
                return;

            var newFullPath = $"{fullPath}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            Directory.Move(fullPath, newFullPath);
        }

        public void Unrestrict()
        {
            _powerShellRepository.RunPowerShell(@"Set_Edge_Lnk_unrestricted.ps1");
        }


        /*
        void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        */
    }
}
