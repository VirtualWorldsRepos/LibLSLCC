﻿#region FileInfo

// 
// File: LSLOpenSimCompilerVisitor.cs
// 
// 
// ============================================================
// ============================================================
// 
// 
// Copyright (c) 2015, Eric A. Blundell
// 
// All rights reserved.
// 
// 
// This file is part of LibLSLCC.
// 
// LibLSLCC is distributed under the following BSD 3-Clause License
// 
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
//     in the documentation and/or other materials provided with the distribution.
// 
// 3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived
//     from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
// ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// 
// ============================================================
// ============================================================
// 
// 

#endregion

#region Imports

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LibLSLCC.CodeValidator;
using LibLSLCC.Collections;
using LibLSLCC.CSharp;
using LibLSLCC.Utility;
using LibLSLCC.Utility.ListParser;

#endregion

namespace LibLSLCC.Compilers
{
    // ReSharper disable InconsistentNaming
    internal sealed class LSLOpenSimCompilerVisitor : LSLValidatorNodeVisitor<bool>
        // ReSharper restore InconsistentNaming
    {

        const string UtilityLibrary =
            @"
//============================
//== Compiler Utility Class ==
//============================
private static class UTILITIES
{
    public static void ForceStatement<T>(T val) {}

    public static bool ToBool({LSLType.String} str)
    {
        return str.Length != 0;
    }

    public static {LSLType.Rotation} Negate({LSLType.Rotation} rot)
    {
        rot.x=(-rot.x);
        rot.y=(-rot.y);
        rot.z=(-rot.z);
        rot.s=(-rot.s);
        return rot;
    }
    public static {LSLType.Vector} Negate({LSLType.Vector} vec)
    {
        vec.x=(-vec.x);
        vec.y=(-vec.y);
        vec.z=(-vec.z);
        return vec;
    }
}
";


        /// <summary>
        ///     Name of the class that will contain global variables
        ///     if the user specifies not to generate a script class.
        ///     Global variables are initialized inside of a container if
        ///     the library user does not specify that a class should be generated.
        /// </summary>
        private const string GlobalsContainerClassName = "GLOBALS";

        /// <summary>
        ///     The global container field name that the globals container class
        ///     gets assigned to in the generated script, when not generating a class.
        /// </summary>
        private const string GlobalContainerFieldName = "Globals";

        /// <summary>
        ///     The name prefix used when defining global variables inside of the
        ///     global variable container class.
        /// </summary>
        private const string GlobalContainerFieldsNamePrefix = "V_";

        /// <summary>
        ///     The name prefix used when defining global variables inside of the
        ///     script class itself when a class and class constructor is
        ///     generated for the script.
        /// </summary>
        private const string GlobalFieldsNamePrefix = "GV_";

        /// <summary>
        ///     The local variable prefix to mangle local variables with,
        ///     the compiler appends an integral scope ID after this prefix, followed by an underscore.
        /// </summary>
        private const string LocalVariableNamePrefix = "LV";

        /// <summary>
        ///     The name prefix to mangle parameters names in event handlers and function declarations with.
        /// </summary>
        private const string LocalParameterNamePrefix = "PM_";

        /// <summary>
        ///     The name prefix to mangle user defined function names with.
        /// </summary>
        private const string FunctionNamePrefix = "FN_";

        /// <summary>
        ///     Keeps track of what binary operations have been used in the script, stubs are dynamically generated for them
        ///     at the end of the class.
        ///     The stubs reverse the order of evaluation for binary operators.
        /// </summary>
        private readonly HashSet<LSLBinaryOperationSignature> _binOpsUsed = new HashSet<LSLBinaryOperationSignature>();

        /// <summary>
        ///     Tracks what event handler node compilation is currently taking place in.
        /// </summary>
        private ILSLEventHandlerNode _currentLslEventHandlerNode;

        /*/// <summary>
        /// Tracks what function declaration node compilation is currently taking place in.
        /// </summary>
        private ILSLFunctionDeclarationNode _currentLslFunctionDeclarationNode;*/


        /// <summary>
        ///     Tracks what state body that LSL code generation is taking place in
        ///     use to determine the names of generated event handler functions.
        /// </summary>
        private ILSLStateScopeNode _currentLslStateNode;

        /// <summary>
        ///     The indent level, used to create pretty output
        /// </summary>
        private int _indentLevel;

        /// <summary>
        ///     Gets or sets the code generation settings.
        ///     this must not be null, when <see cref="WriteAndFlush" /> is called.
        /// </summary>
        /// <value>
        ///     The code generation settings.
        /// </value>
        public LSLOpenSimCompilerSettings Settings { get; set; }

        public ILSLBasicLibraryDataProvider LibraryDataProvider { get; set; }
        public TextWriter Writer { get; private set; }


        private bool _writingGlobalVariableContainerClassConstructor;



        private static string GenBinaryOperationStubName(LSLBinaryOperationSignature binOp)
        {
            return "_o" + ((int) binOp.Left) + "" + ((int) binOp.Operation) + "" + ((int) binOp.Right);
        }


        private string GetCoOpTerminationCallString()
        {
            return Settings.CoOpTerminationFunctionCall == null
                ? LSLOpenSimCompilerSettings.DefaultCoOpTerminationFunctionCall
                : Settings.CoOpTerminationFunctionCall.FullSignature;
        }


        /// <summary>
        ///     Compiles a syntax tree into OpenSim compatible CSharp code, writing the output to the specified TextWriter.
        /// </summary>
        /// <param name="compilationUnit">
        ///     The top node of an LSL Syntax tree to compile.
        ///     This is returned from <see cref="LSLCodeValidator.Validate" /> or user implemented Code DOM.
        /// </param>
        /// <param name="writer">The text writer to write the generated code to.</param>
        /// <param name="closeStream">
        ///     Whether or not to close <paramref name="writer" /> once compilation is done.  The default
        ///     value is <c>false</c>.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     If <see cref="ILSLReadOnlySyntaxTreeNode.HasErrors" /> is <c>true</c> in
        ///     <paramref name="compilationUnit" />.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="compilationUnit" /> or <paramref name="writer" /> is
        ///     <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException"><see cref="LSLOpenSimCompilerVisitor.Settings" /> is <c>null</c>.</exception>
        /// <exception cref="IOException">When an IO Error occurs while writing to <paramref name="writer" />.</exception>
        /// <exception cref="ObjectDisposedException">If <paramref name="writer" /> is already disposed.</exception>
        public void WriteAndFlush(ILSLCompilationUnitNode compilationUnit, TextWriter writer, bool closeStream = true)
        {
            if (compilationUnit == null)
            {
                throw new ArgumentNullException("compilationUnit");
            }

            if (compilationUnit.HasErrors)
            {
                throw new ArgumentException(typeof (ILSLCompilationUnitNode).Name +
                                            ".HasErrors is true, cannot compile a tree with syntax errors.");
            }

            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (Settings == null)
            {
                throw new InvalidOperationException(GetType().Name + ".Settings property cannot be null!");
            }

            try
            {
                Writer = writer;

                Visit(compilationUnit);

                Writer.Flush();
            }
            finally
            {
                if (closeStream)
                {
                    Writer.Close();
                }

                Reset();
            }
        }


        public void Reset()
        {
            //_currentLslFunctionDeclarationNode = null;

            _currentLslStateNode = null;

            _currentLslEventHandlerNode = null;

            _indentLevel = 0;

            _binOpsUsed.Clear();
        }

        #region Expressions

        #region BasicExpressions

        public override bool VisitBinaryExpression(ILSLBinaryExpressionNode node)
        {
            if (node.Operation == LSLBinaryOperationType.LogicalAnd)
            {
                VisitLogicalAnd(node);

                return false;
            }

            if (node.Operation == LSLBinaryOperationType.LogicalOr)
            {
                VisitLogicalOr(node);

                return false;
            }

            if (node.Operation == LSLBinaryOperationType.MultiplyAssign)
            {
                if (node.LeftExpression.Type == LSLType.Integer && node.RightExpression.Type == LSLType.Float)
                {
                    VisitMultipyAssignIntegerAndFloat(node);
                    return false;
                }
            }


            if (node.OperationString == "=")
            {
                VisitPlainAssignmentExpression(node);
                return false;
            }

            if (node.OperationString.EqualsOneOf("*=", "+=", "/=", "%=", "-="))
            {
                VisitOtherModifyingAssignmentExpression(node);
                return false;
            }


            VisitOtherBinaryExpression(node);

            return false;
        }


        private void VisitOtherBinaryExpression(ILSLBinaryExpressionNode node)
        {
            var operationSignature = new LSLBinaryOperationSignature(node.OperationString, node.Type,
                node.LeftExpression.Type,
                node.RightExpression.Type);


            if (!_binOpsUsed.Contains(operationSignature))
            {
                _binOpsUsed.Add(operationSignature);
            }


            Writer.Write(GenBinaryOperationStubName(operationSignature));
            Writer.Write("(");
            Visit(node.RightExpression);
            Writer.Write(",");
            Visit(node.LeftExpression);
            Writer.Write(")");
        }


        private void VisitLogicalAnd(ILSLBinaryExpressionNode node)
        {
            Writer.Write("((bool)(");
            Visit(node.RightExpression);
            Writer.Write("))");

            Writer.Write(" & ");

            Writer.Write("((bool)(");
            Visit(node.LeftExpression);
            Writer.Write("))");
        }


        private void VisitLogicalOr(ILSLBinaryExpressionNode node)
        {
            Writer.Write("((bool)(");
            Visit(node.RightExpression);
            Writer.Write("))");

            Writer.Write(" | ");

            Writer.Write("((bool)(");
            Visit(node.LeftExpression);
            Writer.Write("))");
        }


        private void VisitMultipyAssignIntegerAndFloat(ILSLBinaryExpressionNode node)
        {
            Visit(node.LeftExpression);
            Writer.Write(" = new " + LSLType_To_CSharpType(LSLType.Integer) + "(System.Math.Round((double)");
            Visit(node.LeftExpression);
            Writer.Write(") * ");
            Visit(node.RightExpression);
            Writer.Write(")");
        }


        private void VisitPlainAssignmentExpression(ILSLBinaryExpressionNode node)
        {
            Visit(node.LeftExpression);

            Writer.Write(" " + node.OperationString + " ");

            if (Settings.KeysAreStrings)
            {
                Visit(node.RightExpression);
            }
            else
            {
                if (node.LeftExpression.Type == LSLType.Key &&
                    node.RightExpression.Type == LSLType.String)
                {
                    var rightExpressionAsStringLiteral = node.RightExpression as ILSLStringLiteralNode;

                    if (rightExpressionAsStringLiteral != null)
                    {
                        Writer.Write(rightExpressionAsStringLiteral.PreProcessedText);
                    }
                    else
                    {
                        Writer.Write("new " + LSLType_To_CSharpType(LSLType.Key) + "(");
                        Visit(node.RightExpression);
                        Writer.Write(")");
                    }
                }
                else
                {
                    Visit(node.RightExpression);
                }
            }
        }



        private void VisitOtherModifyingAssignmentExpression(ILSLBinaryExpressionNode node)
        {
            string operation = node.OperationString.Substring(0, 1);

            var operationSignature = new LSLBinaryOperationSignature(operation, node.Type,
                node.LeftExpression.Type,
                node.RightExpression.Type);

            if (!_binOpsUsed.Contains(operationSignature))
            {
                _binOpsUsed.Add(operationSignature);
            }

            Visit(node.LeftExpression);

            Writer.Write(" = ");

            Writer.Write(GenBinaryOperationStubName(operationSignature));
            Writer.Write("(");
            Visit(node.RightExpression);
            Writer.Write(",");
            Visit(node.LeftExpression);
            Writer.Write(")");
        }




        public override bool VisitPostfixOperation(ILSLPostfixOperationNode node)
        {
            var parenths = !(node.Parent is ILSLExpressionStatementNode || node.Parent is ILSLExpressionListNode);

            if (parenths)
            {
                Writer.Write("(");
            }

            Visit(node.LeftExpression);
            Writer.Write(node.OperationString);

            if (parenths)
            {
                Writer.Write(")");
            }


            return false;
        }


        public override bool VisitPrefixOperation(ILSLPrefixOperationNode node)
        {
            if (node.RightExpression.Type == LSLType.Rotation ||
                node.RightExpression.Type == LSLType.Vector && node.OperationString == "-")
            {
                Writer.Write("UTILITIES.Negate(");
                Visit(node.RightExpression);
                Writer.Write(")");
            }
            else
            {
                var isExprStatement = node.Parent is ILSLExpressionStatementNode;

                var isUnary = node.Operation != LSLPrefixOperationType.Decrement &&
                              node.Operation != LSLPrefixOperationType.Increment;

                var cantStandAlone = isExprStatement && isUnary;

                var parenths = !(isExprStatement || node.Parent is ILSLExpressionListNode);


                if (parenths)
                {
                    Writer.Write("(");
                }

                if (!cantStandAlone)
                {
                    Writer.Write(node.OperationString);

                    if (isUnary)
                    {
                        Writer.Write("(");
                        Visit(node.RightExpression);
                        Writer.Write(")");
                    }
                    else
                    {
                        Visit(node.RightExpression);
                    }
                }
                else
                {
                    Writer.Write("UTILITIES.ForceStatement(");
                    Visit(node.RightExpression);
                    Writer.Write(")");
                }

                if (parenths)
                {
                    Writer.Write(")");
                }
            }


            return false;
        }


        public override bool VisitVecRotAccessor(ILSLTupleAccessorNode node)
        {
            Visit(node.AccessedExpression);
            Writer.Write(".");
            Writer.Write(node.AccessedComponent);

            return false;
        }


        public override bool VisitParenthesizedExpression(ILSLParenthesizedExpressionNode node)
        {
            var isExprStatement = node.Parent is ILSLExpressionStatementNode;

            var parenths = !(isExprStatement || node.Parent is ILSLExpressionListNode);

            if (parenths)
            {
                Writer.Write("(");
            }

            if (!isExprStatement)
            {
                Visit(node.InnerExpression);
            }
            else
            {
                Writer.Write("UTILITIES.ForceStatement(");
                Visit(node.InnerExpression);
                Writer.Write(")");
            }

            if (parenths)
            {
                Writer.Write(")");
            }
            return false;
        }


        public override bool VisitTypecastExpression(ILSLTypecastExprNode node)
        {
            if (node.CastToType == node.CastedExpression.Type)
            {
                Visit(node.CastedExpression);
                return false;
            }


            //the compiler uses strings for keys unless testing a boolean
            //when a key is used for a boolean condition LSL_Types.key is constructed around the expression
            if (Settings.KeysAreStrings && (node.CastToType == LSLType.String || node.CastToType == LSLType.Key) &&
                                           (node.CastedExpression.Type == LSLType.String || node.CastedExpression.Type == LSLType.Key))
            {
                Visit(node.CastedExpression);
                return false;
            }

            if (node.CastToType == LSLType.Key && node.CastedExpression.Type == LSLType.String)
            {
                var asStringLiteral = node.CastedExpression as ILSLStringLiteralNode;
                if (asStringLiteral != null)
                {
                    Writer.Write("(new" + LSLType_To_CSharpType(node.CastToType) + "(" +
                                 asStringLiteral.PreProcessedText + "))");
                }
                else
                {
                    Writer.Write("(new " + LSLType_To_CSharpType(LSLType.Key) + "(");
                    Visit(node.CastedExpression);
                    Writer.Write("))");
                }
            }
            else
            {
                Writer.Write("(" + LSLType_To_CSharpType(node.CastToType) + ")");
                Writer.Write("(");
                Visit(node.CastedExpression);
                Writer.Write(")");
            }

            return false;
        }

        #endregion

        #region FunctionCalls

        public override bool VisitUserFunctionCall(ILSLFunctionCallNode node)
        {
            var functionName = FunctionNamePrefix + node.Name;

            if (node.ArgumentExpressionList.Expressions.Count > 0)
            {
                Writer.Write(functionName + "(");

                VisitUserFunctionCallParameters(node.ArgumentExpressionList);

                Writer.Write(")");
            }
            else
            {
                Writer.Write(functionName + "()");
            }
            return false;
        }


        private readonly IReadOnlyHashMap<LSLType, string> _modInvokeFunctionMap
            = new HashMap<LSLType, string>
            {
                {LSLType.Void, "modInvokeN"},
                {LSLType.String, "modInvokeS"},
                {LSLType.Integer, "modInvokeI"},
                {LSLType.Float, "modInvokeF"},
                {LSLType.Key, "modInvokeK"},
                {LSLType.List, "modInvokeL"},
                {LSLType.Vector, "modInvokeV"},
                {LSLType.Rotation, "modInvokeR"}
            };




        public override bool VisitLibraryFunctionCall(ILSLFunctionCallNode node)
        {
            var libDataNode = LibraryDataProvider.GetLibraryFunctionSignature(node.Signature);


            if (libDataNode.ModInvoke)
            {
                var modInvokeFunction = "this." + _modInvokeFunctionMap[node.Signature.ReturnType];

                var afterName = node.ArgumentExpressionList.Expressions.Count > 0 ? ", " : "";

                Writer.Write(modInvokeFunction + "(\"" + node.Name + "\"" + afterName);


                VisitLibraryFunctionCallParameters(node.ArgumentExpressionList);

                Writer.Write(")");
            }
            else
            {
                var functionName = "this." + node.Name;

                if (node.ArgumentExpressionList.Expressions.Count > 0)
                {
                    Writer.Write(functionName + "(");

                    VisitLibraryFunctionCallParameters(node.ArgumentExpressionList);

                    Writer.Write(")");
                }
                else
                {
                    Writer.Write(functionName + "()");
                }
            }
            return false;
        }

        #endregion

        #region VariableReferences

        public override bool VisitGlobalVariableReference(ILSLVariableNode node)
        {
            if (Settings.GenerateClass)
            {
                //we are referencing a class field defined in the generated script class.
                //because we were able to generate a constructor to initialize them from
                //instead of making a new object to contain them.
                Writer.Write("this." + GlobalFieldsNamePrefix + node.Name);
            }
            else
            {
                if (_writingGlobalVariableContainerClassConstructor)
                {
                    //creating the global variable container class constructor,
                    //reference the variables as if they are in the local class.
                    Writer.Write("this." + GlobalContainerFieldsNamePrefix + node.Name);
                }
                else
                {

                    //reference them in the globals field, it has an instance of the globals
                    //class since we did not generate a script constructor.
                    //
                    //the user specified not to generate a class so variable initialization cannot
                    //happen in the constructor, since they may not have provided a class name.
                    Writer.Write("this." + GlobalContainerFieldName + "." + GlobalContainerFieldsNamePrefix + node.Name);
                }
            }
            return false;
        }


        public override bool VisitLocalVariableReference(ILSLVariableNode node)
        {
            /*
                See VisitCodeScope to see where variables defined inside of dead code are put.
                If a variable is declared inside of dead code, it is put at the very top of the scope it was 
                defined in and initialized with the default value for its type.
            */
            Writer.Write(LocalVariableNamePrefix + node.Declaration.ParentScopeId + "_" + node.Name);
            return false;
        }


        public override bool VisitParameterVariableReference(ILSLVariableNode node)
        {
            Writer.Write(LocalParameterNamePrefix + node.Name);
            return false;
        }


        private string GenerateExpandedListConstant(string constantValueString)
        {
            return "new "+LSLType_To_CSharpType(LSLType.List)+"(" +
                   string.Join(", ", LSLListParser.ParseList("[" + constantValueString + "]").Select(e =>
                   {
                       switch (e.Type)
                       {
                           case LSLType.String:
                               return ("new " + LSLType_To_CSharpType(LSLType.String) + "(\"" +LSLFormatTools.ShowControlCodeEscapes(e.ValueString) + "\")");

                           case LSLType.Key:
                               return ("new " + LSLType_To_CSharpType(LSLType.Key, Settings.KeyElementsInListConstantsThatExpandAreStrings) + "(\"" + LSLFormatTools.ShowControlCodeEscapes(e.ValueString) + "\")");

                           case LSLType.Vector:
                               return ("new " + LSLType_To_CSharpType(LSLType.Vector) + "(" + e.ValueString + ")");

                           case LSLType.Rotation:
                               return ("new " + LSLType_To_CSharpType(LSLType.Rotation) + "(" + e.ValueString + ")");

                           case LSLType.Integer:
                               return ("new " + LSLType_To_CSharpType(LSLType.Integer) + "(" + e.ValueString + ")");

                           case LSLType.Float:
                               return ("new " + LSLType_To_CSharpType(LSLType.Float) + "(" + e.ValueString + ")");

                           case LSLType.List:
                               return ("new " + LSLType_To_CSharpType(LSLType.List) + "(" + e.ValueString + ")");
                           default:
                               throw new InvalidOperationException(
                                   typeof (LSLOpenSimCompilerVisitor).Name +
                                   ".GenerateExpandedListConstant encountered a Void list element type.");
                       }
                   })) + ")";
        }


        public override bool VisitLibraryConstantVariableReference(ILSLVariableNode node)
        {
            var x = LibraryDataProvider.GetLibraryConstantSignature(node.Name);
            if (x.Expand)
            {
                switch (x.Type)
                {
                    case LSLType.String:
                        Writer.Write("new " + LSLType_To_CSharpType(LSLType.String) + "(" + x.ValueStringAsCodeLiteral + ")");
                        break;
                    case LSLType.Key:
                        Writer.Write("new " + LSLType_To_CSharpType(LSLType.Key, Settings.KeyConstantsThatExpandAreStrings) + "(" + x.ValueStringAsCodeLiteral + ")");
                        break;
                    case LSLType.Vector:
                        Writer.Write("new " + LSLType_To_CSharpType(LSLType.Vector) + "(" + x.ValueString + ")");
                        break;
                    case LSLType.Rotation:
                        Writer.Write("new " + LSLType_To_CSharpType(LSLType.Rotation) + "(" + x.ValueString + ")");
                        break;
                    case LSLType.Integer:
                        Writer.Write("new " + LSLType_To_CSharpType(LSLType.Integer) + "(" + x.ValueString + ")");
                        break;
                    case LSLType.Float:
                        Writer.Write("new " + LSLType_To_CSharpType(LSLType.Float) + "(" + x.ValueString + ")");
                        break;
                    case LSLType.List:
                        Writer.Write(GenerateExpandedListConstant(x.ValueString));
                        break;
                    default:
                        throw new InvalidOperationException(
                            typeof (LSLOpenSimCompilerVisitor).Name +
                            ".VisitLibraryConstantVariableReference retrieved a library "
                            + "constant from the library data provider using 'LSLType.Void' as its Type.");
                }
            }
            else
            {
                Writer.Write(node.Name);
            }


            return false;
        }

        #endregion

        #endregion

        #region ExpressionLists




        public override bool VisitExpressionList(ILSLExpressionListNode node)
        {
            if (node.Expressions.Count == 0) return false;

            var omitExpressionsWithoutEffects = node.ListType == LSLExpressionListType.ForLoopInitExpressions ||
                                                node.ListType == LSLExpressionListType.ForLoopAfterthoughts;

            var i = 0;

            for (; i < node.Expressions.Count - 1; i++)
            {
                var expression = node.Expressions[i];

                if(omitExpressionsWithoutEffects && !expression.HasPossibleSideEffects) continue;

                
                Visit(expression);

                Writer.Write(",");
            }

            var lastExpression = node.Expressions[i];

            if (omitExpressionsWithoutEffects && !lastExpression.HasPossibleSideEffects) return false;


            Visit(lastExpression);

            return false;
        }


        public override bool VisitLibraryFunctionCallParameters(ILSLExpressionListNode node)
        {
            /* leaving this skeleton function here and using it to visit all library function call parameters.
               In-case I find a reason that code generation needs to be different for library function call parameters in the future. */


            VisitExpressionList(node);
            return false;
        }


        public override bool VisitUserFunctionCallParameters(ILSLExpressionListNode node)
        {
            /* leaving this skeleton function here and using it to visit all user defined function call parameters.
               In-case I find a reason that code generation needs to be different for user defined function call parameters in the future. */

            /* previously I thought that list, keys and strings may need to be copied into user defined function calls to prevent them from being
               mutated, but some testing has revealed this is not the case. */

            VisitExpressionList(node);
            return false;
        }

        #endregion

        #region Literals

        public override bool VisitFloatLiteral(ILSLFloatLiteralNode node)
        {
            bool parentIsFunctionCall = false;


            var parentExpressionList = node.Parent as ILSLExpressionListNode;

            var parentAsBinaryExpression = node.Parent as ILSLBinaryExpressionNode;


            //Floats cannot be used with logical operators, but the same check is made here.
            //This is here so that, if the binary operation validator is changed to allow it;
            //than we will still be generating correct code.
            bool parentIsNonLogicBinaryOperation = (parentAsBinaryExpression != null &&
                                                    !(parentAsBinaryExpression.Operation ==
                                                      LSLBinaryOperationType.LogicalAnd ||
                                                      parentAsBinaryExpression.Operation ==
                                                      LSLBinaryOperationType.LogicalOr));


            var inVectorOrRotationInitializer =
                node.Parent is ILSLVectorLiteralNode ||
                node.Parent is ILSLRotationLiteralNode;


            ILSLFunctionCallNode parentFunctionCallNode = null;
            if (parentExpressionList != null)
            {
                parentFunctionCallNode = node.Parent.Parent as ILSLFunctionCallNode;
                if (parentFunctionCallNode != null)
                {
                    parentIsFunctionCall = true;
                }
            }

            bool inModInvokeTopLevel = false;

            if (parentFunctionCallNode != null && parentFunctionCallNode.IsLibraryFunctionCall())
            {
                var libDataNode =
                    LibraryDataProvider.GetLibraryFunctionSignature(parentFunctionCallNode.Signature);

                inModInvokeTopLevel = libDataNode.ModInvoke;
            }

            //If the parent is a binary expression, the conversion will happen automagically because
            //the float literal becomes the argument of a stub function

            //Except if the parent is a logical operator, in which case a stub is not used.
            //so it needs to be boxed.
            var box =
                !(parentIsFunctionCall || parentIsNonLogicBinaryOperation || inVectorOrRotationInitializer) ||
                inModInvokeTopLevel;


            if (box)
            {
                Writer.Write("new " + LSLType_To_CSharpType(LSLType.Float) + "(");
            }



            var checkOverFlow = node.CheckForOverflow();

            switch (checkOverFlow)
            {
                case LSLLiteralOverflowType.None:
                    Writer.Write(LSLFormatTools.FormatFloatString(node.RawText));
                    break;
                case LSLLiteralOverflowType.Overflow:
                    Writer.Write("float.PositiveInfinity");
                    break;
                case LSLLiteralOverflowType.Underflow:
                    Writer.Write("0.0");
                    break;
            }


            if (box)
            {
                Writer.Write(")");
            }

            return false;
        }


        public override bool VisitIntegerLiteral(ILSLIntegerLiteralNode node)
        {
            bool parentIsFunctionCall = false;

            var parentAsBinaryExpression = node.Parent as ILSLBinaryExpressionNode;

            bool parentIsNonLogicBinaryOperation = (parentAsBinaryExpression != null &&
                                                    !(parentAsBinaryExpression.Operation ==
                                                      LSLBinaryOperationType.LogicalAnd ||
                                                      parentAsBinaryExpression.Operation ==
                                                      LSLBinaryOperationType.LogicalOr));


            bool parentIsUnaryNegate = node.IsNegated();


            var parentExpressionList = node.Parent as ILSLExpressionListNode;

            var inVectorOrRotationInitializer =
                node.Parent is ILSLVectorLiteralNode ||
                node.Parent is ILSLRotationLiteralNode;

            ILSLFunctionCallNode parentFunctionCallNode = null;
            if (parentExpressionList != null)
            {
                parentFunctionCallNode = node.Parent.Parent as ILSLFunctionCallNode;
                if (parentFunctionCallNode != null)
                {
                    parentIsFunctionCall = true;
                }
            }

            bool inModInvokeTopLevel = false;

            if (parentFunctionCallNode != null && parentFunctionCallNode.IsLibraryFunctionCall())
            {
                var libDataNode =
                    LibraryDataProvider.GetLibraryFunctionSignature(parentFunctionCallNode.Signature);

                inModInvokeTopLevel = libDataNode.ModInvoke;
            }

            //If the parent is a binary expression, the conversion will happen automagically because
            //the integer literal becomes the argument of a stub function

            //Except if the parent is a logical operator, in which case a stub is not used.
            //so it needs to be boxed.
            var box =
                !(parentIsFunctionCall || parentIsNonLogicBinaryOperation || inVectorOrRotationInitializer) ||
                inModInvokeTopLevel;


            if (box)
            {
                Writer.Write("new " + LSLType_To_CSharpType(LSLType.Integer) + "(");
            }

            

            Writer.Write(node.CheckForOverflow() != LSLLiteralOverflowType.None ? (parentIsUnaryNegate ? "1" : "-1") : node.RawText);

            if (box)
            {
                Writer.Write(")");
            }

            return false;
        }


        public override bool VisitHexLiteral(ILSLHexLiteralNode node)
        {
            var parentAsBinaryExpression = node.Parent as ILSLBinaryExpressionNode;

            bool parentIsNonLogicBinaryOperation = (parentAsBinaryExpression != null &&
                                                    !(parentAsBinaryExpression.Operation ==
                                                      LSLBinaryOperationType.LogicalAnd ||
                                                      parentAsBinaryExpression.Operation ==
                                                      LSLBinaryOperationType.LogicalOr));


            bool parentIsUnaryNegate = node.IsNegated();


            //If the parent is a binary expression, the conversion will happen automagically because
            //the hex literal becomes the argument of a stub function

            //Except if the parent is a logical operator, in which case a stub is not used.
            //so it needs to be boxed.
            if (!parentIsNonLogicBinaryOperation)
            {
                Writer.Write("new " + LSLType_To_CSharpType(LSLType.Integer) + "(");
            }

            Writer.Write(node.CheckForOverflow() != LSLLiteralOverflowType.None ? (parentIsUnaryNegate ? "1" : "-1") : node.RawText);

            if (!parentIsNonLogicBinaryOperation)
            {
                Writer.Write(")");
            }

            return false;
        }


        public override bool VisitListLiteral(ILSLListLiteralNode node)
        {
            if (node.ExpressionList.Expressions.Count > 0)
            {
                Writer.Write("(new " + LSLType_To_CSharpType(LSLType.List) + "(");
                Visit(node.ExpressionList);
                Writer.Write("))");
            }
            else
            {
                Writer.Write("(new " + LSLType_To_CSharpType(LSLType.List) + "())");
            }

            return false;
        }


        public override bool VisitRotationLiteral(ILSLRotationLiteralNode node)
        {
            Writer.Write("(new " + LSLType_To_CSharpType(LSLType.Rotation) + "(");
            Visit(node.XExpression);
            Writer.Write(", ");
            Visit(node.YExpression);
            Writer.Write(", ");
            Visit(node.ZExpression);
            Writer.Write(", ");
            Visit(node.SExpression);
            Writer.Write("))");

            return false;
        }


        public override bool VisitStringLiteral(ILSLStringLiteralNode node)
        {
            bool parentIsFunctionCall = false;


            var parentExpressionList = node.Parent as ILSLExpressionListNode;

            var parentAsBinaryExpression = node.Parent as ILSLBinaryExpressionNode;

            //Strings cannot be used with logical operators, but the same check is made here.
            //This is here so that, if the binary operation validator is changed to allow it;
            //than we will still be generating correct code.
            bool parentIsNonLogicBinaryOperation = (parentAsBinaryExpression != null &&
                                                    !(parentAsBinaryExpression.Operation ==
                                                      LSLBinaryOperationType.LogicalAnd ||
                                                      parentAsBinaryExpression.Operation ==
                                                      LSLBinaryOperationType.LogicalOr));



            bool parentIsVariableDeclaration = node.Parent is ILSLVariableDeclarationNode;


            ILSLFunctionCallNode parentFunctionCallNode = null;
            if (parentExpressionList != null)
            {
                parentFunctionCallNode = node.Parent.Parent as ILSLFunctionCallNode;
                if (parentFunctionCallNode != null)
                {
                    parentIsFunctionCall = true;
                }
            }

            bool inModInvokeTopLevel = false;

            if (parentFunctionCallNode != null && parentFunctionCallNode.IsLibraryFunctionCall())
            {
                var libDataNode =
                    LibraryDataProvider.GetLibraryFunctionSignature(parentFunctionCallNode.Signature);

                inModInvokeTopLevel = libDataNode.ModInvoke;
            }

            //If the parent is a binary expression, the conversion will happen automagically because
            //the string literal becomes the argument of a stub function

            //Except if the parent is a logical operator, in which case a stub is not used.
            var box = !(parentIsFunctionCall || parentIsNonLogicBinaryOperation || parentIsVariableDeclaration) || inModInvokeTopLevel;


            if (box)
            {
                Writer.Write("new " + LSLType_To_CSharpType(LSLType.String) + "(");
            }

            Writer.Write(node.PreProcessedText);

            if (box)
            {
                Writer.Write(")");
            }

            return false;
        }


        public override bool VisitVectorLiteral(ILSLVectorLiteralNode node)
        {
            Writer.Write("(new " + LSLType_To_CSharpType(LSLType.Vector) + "(");
            Visit(node.XExpression);
            Writer.Write(", ");
            Visit(node.YExpression);
            Writer.Write(", ");
            Visit(node.ZExpression);
            Writer.Write("))");

            return false;
        }

        #endregion

        #region Utilitys

        private static string LSLTypeName_To_CSharpDefaultInitializer(string typeName)
        {
            var type = LSLTypeTools.FromLSLTypeName(typeName);

            if (type == LSLType.String || type == LSLType.Key)
            {
                return "\"\"";
            }
            if (type == LSLType.Integer)
            {
                return "0";
            }
            if (type == LSLType.Float)
            {
                return "0.0";
            }
            if (type == LSLType.Rotation)
            {
                return "new " + LSLType_To_CSharpType(type) + "(0,0,0,1)";
            }
            if (type == LSLType.Vector)
            {
                return "new " + LSLType_To_CSharpType(type) + "(0,0,0)";
            }

            return "new " + LSLType_To_CSharpType(type) + "()";
        }


        private static string LSLType_To_CSharpType(LSLType type)
        {
            return LSLType_To_CSharpType(type, false);
        }


        private static string LSLType_To_CSharpType(LSLType type, bool keyAsString)
        {
            switch (type)
            {
                case LSLType.Vector:
                    return "LSL_Types.Vector3";
                case LSLType.Rotation:
                    return "LSL_Types.Quaternion";
                case LSLType.List:
                    return "LSL_Types.list";
                case LSLType.Key:
                    return keyAsString ? "LSL_Types.LSLString" : "LSL_Types.key";
                case LSLType.Integer:
                    return "LSL_Types.LSLInteger";
                case LSLType.String:
                    return "LSL_Types.LSLString";
                case LSLType.Float:
                    return "LSL_Types.LSLFloat";
                case LSLType.Void:
                    return "void";
            }

            return "";
        }


        private void CreateGlobalVariablesConstructor(ILSLCompilationUnitNode node)
        {
            //only generate code for global variables that are referenced from somewhere.
            //complex expressions and functions cannot be used in a variable declaration.
            //
            //you are limited to using a literal, or a reference to another global variable.
            //the only operator thats really allowed in a global declaration expression tree is negation.
            //
            //therefore we do not need to check if theres a modifying operation in the declaration expression
            //before pruning out global variables that are never referenced.
            var referencedGlobalVariables = node.GlobalVariableDeclarations.Where(x => x.References.Count != 0).ToList();


            //define public members, without initialization
            foreach (var gvar in referencedGlobalVariables)
            {
                Writer.WriteLine(GenIndent() + "public " +
                                 LSLType_To_CSharpType(gvar.Type, Settings.KeysAreStrings) +
                                 " " + GlobalFieldsNamePrefix + gvar.Name + ";");
            }


            if (referencedGlobalVariables.Count > 0)
            {
                Writer.Write(Environment.NewLine + Environment.NewLine);
            }

            //use the fall-back class name if the settings did not specify one
            var className = Settings.GeneratedClassName == null
                ? new CSharpClassDeclarationName(LSLOpenSimCompilerSettings.DefaultGeneratedClassName).BaseName
                : Settings.GeneratedClassName.BaseName;


            var constructorSig = Settings.GeneratedConstructorSignature == null
                ? LSLOpenSimCompilerSettings.DefaultGeneratedConstructorSignature
                : Settings.GeneratedConstructorSignature.FullSignature;


            Writer.WriteLine(GenIndent() + Settings.GeneratedConstructorAccessibility.ToCSharpKeyword(true) + className + constructorSig);
            Writer.WriteLine(GenIndent() + "{");

            _indentLevel++;


            //initialize them in the constructor, as LSL allows its globals to reference each other
            //and CSharp does not allow class members to reference each other when being initialized
            //in the top level of the class
            foreach (var gvar in referencedGlobalVariables)
            {
                Writer.Write(GenIndent() + GlobalFieldsNamePrefix + gvar.Name + " = ");

                VisitVariableDeclarationExpression(gvar);
                Writer.WriteLine(";");
            }


            _indentLevel--;

            Writer.WriteLine(GenIndent() + "}");

            Writer.Write(Environment.NewLine + Environment.NewLine);
        }


        private void CreateGlobalVariablesClass(ILSLCompilationUnitNode node)
        {
            //only generate code for global variables that are referenced from somewhere.
            //complex expressions and functions cannot be used in a variable declaration.
            //
            //you are limited to using a literal, or a reference to another global variable.
            //the only operator thats really allowed in a global declaration expression tree is negation.
            //
            //therefore we do not need to check if theres a modifying operation in the declaration expression
            //before pruning out global variables that are never referenced.
            var referencedGlobalVariables = node.GlobalVariableDeclarations.Where(x => x.References.Count != 0).ToList();


            if (referencedGlobalVariables.Count == 0)
            {
                //don't even bother to create the container class if we do not have any global variables
                //or none of them were ever referenced anywhere.

                return;
            }


            Writer.WriteLine(GenIndent() + "//===============================");
            Writer.WriteLine(GenIndent() + "//== Global Variable Container ==");
            Writer.WriteLine(GenIndent() + "//===============================");
            Writer.WriteLine(GenIndent() + "private class " + GlobalsContainerClassName);
            Writer.WriteLine(GenIndent() + "{");

            _indentLevel++;


            //define public members, without initialization
            foreach (var gvar in referencedGlobalVariables)
            {
                Writer.WriteLine(GenIndent() + "public " +
                                 LSLType_To_CSharpType(gvar.Type, Settings.KeysAreStrings) +
                                 " " + GlobalContainerFieldsNamePrefix + gvar.Name + ";");
            }

            Writer.WriteLine(GenIndent() + "public GLOBALS()");
            Writer.WriteLine(GenIndent() + "{");

            _indentLevel++;

            _writingGlobalVariableContainerClassConstructor = true;

            //initialize them in the constructor, as LSL allows its globals to reference each other
            //and CSharp does not allow class members to reference each other when being initialized
            //in the top level of the class
            foreach (var gvar in referencedGlobalVariables)
            {
                Writer.Write(GenIndent() + "this." + GlobalContainerFieldsNamePrefix + gvar.Name + " = ");

                VisitVariableDeclarationExpression(gvar);
                Writer.WriteLine(";");
            }

            _writingGlobalVariableContainerClassConstructor = false;

            _indentLevel--;

            Writer.WriteLine(GenIndent() + "}");

            _indentLevel--;

            Writer.WriteLine(GenIndent() + "}");

            Writer.Write(Environment.NewLine + Environment.NewLine);

            Writer.WriteLine(GenIndent() + "GLOBALS Globals = new GLOBALS();");

            Writer.Write(Environment.NewLine + Environment.NewLine);
        }


        /// <summary>
        /// This stub is used for both local variable declaration expressions and global variable.
        /// declaration expressions.
        /// </summary>
        /// <param name="declaration">The declaration.</param>
        private void VisitVariableDeclarationExpression(ILSLVariableDeclarationNode declaration)
        {
            if (!declaration.HasDeclarationExpression)
            {
                Writer.Write(LSLTypeName_To_CSharpDefaultInitializer(declaration.TypeName));
                return;
            }

            if (Settings.KeysAreStrings)
            {
                Visit(declaration.DeclarationExpression);
            }
            else
            {
                if (declaration.Type == LSLType.Key &&
                    declaration.DeclarationExpression.Type == LSLType.String)
                {
                    var declarationExpressionAsLiteral = declaration.DeclarationExpression as ILSLStringLiteralNode;

                    if (declarationExpressionAsLiteral != null)
                    {
                        Writer.Write(declarationExpressionAsLiteral.PreProcessedText);
                    }
                    else
                    {
                        Writer.Write("new " + LSLType_To_CSharpType(LSLType.Key) + "(");
                        Visit(declaration.DeclarationExpression);
                        Writer.Write(")");
                    }
                }
                else
                {
                    Visit(declaration.DeclarationExpression);
                }
            }
        }


        private string GenIndent(int extra = 0)
        {
            var result = "";
            for (var i = 0; i < _indentLevel + extra; i++)
            {
                result += "\t";
            }
            return result;
        }


        private void WriteBooleanConditionContent(LSLType expressionType, ILSLReadOnlyExprNode conditionExpression)
        {
            if (Settings.KeysAreStrings && expressionType == LSLType.Key)
            {
                Writer.Write("new " + LSLType_To_CSharpType(LSLType.Key) + "(");

                var isTypeCast = conditionExpression as ILSLTypecastExprNode;
                if (isTypeCast != null)
                {
                    var isStringLiteral = isTypeCast.CastedExpression as ILSLStringLiteralNode;

                    //slight optimization
                    if (isStringLiteral != null)
                    {
                        Writer.Write(isStringLiteral.PreProcessedText);
                    }
                    else
                    {
                        Visit(conditionExpression);
                    }
                }
                else
                {
                    Visit(conditionExpression);
                }

                Writer.Write(")");
            }
            else if (expressionType == LSLType.String)
            {
                Writer.Write("UTILITIES.ToBool(");
                Visit(conditionExpression);
                Writer.Write(")");
            }
            else
            {
                Visit(conditionExpression);
            }
        }


        /// <summary>
        ///     need a small utility library compiled in the generated csharp code
        ///     because OpenSim lacks several lsl features like using vectors, rotations, and list in condition statements
        /// </summary>
        private void WriteUtilityLibrary()
        {
            var typeReplacer = new Regex(@"\{\s*?LSLType\.([A-z]+?)\s*?\}");

            WriteMultiLineIndentedString(
                typeReplacer.Replace(UtilityLibrary, match =>
            {
                var type = match.Groups[1].ToString();
                LSLType parsed;
                if (Enum.TryParse(type, out parsed))
                {
                    return LSLType_To_CSharpType(parsed);
                }
                throw new LSLCompilerInternalException(string.Format("Unexpected LSLType template \"{0}\" while replacing type templates in compiler utility library.", type));
            }));
        }


        private void WriteMultiLineIndentedString(string str)
        {
            var splitStr = str.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

            foreach (var line in splitStr)
            {
                Writer.WriteLine(GenIndent() + line);
            }
        }

        #endregion

        #region LoopConstructs

        public override bool VisitDoLoop(ILSLDoLoopNode node)
        {
            if (node.IsDeadCode) return false;

            Writer.WriteLine(GenIndent() + "do");

            Visit(node.Code);

            Writer.Write(GenIndent() + "while(");

            WriteBooleanConditionContent(node.ConditionExpression.Type, node.ConditionExpression);

            Writer.WriteLine(");");

            return false;
        }


        public override bool VisitForLoop(ILSLForLoopNode node)
        {
            if (node.IsDeadCode) return false;

            Writer.Write(GenIndent() + "for(");

            if (node.HasInitExpressions)
            {
                Visit(node.InitExpressionList);
            }

            Writer.Write(";");

            if (node.HasConditionExpression)
            {
                WriteBooleanConditionContent(node.ConditionExpression.Type, node.ConditionExpression);
            }

            Writer.Write(";");


            if (node.HasAfterthoughtExpressions)
            {
                Visit(node.AfterthoughtExpressionList);
            }

            Writer.WriteLine(")");

            Visit(node.Code);

            return false;
        }


        public override bool VisitWhileLoop(ILSLWhileLoopNode node)
        {
            if (node.IsDeadCode) return false;

            Writer.Write(GenIndent() + "while(");

            WriteBooleanConditionContent(node.ConditionExpression.Type, node.ConditionExpression);

            Writer.WriteLine(")");


            Visit(node.Code);

            return false;
        }

        #endregion

        #region ScopesAndDeclarations

        public override bool VisitCodeScope(ILSLCodeScopeNode node)
        {
            if (node.IsDeadCode) return false;

            Writer.WriteLine(GenIndent() + "{");

            _indentLevel++;

            if (Settings.InsertCoOpTerminationCalls)
            {
                //if the co-op termination setting is enabled, insert call stubs
                //at the top of user defined functions, event handlers, and loop constructs
                switch (node.CodeScopeType)
                {
                    case LSLCodeScopeType.EventHandler:
                    case LSLCodeScopeType.Function:
                    case LSLCodeScopeType.DoLoop:
                    case LSLCodeScopeType.WhileLoop:
                    case LSLCodeScopeType.ForLoop:


                        Writer.WriteLine(GenIndent() + GetCoOpTerminationCallString() + ";");
                        break;
                }
            }


            /*
                Write all variable declarations that are considered to be dead due to a jump over code in this scope
                to the top of the scope and initialize them with the default value for their type.

                Dead variable declarations inside of a constant jump over code can still be referenced within the same scope,
                we need to make sure they are initialized or the CSharp compiler will complain when they are referenced.

                They can be put at the top of the scope because the code validator has guaranteed they are only referenced
                from places after their original definition point.

                When a dead variable declaration node is encountered later in the syntax tree, it is discarded.  Because if it
                needed to be defined to produce valid generated code it was already defined here at the top of the scope with a default value.
            */
            var deadVariableDeclarationNodes =
                node.CodeStatements.Where(
                    x =>
                        x.IsDeadCode && x.DeadCodeType == LSLDeadCodeType.JumpOverCode &&
                        x is ILSLVariableDeclarationNode)
                    .Cast<ILSLVariableDeclarationNode>();


            foreach (var deadVariableDeclarationNode in deadVariableDeclarationNodes)
            {
                //we also want to filter out variables that were never referenced, and who's declarations have no side effect on program state.
                if (SafeToPruneLocalVariableDeclaration(deadVariableDeclarationNode)) continue;


                var variableName = "Var" + node.ParentScopeId + "_" + deadVariableDeclarationNode.Name;

                Writer.Write(GenIndent());
                Writer.Write(LSLType_To_CSharpType(deadVariableDeclarationNode.Type, Settings.KeysAreStrings));
                Writer.Write(" ");
                Writer.Write(variableName);
                Writer.Write(" = ");
                Writer.Write(LSLTypeName_To_CSharpDefaultInitializer(deadVariableDeclarationNode.TypeName));
                Writer.WriteLine(";");
            }


            foreach (var statement in node.CodeStatements)
            {
                Visit(statement);
            }

            _indentLevel--;

            Writer.WriteLine(GenIndent() + "}");

            return false;
        }


        private void WriteBinaryOperatorOverloadStubs()
        {
            //re-write overloads for used binary operations, they take parameters in reverse
            //order and execute the operation in normal order to simulate lsl's weird ass right to 
            //left evaluation, _binOpsUsed is added to whenever a binary expression is encountered


            var binopCount = _binOpsUsed.Count;
            var binopCounter = 0;


            if (binopCount > 0)
            {
                Writer.WriteLine();
                Writer.WriteLine(GenIndent() + "//===========================");
                Writer.WriteLine(GenIndent() + "//== Binary Operator Stubs ==");
                Writer.WriteLine(GenIndent() + "//===========================");
                Writer.Write(Environment.NewLine + Environment.NewLine);
            }
            else
            {
                return;
            }


            foreach (var binOp in _binOpsUsed)
            {
                //it's safe to convert any key parameter types to strings here.
                //key arguments will implicitly convert into string parameters.
                //the only binary operations allowed on keys are equality, which returns an integer.

                //OpenSim does not support binary operations between its runtime key type and string type.
                //so all keys must be implicitly converted to strings here for now.

                //the return type generation behavior for expressions that return key depends on Settings.KeysAreStrings.
                //no binary operation returns a key currently, but if one does in the future than the stub will behave as expected; per settings.

                Writer.WriteLine(GenIndent() + "private " +
                                 LSLType_To_CSharpType(binOp.Returns, Settings.KeysAreStrings) + " " + GenBinaryOperationStubName(binOp) +
                                 "(" +
                                 LSLType_To_CSharpType(binOp.Right, true) + " right, " +
                                 LSLType_To_CSharpType(binOp.Left, true) + " left)");

                Writer.WriteLine(GenIndent() + "{");
                _indentLevel++;

                Writer.WriteLine(GenIndent() + "return left" + binOp.Operation.ToOperatorString() + "right;");

                _indentLevel--;
                Writer.WriteLine(GenIndent() + "}");

                if (binopCounter != (binopCount - 1))
                {
                    Writer.Write(Environment.NewLine + Environment.NewLine);
                }

                binopCounter++;
            }

            _binOpsUsed.Clear();
        }


        public override bool VisitCompilationUnit(ILSLCompilationUnitNode unode)
        {
            bool hasGeneratedClassNameSpaceName = Settings.GeneratedClassNamespace != null;


            if (!string.IsNullOrWhiteSpace(Settings.ScriptHeader))
            {
                Writer.WriteLine(Settings.ScriptHeader);
                Writer.Write(Environment.NewLine);
            }


            Writer.WriteLine(GenIndent() + "//Compiled by LibLSLCC, Date: {0}", DateTime.Now);
            Writer.Write(Environment.NewLine);

            if (Settings.GenerateClass)
            {
                if (Settings.GeneratedNamespaceImports != null)
                {
                    foreach (var ns in Settings.GeneratedNamespaceImports)
                    {
                        Writer.WriteLine("using " + ns.FullSignature + ";");
                    }
                }

                Writer.Write(Environment.NewLine);

                if (hasGeneratedClassNameSpaceName)
                {
                    Writer.WriteLine("namespace {0}", Settings.GeneratedClassNamespace);
                    Writer.WriteLine("{");
                    _indentLevel++;
                }

                var className = Settings.GeneratedClassName == null
                    ? LSLOpenSimCompilerSettings.DefaultGeneratedClassName
                    : Settings.GeneratedClassName.FullSignature;

                var classAccessibilityString =
                    Settings.GeneratedClassAccessibility.ToCSharpKeyword(true);


                if (Settings.GeneratedInheritanceList != null && !Settings.GeneratedInheritanceList.IsEmpty)
                {
                    Writer.WriteLine(GenIndent() + "{0}class {1} {2}",
                        classAccessibilityString, className,
                        Settings.GeneratedInheritanceList.ListWithColonIfNecessary);
                }
                else
                {
                    Writer.WriteLine(GenIndent() + "{0}class {1}",
                        classAccessibilityString, className);
                }

                Writer.WriteLine(GenIndent() + "{");

                Writer.Write(Environment.NewLine);

                _indentLevel++;
            }


            WriteUtilityLibrary();


            Writer.Write(Environment.NewLine);


            if (Settings.GenerateClass)
            {
                //If we are generating a class, we can define the global variables in the class body
                //and init them in the constructor.
                //
                //This allows global variables to reference each other.
                //
                //This also supports for script resets in OpenSim.
                //OpenSim can serialize and de-serialze the global variables this way
                //when saving and restoring script state since they are just fields in the class body.
                CreateGlobalVariablesConstructor(unode);
            }
            else
            {
                //this method will only create a global variable container
                //if no class is being generated, this does not support script resets in OpenSim.
                CreateGlobalVariablesClass(unode);
            }


            //only visit function declarations that have references to them
            //we do not need to generate code for functions that were never referenced.
            var referencedUserDefinedFunctions = unode.FunctionDeclarations.Where(x => x.References.Count != 0).ToList();

            if (referencedUserDefinedFunctions.Count > 0)
            {
                Writer.WriteLine(GenIndent() + "//============================");
                Writer.WriteLine(GenIndent() + "//== User Defined Functions ==");
                Writer.WriteLine(GenIndent() + "//============================");
                Writer.Write(Environment.NewLine + Environment.NewLine);
            }


            foreach (var ctx in referencedUserDefinedFunctions)
            {
                VisitFunctionDeclaration(ctx);
                Writer.Write(Environment.NewLine + Environment.NewLine);
            }


            if (unode.StateDeclarations.Count > 0)
            {
                Writer.WriteLine();
                Writer.WriteLine(GenIndent() + "//=======================================");
                Writer.WriteLine(GenIndent() + "//== User Defined State Event Handlers ==");
                Writer.WriteLine(GenIndent() + "//=======================================");
                Writer.Write(Environment.NewLine + Environment.NewLine);
            }

            foreach (var ctx in unode.StateDeclarations)
            {
                VisitDefinedState(ctx);
                Writer.Write(Environment.NewLine + Environment.NewLine);
            }

            Writer.WriteLine();
            Writer.WriteLine(GenIndent() + "//==================================");
            Writer.WriteLine(GenIndent() + "//== Default State Event Handlers ==");
            Writer.WriteLine(GenIndent() + "//==================================");
            Writer.Write(Environment.NewLine + Environment.NewLine);


            VisitDefaultState(unode.DefaultStateNode);


            if (_binOpsUsed.Count > 0)
            {
                Writer.Write(Environment.NewLine + Environment.NewLine);
            }


            WriteBinaryOperatorOverloadStubs();


            if (!Settings.GenerateClass) return false;


            if (hasGeneratedClassNameSpaceName)
            {
                _indentLevel--;

                Writer.WriteLine(GenIndent() + "}");
            }


            Writer.WriteLine("}");


            return false;
        }


        public override bool VisitEventHandler(ILSLEventHandlerNode node)
        {
            _currentLslEventHandlerNode = node;

            Writer.Write(GenIndent());

            var handlerName = _currentLslStateNode.StateName + "_event_" + node.Name;


            if (node.ParameterList.Parameters.Count > 0)
            {
                Writer.Write("public void " + handlerName + "(");
                Visit(node.ParameterList);
                Writer.WriteLine(")");
            }
            else
            {
                Writer.WriteLine("public void " + handlerName + "()");
            }

            Visit(node.Code);

            _currentLslEventHandlerNode = null;

            return false;
        }


        /// <summary>
        ///     Default implementation calls Visit(node.ParameterListNode) then Visit(node.FunctionBodyNode)
        ///     and returns default(T)
        /// </summary>
        /// <param name="node">An object describing the function declaration</param>
        /// <returns>default(T)</returns>
        public override bool VisitFunctionDeclaration(ILSLFunctionDeclarationNode node)
        {
            //_currentLslFunctionDeclarationNode = node;

            Writer.Write(GenIndent() + "public ");

            if (node.ReturnType != LSLType.Void)
            {
                Writer.Write(LSLType_To_CSharpType(node.ReturnType, Settings.KeysAreStrings) + " ");
            }
            else
            {
                Writer.Write("void ");
            }

            var functionName = FunctionNamePrefix + node.Name;


            Writer.Write(functionName + "(");

            if (node.ParameterList.Parameters.Count > 0)
            {
                Visit(node.ParameterList);
            }

            Writer.WriteLine(")");

            Visit(node.Code);

            //_currentLslFunctionDeclarationNode = null;

            return false;
        }


        public override bool VisitDefaultState(ILSLStateScopeNode node)
        {
            _currentLslStateNode = node;

            var eventHandlers = node.EventHandlers;
            var i = 0;
            for (; i < eventHandlers.Count - 1; i++)
            {
                Visit(eventHandlers[i]);
                Writer.WriteLine();
            }

            Visit(eventHandlers[i]);

            _currentLslStateNode = null;

            return false;
        }


        public override bool VisitDefinedState(ILSLStateScopeNode node)
        {
            _currentLslStateNode = node;

            var eventHandlers = node.EventHandlers;
            var i = 0;
            for (; i < eventHandlers.Count - 1; i++)
            {
                Visit(eventHandlers[i]);
                Writer.WriteLine();
            }

            Visit(eventHandlers[i]);

            _currentLslStateNode = null;

            return false;
        }


        public override bool VisitParameterDefinition(ILSLParameterNode node)
        {
            Writer.Write(LSLType_To_CSharpType(node.Type, Settings.KeysAreStrings) + " " + LocalParameterNamePrefix + node.Name);
            return false;
        }


        public override bool VisitParameterDefinitionList(ILSLParameterListNode node)
        {
            if (node.Parameters.Count > 1)
            {
                var i = 0;
                for (; i < node.Parameters.Count - 1; i++)
                {
                    Visit(node.Parameters[i]);
                    Writer.Write(", ");
                }

                Visit(node.Parameters[i]);
            }
            else if (node.Parameters.Count == 1)
            {
                Visit(node.Parameters[0]);
            }


            return false;
        }

        #endregion

        #region CodeStatements

        public override bool VisitReturnStatement(ILSLReturnStatementNode node)
        {
            if (node.IsDeadCode) return false;

            //Check if we are inside of an event handler, and if a return value is present.
            if (_currentLslEventHandlerNode != null && node.HasReturnExpression)
            {
                //Returning a value from an event handler is allowed in LSL, LibLSLCC only generates a warning now.
                //
                //I am not going to bother optimizing out the need for UTILITIES.ForceStatement,
                //because returning a value from an event handler is pointless and probably used infrequently

                Writer.Write(GenIndent() + "UTILITIES.ForceStatement(");
                Visit(node.ReturnExpression);
                Writer.WriteLine(");");
            }
            else
            {
                Writer.Write(GenIndent() + "return");

                if (node.HasReturnExpression)
                {
                    Writer.Write(" ");
                    Visit(node.ReturnExpression);
                }

                Writer.WriteLine(";");
            }

            return false;
        }


        public override bool VisitSemicolonStatement(ILSLSemicolonStatement node)
        {
            //stand alone semi colons are not necessary as we transform all single block
            //statements into code scopes with { } around them

            //Writer.Write(GenIndent() + ";");

            return false;
        }


        public override bool VisitStateChangeStatement(ILSLStateChangeStatementNode node)
        {
            if (node.IsDeadCode) return false;

            Writer.WriteLine(GenIndent() + "this.state(\"" + node.StateTargetName + "\");");
            return false;
        }


        public override bool VisitJumpStatement(ILSLJumpStatementNode node)
        {
            if (node.IsDeadCode) return false;

            Writer.WriteLine(GenIndent() + "goto " + "LSLLabel_" + node.LabelName + ";");
            return false;
        }


        public override bool VisitLabelStatement(ILSLLabelStatementNode node)
        {
            //Labels should not be omitted if they are dead code, they can be referenced still and if the definition is missing that a problem.
            //if (node.IsDeadCode) return false;

            //we can however, remove the label from the generated source code if it was never referenced.
            if (node.JumpsToHere.Count == 0) return false;

            if (Settings.InsertCoOpTerminationCalls)
            {
                Writer.WriteLine(GenIndent() + "LSLLabel_" + node.LabelName + ":" + GetCoOpTerminationCallString() + ";");
            }
            else
            {
                Writer.WriteLine(GenIndent() + "LSLLabel_" + node.LabelName + ":" +
                                 (node.IsLastStatementInScope ? ";" : ""));
            }

            return false;
        }


        public override bool VisitExpressionStatement(ILSLExpressionStatementNode node)
        {
            if (node.IsDeadCode) return false;

            if (node.HasPossibleSideEffects)
            {
                Writer.Write(GenIndent());
                Visit(node.Expression);
                Writer.WriteLine(";");
            }
            return false;
        }


        /// <summary>
        ///     Returns true if a given variable declaration node has no references to it later in the source code, and
        ///     its declaration contains no expressions that could modify program state.
        /// </summary>
        /// <param name="declarationNode">The declaration node to test.</param>
        /// <returns></returns>
        private static bool SafeToPruneLocalVariableDeclaration(ILSLVariableDeclarationNode declarationNode)
        {
            if (declarationNode.References.Count == 0)
            {
                //the declaration can safely be pruned if it has no declaration expression.
                //simply defining it can have no effect on the program.
                if (!declarationNode.HasDeclarationExpression) return true;

                if (declarationNode.HasDeclarationExpression &&
                    !declarationNode.DeclarationExpression.HasPossibleSideEffects)
                {
                    //prune the variable if its declaration cannot possibly have any side effects on the program state.
                    //this meaning its declaration contains no function calls or operations that modify variables.
                    return true;
                }
            }
            return false;
        }


        public override bool VisitLocalVariableDeclaration(ILSLVariableDeclarationNode node)
        {
            /* 
               See VisitCodeScope to see where declarations of variables that exist inside dead code
               are put in the generated code.  They are put at the top of the scope they are declared in,
               and initialize with their default values.  They are written out in the VisitCodeScope method
               before we even get to this point in the syntax tree, so they do not need to be written again.

               Also, if the variable is never referenced, there is no need to generate code for it.

               SafeToPruneLocalVariableDeclaration is used to check if the variable has no references
               to it, and if its declaration expression contains no operations that can modify the program state.

               if it meets those criteria, it can safely be pruned from the generated code.
            */

            if (node.IsDeadCode || SafeToPruneLocalVariableDeclaration(node)) return false;


            var variableName = LocalVariableNamePrefix + node.ParentScopeId + "_" + node.Name;

            Writer.Write(GenIndent() + LSLType_To_CSharpType(node.Type, Settings.KeysAreStrings) + " " + variableName + " = ");

            VisitVariableDeclarationExpression(node);
            Writer.WriteLine(";");

            return false;
        }


        #endregion

        #region BranchStatements

        public override bool VisitControlStatement(ILSLControlStatementNode node)
        {
            if (node.IsDeadCode) return false;

            return base.VisitControlStatement(node);
        }


        public override bool VisitIfStatement(ILSLIfStatementNode node)
        {
            Writer.Write(GenIndent() + "if(");

            WriteBooleanConditionContent(node.ConditionExpression.Type, node.ConditionExpression);

            Writer.WriteLine(")");

            Visit(node.Code);


            return false;
        }


        public override bool VisitElseIfStatement(ILSLElseIfStatementNode node)
        {
            Writer.Write(GenIndent() + "else if(");

            WriteBooleanConditionContent(node.ConditionExpression.Type, node.ConditionExpression);

            Writer.WriteLine(")");


            Visit(node.Code);

            return false;
        }


        public override bool VisitElseStatement(ILSLElseStatementNode node)
        {
            Writer.WriteLine(GenIndent() + "else");
            Visit(node.Code);
            return false;
        }

        #endregion
    }
}