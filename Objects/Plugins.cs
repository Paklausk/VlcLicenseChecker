using System;
using System.IO;
using System.Collections.Generic;

namespace VlcPluginsLicenseLister.Objects
{
    public class Plugin
    {
        public string FullPath { get; private set; }
        public string RelativePath { get; private set; }
        public string DirectoryName { get; private set; }
        public string FileName { get; private set; }
        public string FileNameWithoutExtension { get; private set; }
        public List<string> SearchSourcePaths { get; private set; } = new List<string>();
        public List<string> SourcePaths { get; private set; } = new List<string>();
        public bool HasSourcePath { get { return SourcePaths.Count > 0; } }
        public License License { get; private set; } = License.Unknown;
        public Plugin()
        { }
        public Plugin(string fullPath, string vlcPluginsPath)
        {
            SetPath(fullPath, vlcPluginsPath);
        }
        public void SetPath(string fullPath, string vlcPluginsPath)
        {
            FullPath = fullPath;
            RelativePath = fullPath.ToLower().Replace(vlcPluginsPath.ToLower(), "");
            FileName = Path.GetFileName(fullPath);
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPath);
            DirectoryName = Path.GetDirectoryName(RelativePath).ToLower();
        }
        public void SetSourcePath(IEnumerable<string> relativeSourcePaths, IEnumerable<string> searchPaths)
        {
            SourcePaths.Clear();
            foreach (string relativeSourcePath in relativeSourcePaths)
                if (!string.IsNullOrEmpty(relativeSourcePath))
                    SourcePaths.Add(relativeSourcePath);
            SearchSourcePaths.Clear();
            foreach (string searchPath in searchPaths)
                if (!string.IsNullOrEmpty(searchPath))
                    SearchSourcePaths.Add(searchPath);
        }
        public void SetLicense(License license)
        {
            License = license;
        }
    }
    public class Plugins : List<Plugin>
    {

    }
    public class PluginsGetter
    {
        public Plugins Get(string vlcPluginsPath)
        {
            Plugins plugins = new Plugins();
            foreach (string filePath in Directory.GetFiles(vlcPluginsPath, "*.dll", SearchOption.AllDirectories))
            {
                Plugin plugin = new Plugin(filePath, vlcPluginsPath);
                plugins.Add(plugin);
            }
            return plugins;
        }
    }
}
