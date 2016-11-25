using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WallNetMalbolge
{
    public class MalbolgeInterpreter
    {
        public enum Instruction
        {
            Jump = 4,
            Out = 5,
            In = 23,
            Rotate = 39,
            Move = 40,
            Crazy = 62,
            Nop = 68,
            End = 81
        }

        public const int MaxValue = 59049;
        public const int MinValue = 0;

        public const int LeadingOneTrit = 19683;

        public const string NormalizationTable =
            "+b(29e*j1VMEKLyC})8&m#~W>qxdRp0wkrUo[D7,XTcA\\\"lI.v%{gJh4G\\\\-=O@5`_3i<?Z\\\';FNQuY]szf$!BS/|t:Pn6^Ha";

        private const string EncryptionTable =
            "9m<.TVac`uY*MK\'X~xDl}REokN:#?G\"i@5z]&gqtyfr$(we4{WP)H-Zn,[%\\3dL+Q;>U!pJS72FhOA1CB6v^=I_0/8|jsb";

        private const string OtherEncryptionTable =
            "5z]&gqtyfr$(we4{WP)H-Zn,[%\\\\3dL+Q;>U!pJS72FhOA1CB6v^=I_0/8|jsb9m<.TVac`uY*MK\\\'X~xDl}REokN:#?G\\\"i@";

        public static readonly int InstructionCount = Enum.GetValues(typeof(Instruction)).Length;

        private static readonly ushort[,] CrazyTable = {{1, 0, 0}, {1, 0, 2}, {2, 2, 1}};

        public static ReadOnlyDictionary<Instruction, char> InstructionsToOpCodes { get; } =
            new ReadOnlyDictionary<Instruction, char>(new Dictionary<Instruction, char>(InstructionCount)
            {
                {Instruction.Jump, 'i'},
                {Instruction.Out, '<'},
                {Instruction.In, '/'},
                {Instruction.Rotate, '*'},
                {Instruction.Move, 'j'},
                {Instruction.Crazy, 'p'},
                {Instruction.Nop, 'o'},
                {Instruction.End, 'v'}
            });

        public static ReadOnlyDictionary<char, Instruction> OpCodesToInstructions { get; } =
            new ReadOnlyDictionary<char, Instruction>(new Dictionary<char, Instruction>(InstructionCount)
            {
                {'i', Instruction.Jump},
                {'<', Instruction.Out},
                {'/', Instruction.In},
                {'*', Instruction.Rotate},
                {'j', Instruction.Move},
                {'p', Instruction.Crazy},
                {'o', Instruction.Nop},
                {'v', Instruction.End}
            });

        private ushort A { get; set; }
        private ushort C { get; set; }

        private Action CurrentOperation
        {
            get
            {
                char maybeInstruction = NormalizationTable[(Tape[C] - 33 + C) % 94];
                Instruction instruction;
                if(OpCodesToInstructions.TryGetValue(maybeInstruction, out instruction))
                {
                    return Operations[instruction];
                }
                return Nop;
            }
        }

        private ushort D { get; set; }

        private Dictionary<Instruction, Action> Operations { get; }

        private Action<char> Print { get; }

        private ushort[] Tape { get; }

        private bool Terminated { get; set; }
        private Func<int> UserInput { get; }

        public MalbolgeInterpreter() : this(null, null) {}

        public MalbolgeInterpreter(Action<char> print, Func<int> userInput)
        {
            if(ReferenceEquals(print, null))
            {
                Print = Console.Write;
            }
            else
            {
                Print = print;
            }
            if(ReferenceEquals(userInput, null))
            {
                UserInput = Console.Read;
            }
            else
            {
                UserInput = userInput;
            }

            Tape = new ushort[MaxValue];
            Operations = new Dictionary<Instruction, Action>(InstructionCount)
            {
                {Instruction.Jump, Jump},
                {Instruction.Out, Out},
                {Instruction.In, In},
                {Instruction.Rotate, Rotate},
                {Instruction.Move, Move},
                {Instruction.Crazy, Crazy},
                {Instruction.Nop, Nop},
                {Instruction.End, End}
            };
            Terminated = false;
        }

        public static ushort Crazy(ushort left, ushort right)
        {
            ushort output = 0;

            ushort ternaryDigit = 1;
            for(int i = 0; i < 10; ++i, ternaryDigit *= 3)
            {
                int leftIndex = left / ternaryDigit % 3;
                int rightIndex = right / ternaryDigit % 3;
                ushort outputValue = CrazyTable[rightIndex, leftIndex];
                output += (ushort) (ternaryDigit * outputValue);
            }
            return output;
        }

        /* Opcodes -> Actual Malbolge program */

        public static string DeNormalize(string opCodes)
        {
            StringBuilder normalized = new StringBuilder(opCodes.Length);
            for(int i = 0; i < opCodes.Length; ++i)
            {
                char opCode = opCodes[i];
                Instruction instruction;
                if(OpCodesToInstructions.TryGetValue(opCode, out instruction))
                {
                    int index = NormalizationTable.IndexOf(opCode);
                    char malbolgeInstruction = (char) ((index - i) % 94 + 33);
                    normalized.Append(malbolgeInstruction);
                }
            }
            return normalized.ToString();
        }

        public void LoadProgram(string program)
        {
            Terminated = false;
            // TODO: Validation
            for(int i = 0; i < program.Length; ++i)
            {
                Tape[i] = program[i];
            }
            Array.Clear(Tape, program.Length, Tape.Length - program.Length);
            for(int i = program.Length; i < Tape.Length; ++i)
            {
                Tape[i] = Crazy(Tape[WrappedIncrement((ushort) i, -2)], Tape[WrappedIncrement((ushort) i, -1)]);
            }
        }

        /* Actual Malbolge Program -> Opcodes */

        public static string Normalize(string instructions)
        {
            StringBuilder normalized = new StringBuilder(instructions.Length);
            for(int i = 0; i < instructions.Length; ++i)
            {
                int maybeInstructionIndex = NormalizationTable[(instructions[i] + i - 33) % 94];
                Instruction? maybeInstruction = InstructionFor(maybeInstructionIndex);
                if(maybeInstruction.HasValue)
                {
                    normalized.Append(InstructionsToOpCodes[maybeInstruction.Value]);
                }
            }
            return normalized.ToString();
        }

        public void Run()
        {
            while(!Terminated)
            {
                StepOnce();
            }
        }

        public void StepOnce()
        {
            if(Terminated)
            {
                return;
            }

            Action currentOperation = CurrentOperation;
            currentOperation();
            Encrypt();
            IncrementProgramAndDataCounters();
        }

        private void Crazy()
        {
            ushort output = Crazy(A, Tape[D]);
            Tape[D] = output;
            A = output;
        }

        private void Encrypt()
        {
            //ushort index = currentOperation != Jump ? C : WrappedIncrement(C, -1);
            if((33 <= Tape[C]) && (Tape[C] <= 126))
            {
                Tape[C] = OtherEncryptionTable[Tape[C] - 33];
            }

            // TODO: Throw if not above?
        }

        private void End()
        {
            Terminated = true;
        }

        private void In()
        {
            A = unchecked ((ushort) UserInput());
        }

        private void IncrementProgramAndDataCounters()
        {
            C = WrappedIncrement(C, 1);
            D = WrappedIncrement(D, 1);
        }

        private static Instruction? InstructionFor(int maybeInstruction)
        {
            if(Enum.IsDefined(typeof(Instruction), maybeInstruction))
            {
                Instruction instruction = (Instruction) maybeInstruction;
                return instruction;
            }
            return null;
        }

        private void Jump()
        {
            C = Tape[D];
        }

        private void Move()
        {
            D = Tape[D];
        }

        private void Nop() {}

        private void Out()
        {
            Print((char) (A % 256));
        }

        private void Rotate()
        {
            /* 
                We could theoretically turn these values into ternary and all that, but some simple math saves the day.
                In order to rotate in ternary, all we need to do is take the last ternary value and pop it at the "head"
                of the value in ternary, shifting everything down.
                So, recap:
                    * Get last ternary value
                    * Shift everything else down
                    * Replace leading 0 with the previously grabbed "last" ternary value

                We can get the last ternary value by num % 3.
                We can shift everything else down by num /= 3;
                We can replace leading zero with num += (ternaryDigit * LeadingOneTrit)

                Ez, no translation required.
            */

            ushort currentValue = Tape[D];
            ushort lastTernaryValue = (ushort) (currentValue % 3);
            currentValue /= 3;
            currentValue += (ushort) (lastTernaryValue * LeadingOneTrit);
            Tape[D] = currentValue;
            A = currentValue;
        }

        private static ushort WrappedIncrement(ushort original, int increment)
        {
            while(increment < 0)
            {
                increment += MaxValue;
            }
            return (ushort) ((original + increment) % MaxValue);
        }
    }
}