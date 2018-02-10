using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jint.Require.Test
{
    [TestClass]
    public class RequireTests
    {
        [TestMethod]
        public void Require()
        {
            var e = new Engine().ImplementRequire(new Settings {RequireFunctionName = "req"});
            var enc = new UTF8Encoding(false);
            File.WriteAllText("sample.json", "{a:1,b:'2',c:3.3,e:[null,true,false,_file_]}", enc);
            File.WriteAllText("sample.js", "let e={f:function(){return _file_;}};e", enc);

            e.Execute("let r = req('sample.js'); var j ={x:r.f(), y:req('sample.json'), z:_file_}; j");
            var json = e.GetCompletionValueJson();
            Debug.WriteLine(json);
            var flat = Regex.Replace(json, @"(\s|"")", "");
            Assert.IsTrue(flat == "{x:sample.js,y:{a:1,b:2,c:3.3,e:[null,true,false,sample.json]},z:sample.json}");
        }

        [TestMethod, ExpectedException(typeof(FileNotFoundException))]
        public void NotFound()
        {
            var e = new Engine().ImplementRequire(new Settings
            {
                LoadFileFunctionName = new Settings().LoadFileFunctionName,
                LoadFileHandler = new Settings().LoadFileHandler,
                RequireFunctionName = new Settings().RequireFunctionName,
            });
            e.Execute("require('404');");
        }
    }
}
