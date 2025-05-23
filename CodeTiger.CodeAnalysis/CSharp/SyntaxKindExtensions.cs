﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeTiger.CodeAnalysis.CSharp;

internal static class SyntaxKindExtensions
{
    public static string GetDeclarationName(this SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind.FieldDeclaration:
                return "field";
            case SyntaxKind.EventFieldDeclaration:
                return "event field";
            case SyntaxKind.PropertyDeclaration:
                return "property";
            case SyntaxKind.IndexerDeclaration:
                return "indexer";
            case SyntaxKind.DelegateDeclaration:
                return "delegate";
            case SyntaxKind.EventDeclaration:
                return "event";
            case SyntaxKind.ConstructorDeclaration:
                return "constructor";
            case SyntaxKind.DestructorDeclaration:
                return "destructor";
            case SyntaxKind.MethodDeclaration:
                return "method";
            case SyntaxKind.ConversionOperatorDeclaration:
                return "conversion operator";
            case SyntaxKind.OperatorDeclaration:
                return "operator";
            case SyntaxKind.EnumDeclaration:
                return "enum";
            case SyntaxKind.InterfaceDeclaration:
                return "interface";
            case SyntaxKind.ClassDeclaration:
                return "class";
            case SyntaxKind.StructDeclaration:
                return "struct";
            default:
                return kind.ToString();
        }
    }

    public static string GetKeywordName(this SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind.BoolKeyword:
                return "bool";
            case SyntaxKind.ByteKeyword:
                return "byte";
            case SyntaxKind.SByteKeyword:
                return "sbyte";
            case SyntaxKind.ShortKeyword:
                return "short";
            case SyntaxKind.UShortKeyword:
                return "ushort";
            case SyntaxKind.IntKeyword:
                return "int";
            case SyntaxKind.UIntKeyword:
                return "uint";
            case SyntaxKind.LongKeyword:
                return "long";
            case SyntaxKind.ULongKeyword:
                return "ulong";
            case SyntaxKind.DoubleKeyword:
                return "double";
            case SyntaxKind.FloatKeyword:
                return "float";
            case SyntaxKind.DecimalKeyword:
                return "decimal";
            case SyntaxKind.StringKeyword:
                return "string";
            case SyntaxKind.CharKeyword:
                return "char";
            case SyntaxKind.VoidKeyword:
                return "void";
            case SyntaxKind.ObjectKeyword:
                return "object";
            case SyntaxKind.TypeOfKeyword:
                return "typeof";
            case SyntaxKind.SizeOfKeyword:
                return "sizeof";
            case SyntaxKind.NullKeyword:
                return "null";
            case SyntaxKind.TrueKeyword:
                return "true";
            case SyntaxKind.FalseKeyword:
                return "false";
            case SyntaxKind.IfKeyword:
                return "if";
            case SyntaxKind.ElseKeyword:
                return "else";
            case SyntaxKind.WhileKeyword:
                return "while";
            case SyntaxKind.ForKeyword:
                return "for";
            case SyntaxKind.ForEachKeyword:
                return "foreach";
            case SyntaxKind.DoKeyword:
                return "do";
            case SyntaxKind.SwitchKeyword:
                return "switch";
            case SyntaxKind.CaseKeyword:
                return "case";
            case SyntaxKind.DefaultKeyword:
                return "default";
            case SyntaxKind.TryKeyword:
                return "try";
            case SyntaxKind.CatchKeyword:
                return "catch";
            case SyntaxKind.FinallyKeyword:
                return "finally";
            case SyntaxKind.LockKeyword:
                return "lock";
            case SyntaxKind.GotoKeyword:
                return "goto";
            case SyntaxKind.BreakKeyword:
                return "break";
            case SyntaxKind.ContinueKeyword:
                return "continue";
            case SyntaxKind.ReturnKeyword:
                return "return";
            case SyntaxKind.ThrowKeyword:
                return "throw";
            case SyntaxKind.PublicKeyword:
                return "public";
            case SyntaxKind.PrivateKeyword:
                return "private";
            case SyntaxKind.InternalKeyword:
                return "internal";
            case SyntaxKind.ProtectedKeyword:
                return "protected";
            case SyntaxKind.StaticKeyword:
                return "static";
            case SyntaxKind.ReadOnlyKeyword:
                return "readonly";
            case SyntaxKind.SealedKeyword:
                return "sealed";
            case SyntaxKind.ConstKeyword:
                return "const";
            case SyntaxKind.FixedKeyword:
                return "fixed";
            case SyntaxKind.StackAllocKeyword:
                return "stackalloc";
            case SyntaxKind.VolatileKeyword:
                return "volatile";
            case SyntaxKind.NewKeyword:
                return "new";
            case SyntaxKind.OverrideKeyword:
                return "override";
            case SyntaxKind.AbstractKeyword:
                return "abstract";
            case SyntaxKind.VirtualKeyword:
                return "virtual";
            case SyntaxKind.EventKeyword:
                return "event";
            case SyntaxKind.ExternKeyword:
                return "extern";
            case SyntaxKind.RefKeyword:
                return "ref";
            case SyntaxKind.OutKeyword:
                return "out";
            case SyntaxKind.InKeyword:
                return "in";
            case SyntaxKind.IsKeyword:
                return "is";
            case SyntaxKind.AsKeyword:
                return "as";
            case SyntaxKind.ParamsKeyword:
                return "params";
            case SyntaxKind.ThisKeyword:
                return "this";
            case SyntaxKind.BaseKeyword:
                return "base";
            case SyntaxKind.NamespaceKeyword:
                return "namespace";
            case SyntaxKind.UsingKeyword:
                return "using";
            case SyntaxKind.ClassKeyword:
                return "class";
            case SyntaxKind.StructKeyword:
                return "struct";
            case SyntaxKind.InterfaceKeyword:
                return "interface";
            case SyntaxKind.EnumKeyword:
                return "enum";
            case SyntaxKind.DelegateKeyword:
                return "delegate";
            case SyntaxKind.CheckedKeyword:
                return "checked";
            case SyntaxKind.UncheckedKeyword:
                return "unchecked";
            case SyntaxKind.UnsafeKeyword:
                return "unsafe";
            case SyntaxKind.OperatorKeyword:
                return "operator";
            case SyntaxKind.ExplicitKeyword:
                return "explicit";
            case SyntaxKind.ImplicitKeyword:
                return "implicit";
            case SyntaxKind.YieldKeyword:
                return "yield";
            case SyntaxKind.PartialKeyword:
                return "partial";
            case SyntaxKind.GlobalKeyword:
                return "global";
            case SyntaxKind.GetKeyword:
                return "get";
            case SyntaxKind.SetKeyword:
                return "set";
            case SyntaxKind.AddKeyword:
                return "add";
            case SyntaxKind.RemoveKeyword:
                return "remove";
            case SyntaxKind.WhereKeyword:
                return "where";
            case SyntaxKind.FromKeyword:
                return "from";
            case SyntaxKind.GroupKeyword:
                return "group";
            case SyntaxKind.JoinKeyword:
                return "join";
            case SyntaxKind.IntoKeyword:
                return "into";
            case SyntaxKind.LetKeyword:
                return "let";
            case SyntaxKind.ByKeyword:
                return "by";
            case SyntaxKind.SelectKeyword:
                return "select";
            case SyntaxKind.OrderByKeyword:
                return "orderby";
            case SyntaxKind.OnKeyword:
                return "on";
            case SyntaxKind.EqualsKeyword:
                return "equals";
            case SyntaxKind.AscendingKeyword:
                return "asc";
            case SyntaxKind.DescendingKeyword:
                return "desc";
            case SyntaxKind.NameOfKeyword:
                return "nameof";
            case SyntaxKind.AsyncKeyword:
                return "async";
            case SyntaxKind.AwaitKeyword:
                return "await";
            default:
                return kind.ToString();
        }
    }

    public static bool IsCommentTrivia(this SyntaxKind kind)
    {
        switch (kind)
        {
            case SyntaxKind.DocumentationCommentExteriorTrivia:
            case SyntaxKind.EndOfDocumentationCommentToken:
            case SyntaxKind.MultiLineCommentTrivia:
            case SyntaxKind.MultiLineDocumentationCommentTrivia:
            case SyntaxKind.SingleLineCommentTrivia:
            case SyntaxKind.SingleLineDocumentationCommentTrivia:
            case SyntaxKind.XmlComment:
            case SyntaxKind.XmlCommentEndToken:
            case SyntaxKind.XmlCommentStartToken:
                return true;
            default:
                return false;
        }
    }
}
