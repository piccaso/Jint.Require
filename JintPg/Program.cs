using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Jint;
using Jint.Native;
using Jint.Parser;
using Jint.Require;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JintPg
{
    class Program
    {
        delegate object IncludeDelegate (string file, string path = "");

        static void Main(string[] args)
        {
            var engine = new Engine(c=>c.Strict(true));
            var jsonConfig = "let y = {prod:11, dev:22}; let x = {opt1: 2*2, opt2: y['prod'], temp: expand('this is temp: %TEMP%'), user: env('USER'), inc:include()}; x";
            jsonConfig = "{a:1,b:2, c:[include(),1,2,3,invoke(function(){cla=3;return cla})]}";
            jsonConfig = "{x:require('foo.js').f()}";

            engine.ImplementRequire(new Settings()
            {
               LoadFileHandler = path => "let x={}; x.f=function(){return 2}; x",
            });
            void Dump(Engine val)
            {
                var json = JToken.FromObject(val.GetCompletionValue().ToObject()).ToString(Formatting.Indented);
                Console.WriteLine(json);
            }

            object Include(string file, string path = null)
            {
                IDictionary<string,object> o = new ExpandoObject();
                dynamic include = new ExpandoObject();

                if (!string.IsNullOrEmpty(file)) include.file = file;
                if(!string.IsNullOrEmpty(path)) include.path = path;
                o["$include"] = include;
                return o;
            }

            engine.SetValue("include", new IncludeDelegate(Include));
            engine.SetValue("expand", new Func<string, string>(Environment.ExpandEnvironmentVariables));
            engine.SetValue("env", new Func<string, string>(Environment.GetEnvironmentVariable));
            engine.Execute("function invoke(f){ return f(); };");

            jsonConfig = jsonConfig.Trim();
            try
            {
                var config = $"$__{Guid.NewGuid().ToString().Replace("-","")}";
                var js = $"let {config} = {jsonConfig}; {config};";
                Console.WriteLine(js);
                engine.Execute(js);
            }
            catch (ParserException)
            {
                engine.Execute(jsonConfig);
            }
            
            Dump(engine);

        }
    }
}
