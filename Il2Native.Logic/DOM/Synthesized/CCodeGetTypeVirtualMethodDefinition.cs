﻿// Mr Oleksandr Duzhar licenses this file to you under the MIT license.
// If you need the License file, please send an email to duzhar@googlemail.com
// 
namespace Il2Native.Logic.DOM.Synthesized
{
    using DOM2;
    using Implementations;
    using Microsoft.CodeAnalysis;

    public class CCodeGetTypeVirtualMethodDefinition : CCodeMethodDefinition
    {
        public CCodeGetTypeVirtualMethodDefinition(INamedTypeSymbol type)
            : base(new CCodeGetTypeVirtualMethodDeclaration.GetTypeVirtualMethod(type))
        {
            MethodBodyOpt = new MethodBody(Method)
            {
                Statements =
                {
                    new ReturnStatement
                    {
                        ExpressionOpt =
                            new AddressOfOperator
                            {
                                Operand =
                                    new FieldAccess
                                    {
                                        Field =
                                            new FieldImpl { Name = "__type", ContainingType = type, IsStatic = true }
                                    }
                            }
                    }
                }
            };
        }
    }
}
