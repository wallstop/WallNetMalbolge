using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WallNetMalbolge;
using WallNetMalbolge.Generators;

namespace WallNetMalbolgeTest
{
    [TestClass]
    public class MalbolgeInterpreterSpec
    {
        [TestMethod]
        public void HelloWorld()
        {
            const string HelloWorldMalbolge =
                "\'=<A#?!765:z2xw5u3s+*/(-&+$#i\'gf$#zb~}|^zy[qpunsrqSiQgfNMLKJfHdFba`_A]VUTYR:PUNSRQPON0/.JC+G)(D&<$:987<|432765u-2+*)(n&+k)i\'&f|#z!~`|uz\\[qpoWVrkjihmfkMibgI_dcEa`Y^]?UyYXWVUN65Q3ON0FjJ\nC+*)E>C<$#98=65:z276vu3srq)(\'m+k)ih~}${\"yx`|^zsrZYoWmlqpoQPlNdiKaf_dcbaCY}]{U=<XQ9UT6Lp321GF.JC+G)E\'C<;_\"8~}|{9276543s1q/on&+*#i!&}$#\"!~`=^z\\xZvoWVlqSRnglejihJ`_^F\\\"C_XW?U=<R:POT6LpJ2\nNM/.JI+G@E\'&<$#?8~<;49yx65.ts1q/(nm%*#i\'&f|#cy~w_{zsxwpunVrTSinPfed*haIHdcbaCY}WVUTx;QP8TML43O1G/.-,BGF?D&<A#987<5:z2705u3sr*/(-m%k#i\'&}|B/";

            const string HelloWorldMalbolgeWikipedia =
                "(=<`#9]~6ZY32Vx/4Rs+0No-&Jk)\"Fh}|Bcy?`=*z]Kw%oG4UUS0/@-ejc(:\'8dc";

            const string cat =
                "(aBA@?>=<;:9876543210/.-,JH)(\'&%$#\"!~}|{zy\\J6utsrq\r\nponmlkjihgJ%dcba`_^]\\[ZYXWVUTSRQPONMLKJIHGF(\'C%$$^\r\nK~<;4987654321a/.-,\\*)\r\nj\r\n!~%|{zya}|{zyxwvutsrqSonmlO\r\njLhg`edcba`_^]\\[ZYXWV8TSRQ4\r\nONM/KJIBGFE>CBA@?>=<;{9876w\r\n43210/.-m+*)(\'&%$#\"!~}|{zy\\\r\nwvunslqponmlkjihgfedcEa`_^A\r\n\\>ZYXWPUTSRQPONMLKJIH*FEDC&\r\nA@?>=<;:9876543210/.-m+*)(i\r\n&%$#\"!~}|{zyxwvutsrqpRnmlkN\r\nihgfedcba`_^]\\[ZYXWVU7SRQP3\r\nNMLKJIHGFEDCBA@?>=<;:z8765v\r\n3210/.-,+*)(\'&%$#\"!~}_{zyx[\r\nvutsrqjonmlejihgfedcba`_^]@\r\n[ZYXWVUTSRo";

            const string ack = "D\'`%^#\"!m5{XV7USRu,PN).Km8$#(hh}CAcy>a<<)L[Zp6WVrkjong-kMiha`ed]#a`_A@\\[TxXWVUN6LpJOHl/EDIBf@(DCB;_?!~6;492V6/43,Pq).\'&%$Hi!&}$#z@x}vut:xwvotsl2SRngfe+iKJ`ed]b[!_^W?[ZYRvVU7Mq43ImGLKJCg*FEDCBA:^!~<;:3WD";

            LinearGenerator linearGenerator = new LinearGenerator();
            //string malbolgeProgram = linearGenerator.GenerateProgram("A!");

            string output = "";
            MalbolgeInterpreter interpreter = new MalbolgeInterpreter(_ =>
            {
                Console.Write(_);
                output += _;
            }, null);
            interpreter.LoadProgram(ack);
            interpreter.Run();
            Assert.AreEqual("Ack!", output);
        }
    }
}