//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.5.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from AntlrParser\LSL.g4 by ANTLR 4.5.2

// Unreachable code detected

using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace LibLSLCC.AntlrParser {
    /// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="LSLParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[GeneratedCode("ANTLR", "4.5.2")]
[CLSCompliant(false)]
public interface ILSLVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.vectorLiteral"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVectorLiteral([NotNull] LSLParser.VectorLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.rotationLiteral"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRotationLiteral([NotNull] LSLParser.RotationLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.functionDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionDeclaration([NotNull] LSLParser.FunctionDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.elseStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitElseStatement([NotNull] LSLParser.ElseStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.controlStructure"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitControlStructure([NotNull] LSLParser.ControlStructureContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.codeScope"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCodeScope([NotNull] LSLParser.CodeScopeContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.doLoop"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDoLoop([NotNull] LSLParser.DoLoopContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.whileLoop"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhileLoop([NotNull] LSLParser.WhileLoopContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.forLoop"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitForLoop([NotNull] LSLParser.ForLoopContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.loopStructure"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLoopStructure([NotNull] LSLParser.LoopStructureContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.codeStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCodeStatement([NotNull] LSLParser.CodeStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.expressionStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpressionStatement([NotNull] LSLParser.ExpressionStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.returnStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturnStatement([NotNull] LSLParser.ReturnStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.labelStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLabelStatement([NotNull] LSLParser.LabelStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.jumpStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitJumpStatement([NotNull] LSLParser.JumpStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.stateChangeStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStateChangeStatement([NotNull] LSLParser.StateChangeStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.localVariableDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLocalVariableDeclaration([NotNull] LSLParser.LocalVariableDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.globalVariableDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGlobalVariableDeclaration([NotNull] LSLParser.GlobalVariableDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.expressionListTail"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpressionListTail([NotNull] LSLParser.ExpressionListTailContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.expressionList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpressionList([NotNull] LSLParser.ExpressionListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.dotAccessorExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDotAccessorExpr([NotNull] LSLParser.DotAccessorExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.modifiableLeftValue"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitModifiableLeftValue([NotNull] LSLParser.ModifiableLeftValueContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_PrefixOperation</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_PrefixOperation([NotNull] LSLParser.Expr_PrefixOperationContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ParenthesizedExpression</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParenthesizedExpression([NotNull] LSLParser.ParenthesizedExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_Atom</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_Atom([NotNull] LSLParser.Expr_AtomContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_TypeCast</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_TypeCast([NotNull] LSLParser.Expr_TypeCastContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_DotAccessorExpr</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_DotAccessorExpr([NotNull] LSLParser.Expr_DotAccessorExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_BitwiseShift</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_BitwiseShift([NotNull] LSLParser.Expr_BitwiseShiftContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_LogicalCompare</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_LogicalCompare([NotNull] LSLParser.Expr_LogicalCompareContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_LogicalEquality</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_LogicalEquality([NotNull] LSLParser.Expr_LogicalEqualityContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_BitwiseOr</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_BitwiseOr([NotNull] LSLParser.Expr_BitwiseOrContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_Logical_And_Or</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_Logical_And_Or([NotNull] LSLParser.Expr_Logical_And_OrContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_BitwiseXor</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_BitwiseXor([NotNull] LSLParser.Expr_BitwiseXorContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_Assignment</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_Assignment([NotNull] LSLParser.Expr_AssignmentContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_MultDivMod</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_MultDivMod([NotNull] LSLParser.Expr_MultDivModContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_BitwiseAnd</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_BitwiseAnd([NotNull] LSLParser.Expr_BitwiseAndContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_PostfixOperation</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_PostfixOperation([NotNull] LSLParser.Expr_PostfixOperationContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_ModifyingAssignment</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_ModifyingAssignment([NotNull] LSLParser.Expr_ModifyingAssignmentContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_FunctionCall</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_FunctionCall([NotNull] LSLParser.Expr_FunctionCallContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Expr_AddSub</c>
	/// labeled alternative in <see cref="LSLParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr_AddSub([NotNull] LSLParser.Expr_AddSubContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.optionalExpressionList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOptionalExpressionList([NotNull] LSLParser.OptionalExpressionListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.optionalParameterList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOptionalParameterList([NotNull] LSLParser.OptionalParameterListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.listLiteral"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitListLiteral([NotNull] LSLParser.ListLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.compilationUnit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCompilationUnit([NotNull] LSLParser.CompilationUnitContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.parameterDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameterDefinition([NotNull] LSLParser.ParameterDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.parameterList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameterList([NotNull] LSLParser.ParameterListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.eventHandler"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEventHandler([NotNull] LSLParser.EventHandlerContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.definedState"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDefinedState([NotNull] LSLParser.DefinedStateContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="LSLParser.defaultState"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDefaultState([NotNull] LSLParser.DefaultStateContext context);
}
} // namespace LibLSLCC.AntlrParser
