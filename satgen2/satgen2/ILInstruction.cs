using System;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace VRCheat.IL
{
    public struct ILInstruction
    {
        public readonly OpCode OpCode;
        public readonly object Argument;
        public readonly bool HasArgument;
        public readonly bool HasSingleByteArgument;
        public readonly int Length;

        public ILInstruction(OpCode opCode, byte[] ilCode, int index, Module manifest)
        {
            OpCode = opCode;
            HasArgument = opCode.OperandType != OperandType.InlineNone;
            HasSingleByteArgument = OpCodes.TakesSingleByteArgument(opCode);
            Length = opCode.Size + (HasArgument ? HasSingleByteArgument ? 1 : 4 : 0);

            if (HasArgument)
            {
                if (HasSingleByteArgument)
                    Argument = ilCode[index + opCode.Size];
                else
                    Argument = BitConverter.ToInt32(ilCode, index + opCode.Size);

                if (OpCode == OpCodes.Ldstr)
                    Argument = manifest.ResolveString((int)Argument);
                else if (OpCode == OpCodes.Call || OpCode == OpCodes.Callvirt)
                    Argument = manifest.ResolveMethod((int)Argument);
                else if (OpCode == OpCodes.Box)
                    Argument = manifest.ResolveType((int)Argument);
                else if (OpCode == OpCodes.Ldfld || OpCode == OpCodes.Ldflda || 
                         OpCode == OpCodes.Stfld || OpCode == OpCodes.Ldsfld || 
                         OpCode == OpCodes.Stsfld || OpCode == OpCodes.Ldsflda )
                    Argument = manifest.ResolveField((int)Argument);
                else if (OpCode == OpCodes.Newobj)
                    Argument = manifest.ResolveMethod((int) Argument);
            }
            else
                Argument = null;
        }

        public T GetArgument<T>() => (T)Argument;

        public override string ToString()
        {
            string str = string.Empty;

            if (HasArgument)
            {
                if (Argument is int || Argument is byte)
                    str = string.Format(" 0x{0:X}", Argument);
                else if (Argument is string)
                    str = string.Concat(" \"", Argument.ToString(), '"');
                else if (Argument is MethodInfo)
                    str = string.Concat(" ", ((MethodInfo) Argument).FullDescription());
                else if (Argument is ConstructorInfo)
                    str = string.Concat(" ", ((ConstructorInfo) Argument).FullDescription());
                else
                    str = string.Concat(" ", Argument.ToString());
            }

            return string.Format("{0}{1}", OpCode.Name, str);
        }
    }
}