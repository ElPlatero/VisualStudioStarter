using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace VisualStudioStarter
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VisualStudioOptions 
    {
        private const string VS_VERSION_INFOTEXT = "Microsoft Visual Studio Solution File, Format Version ";
        private const string VS_VERSION = "# Visual Studio ";

        public ICollection<VisualStudioInfo> Studios { get; set; }
        public ICollection<RunnerInfo> Runners { get; set; }

        public string GetExecutable(string slnPath)
        {
            var lines = File.ReadAllLines(slnPath);

            var formatVersionLine = lines.FirstOrDefault(p => p.StartsWith(VS_VERSION_INFOTEXT));
            if(formatVersionLine == null) throw new InvalidOperationException("Formatversion nicht gefunden.");

            var versionLine = lines.FirstOrDefault(p => p.StartsWith(VS_VERSION));
            if (versionLine == null) throw new InvalidOperationException("Studioversion nicht gefunden.");

            var formatVersion = formatVersionLine.Substring(VS_VERSION_INFOTEXT.Length);
            var version = Regex.Match(versionLine, VS_VERSION + @"(Version ){0,1}(?<version>.*)").Groups["version"].Value;

            var runner = Runners.FirstOrDefault(p => StringComparer.CurrentCultureIgnoreCase.Equals(p.FormatVersion, formatVersion) && StringComparer.CurrentCultureIgnoreCase.Equals(p.VisualStudioString, version));
            if (runner == null) throw new InvalidOperationException($"Version nicht konfiguriert: {formatVersion} / VS {version}.");
            if (Studios.All(p => p.Ident != runner.Studio)) throw new InvalidOperationException($"Studio nicht konfiguriert: {runner.Studio}.");

            return Studios.First(p => p.Ident == runner.Studio).ExePath;
        }

        public class RunnerInfo
        {
            public string FormatVersion { get; set; }
            public string VisualStudioString { get; set; }
            public string Studio { get; set; }
        }

        public class VisualStudioInfo
        {
            public string Ident { get; set; }
            public string ExePath { get; set; }
        }
    }



}
