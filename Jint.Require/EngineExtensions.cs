using System;
using System.IO;
using Jint.Parser.Ast;

namespace Jint.Require
{
    public static class EngineExtensions
    {
        public delegate string LoadFileDelagate(string path);
        public static void ImplementRequire(this Engine e, LoadFileDelagate loadFileHandler = null)
        {
            e.SetValue("loadFile", loadFileHandler ?? LoadFile);
            e.Execute(@"function require(file){
                let content = loadFile(file);
                return eval(content);
            }");
        }

        
        private static string LoadFile(string path)
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
