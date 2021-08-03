using System;
using System.Collections.Generic;
using System.IO;

namespace VlcPluginsLicenseLister.Objects
{
    public class MakeFile
    {
        public const string NEW_LINE = "\n";
        public string DirPath { get; private set; }
        public string FilePath { get; private set; }
        public string DirectoryName { get; private set; }
        public string Content { get; private set; }
        public MakeFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File '{filePath}' not found");
            DirPath = Path.GetDirectoryName(filePath);
            FilePath = filePath;
            DirectoryName = new DirectoryInfo(FilePath).Parent.Name.ToLower();
            Content = File.ReadAllText(filePath);
        }
    }
    public class MakeFiles : List<MakeFile>
    {

    }
    public class MakeFilesGetter
    {
        public MakeFiles Get(string vlcPluginsSourcePath)
        {
            MakeFiles makeFiles = new MakeFiles();
            foreach (string filePath in Directory.GetFiles(vlcPluginsSourcePath, "*.am", SearchOption.AllDirectories))
            {
                MakeFile makeFile = new MakeFile(filePath);
                makeFiles.Add(makeFile);
            }
            return makeFiles;
        }
    }
}
