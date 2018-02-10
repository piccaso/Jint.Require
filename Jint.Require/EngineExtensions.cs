using System;
using System.IO;
using Jint.Native;
using Jint.Runtime;

namespace Jint.Require
{
    public delegate string LoadFileDelagate(string path, bool wrapJson = true);
    public class Settings
    {
        public string LoadFileFunctionName { get; set; } = "loadfile";
        public string RequireFunctionName { get; set; } = "require";
        public LoadFileDelagate LoadFileHandler { get; set; } = EngineExtensions.LoadFile;
    }
    public static class EngineExtensions
    {
        
        public static Engine ImplementRequire(this Engine e, Settings settings = null)
        {
            if(settings == null) settings = new Settings();
            e.SetValue(settings.LoadFileFunctionName, settings.LoadFileHandler);
            return e.Execute($@"function {settings.RequireFunctionName}(file){{
                let content = {settings.LoadFileFunctionName}(file, true);
                try {{
                    this._file_ = file;
                    return eval(content);
                    this._file_ = null;
                }}catch(e){{
                    throw 'error evaluating content of ' + file;
                }}
            }}");
        }

        public static string ToJson(this JsValue result, Engine engine)
        {
            if (result.Type != Types.None && result.Type != Types.Null && result.Type != Types.Undefined)
            {
                return TypeConverter.ToString(engine.Json.Stringify(engine.Json, Arguments.From(result, Undefined.Instance, "  ")));   
            }

            return null;
        }

        public static string GetCompletionValueJson(this Engine e)
        {
            return e.GetCompletionValue().ToJson(e);
        }

        
        internal static string LoadFile(string path, bool wrapJson = true)
        {
            var text = File.ReadAllText(path);
            if (wrapJson && Path.GetExtension(path).ToLower().EndsWith(".json"))
            {
                var uid = Guid.NewGuid().ToString().Replace("-", "");
                var id = $"jsonObject_{uid}";
                text = $";let {id}={text};{id}";
            }

            return text;
        }

    }

}
