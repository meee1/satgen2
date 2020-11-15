using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace VRCheat.IL
{
    public static class ILParser
    {
        private static readonly OpCode[] OpCodes = new OpCode[256];
        private static readonly OpCode[] MultiOpCodes = new OpCode[31];

        static ILParser()
        {
            foreach (FieldInfo field in typeof(OpCodes).GetFields())
            {

                OpCode opCode = (OpCode)field.GetValue(null);

                if (opCode.Size == 1)
                    OpCodes[opCode.Value] = opCode;
                else
                    MultiOpCodes[opCode.Value & 0xFF] = opCode;
            }
        }

        public static ILInstruction[] Parse(this MethodInfo method)
            => Parse(method.GetMethodBody().GetILAsByteArray(), method.DeclaringType.Assembly.ManifestModule);

        public static ILInstruction[] Parse(this MethodBase methodBase)
            => Parse(methodBase.GetMethodBody().GetILAsByteArray(), methodBase.Module);

        public static ILInstruction[] Parse(this MethodBody methodBody, Module manifest)
            => Parse(methodBody.GetILAsByteArray(), manifest);

        private static ILInstruction[] Parse(byte[] ilCode, Module manifest)
        {
            List<ILInstruction> instructions = new List<ILInstruction>();

            for (int i = 0; i < ilCode.Length; i++)
            {
                ILInstruction instruction = new ILInstruction(ilCode[i] == 0xFE ? MultiOpCodes[ilCode[i + 1]] : OpCodes[ilCode[i]], ilCode, i, manifest);
                instructions.Add(instruction);

                i += instruction.Length - 1;
            }

            return instructions.ToArray();
        }
    }
}