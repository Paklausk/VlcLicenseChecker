using System;
using System.IO;
using System.Linq;
using System.Text;
using VlcPluginsLicenseLister.Objects;

namespace VlcPluginsLicenseLister
{
    class Program
    {
        static void Main(string[] args)
        {
            string vlcPluginsSourcePath = GetArgument(args, 0, @"VLC plugins source path set to: ", @"Type in VLC plugins source path(e.g. D:\files\vlc-3.0-3.0.11\modules\):");
            string vlcPluginsPath = GetArgument(args, 1, @"VLC plugins path set to: ", @"Type in VLC plugins path(e.g. C:\Program Files\VLC\plugins\):");
            Console.WriteLine($"Processing...");
            MakeFiles makeFiles = new MakeFilesGetter().Get(vlcPluginsSourcePath);
            Plugins plugins = new PluginsGetter().Get(vlcPluginsPath);
            new PluginsSourcePathsFinder().FindAndSet(plugins, makeFiles, vlcPluginsSourcePath);
            new PluginsLicenseFinder().FindAndSet(plugins);
            int hasSource = 0, hasNoSource = 0, totalPlugins = plugins.Count, unknownLic = 0, gpl = 0, lgpl = 0, mit = 0;
            StringBuilder fileSb = new StringBuilder();
            Console.WriteLine($"UNKNOWN LICENSE PLUGINS:");
            foreach (var plugin in plugins)
            {
                if (plugin.HasSourcePath) hasSource++; else hasNoSource++;
                fileSb.AppendLine($"{plugin.RelativePath} {plugin.License}");
                switch (plugin.License)
                {
                    case License.Unknown:
                        Console.WriteLine($"{plugin.RelativePath}");
                        unknownLic++;
                        break;
                    case License.GPL:
                        gpl++;
                        break;
                    case License.LGPL:
                        lgpl++;
                        break;
                    case License.MIT:
                        mit++;
                        break;
                }
            }
            File.WriteAllText("VlcPluginsLicense.txt", fileSb.ToString());
            Console.WriteLine();
            Console.WriteLine($"hasSource={hasSource} hasNoSource={hasNoSource} total={totalPlugins}");
            Console.WriteLine($"Unknown={unknownLic} GPL={gpl} LGPL={lgpl} MIT={mit}");
            if (gpl + lgpl + mit == 0)
                Console.WriteLine("Because there were no plugin licenses found possibly wrong plugins or plugin sources paths were chosen.");
            if (hasNoSource > 0)
                Console.WriteLine("Not all plugins have a source possibly different plugins and plugin sources versions were used.");
            if (gpl > 0)
            {
                Console.Write("Delete all GPL plugins? Type 'yes' or 'y' to confirm: ");
                var result = Console.ReadLine().ToLowerInvariant();
                var confirmed = result.Equals("yes") || result.Equals("y");
                if (confirmed)
                {
                    var gplPlugins = plugins.Where(plugin => plugin.License.Equals(License.GPL));
                    foreach (var gplPlugin in gplPlugins)
                    {
                        File.Delete(gplPlugin.FullPath);
                    }
                    Console.Write($"{gplPlugins.Count()} GPL plugins deleted.");
                }
            }
            Console.ReadKey(true);
        }
        static string GetArgument(string[] args, int argumentIndex, string found, string notFound)
        {
            string arg;
            if (args.Length > argumentIndex)
            {
                arg = args[argumentIndex];
                Console.WriteLine(found + arg);
            }
            else
            {
                Console.WriteLine(notFound);
                arg = Console.ReadLine();
            }
            return arg;
        }
    }
}
