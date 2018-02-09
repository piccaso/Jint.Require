using System;
using System.IO;
using Jint.Parser.Ast;

namespace Jint.Require
{
    public delegate string LoadFileDelagate(string path);
    public class Settings
    {
        public string LoadFileFunctionName { get; set; } = "loadfile";
        public string RequireFunctionName { get; set; } = "require";
        public LoadFileDelagate LoadFileHandler { get; set; } = EngineExtensions.LoadFile;
    }
    public static class EngineExtensions
    {
        
        public static void ImplementRequire(this Engine e, Settings settings = null)
        {
            if(settings == null) settings = new Settings();
            e.SetValue(settings.LoadFileFunctionName, settings.LoadFileHandler);
            e.Execute($@"function {settings.RequireFunctionName}(file){{
                let content = {settings.LoadFileFunctionName}(file);
                return eval(content);
            }}");
        }

        
        internal static string LoadFile(string path)
        {
            var text = File.ReadAllText(path);
            if (Path.GetExtension(path).ToLower().EndsWith("json"))
            {
                var uid = Guid.NewGuid().ToString().Replace("-", "");
                var id = $"jsonObject_{uid}";
                text = $";var {id}={text};{id}";
            }

            return text;
        }

    }
}
