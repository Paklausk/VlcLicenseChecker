using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VlcPluginsLicenseLister.Objects
{
    public class PluginsSourcePathsFinder
    {
        const string VAR_PREFIX = "SOURCES_", VAR_POSTFIX = "_la_SOURCES";
        Regex _filePathFinder = new Regex(@"[\w\-.\/]+");
        public IEnumerable<string> Find(Plugin plugin, MakeFiles makeFiles, out string dirPath)
        {
            foreach (MakeFile makeFile in makeFiles)
            {
                dirPath = makeFile.DirPath;
                IEnumerable<string> sourcePathsResult = SearchMakeFile(plugin, makeFile);
                bool foundPluginSourcePaths = sourcePathsResult.Count() > 0;
                if (foundPluginSourcePaths)
                    return sourcePathsResult;
            }
            dirPath = "";
            return new string[0];
        }
        public void FindAndSet(Plugin plugin, MakeFiles makeFiles, string vlcPluginsSourcePath)
        {
            string dirPath;
            IEnumerable<string> relativeFilePaths = Find(plugin, makeFiles, out dirPath);
            string[] searchPaths = new string[] { dirPath, vlcPluginsSourcePath };
            plugin.SetSourcePath(relativeFilePaths, searchPaths);
        }
        public void FindAndSet(Plugins plugins, MakeFiles makeFiles, string vlcPluginsSourcePath)
        {
            plugins.ForEach((plugin) => FindAndSet(plugin, makeFiles, vlcPluginsSourcePath));
        }
        private IEnumerable<string> SearchMakeFile(Plugin plugin, MakeFile makeFile)
        {
            string[] possibleVariableNames = new string[] {
                plugin.FileNameWithoutExtension + VAR_POSTFIX,
                VAR_PREFIX + plugin.FileNameWithoutExtension.Substring(3, plugin.FileNameWithoutExtension.Length - 3 - 7)
            };
            IEnumerable<string> sourcePaths = new List<string>();
            foreach (var varName in possibleVariableNames)
            {
                sourcePaths = SearchVariable(varName, makeFile);
                if (sourcePaths.Count() > 0)
                    break;
            }
            return sourcePaths;
        }
        private IEnumerable<string> SearchVariable(string variableName, MakeFile makeFile)
        {
            int varIndex = makeFile.Content.IndexOf(variableName);
            List<string> sourcePaths = new List<string>();
            if (varIndex >= 0)
            {
                int valueStartIndex = makeFile.Content.IndexOf('=', varIndex + variableName.Length);
                while ((makeFile.Content[valueStartIndex] == ' ' || makeFile.Content[valueStartIndex] == '=') && valueStartIndex < makeFile.Content.Length)
                    valueStartIndex++;
                int valueEndIndex = makeFile.Content.IndexOf(MakeFile.NEW_LINE, valueStartIndex);
                while (makeFile.Content[valueEndIndex - 1] == '\\')
                    valueEndIndex = makeFile.Content.IndexOf(MakeFile.NEW_LINE, valueEndIndex + 2);
                string varValue = makeFile.Content.Substring(valueStartIndex, valueEndIndex - valueStartIndex);
                if (IsConstant(varValue))
                {
                    string constantName = RemoveConstants(varValue);
                    return SearchVariable(constantName, makeFile);
                }
                var matches = _filePathFinder.Matches(varValue);
                for (int i = 0; i < matches.Count; i++)
                    if (matches[i].Success)
                        sourcePaths.Add(matches[i].Value.Replace('/', '\\'));
            }
            return sourcePaths;
        }
        private bool IsConstant(string value)
        {
            return value.IndexOf("$(") >= 0;
        }
        private string RemoveConstants(string value)
        {
            string valueWithoutConstants = value, startString = "$(";
            char endString = ')';
            int constStartIndex = -1, constEndIndex = 0;
            while ((constStartIndex = valueWithoutConstants.IndexOf(startString)) >= 0)
            {
                constStartIndex += startString.Length;
                constEndIndex = valueWithoutConstants.IndexOf(endString, constStartIndex);
                if (constEndIndex < 0)
                    constEndIndex = valueWithoutConstants.Length - 1;
                valueWithoutConstants = valueWithoutConstants.Substring(constStartIndex, constEndIndex - constStartIndex);
            }
            return valueWithoutConstants;
        }
    }
    public class PluginsLicenseFinder
    {
        Regex _lgplChecker = new Regex(@"\*.+GNU Lesser General.+"),
            _gplChecker = new Regex(@"\*.+GNU General.+"),
            _mitChecker = new Regex(@"\*.+MIT License.+");
        public License Find(Plugin plugin)
        {
            License license = License.Unknown;
            if (plugin.HasSourcePath)
                foreach (var sourcePath in plugin.SourcePaths)
                    foreach (var searchSourcePath in plugin.SearchSourcePaths)
                    {
                        string fullSourcePath = Path.Combine(searchSourcePath, sourcePath);
                        if (File.Exists(fullSourcePath))
                        {
                            string sourceContent = File.ReadAllText(fullSourcePath);
                            if (_gplChecker.IsMatch(sourceContent))
                                license = License.GPL;
                            else if (_lgplChecker.IsMatch(sourceContent))
                                license = License.LGPL;
                            else if (_mitChecker.IsMatch(sourceContent))
                                license = License.MIT;
                            if (license != License.Unknown)
                                return license;
                            break;
                        }
                    }
            return license;
        }
        public void FindAndSet(Plugin plugin)
        {
            plugin.SetLicense(Find(plugin));
        }
        public void FindAndSet(Plugins plugins)
        {
            plugins.ForEach((plugin) => FindAndSet(plugin));
        }
    }
}
