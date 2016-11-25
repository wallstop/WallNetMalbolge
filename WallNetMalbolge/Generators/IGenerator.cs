namespace WallNetMalbolge.Generators
{
    internal interface IGenerator
    {
        string GenerateOpCodes(string target);

        string GenerateProgram(string target);
    }
}