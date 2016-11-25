using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WallNetMalbolge.Generators
{
    public sealed class LinearGenerator : IGenerator
    {
        private const int NumOpCodes = 3;

        private const int MutationLength = 5;

        public static readonly char TerminalOpCode =
            MalbolgeInterpreter.InstructionsToOpCodes[MalbolgeInterpreter.Instruction.End];

        public static readonly char PrintOpCode =
            MalbolgeInterpreter.InstructionsToOpCodes[MalbolgeInterpreter.Instruction.Out];

        private static readonly Random RGen = new Random();

        public static ReadOnlyCollection<char> OpCodes { get; } =
            new ReadOnlyCollection<char>(new List<char>(NumOpCodes)
            {
                MalbolgeInterpreter.InstructionsToOpCodes[MalbolgeInterpreter.Instruction.Nop],
                MalbolgeInterpreter.InstructionsToOpCodes[MalbolgeInterpreter.Instruction.Crazy],
                MalbolgeInterpreter.InstructionsToOpCodes[MalbolgeInterpreter.Instruction.Rotate]
            });

        private static ReadOnlyCollection<string> ForcedOpCodeMutations { get; }

        private static ReadOnlyCollection<string> OpCodeMutations { get; }

        static LinearGenerator()
        {
            List<string> opCodeMutations = new List<string>();
            for(int i = 0; i < MutationLength; ++i)
            {
                foreach(string opCodeMutation in opCodeMutations.ToArray())
                {
                    foreach(char opCode in OpCodes)
                    {
                        opCodeMutations.Add(opCodeMutation + opCode);
                    }
                }
                foreach(char opCode in OpCodes)
                {
                    opCodeMutations.Add(opCode.ToString());
                }
            }
            opCodeMutations.Reverse(); // Shortest first
            OpCodeMutations = new ReadOnlyCollection<string>(opCodeMutations);
            ForcedOpCodeMutations =
                new ReadOnlyCollection<string>(OpCodeMutations.Where(_ => _.Length == MutationLength).ToList());
        }

        public string GenerateOpCodes(string target)
        {
            StringBuilder outputBuilder = new StringBuilder();
            MalbolgeInterpreter interpreter = new MalbolgeInterpreter(output => outputBuilder.Append(output), null);
            StringBuilder programAsOpCodes = new StringBuilder();

            for(int i = 1; i <= target.Length; ++i)
            {
                string currentTarget = target.Substring(0, i);
                bool found = false;
                while(!found)
                {
                    StringBuilder currentProgram = new StringBuilder(programAsOpCodes.ToString());
                    foreach(string opCodeMutation in OpCodeMutations)
                    {
                        currentProgram.Append(opCodeMutation);
                        currentProgram.Append(PrintOpCode);
                        currentProgram.Append(TerminalOpCode);
                        string malbolgeProgram = MalbolgeInterpreter.DeNormalize(currentProgram.ToString());
                        interpreter.LoadProgram(malbolgeProgram);
                        interpreter.Run();
                        string currentOutput = outputBuilder.ToString();
                        if(currentTarget.Equals(currentOutput))
                        {
                            programAsOpCodes.Append(opCodeMutation);
                            programAsOpCodes.Append(PrintOpCode);
                            found = true;
                            Console.Write(target[i]);
                            break;
                        }
                        int currentAttemptLength = opCodeMutation.Length + 2;
                        currentProgram.Remove(currentProgram.Length - currentAttemptLength, currentAttemptLength);
                    }
                    if(!found)
                    {
                        // If none found, just pick one at random, it'll be fine.
                        int randomIndex = RGen.Next(ForcedOpCodeMutations.Count);
                        string opCodeMutation = ForcedOpCodeMutations[randomIndex];
                        programAsOpCodes.Append(opCodeMutation);
                    }
                }
            }

            programAsOpCodes.Append(TerminalOpCode);
            return programAsOpCodes.ToString();
        }

        public string GenerateProgram(string target)
        {
            return MalbolgeInterpreter.DeNormalize(GenerateOpCodes(target));
        }
    }
}