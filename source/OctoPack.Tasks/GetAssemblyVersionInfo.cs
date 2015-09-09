using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OctoPack.Tasks
{
    public class GetAssemblyVersionInfo : AbstractTask
    {

        /// <summary>
        /// Specifies the files the retrieve info from.
        /// </summary>
        [Required]
        public ITaskItem[] AssemblyFiles { get; set; }

        /// <summary>
        /// Forces usage of assembly file version to create package.
        /// </summary>
        [Required]
        public ITaskItem[] ForceUseFileVersion { get; set; }

        /// <summary>
        /// Contains the retrieved version info
        /// </summary>
        [Output]
        public ITaskItem[] AssemblyVersionInfo { get; set; }

        public override bool Execute()
        {
            if (AssemblyFiles.Length <= 0)
            {
                return false;
            }

            var forceAssemblyFileVersion = (ForceUseFileVersion[0].ItemSpec == "true");

            var infos = new List<ITaskItem>();
            foreach (var assemblyFile in AssemblyFiles)
            {
                LogMessage(String.Format("Get version info from assembly: {0}", assemblyFile), MessageImportance.Normal);

                infos.Add(CreateTaskItemFromFileVersionInfo(assemblyFile.ItemSpec, forceAssemblyFileVersion));
            }
            AssemblyVersionInfo = infos.ToArray();
            return true;
        }

        private static TaskItem CreateTaskItemFromFileVersionInfo(string path, bool forceAssemblyFileVersion)
        {
            var info = FileVersionInfo.GetVersionInfo(path);
            var currentAssemblyName = AssemblyName.GetAssemblyName(info.FileName);

            var assemblyVersion = currentAssemblyName.Version;
            var assemblyFileVersion = info.FileVersion;
            var assemblyVersionInfo = info.ProductVersion;

            if (assemblyFileVersion == assemblyVersionInfo && !forceAssemblyFileVersion)
            {
                // Info version defaults to file version, so if they are the same, the customer probably doesn't want to use file version. Instead, use assembly version.
                return new TaskItem(info.FileName, new Hashtable
                {
                    {"Version", assemblyVersion.ToString()},
                });
            }
            
            // If the info version is different from file version, that must be what they want to use
            return new TaskItem(info.FileName, new Hashtable
            {
                {"Version", assemblyVersionInfo},
            });
        }
    }
}
