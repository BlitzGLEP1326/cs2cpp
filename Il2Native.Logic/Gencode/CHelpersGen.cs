﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LlvmHelpersGen.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Il2Native.Logic.Gencode
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using CodeParts;
    using Exceptions;

    using Il2Native.Logic.Gencode.SynthesizedMethods;

    using InternalMethods;
    using PEAssemblyReader;
    using OpCodesEmit = System.Reflection.Emit.OpCodes;

    /// <summary>
    /// </summary>
    public static class CHelpersGen
    {
        public static IType GetIntTypeByBitSize(this ICodeWriter codeWriter, int bitSize)
        {
            IType toType = null;
            switch (bitSize)
            {
                case 1:
                    toType = codeWriter.System.System_Boolean;
                    break;
                case 8:
                    toType = codeWriter.System.System_SByte;
                    break;
                case 16:
                    toType = codeWriter.System.System_Int16;
                    break;
                case 32:
                    toType = codeWriter.System.System_Int32;
                    break;
                case 64:
                    toType = codeWriter.System.System_Int64;
                    break;
            }

            return toType;
        }

        /// <summary>
        /// </summary>
        /// <param name="codeWriterer">
        /// </param>
        /// <param name="byteSize">
        /// </param>
        /// <returns>
        /// </returns>
        public static IType GetIntTypeByByteSize(this ICodeWriter codeWriter, int byteSize)
        {
            IType toType = null;
            switch (byteSize)
            {
                case 1:
                    toType = codeWriter.System.System_SByte;
                    break;
                case 2:
                    toType = codeWriter.System.System_Int16;
                    break;
                case 4:
                    toType = codeWriter.System.System_Int32;
                    break;
                case 8:
                    toType = codeWriter.System.System_Int64;
                    break;
            }

            return toType;
        }

        public static IType GetUIntTypeByBitSize(this ICodeWriter codeWriter, int bitSize)
        {
            IType toType = null;
            switch (bitSize)
            {
                case 1:
                    toType = codeWriter.System.System_Boolean;
                    break;
                case 8:
                    toType = codeWriter.System.System_Byte;
                    break;
                case 16:
                    toType = codeWriter.System.System_UInt16;
                    break;
                case 32:
                    toType = codeWriter.System.System_UInt32;
                    break;
                case 64:
                    toType = codeWriter.System.System_UInt64;
                    break;
            }

            return toType;
        }

        /// <summary>
        /// </summary>
        /// <param name="codeWriterer">
        /// </param>
        /// <param name="byteSize">
        /// </param>
        /// <returns>
        /// </returns>
        public static IType GetUIntTypeByByteSize(this ICodeWriter codeWriter, int byteSize)
        {
            IType toType = null;
            switch (byteSize)
            {
                case 1:
                    toType = codeWriter.System.System_Byte;
                    break;
                case 2:
                    toType = codeWriter.System.System_UInt16;
                    break;
                case 4:
                    toType = codeWriter.System.System_UInt32;
                    break;
                case 8:
                    toType = codeWriter.System.System_UInt64;
                    break;
            }

            return toType;
        }

        /// <summary>
        /// </summary>
        /// <param name="cWriter">
        /// </param>
        /// <param name="opCode">
        /// </param>
        /// <param name="toType">
        /// </param>
        public static void WriteCCast(this CWriter cWriter, OpCodePart opCode, IType toType)
        {
            var writer = cWriter.Output;

            cWriter.WriteCCastOnly(toType);

            writer.Write("(");
            cWriter.WriteResultOrActualWrite(opCode);
            writer.Write(")");
        }

        public static void WriteCCastOperand(this CWriter cWriter, OpCodePart opCode, int operand, IType toType)
        {
            var writer = cWriter.Output;

            cWriter.WriteCCastOnly(toType);

            writer.Write("(");
            cWriter.WriteOperandResultOrActualWrite(writer, opCode, operand);
            writer.Write(")");
        }

        public static void WriteCCastOnly(this CWriter cWriter, IType toType)
        {
            var writer = cWriter.Output;

            writer.Write("(");
            toType.WriteTypePrefix(cWriter);
            writer.Write(") ");
        }

        /// <summary>
        /// </summary>
        /// <param name="cWriter">
        /// </param>
        /// <param name="opCodeMethodInfo">
        /// </param>
        /// <param name="methodInfo">
        /// </param>
        /// <returns>
        /// </returns>
        public static bool ProcessPluggableMethodCall(
            this CWriter cWriter,
            OpCodePart opCodeMethodInfo,
            IMethod methodInfo)
        {
            if (methodInfo.HasProceduralBody)
            {
                var customAction = methodInfo as IMethodBodyCustomAction;
                if (customAction != null)
                {
                    customAction.Execute(cWriter, opCodeMethodInfo);
                }

                return true;
            }

            // TODO: it seems, you can preprocess MSIL code and replace all functions with MSIL code blocks to stop writing the code manually.
            // for example call System.Activator.CreateInstance<X>() can be replace with "Code.NewObj x"
            // the same interlocked functions and the same for TypeOf operators
            if (methodInfo.IsTypeOfCallFunction() && opCodeMethodInfo.WriteTypeOfFunction(cWriter))
            {
                return true;
            }

            if (methodInfo.IsInterlockedFunction())
            {
                methodInfo.WriteInterlockedFunction(opCodeMethodInfo, cWriter);
                return true;
            }

            if (methodInfo.IsThreadFunction())
            {
                methodInfo.WriteThreadFunction(opCodeMethodInfo, cWriter);
                return true;
            }

            if (methodInfo.IsMonitorFunction(cWriter))
            {
                methodInfo.WriteMonitorFunction(opCodeMethodInfo, cWriter);
                return true;
            }

            if (methodInfo.IsActivatorFunction())
            {
                methodInfo.WriteActivatorFunction(opCodeMethodInfo, cWriter);
                return true;
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="opCodePart">
        /// </param>
        /// <param name="label">
        /// </param>
        public static void SetCustomLabel(OpCodePart opCodePart, string label)
        {
            if (opCodePart.AddressStart == 0 && opCodePart.UsedBy != null)
            {
                opCodePart.UsedBy.OpCode.CreatedLabel = label;
            }
            else
            {
                opCodePart.CreatedLabel = label;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="cWriter">
        /// </param>
        /// <param name="opCode">
        /// </param>
        /// <param name="source">
        /// </param>
        /// <param name="toType">
        /// </param>
        public static void WriteCCast(
            this CWriter cWriter,
            OpCodePart opCode,
            FullyDefinedReference source,
            IType toType,
            bool asReference = true)
        {
            cWriter.WriteStartCCast(opCode, toType, asReference);
            cWriter.WriteResult(source);
            cWriter.WriteEndCCast(opCode, toType);
        }

        public static void WriteEndCCast(this CWriter cWriter, OpCodePart opCode, IType toType)
        {
            cWriter.Output.Write(")");
        }

        public static void WriteStartCCast(this CWriter cWriter, OpCodePart opCode, IType toType, bool asReference = false)
        {
            var writer = cWriter.Output;

            writer.Write("(");
            toType.WriteTypePrefix(cWriter, asReference);
            writer.Write(") (");
        }

        /// <summary>
        /// </summary>
        /// <param name="cWriter">
        /// </param>
        /// <param name="opCode">
        /// </param>
        /// <param name="fromResult">
        /// </param>
        /// <param name="toType">
        /// </param>
        /// <param name="throwExceptionIfNull">
        /// </param>
        /// <returns>
        /// </returns>
        public static bool WriteCast(
            this CWriter cWriter,
            OpCodePart opCode,
            OpCodePart opCodeOperand,
            IType toType,
            bool throwExceptionIfNull = false)
        {
            var writer = cWriter.Output;

            var estimatedOperandResultOf = cWriter.EstimatedResultOf(opCodeOperand);

            var bareType = !estimatedOperandResultOf.Type.IsArray
                ? estimatedOperandResultOf.Type.ToBareType()
                : estimatedOperandResultOf.Type;

            var isNull = estimatedOperandResultOf.Type.IsPointer && estimatedOperandResultOf.Type.GetElementType().IsVoid();

            if (toType.IsInterface && !isNull)
            {
                if (bareType.IsInterface && toType.IsFirstInterfaceOf(bareType))
                {
                    // no need to cast derived interface to base interface at first level
                    WriteCCast(cWriter, opCodeOperand, toType);
                    return true;
                }

                if (bareType.GetAllInterfaces().Contains(toType))
                {
                    cWriter.WriteCCastOnly(toType);
                    writer.Write("__new_interface{0}((Void*)", cWriter.GcDebug ? "_debug" : string.Empty);

                    var mainOperand = opCodeOperand;
                    if (bareType.IsInterface)
                    {
                        var opCodeLoadField = new OpCodeFieldInfoPart(OpCodesEmit.Ldfld, 0, 0, bareType.GetFieldByName("__this", cWriter));
                        opCodeLoadField.OpCodeOperands = new OpCodePart[] { opCodeOperand };
                        mainOperand = opCodeLoadField;
                    }

                    // actual call box
                    cWriter.WriteResultOrActualWrite(mainOperand);

                    writer.Write(", (Void**)");

                    if (bareType.IsInterface)
                    {
                        writer.Write("(Void**) &(");
                        cWriter.WriteInterfaceAccess(opCodeOperand, bareType, toType);
                        writer.Write(")");
                    }
                    else
                    {
                        if (bareType.FindInterfaceEntry(toType).TypeNotEquals(toType))
                        {
                            writer.Write("&");
                        }

                        writer.Write("(");
                        cWriter.WriteCCastOnly(bareType.ToVirtualTable());
                        cWriter.WriteResultOrActualWrite(mainOperand);
                        cWriter.WriteFieldAccess(bareType, cWriter.System.System_Object.GetFieldByName(CWriter.VTable, cWriter));
                        writer.Write(")->");
                        cWriter.WriteInterfacePath(bareType, toType, false);
                    }

                    if (cWriter.GcDebug)
                    {
                        writer.Write(", (SByte*)__FILE__, __LINE__");
                    }

                    writer.Write(")");
                }
                else
                {
                    return cWriter.WriteDynamicCast(writer, opCode, opCodeOperand, toType, throwExceptionIfNull: throwExceptionIfNull);
                }
            }
            else if (estimatedOperandResultOf.Type.IsInterface && !toType.IsPointer && !toType.IsByRef)
            {
                cWriter.WriteInterfaceToObjectCast(writer, opCodeOperand, toType);
                return true;
            }
            else if (estimatedOperandResultOf.Type.IntTypeBitSize() == CWriter.PointerSize * 8 &&
                     (toType.IsPointer || toType.IsByRef))
            {
                WriteCCast(cWriter, opCodeOperand, toType);
            }
            else if ((estimatedOperandResultOf.Type.IsEnum || estimatedOperandResultOf.Type.IntTypeBitSize() > 0) && (toType.IsEnum || toType.IntTypeBitSize() > 0))
            {
                WriteCCast(cWriter, opCodeOperand, toType);
            }
            else if ((estimatedOperandResultOf.Type.IsPointer || estimatedOperandResultOf.Type.IntTypeBitSize() >= CWriter.PointerSize * 8) && toType.IsIntPtrOrUIntPtr() && !toType.IsReference())
            {
                cWriter.Output.Write("System_{0}_System_{0}_op_ExplicitFVoidPN((Void*)", toType.Name);
                cWriter.WriteResultOrActualWrite(opCodeOperand);
                cWriter.Output.Write(")");
            }
            else if (estimatedOperandResultOf.Type.IsArray
                     || (estimatedOperandResultOf.Type.IsPointer && bareType.TypeEquals(cWriter.System.System_Void))
                     || (estimatedOperandResultOf.Type.IsPointer && toType.UseAsClass)
                     || toType.IsPointer
                     || toType.IsByRef
                     || bareType.IsDerivedFrom(toType)
                     || (toType.IsArray && (estimatedOperandResultOf.Type.TypeEquals(toType.BaseType) || estimatedOperandResultOf.Type.IsObject))
                     || estimatedOperandResultOf.Type.IsPointer && toType.IntTypeBitSize() == CWriter.PointerSize * 8)
            {
                WriteCCast(cWriter, opCodeOperand, toType);
            }
            else
            {
                Debug.Assert(estimatedOperandResultOf.IsReference || estimatedOperandResultOf.Type.IntTypeBitSize() == 0);
                var done = cWriter.WriteDynamicCast(writer, opCode, opCodeOperand, toType, throwExceptionIfNull: throwExceptionIfNull);
                if (!done && opCode.IsVirtual())
                {
                    // forcebly apply cast for virtual cast
                    WriteCCast(cWriter, opCodeOperand, toType);
                }
                else
                {
                    return done;
                }
            }

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="cWriter">
        /// </param>
        /// <param name="opCode">
        /// </param>
        /// <param name="typeToSave">
        /// </param>
        /// <param name="operandIndex">
        /// </param>
        /// <param name="destination">
        /// </param>
        public static void WriteSave(
            this CWriter cWriter,
            OpCodePart opCode,
            IType typeToSave,
            FullyDefinedReference destination)
        {
            var writer = cWriter.Output;

            cWriter.WriteResult(destination);
            writer.Write(" = ");
            cWriter.Pop();
        }

        public static void WriteSaveVolatile(
            this CWriter cWriter,
            OpCodePart opCode,
            IType typeToSave,
            int operandIndex,
            FullyDefinedReference destination)
        {
            var writer = cWriter.Output;

            writer.Write("swap(&");
            cWriter.WriteResult(destination);

            if (destination.Type.IsIntPtrOrUIntPtr())
            {
                cWriter.WriteFieldAccess(destination.Type, destination.Type.GetFieldByFieldNumber(0, cWriter));
            }

            writer.Write(", ");
            cWriter.WriteOperandResultOrActualWrite(writer, opCode, operandIndex);

            if (destination.Type.IsIntPtrOrUIntPtr())
            {
                cWriter.WriteFieldAccess(destination.Type, destination.Type.GetFieldByFieldNumber(0, cWriter));
            }

            writer.Write(")");
        }

        public static void WriteSaveThreadStatic(
            this CWriter cWriter,
            OpCodePart opCode,
            IType typeToSave,
            int operandIndex,
            FullyDefinedReference destination)
        {
            var writer = cWriter.Output;

            writer.Write("__set_thread_static((Int32)&");
            cWriter.WriteResult(destination);
            writer.Write(", ");
            cWriter.WriteOperandResultOrActualWrite(writer, opCode, operandIndex);
            writer.Write(")");
        }

        [Obsolete]
        public static void WriteSavePrimitiveIntoStructure(
            this CWriter cWriter,
            OpCodePart opCode,
            FullyDefinedReference source,
            FullyDefinedReference destination)
        {
            // write access to a field
            IField field;
            if ((field = cWriter.WriteFieldAccess(
                opCode,
                destination.Type.ToClass(),
                destination.Type.ToClass(),
                0,
                destination)) == null)
            {
                return;
            }

            cWriter.SaveToField(opCode, field.FieldType, 0);
        }

        public static void WriteMemCopy(
            this CWriter cWriter,
            OpCodePart op1,
            OpCodePart op2,
            OpCodePart size)
        {
            var writer = cWriter.Output;

            writer.Write("Memcpy(");
            cWriter.WriteResultOrActualWrite(op1);
            writer.Write(", ");
            cWriter.WriteResultOrActualWrite(op2);
            writer.Write(", ");
            cWriter.WriteResultOrActualWrite(size);
            writer.Write(")");
        }

        /// <summary>
        /// </summary>
        /// <param name="cWriter">
        /// </param>
        /// <param name="type">
        /// </param>
        /// <param name="op1">
        /// </param>
        public static void WriteMemSet(this CWriter cWriter, IType type, OpCodePart op1)
        {
            var writer = cWriter.Output;

            writer.Write("Memset((Byte*) (");
            cWriter.Pop();
            writer.Write("), 0, sizeof(");
            cWriter.Pop();
            writer.Write("))");
        }

        public static void WriteMemSet(this CWriter cWriter, OpCodePart op1, OpCodePart size)
        {
            var writer = cWriter.Output;

            writer.Write("Memset((Byte*) (");
            cWriter.Pop();
            writer.Write("), 0, (");
            cWriter.Pop();
            writer.Write("))");
        }

        public static void WriteMemSet(
            this CWriter cWriter,
            OpCodePart reference,
            OpCodePart init,
            OpCodePart size)
        {
            var writer = cWriter.Output;
            writer.Write("Memset((Byte*) (");
            cWriter.Pop();
            writer.Write("), ");
            cWriter.Pop();
            writer.Write(", (");
            cWriter.Pop();
            writer.Write("))");
        }

        public static void WriteMemSet(
            this CWriter cWriter,
            FullyDefinedReference reference,
            int init,
            IType size)
        {
            var writer = cWriter.Output;
            writer.Write("Memset((Byte*) ({0}", reference);
            writer.Write("), {0}", init);
            writer.Write(", sizeof(");
            size.WriteTypePrefix(cWriter);
            writer.Write("))");
        }
    }
}