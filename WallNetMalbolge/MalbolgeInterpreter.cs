using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WallNetMalbolge
{
    public class MalbolgeInterpreter
    {
        public enum Instruction
        {
            Jump,
            Out,
            In,
            Rotate,
            Move,
            Crazy,
            Nop,
            End
        }

        /* Our "trit" space spans [0, 3^10) */
        public const int MaxValue = 59049;
        public const int MinValue = 0;

        /* 3 ^ 9, useful for ternary conversions */
        public const int LeadingOneTrit = 19683;

        /* OpCode lookup */

        public const string NormalizationTable =
            @"+b(29e*j1VMEKLyC})8&m#~W>qxdRp0wkrUo[D7,XTcA""lI.v%{gJh4G\-=O@5`_3i<?Z';FNQuY]szf$!BS/|t:Pn6^Ha";

        private const string EncryptionTable =
            @"5z]&gqtyfr$(we4{WP)H-Zn,[%\3dL+Q;>U!pJS72FhOA1CB6v^=I_0/8|jsb9m<.TVac`uY*MK'X~xDl}REokN:#?G""i@";

        public static readonly int InstructionCount = Enum.GetValues(typeof(Instruction)).Length;

        private static readonly ushort[,] CrazyTable = {{1, 0, 0}, {1, 0, 2}, {2, 2, 1}};

        public ushort A { get; private set; }
        public ushort C { get; private set; }

        public ushort D { get; private set; }

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

        private static ReadOnlyDictionary<char, int> DeNormalizedOpCodes { get; } =
            new ReadOnlyDictionary<char, int>(
                Enum.GetValues(typeof(Instruction))
                    .OfType<Instruction>()
                    .Select(instruction => InstructionsToOpCodes[instruction])
                    .ToDictionary(_ => _, instruction => NormalizationTable.IndexOf(instruction)));

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

        /**
            <summary>
                Converts an OpCode representation of a Malbolge program to it's mangled, "real" form
            </summary>
        */

        public static string DeNormalize(string opCodes)
        {
            StringBuilder normalized = new StringBuilder(opCodes.Length);
            for(int i = 0; i < opCodes.Length; ++i)
            {
                char opCode = opCodes[i];
                Instruction instruction;
                if(OpCodesToInstructions.TryGetValue(opCode, out instruction))
                {
                    int index = DeNormalizedOpCodes[opCode];
                    char malbolgeInstruction = (char) ((index - i) % 94 + 33);
                    normalized.Append(malbolgeInstruction);
                }
            }
            return normalized.ToString();
        }

        public void LoadProgram(string program)
        {
            HardReset();
            // TODO: Validation
            for(int i = 0; i < program.Length; ++i)
            {
                Tape[i] = program[i];
            }
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
                Instruction instruction;
                if(TryGetInstruction(instructions[i], i, out instruction))
                {
                    normalized.Append(InstructionsToOpCodes[instruction]);
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

        public static bool TryGetInstruction(char value, int index, out Instruction instruction)
        {
            char maybeOpCode = NormalizationTable[(value + index - 33) % 94];
            return OpCodesToInstructions.TryGetValue(maybeOpCode, out instruction);
        }

        private void Crazy()
        {
            ushort output = Crazy(A, Tape[D]);
            Tape[D] = output;
            A = output;
        }

        private void Encrypt()
        {
            if((33 <= Tape[C]) && (Tape[C] <= 126))
            {
                Tape[C] = EncryptionTable[Tape[C] - 33];
            }
            else
            {
                // Invalid program
                // TODO: Throw?
                Terminated = true;
            }
        }

        private void End()
        {
            Terminated = true;
        }

        /**
            <summary>
                Resets the machine to a pristine state
            </summary>
        */

        private void HardReset()
        {
            A = 0;
            C = 0;
            D = 0;
            Terminated = false;
            Array.Clear(Tape, 0, Tape.Length);
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

            ushort output = (ushort) (Tape[D] % 3 * LeadingOneTrit + Tape[D] / 3);
            Tape[D] = output;
            A = output;
        }

        private static ushort WrappedIncrement(ushort original, int increment)
        {
            return (ushort) (((original + increment) % MaxValue + MaxValue) % MaxValue);
        }
    }
}