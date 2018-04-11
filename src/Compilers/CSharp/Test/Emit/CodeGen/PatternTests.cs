﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests.CodeGen
{
    public class PatternTests : EmitMetadataTestBase
    {
        [Fact, WorkItem(18811, "https://github.com/dotnet/roslyn/issues/18811")]
        public void MissingNullable_01()
        {
            var source = @"namespace System {
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public struct Boolean { }
    public struct Int32 { }
}
static class C {
    public static bool M() => ((object)123) is int i;
}
";
            var compilation = CreateEmptyCompilation(source, options: TestOptions.ReleaseDll);
            compilation.GetDiagnostics().Verify();
            compilation.GetEmitDiagnostics().Verify(
                // warning CS8021: No value for RuntimeMetadataVersion found. No assembly containing System.Object was found nor was a value for RuntimeMetadataVersion specified through options.
                Diagnostic(ErrorCode.WRN_NoRuntimeMetadataVersion).WithLocation(1, 1)
                );
        }

        [Fact, WorkItem(18811, "https://github.com/dotnet/roslyn/issues/18811")]
        public void MissingNullable_02()
        {
            var source = @"namespace System {
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public struct Boolean { }
    public struct Int32 { }
    public struct Nullable<T> where T : struct { }
}
static class C {
    public static bool M() => ((object)123) is int i;
}
";
            var compilation = CreateEmptyCompilation(source, options: TestOptions.UnsafeReleaseDll);
            compilation.GetDiagnostics().Verify();
            compilation.GetEmitDiagnostics().Verify(
                // warning CS8021: No value for RuntimeMetadataVersion found. No assembly containing System.Object was found nor was a value for RuntimeMetadataVersion specified through options.
                Diagnostic(ErrorCode.WRN_NoRuntimeMetadataVersion)
                );
        }

        [Fact]
        public void MissingNullable_03()
        {
            var source = @"namespace System {
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public struct Boolean { }
    public struct Int32 { }
    public struct Nullable<T> where T : struct { }
}
static class C {
    static void M1(int? x)
    {
        switch (x)
        {
            case int i: break;
        }
    }
    static bool M2(int? x) => x is int i;
}
";
            var compilation = CreateEmptyCompilation(source, options: TestOptions.UnsafeReleaseDll);
            compilation.GetDiagnostics().Verify();
            compilation.GetEmitDiagnostics().Verify(
                // warning CS8021: No value for RuntimeMetadataVersion found. No assembly containing System.Object was found nor was a value for RuntimeMetadataVersion specified through options.
                Diagnostic(ErrorCode.WRN_NoRuntimeMetadataVersion).WithLocation(1, 1),
                // (14,18): error CS0656: Missing compiler required member 'System.Nullable`1.get_HasValue'
                //             case int i: break;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "int i").WithArguments("System.Nullable`1", "get_HasValue").WithLocation(14, 18),
                // (14,18): error CS0656: Missing compiler required member 'System.Nullable`1.GetValueOrDefault'
                //             case int i: break;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "int i").WithArguments("System.Nullable`1", "GetValueOrDefault").WithLocation(14, 18),
                // (14,18): error CS0656: Missing compiler required member 'System.Nullable`1.get_Value'
                //             case int i: break;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "int i").WithArguments("System.Nullable`1", "get_Value").WithLocation(14, 18),
                // (17,36): error CS0656: Missing compiler required member 'System.Nullable`1.get_HasValue'
                //     static bool M2(int? x) => x is int i;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "int i").WithArguments("System.Nullable`1", "get_HasValue").WithLocation(17, 36),
                // (17,36): error CS0656: Missing compiler required member 'System.Nullable`1.GetValueOrDefault'
                //     static bool M2(int? x) => x is int i;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "int i").WithArguments("System.Nullable`1", "GetValueOrDefault").WithLocation(17, 36),
                // (17,36): error CS0656: Missing compiler required member 'System.Nullable`1.get_Value'
                //     static bool M2(int? x) => x is int i;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "int i").WithArguments("System.Nullable`1", "get_Value").WithLocation(17, 36)
                );
        }

        [Fact]
        public void MissingNullable_04()
        {
            var source = @"namespace System {
    public class Object { }
    public abstract class ValueType { }
    public struct Void { }
    public struct Boolean { }
    public struct Int32 { }
    public struct Nullable<T> where T : struct { public T GetValueOrDefault() => default(T); }
}
static class C {
    static void M1(int? x)
    {
        switch (x)
        {
            case int i: break;
        }
    }
    static bool M2(int? x) => x is int i;
}
";
            var compilation = CreateEmptyCompilation(source, options: TestOptions.UnsafeReleaseDll);
            compilation.GetDiagnostics().Verify();
            compilation.GetEmitDiagnostics().Verify(
                // warning CS8021: No value for RuntimeMetadataVersion found. No assembly containing System.Object was found nor was a value for RuntimeMetadataVersion specified through options.
                Diagnostic(ErrorCode.WRN_NoRuntimeMetadataVersion).WithLocation(1, 1),
                // (14,18): error CS0656: Missing compiler required member 'System.Nullable`1.get_HasValue'
                //             case int i: break;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "int i").WithArguments("System.Nullable`1", "get_HasValue").WithLocation(14, 18),
                // (17,36): error CS0656: Missing compiler required member 'System.Nullable`1.get_HasValue'
                //     static bool M2(int? x) => x is int i;
                Diagnostic(ErrorCode.ERR_MissingPredefinedMember, "int i").WithArguments("System.Nullable`1", "get_HasValue").WithLocation(17, 36)
                );
        }

        [Fact, WorkItem(17266, "https://github.com/dotnet/roslyn/issues/17266")]
        public void DoubleEvaluation01()
        {
            var source =
@"using System;
public class C
{
    public static void Main()
    {
        if (TryGet() is int index)
        {
            Console.WriteLine(index);
        }
    }

    public static int? TryGet()
    {
        Console.WriteLine(""eval"");
        return null;
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.DebugExe);
            compilation.VerifyDiagnostics();
            var expectedOutput = @"eval";
            var compVerifier = CompileAndVerify(compilation, expectedOutput: expectedOutput);
            compVerifier.VerifyIL("C.Main",
@"{
  // Code size       42 (0x2a)
  .maxstack  1
  .locals init (int V_0, //index
                bool V_1,
                int? V_2)
  IL_0000:  nop
  IL_0001:  call       ""int? C.TryGet()""
  IL_0006:  stloc.2
  IL_0007:  ldloca.s   V_2
  IL_0009:  call       ""bool int?.HasValue.get""
  IL_000e:  brfalse.s  IL_001b
  IL_0010:  ldloca.s   V_2
  IL_0012:  call       ""int int?.GetValueOrDefault()""
  IL_0017:  stloc.0
  IL_0018:  ldc.i4.1
  IL_0019:  br.s       IL_001c
  IL_001b:  ldc.i4.0
  IL_001c:  stloc.1
  IL_001d:  ldloc.1
  IL_001e:  brfalse.s  IL_0029
  IL_0020:  nop
  IL_0021:  ldloc.0
  IL_0022:  call       ""void System.Console.WriteLine(int)""
  IL_0027:  nop
  IL_0028:  nop
  IL_0029:  ret
}");

            compilation = CreateCompilation(source, options: TestOptions.ReleaseExe);
            compilation.VerifyDiagnostics();
            compVerifier = CompileAndVerify(compilation, expectedOutput: expectedOutput);
            compVerifier.VerifyIL("C.Main",
@"{
  // Code size       30 (0x1e)
  .maxstack  1
  .locals init (int V_0, //index
                int? V_1)
  IL_0000:  call       ""int? C.TryGet()""
  IL_0005:  stloc.1
  IL_0006:  ldloca.s   V_1
  IL_0008:  call       ""bool int?.HasValue.get""
  IL_000d:  brfalse.s  IL_001d
  IL_000f:  ldloca.s   V_1
  IL_0011:  call       ""int int?.GetValueOrDefault()""
  IL_0016:  stloc.0
  IL_0017:  ldloc.0
  IL_0018:  call       ""void System.Console.WriteLine(int)""
  IL_001d:  ret
}");
        }

        [Fact, WorkItem(19122, "https://github.com/dotnet/roslyn/issues/19122")]
        public void PatternCrash_01()
        {
            var source = @"using System;
using System.Collections.Generic;
using System.Linq;

public class Class2 : IDisposable
{
    public Class2(bool parameter = false)
    {
    }

    public void Dispose()
    {
    }
}

class X<T>
{
    IdentityAccessor<T> idAccessor = new IdentityAccessor<T>();
    void Y<U>() where U : T
    {
        // BUG: The following line is the problem
        if (GetT().FirstOrDefault(p => idAccessor.GetId(p) == Guid.Empty) is U u)
        {
        }
    }

    IEnumerable<T> GetT()
    {
        yield return default(T);
    }
}
class IdentityAccessor<T>
{
    public Guid GetId(T t)
    {
        return Guid.Empty;
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.DebugDll);
            compilation.VerifyDiagnostics();
            var compVerifier = CompileAndVerify(compilation);
            compVerifier.VerifyIL("X<T>.Y<U>",
@"{
  // Code size       61 (0x3d)
  .maxstack  3
  .locals init (U V_0, //u
                bool V_1,
                T V_2)
  IL_0000:  nop
  IL_0001:  ldarg.0
  IL_0002:  call       ""System.Collections.Generic.IEnumerable<T> X<T>.GetT()""
  IL_0007:  ldarg.0
  IL_0008:  ldftn      ""bool X<T>.<Y>b__1_0<U>(T)""
  IL_000e:  newobj     ""System.Func<T, bool>..ctor(object, System.IntPtr)""
  IL_0013:  call       ""T System.Linq.Enumerable.FirstOrDefault<T>(System.Collections.Generic.IEnumerable<T>, System.Func<T, bool>)""
  IL_0018:  stloc.2
  IL_0019:  ldloc.2
  IL_001a:  box        ""T""
  IL_001f:  isinst     ""U""
  IL_0024:  brfalse.s  IL_0035
  IL_0026:  ldloc.2
  IL_0027:  box        ""T""
  IL_002c:  unbox.any  ""U""
  IL_0031:  stloc.0
  IL_0032:  ldc.i4.1
  IL_0033:  br.s       IL_0036
  IL_0035:  ldc.i4.0
  IL_0036:  stloc.1
  IL_0037:  ldloc.1
  IL_0038:  brfalse.s  IL_003c
  IL_003a:  nop
  IL_003b:  nop
  IL_003c:  ret
}");
        }

        [Fact, WorkItem(24522, "https://github.com/dotnet/roslyn/issues/24522")]
        public void IgnoreDeclaredConversion_01()
        {
            var source =
@"class Base<T>
{
    public static implicit operator Derived(Base<T> obj)
    {
        return new Derived();
    }
}

class Derived : Base<object>
{
}

class Program
{
    static void Main(string[] args)
    {
        Base<object> x = new Derived();
        System.Console.WriteLine(x is Derived);
        System.Console.WriteLine(x is Derived y);
        switch (x)
        {
            case Derived z: System.Console.WriteLine(true); break;
        }
        System.Console.WriteLine(null != (x as Derived));
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.DebugExe, references: new[] { LinqAssemblyRef });
            compilation.VerifyDiagnostics();
            var expectedOutput =
@"True
True
True
True";
            var compVerifier = CompileAndVerify(compilation, expectedOutput: expectedOutput);
            compVerifier.VerifyIL("Program.Main",
@"{
  // Code size       84 (0x54)
  .maxstack  2
  .locals init (Base<object> V_0, //x
                Derived V_1, //y
                Derived V_2, //z
                Base<object> V_3,
                Base<object> V_4)
  IL_0000:  nop
  IL_0001:  newobj     ""Derived..ctor()""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  isinst     ""Derived""
  IL_000d:  ldnull
  IL_000e:  cgt.un
  IL_0010:  call       ""void System.Console.WriteLine(bool)""
  IL_0015:  nop
  IL_0016:  ldloc.0
  IL_0017:  isinst     ""Derived""
  IL_001c:  stloc.1
  IL_001d:  ldloc.1
  IL_001e:  ldnull
  IL_001f:  cgt.un
  IL_0021:  call       ""void System.Console.WriteLine(bool)""
  IL_0026:  nop
  IL_0027:  ldloc.0
  IL_0028:  stloc.s    V_4
  IL_002a:  ldloc.s    V_4
  IL_002c:  stloc.3
  IL_002d:  ldloc.3
  IL_002e:  isinst     ""Derived""
  IL_0033:  stloc.2
  IL_0034:  ldloc.2
  IL_0035:  brtrue.s   IL_0039
  IL_0037:  br.s       IL_0044
  IL_0039:  br.s       IL_003b
  IL_003b:  ldc.i4.1
  IL_003c:  call       ""void System.Console.WriteLine(bool)""
  IL_0041:  nop
  IL_0042:  br.s       IL_0044
  IL_0044:  ldloc.0
  IL_0045:  isinst     ""Derived""
  IL_004a:  ldnull
  IL_004b:  cgt.un
  IL_004d:  call       ""void System.Console.WriteLine(bool)""
  IL_0052:  nop
  IL_0053:  ret
}");
        }

        [Fact]
        public void DoublePattern01()
        {
            var source =
@"using System;
class Program
{
    static bool P1(double d) => d is double.NaN;
    static bool P2(float f) => f is float.NaN;
    static bool P3(double d) => d is 3.14d;
    static bool P4(float f) => f is 3.14f;
    static bool P5(object o)
    {
        switch (o)
        {
            case double.NaN: return true;
            case float.NaN: return true;
            case 3.14d: return true;
            case 3.14f: return true;
            default: return false;
        }
    }
    public static void Main(string[] args)
    {
        Console.Write(P1(double.NaN));
        Console.Write(P1(1.0));
        Console.Write(P2(float.NaN));
        Console.Write(P2(1.0f));
        Console.Write(P3(3.14));
        Console.Write(P3(double.NaN));
        Console.Write(P4(3.14f));
        Console.Write(P4(float.NaN));
        Console.Write(P5(double.NaN));
        Console.Write(P5(0.0d));
        Console.Write(P5(float.NaN));
        Console.Write(P5(0.0f));
        Console.Write(P5(3.14d));
        Console.Write(P5(125));
        Console.Write(P5(3.14f));
        Console.Write(P5(1.0f));
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.DebugExe);
            compilation.VerifyDiagnostics();
            var expectedOutput = @"TrueFalseTrueFalseTrueFalseTrueFalseTrueFalseTrueFalseTrueFalseTrueFalse";
            var compVerifier = CompileAndVerify(compilation, expectedOutput: expectedOutput);
            compVerifier.VerifyIL("Program.P1",
@"{
  // Code size        7 (0x7)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  call       ""bool double.IsNaN(double)""
  IL_0006:  ret
}");
            compVerifier.VerifyIL("Program.P2",
@"{
  // Code size        7 (0x7)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  call       ""bool float.IsNaN(float)""
  IL_0006:  ret
}");
            compVerifier.VerifyIL("Program.P3",
@"{
  // Code size       13 (0xd)
  .maxstack  2
  IL_0000:  ldc.r8     3.14
  IL_0009:  ldarg.0
  IL_000a:  ceq
  IL_000c:  ret
}");
            compVerifier.VerifyIL("Program.P4",
@"{
  // Code size        9 (0x9)
  .maxstack  2
  IL_0000:  ldc.r4     3.14
  IL_0005:  ldarg.0
  IL_0006:  ceq
  IL_0008:  ret
}");
            compVerifier.VerifyIL("Program.P5",
@"{
  // Code size      103 (0x67)
  .maxstack  2
  .locals init (object V_0,
                double V_1,
                float V_2,
                object V_3,
                bool V_4)
  IL_0000:  nop
  IL_0001:  ldarg.0
  IL_0002:  stloc.3
  IL_0003:  ldloc.3
  IL_0004:  stloc.0
  IL_0005:  ldloc.0
  IL_0006:  isinst     ""double""
  IL_000b:  brfalse.s  IL_002a
  IL_000d:  ldloc.0
  IL_000e:  unbox.any  ""double""
  IL_0013:  stloc.1
  IL_0014:  ldloc.1
  IL_0015:  call       ""bool double.IsNaN(double)""
  IL_001a:  brtrue.s   IL_004b
  IL_001c:  ldc.r8     3.14
  IL_0025:  ldloc.1
  IL_0026:  beq.s      IL_0055
  IL_0028:  br.s       IL_005f
  IL_002a:  ldloc.0
  IL_002b:  isinst     ""float""
  IL_0030:  brfalse.s  IL_005f
  IL_0032:  ldloc.0
  IL_0033:  unbox.any  ""float""
  IL_0038:  stloc.2
  IL_0039:  ldloc.2
  IL_003a:  call       ""bool float.IsNaN(float)""
  IL_003f:  brtrue.s   IL_0050
  IL_0041:  ldc.r4     3.14
  IL_0046:  ldloc.2
  IL_0047:  beq.s      IL_005a
  IL_0049:  br.s       IL_005f
  IL_004b:  ldc.i4.1
  IL_004c:  stloc.s    V_4
  IL_004e:  br.s       IL_0064
  IL_0050:  ldc.i4.1
  IL_0051:  stloc.s    V_4
  IL_0053:  br.s       IL_0064
  IL_0055:  ldc.i4.1
  IL_0056:  stloc.s    V_4
  IL_0058:  br.s       IL_0064
  IL_005a:  ldc.i4.1
  IL_005b:  stloc.s    V_4
  IL_005d:  br.s       IL_0064
  IL_005f:  ldc.i4.0
  IL_0060:  stloc.s    V_4
  IL_0062:  br.s       IL_0064
  IL_0064:  ldloc.s    V_4
  IL_0066:  ret
}");
        }

        [Fact]
        public void DecimalEquality()
        {
            // demonstrate that pattern-matching against a decimal constant is
            // at least as efficient as simply using ==
            var source =
@"using System;
public class C
{
    public static void Main()
    {
        Console.Write(M1(1.0m));
        Console.Write(M2(1.0m));
        Console.Write(M1(2.0m));
        Console.Write(M2(2.0m));
    }

    static int M1(decimal d)
    {
        if (M(d) is 1.0m) return 1;
        return 0;
    }

    static int M2(decimal d)
    {
        if (M(d) == 1.0m) return 1;
        return 0;
    }

    public static decimal M(decimal d)
    {
        return d;
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.DebugExe);
            compilation.VerifyDiagnostics();
            var expectedOutput = @"1100";
            var compVerifier = CompileAndVerify(compilation, expectedOutput: expectedOutput);
            compVerifier.VerifyIL("C.M1",
@"{
  // Code size       39 (0x27)
  .maxstack  5
  .locals init (bool V_0,
                decimal V_1,
                int V_2)
  IL_0000:  nop
  IL_0001:  ldarg.0
  IL_0002:  call       ""decimal C.M(decimal)""
  IL_0007:  stloc.1
  IL_0008:  ldc.i4.s   10
  IL_000a:  ldc.i4.0
  IL_000b:  ldc.i4.0
  IL_000c:  ldc.i4.0
  IL_000d:  ldc.i4.1
  IL_000e:  newobj     ""decimal..ctor(int, int, int, bool, byte)""
  IL_0013:  ldloc.1
  IL_0014:  call       ""bool decimal.op_Equality(decimal, decimal)""
  IL_0019:  stloc.0
  IL_001a:  ldloc.0
  IL_001b:  brfalse.s  IL_0021
  IL_001d:  ldc.i4.1
  IL_001e:  stloc.2
  IL_001f:  br.s       IL_0025
  IL_0021:  ldc.i4.0
  IL_0022:  stloc.2
  IL_0023:  br.s       IL_0025
  IL_0025:  ldloc.2
  IL_0026:  ret
}");
            compVerifier.VerifyIL("C.M2",
@"{
  // Code size       37 (0x25)
  .maxstack  6
  .locals init (bool V_0,
                int V_1)
  IL_0000:  nop
  IL_0001:  ldarg.0
  IL_0002:  call       ""decimal C.M(decimal)""
  IL_0007:  ldc.i4.s   10
  IL_0009:  ldc.i4.0
  IL_000a:  ldc.i4.0
  IL_000b:  ldc.i4.0
  IL_000c:  ldc.i4.1
  IL_000d:  newobj     ""decimal..ctor(int, int, int, bool, byte)""
  IL_0012:  call       ""bool decimal.op_Equality(decimal, decimal)""
  IL_0017:  stloc.0
  IL_0018:  ldloc.0
  IL_0019:  brfalse.s  IL_001f
  IL_001b:  ldc.i4.1
  IL_001c:  stloc.1
  IL_001d:  br.s       IL_0023
  IL_001f:  ldc.i4.0
  IL_0020:  stloc.1
  IL_0021:  br.s       IL_0023
  IL_0023:  ldloc.1
  IL_0024:  ret
}");

            compilation = CreateCompilation(source, options: TestOptions.ReleaseExe);
            compilation.VerifyDiagnostics();
            compVerifier = CompileAndVerify(compilation, expectedOutput: expectedOutput);
            compVerifier.VerifyIL("C.M1",
@"{
  // Code size       30 (0x1e)
  .maxstack  5
  .locals init (decimal V_0)
  IL_0000:  ldarg.0
  IL_0001:  call       ""decimal C.M(decimal)""
  IL_0006:  stloc.0
  IL_0007:  ldc.i4.s   10
  IL_0009:  ldc.i4.0
  IL_000a:  ldc.i4.0
  IL_000b:  ldc.i4.0
  IL_000c:  ldc.i4.1
  IL_000d:  newobj     ""decimal..ctor(int, int, int, bool, byte)""
  IL_0012:  ldloc.0
  IL_0013:  call       ""bool decimal.op_Equality(decimal, decimal)""
  IL_0018:  brfalse.s  IL_001c
  IL_001a:  ldc.i4.1
  IL_001b:  ret
  IL_001c:  ldc.i4.0
  IL_001d:  ret
}");
            compVerifier.VerifyIL("C.M2",
@"{
  // Code size       28 (0x1c)
  .maxstack  6
  IL_0000:  ldarg.0
  IL_0001:  call       ""decimal C.M(decimal)""
  IL_0006:  ldc.i4.s   10
  IL_0008:  ldc.i4.0
  IL_0009:  ldc.i4.0
  IL_000a:  ldc.i4.0
  IL_000b:  ldc.i4.1
  IL_000c:  newobj     ""decimal..ctor(int, int, int, bool, byte)""
  IL_0011:  call       ""bool decimal.op_Equality(decimal, decimal)""
  IL_0016:  brfalse.s  IL_001a
  IL_0018:  ldc.i4.1
  IL_0019:  ret
  IL_001a:  ldc.i4.0
  IL_001b:  ret
}");
        }

        [Fact, WorkItem(16878, "https://github.com/dotnet/roslyn/issues/16878")]
        public void RedundantNullCheck()
        {
            var source =
@"public class C
{
    static int M1(bool? b1, bool b2)
    {
        switch (b1)
        {
            case null:
                return 1;
            case var _ when b2:
                return 2;
            case true:
                return 3;
            case false:
                return 4;
        }
    }

    static int M2(object o, bool b)
    {
        switch (o)
        {
            case string a when b:
                return 1;
            case string a:
                return 2;
        }
        return 3;
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.ReleaseDll);
            compilation.VerifyDiagnostics();
            var compVerifier = CompileAndVerify(compilation);
            compVerifier.VerifyIL("C.M1",
@"{
  // Code size       33 (0x21)
  .maxstack  1
  IL_0000:  ldarga.s   V_0
  IL_0002:  call       ""bool bool?.HasValue.get""
  IL_0007:  brfalse.s  IL_0016
  IL_0009:  br.s       IL_0018
  IL_000b:  ldarga.s   V_0
  IL_000d:  call       ""bool bool?.GetValueOrDefault()""
  IL_0012:  brtrue.s   IL_001d
  IL_0014:  br.s       IL_001f
  IL_0016:  ldc.i4.1
  IL_0017:  ret
  IL_0018:  ldarg.1
  IL_0019:  brfalse.s  IL_000b
  IL_001b:  ldc.i4.2
  IL_001c:  ret
  IL_001d:  ldc.i4.3
  IL_001e:  ret
  IL_001f:  ldc.i4.4
  IL_0020:  ret
}");
            compVerifier.VerifyIL("C.M2",
@"{
  // Code size       19 (0x13)
  .maxstack  1
  .locals init (string V_0) //a
  IL_0000:  ldarg.0
  IL_0001:  isinst     ""string""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  brfalse.s  IL_0011
  IL_000a:  ldarg.1
  IL_000b:  brfalse.s  IL_000f
  IL_000d:  ldc.i4.1
  IL_000e:  ret
  IL_000f:  ldc.i4.2
  IL_0010:  ret
  IL_0011:  ldc.i4.3
  IL_0012:  ret
}");
        }

        [Fact, WorkItem(12813, "https://github.com/dotnet/roslyn/issues/12813")]
        public void NoBoxingOnIntegerConstantPattern()
        {
            var source =
@"public class C
{
    static bool M1(int x)
    {
        return x is 42;
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.ReleaseDll);
            compilation.VerifyDiagnostics();
            var compVerifier = CompileAndVerify(compilation);
            compVerifier.VerifyIL("C.M1",
@"{
  // Code size        6 (0x6)
  .maxstack  2
  IL_0000:  ldc.i4.s   42
  IL_0002:  ldarg.0
  IL_0003:  ceq
  IL_0005:  ret
}");
        }

        [Fact, WorkItem(22654, "https://github.com/dotnet/roslyn/issues/22654")]
        public void NoRedundantTypeCheck()
        {
            var source =
@"using System;
public class C
{
    public void SwitchBasedPatternMatching(object o)
    {
        switch (o)
        {
            case int n when n == 1:
                Console.WriteLine(""1""); break;
            case string s:
                Console.WriteLine(""s""); break;
            case int n when n == 2:
                Console.WriteLine(""2""); break;
        }
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.ReleaseDll);
            compilation.VerifyDiagnostics();
            var compVerifier = CompileAndVerify(compilation);
            compVerifier.VerifyIL("C.SwitchBasedPatternMatching",
@"{
  // Code size       67 (0x43)
  .maxstack  2
  .locals init (int V_0) //n
  IL_0000:  ldarg.1
  IL_0001:  isinst     ""int""
  IL_0006:  brfalse.s  IL_0011
  IL_0008:  ldarg.1
  IL_0009:  unbox.any  ""int""
  IL_000e:  stloc.0
  IL_000f:  br.s       IL_001a
  IL_0011:  ldarg.1
  IL_0012:  isinst     ""string""
  IL_0017:  brtrue.s   IL_0029
  IL_0019:  ret
  IL_001a:  ldloc.0
  IL_001b:  ldc.i4.1
  IL_001c:  bne.un.s   IL_0034
  IL_001e:  ldstr      ""1""
  IL_0023:  call       ""void System.Console.WriteLine(string)""
  IL_0028:  ret
  IL_0029:  ldstr      ""s""
  IL_002e:  call       ""void System.Console.WriteLine(string)""
  IL_0033:  ret
  IL_0034:  ldloc.0
  IL_0035:  ldc.i4.2
  IL_0036:  bne.un.s   IL_0042
  IL_0038:  ldstr      ""2""
  IL_003d:  call       ""void System.Console.WriteLine(string)""
  IL_0042:  ret
}");
        }

        [Fact, WorkItem(15437, "https://github.com/dotnet/roslyn/issues/15437")]
        public void IsTypeDiscard()
        {
            var source =
@"public class C
{
    public bool IsString(object o)
    {
        return o is string _;
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.ReleaseDll);
            compilation.VerifyDiagnostics();
            var compVerifier = CompileAndVerify(compilation);
            compVerifier.VerifyIL("C.IsString",
@"{
  // Code size       10 (0xa)
  .maxstack  2
  IL_0000:  ldarg.1
  IL_0001:  isinst     ""string""
  IL_0006:  ldnull
  IL_0007:  cgt.un
  IL_0009:  ret
}");
        }

        [Fact, WorkItem(19150, "https://github.com/dotnet/roslyn/issues/19150")]
        public void RedundantHasValue()
        {
            var source =
@"using System;
public class C
{
    public static void M(int? x)
    {
        switch (x)
        {
            case int i:
                Console.Write(i);
                break;
            case null:
                Console.Write(""null"");
                break;
        }
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.ReleaseDll);
            compilation.VerifyDiagnostics();
            var compVerifier = CompileAndVerify(compilation);
            compVerifier.VerifyIL("C.M",
@"{
  // Code size       33 (0x21)
  .maxstack  1
  IL_0000:  ldarga.s   V_0
  IL_0002:  call       ""bool int?.HasValue.get""
  IL_0007:  brfalse.s  IL_0016
  IL_0009:  ldarga.s   V_0
  IL_000b:  call       ""int int?.GetValueOrDefault()""
  IL_0010:  call       ""void System.Console.Write(int)""
  IL_0015:  ret
  IL_0016:  ldstr      ""null""
  IL_001b:  call       ""void System.Console.Write(string)""
  IL_0020:  ret
}");
        }

        [Fact, WorkItem(19153, "https://github.com/dotnet/roslyn/issues/19153")]
        public void RedundantBox()
        {
            var source = @"using System;
public class C
{
    public static void M<T, U>(U x) where T : U
    {
        // when T is not known to be a reference type, there is an unboxing conversion from
        // a type parameter U to T, provided T depends on U.
        switch (x)
        {
            case T i:
                Console.Write(i);
                break;
        }
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.ReleaseDll);
            compilation.VerifyDiagnostics();
            var compVerifier = CompileAndVerify(compilation);
            compVerifier.VerifyIL("C.M<T, U>(U)",
@"{
  // Code size       35 (0x23)
  .maxstack  1
  IL_0000:  ldarg.0
  IL_0001:  box        ""U""
  IL_0006:  isinst     ""T""
  IL_000b:  brfalse.s  IL_0022
  IL_000d:  ldarg.0
  IL_000e:  box        ""U""
  IL_0013:  unbox.any  ""T""
  IL_0018:  box        ""T""
  IL_001d:  call       ""void System.Console.Write(object)""
  IL_0022:  ret
}");
        }

        [Fact, WorkItem(20641, "https://github.com/dotnet/roslyn/issues/20641")]
        public void PatternsVsAs01()
        {
            var source = @"using System.Collections;
using System.Collections.Generic;

class Program
{
    static void Main() { }

    internal static bool TryGetCount1<T>(IEnumerable<T> source, out int count)
    {
        ICollection nonGeneric = source as ICollection;
        if (nonGeneric != null)
        {
            count = nonGeneric.Count;
            return true;
        }

        ICollection<T> generic = source as ICollection<T>;
        if (generic != null)
        {
            count = generic.Count;
            return true;
        }

        count = -1;
        return false;
    }

    internal static bool TryGetCount2<T>(IEnumerable<T> source, out int count)
    {
        switch (source)
        {
            case ICollection nonGeneric:
                count = nonGeneric.Count;
                return true;

            case ICollection<T> generic:
                count = generic.Count;
                return true;

            default:
                count = -1;
                return false;
        }
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.ReleaseDll);
            compilation.VerifyDiagnostics();
            var compVerifier = CompileAndVerify(compilation);
            compVerifier.VerifyIL("Program.TryGetCount1<T>",
@"{
  // Code size       45 (0x2d)
  .maxstack  2
  .locals init (System.Collections.ICollection V_0, //nonGeneric
                System.Collections.Generic.ICollection<T> V_1) //generic
  IL_0000:  ldarg.0
  IL_0001:  isinst     ""System.Collections.ICollection""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  brfalse.s  IL_0014
  IL_000a:  ldarg.1
  IL_000b:  ldloc.0
  IL_000c:  callvirt   ""int System.Collections.ICollection.Count.get""
  IL_0011:  stind.i4
  IL_0012:  ldc.i4.1
  IL_0013:  ret
  IL_0014:  ldarg.0
  IL_0015:  isinst     ""System.Collections.Generic.ICollection<T>""
  IL_001a:  stloc.1
  IL_001b:  ldloc.1
  IL_001c:  brfalse.s  IL_0028
  IL_001e:  ldarg.1
  IL_001f:  ldloc.1
  IL_0020:  callvirt   ""int System.Collections.Generic.ICollection<T>.Count.get""
  IL_0025:  stind.i4
  IL_0026:  ldc.i4.1
  IL_0027:  ret
  IL_0028:  ldarg.1
  IL_0029:  ldc.i4.m1
  IL_002a:  stind.i4
  IL_002b:  ldc.i4.0
  IL_002c:  ret
}");
            compVerifier.VerifyIL("Program.TryGetCount2<T>",
@"{
  // Code size       47 (0x2f)
  .maxstack  2
  .locals init (System.Collections.ICollection V_0, //nonGeneric
                System.Collections.Generic.ICollection<T> V_1) //generic
  IL_0000:  ldarg.0
  IL_0001:  isinst     ""System.Collections.ICollection""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  brtrue.s   IL_0016
  IL_000a:  ldarg.0
  IL_000b:  isinst     ""System.Collections.Generic.ICollection<T>""
  IL_0010:  stloc.1
  IL_0011:  ldloc.1
  IL_0012:  brtrue.s   IL_0020
  IL_0014:  br.s       IL_002a
  IL_0016:  ldarg.1
  IL_0017:  ldloc.0
  IL_0018:  callvirt   ""int System.Collections.ICollection.Count.get""
  IL_001d:  stind.i4
  IL_001e:  ldc.i4.1
  IL_001f:  ret
  IL_0020:  ldarg.1
  IL_0021:  ldloc.1
  IL_0022:  callvirt   ""int System.Collections.Generic.ICollection<T>.Count.get""
  IL_0027:  stind.i4
  IL_0028:  ldc.i4.1
  IL_0029:  ret
  IL_002a:  ldarg.1
  IL_002b:  ldc.i4.m1
  IL_002c:  stind.i4
  IL_002d:  ldc.i4.0
  IL_002e:  ret
}");
        }

        [Fact, WorkItem(20641, "https://github.com/dotnet/roslyn/issues/20641")]
        public void PatternsVsAs02()
        {
            var source = @"using System.Collections;
class Program
{
    static void Main() { }

    internal static bool IsEmpty1(IEnumerable source)
    {
        var c = source as ICollection;
        return c != null && c.Count > 0;
    }

    internal static bool IsEmpty2(IEnumerable source)
    {
        return source is ICollection c && c.Count > 0;
    }

}";
            var compilation = CreateCompilation(source, options: TestOptions.ReleaseDll);
            compilation.VerifyDiagnostics();
            var compVerifier = CompileAndVerify(compilation);
            compVerifier.VerifyIL("Program.IsEmpty1",
@"{
  // Code size       22 (0x16)
  .maxstack  2
  .locals init (System.Collections.ICollection V_0) //c
  IL_0000:  ldarg.0
  IL_0001:  isinst     ""System.Collections.ICollection""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  brfalse.s  IL_0014
  IL_000a:  ldloc.0
  IL_000b:  callvirt   ""int System.Collections.ICollection.Count.get""
  IL_0010:  ldc.i4.0
  IL_0011:  cgt
  IL_0013:  ret
  IL_0014:  ldc.i4.0
  IL_0015:  ret
}");
            compVerifier.VerifyIL("Program.IsEmpty2",
@"{
  // Code size       22 (0x16)
  .maxstack  2
  .locals init (System.Collections.ICollection V_0) //c
  IL_0000:  ldarg.0
  IL_0001:  isinst     ""System.Collections.ICollection""
  IL_0006:  stloc.0
  IL_0007:  ldloc.0
  IL_0008:  brfalse.s  IL_0014
  IL_000a:  ldloc.0
  IL_000b:  callvirt   ""int System.Collections.ICollection.Count.get""
  IL_0010:  ldc.i4.0
  IL_0011:  cgt
  IL_0013:  ret
  IL_0014:  ldc.i4.0
  IL_0015:  ret
}");
        }

        [Fact]
        [WorkItem(20641, "https://github.com/dotnet/roslyn/issues/20641")]
        [WorkItem(1395, "https://github.com/dotnet/csharplang/issues/1395")]
        public void TupleSwitch01()
        {
            var source = @"using System;

public class Door
{
    public DoorState State;

    public enum DoorState { Opened, Closed, Locked }

    public enum Action { Open, Close, Lock, Unlock }

    public void Act0(Action action, bool haveKey = false)
    {
        Console.Write($""{State} {action}{(haveKey ? "" withKey"" : null)}"");
        State = ChangeState0(State, action, haveKey);
        Console.WriteLine($"" -> {State}"");
    }

    public void Act1(Action action, bool haveKey = false)
    {
        Console.Write($""{State} {action}{(haveKey ? "" withKey"" : null)}"");
        State = ChangeState1(State, action, haveKey);
        Console.WriteLine($"" -> {State}"");
    }

    public static DoorState ChangeState0(DoorState state, Action action, bool haveKey = false)
    {
        switch (state, action)
        {
            case (DoorState.Opened, Action.Close):
                return DoorState.Closed;
            case (DoorState.Closed, Action.Open):
                return DoorState.Opened;
            case (DoorState.Closed, Action.Lock) when haveKey:
                return DoorState.Locked;
            case (DoorState.Locked, Action.Unlock) when haveKey:
                return DoorState.Closed;
            case var (oldState, _):
                return oldState;
        }
    }

    public static DoorState ChangeState1(DoorState state, Action action, bool haveKey = false) =>
        (state, action) switch {
            (DoorState.Opened, Action.Close) => DoorState.Closed,
            (DoorState.Closed, Action.Open) => DoorState.Opened,
            (DoorState.Closed, Action.Lock) when haveKey => DoorState.Locked,
            (DoorState.Locked, Action.Unlock) when haveKey => DoorState.Closed,
            _ => state };
}

class Program
{
    static void Main(string[] args)
    {
        var door = new Door();
        door.Act0(Door.Action.Close);
        door.Act0(Door.Action.Lock);
        door.Act0(Door.Action.Lock, true);
        door.Act0(Door.Action.Open);
        door.Act0(Door.Action.Unlock);
        door.Act0(Door.Action.Unlock, true);
        door.Act0(Door.Action.Open);
        Console.WriteLine();

        door = new Door();
        door.Act1(Door.Action.Close);
        door.Act1(Door.Action.Lock);
        door.Act1(Door.Action.Lock, true);
        door.Act1(Door.Action.Open);
        door.Act1(Door.Action.Unlock);
        door.Act1(Door.Action.Unlock, true);
        door.Act1(Door.Action.Open);
    }
}";
            var expectedOutput =
@"Opened Close -> Closed
Closed Lock -> Closed
Closed Lock withKey -> Locked
Locked Open -> Locked
Locked Unlock -> Locked
Locked Unlock withKey -> Closed
Closed Open -> Opened

Opened Close -> Closed
Closed Lock -> Closed
Closed Lock withKey -> Locked
Locked Open -> Locked
Locked Unlock -> Locked
Locked Unlock withKey -> Closed
Closed Open -> Opened
";
            var compilation = CreateCompilation(source, options: TestOptions.ReleaseExe, parseOptions: TestOptions.RegularWithRecursivePatterns);
            compilation.VerifyDiagnostics();
            var compVerifier = CompileAndVerify(compilation, expectedOutput: expectedOutput);
            compVerifier.VerifyIL("Door.ChangeState0",
@"{
  // Code size       59 (0x3b)
  .maxstack  2
  .locals init (Door.DoorState V_0) //oldState
  IL_0000:  ldarg.0
  IL_0001:  stloc.0
  IL_0002:  ldloc.0
  IL_0003:  switch    (
        IL_0016,
        IL_001c,
        IL_0025)
  IL_0014:  br.s       IL_0039
  IL_0016:  ldc.i4.1
  IL_0017:  ldarg.1
  IL_0018:  beq.s      IL_002b
  IL_001a:  br.s       IL_0039
  IL_001c:  ldarg.1
  IL_001d:  brfalse.s  IL_002d
  IL_001f:  ldarg.1
  IL_0020:  ldc.i4.2
  IL_0021:  beq.s      IL_002f
  IL_0023:  br.s       IL_0039
  IL_0025:  ldc.i4.3
  IL_0026:  ldarg.1
  IL_0027:  beq.s      IL_0034
  IL_0029:  br.s       IL_0039
  IL_002b:  ldc.i4.1
  IL_002c:  ret
  IL_002d:  ldc.i4.0
  IL_002e:  ret
  IL_002f:  ldarg.2
  IL_0030:  brfalse.s  IL_0039
  IL_0032:  ldc.i4.2
  IL_0033:  ret
  IL_0034:  ldarg.2
  IL_0035:  brfalse.s  IL_0039
  IL_0037:  ldc.i4.1
  IL_0038:  ret
  IL_0039:  ldloc.0
  IL_003a:  ret
}");
            compVerifier.VerifyIL("Door.ChangeState1",
@"{
  // Code size       67 (0x43)
  .maxstack  2
  .locals init (Door.DoorState V_0)
  IL_0000:  ldarg.0
  IL_0001:  switch    (
        IL_0014,
        IL_001a,
        IL_0023)
  IL_0012:  br.s       IL_003f
  IL_0014:  ldc.i4.1
  IL_0015:  ldarg.1
  IL_0016:  beq.s      IL_0029
  IL_0018:  br.s       IL_003f
  IL_001a:  ldarg.1
  IL_001b:  brfalse.s  IL_002d
  IL_001d:  ldarg.1
  IL_001e:  ldc.i4.2
  IL_001f:  beq.s      IL_0031
  IL_0021:  br.s       IL_003f
  IL_0023:  ldc.i4.3
  IL_0024:  ldarg.1
  IL_0025:  beq.s      IL_0038
  IL_0027:  br.s       IL_003f
  IL_0029:  ldc.i4.1
  IL_002a:  stloc.0
  IL_002b:  br.s       IL_0041
  IL_002d:  ldc.i4.0
  IL_002e:  stloc.0
  IL_002f:  br.s       IL_0041
  IL_0031:  ldarg.2
  IL_0032:  brfalse.s  IL_003f
  IL_0034:  ldc.i4.2
  IL_0035:  stloc.0
  IL_0036:  br.s       IL_0041
  IL_0038:  ldarg.2
  IL_0039:  brfalse.s  IL_003f
  IL_003b:  ldc.i4.1
  IL_003c:  stloc.0
  IL_003d:  br.s       IL_0041
  IL_003f:  ldarg.0
  IL_0040:  stloc.0
  IL_0041:  ldloc.0
  IL_0042:  ret
}");
        }

        [Fact]
        [WorkItem(20641, "https://github.com/dotnet/roslyn/issues/20641")]
        [WorkItem(1395, "https://github.com/dotnet/csharplang/issues/1395")]
        public void SharingTemps01()
        {
            var source =
@"class Program
{
    static void Main(string[] args) { }
    void M1(string x)
    {
        switch (x)
        {
            case ""a"":
            case ""b"" when Mutate(ref x): // prevents sharing temps
            case ""c"":
                break;
        }
    }
    void M2(string x)
    {
        switch (x)
        {
            case ""a"":
            case ""b"" when Pure(x):
            case ""c"":
                break;
        }
    }
    static bool Mutate(ref string x) { x = null; return false; }
    static bool Pure(string x) { return false; }
}";
            var compilation = CreateCompilation(source, options: TestOptions.ReleaseExe, parseOptions: TestOptions.RegularWithoutRecursivePatterns);
            compilation.VerifyDiagnostics();
            var compVerifier = CompileAndVerify(compilation);
            compVerifier.VerifyIL("Program.M1",
@"{
  // Code size       53 (0x35)
  .maxstack  2
  .locals init (string V_0)
  IL_0000:  ldarg.1
  IL_0001:  stloc.0
  IL_0002:  ldloc.0
  IL_0003:  brfalse.s  IL_0034
  IL_0005:  ldloc.0
  IL_0006:  ldstr      ""a""
  IL_000b:  call       ""bool string.op_Equality(string, string)""
  IL_0010:  brtrue.s   IL_0034
  IL_0012:  ldloc.0
  IL_0013:  ldstr      ""b""
  IL_0018:  call       ""bool string.op_Equality(string, string)""
  IL_001d:  brtrue.s   IL_002c
  IL_001f:  ldloc.0
  IL_0020:  ldstr      ""c""
  IL_0025:  call       ""bool string.op_Equality(string, string)""
  IL_002a:  pop
  IL_002b:  ret
  IL_002c:  ldarga.s   V_1
  IL_002e:  call       ""bool Program.Mutate(ref string)""
  IL_0033:  pop
  IL_0034:  ret
}");
            compVerifier.VerifyIL("Program.M2",
@"{
  // Code size       50 (0x32)
  .maxstack  2
  IL_0000:  ldarg.1
  IL_0001:  brfalse.s  IL_0031
  IL_0003:  ldarg.1
  IL_0004:  ldstr      ""a""
  IL_0009:  call       ""bool string.op_Equality(string, string)""
  IL_000e:  brtrue.s   IL_0031
  IL_0010:  ldarg.1
  IL_0011:  ldstr      ""b""
  IL_0016:  call       ""bool string.op_Equality(string, string)""
  IL_001b:  brtrue.s   IL_002a
  IL_001d:  ldarg.1
  IL_001e:  ldstr      ""c""
  IL_0023:  call       ""bool string.op_Equality(string, string)""
  IL_0028:  pop
  IL_0029:  ret
  IL_002a:  ldarg.1
  IL_002b:  call       ""bool Program.Pure(string)""
  IL_0030:  pop
  IL_0031:  ret
}");
        }

        [Fact, WorkItem(17266, "https://github.com/dotnet/roslyn/issues/17266")]
        public void IrrefutablePatternInIs01()
        {
            var source =
@"using System;
public class C
{
    public static void Main()
    {
        if (Get() is int index) { }
        Console.WriteLine(index);
    }

    public static int Get()
    {
        Console.WriteLine(""eval"");
        return 1;
    }
}";
            var compilation = CreateCompilation(source, options: TestOptions.DebugExe);
            compilation.VerifyDiagnostics();
            var expectedOutput = @"eval
1";
            var compVerifier = CompileAndVerify(compilation, expectedOutput: expectedOutput);
        }

        [Fact, WorkItem(17266, "https://github.com/dotnet/roslyn/issues/17266")]
        public void IrrefutablePatternInIs02()
        {
            var source =
@"using System;
public class C
{
    public static void Main()
    {
        if (Get() is Assignment(int left, var right)) { }
        Console.WriteLine(left);
        Console.WriteLine(right);
    }

    public static Assignment Get()
    {
        Console.WriteLine(""eval"");
        return new Assignment(1, 2);
    }
}
public struct Assignment
{
    public int Left, Right;
    public Assignment(int left, int right) => (Left, Right) = (left, right);
    public void Deconstruct(out int left, out int right) => (left, right) = (Left, Right);
}
";
            var compilation = CreateCompilation(source, options: TestOptions.DebugExe, parseOptions: TestOptions.RegularWithRecursivePatterns);
            compilation.VerifyDiagnostics();
            var expectedOutput = @"eval
1
2";
            var compVerifier = CompileAndVerify(compilation, expectedOutput: expectedOutput);
        }

        [Fact]
        public void MissingNullCheck01()
        {
            var source =
@"class Program
{
    public static void Main()
    {
        string s = null;
        System.Console.WriteLine(s is string { Length: 3 });
    }
}
";
            var compilation = CreateCompilation(source, options: TestOptions.DebugExe, parseOptions: TestOptions.RegularWithRecursivePatterns);
            compilation.VerifyDiagnostics();
            var expectedOutput = @"False";
            var compVerifier = CompileAndVerify(compilation, expectedOutput: expectedOutput);
        }
    }
}
