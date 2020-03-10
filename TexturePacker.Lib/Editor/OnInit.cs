using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TexturePacker.Lib.UnityEditor.Properties;
using UnityEditor;

namespace TexturePacker.Lib.Editor
{
    [InitializeOnLoad]
    public static class OnInit
    {
        static OnInit()
        {
            string executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
            string assetPath;

            do
            {
                var files = Directory.GetFiles(executingAssemblyLocation);
                if (files.Any(file => file.EndsWith(".csproj")))
                    break;
            }
            while (!string.IsNullOrEmpty(executingAssemblyLocation = Path.GetDirectoryName(executingAssemblyLocation)));

            if (string.IsNullOrEmpty(executingAssemblyLocation))
                throw new Exception("Couldn't locate Assets path!");

            assetPath = Path.Combine(executingAssemblyLocation, "Assets");

            string rspFile = Path.Combine(assetPath, "mcs.rsp");

            if (!File.Exists(rspFile))
                File.WriteAllBytes(rspFile, Resources.mcs);

#if DEBUG
            File.WriteAllText(Path.Combine(assetPath, "TEST.txt"), "TEST");
#endif
        }
    }
}