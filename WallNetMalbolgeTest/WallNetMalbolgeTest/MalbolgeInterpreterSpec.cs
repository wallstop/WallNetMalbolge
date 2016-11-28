using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WallNetMalbolge;

namespace WallNetMalbolgeTest
{
    [TestClass]
    public class MalbolgeInterpreterSpec
    {
        [TestMethod]
        public void HelloWorld()
        {
            const string HelloWorldWikipedia = @"(=<`#9]~6ZY32Vx/4Rs+0No-&Jk)""Fh}|Bcy?`=*z]Kw%oG4UUS0/@-ejc(:'8dc";
            string output = "";
            MalbolgeInterpreter interpreter = new MalbolgeInterpreter(_ =>
            {
                Console.Write(_);
                output += _;
            }, null);
            interpreter.LoadProgram(HelloWorldWikipedia);
            interpreter.Run();
            Assert.AreEqual("Hello World!", output);
        }
    }
}