﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using static System.Linq.Expressions.Expression;
using L = FastExpressionCompiler.LightExpression.Expression;

namespace FastExpressionCompiler.Benchmarks
{
    public class AutoMapper_UseCase_Simplified_OneProperty
    {
        /*
        ## Initial results with not yet released v2.1
        BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.765 (1803/April2018Update/Redstone4)
        Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
        Frequency=2156250 Hz, Resolution=463.7681 ns, Timer=TSC
        .NET Core SDK=3.0.100-preview3-010431
          [Host]     : .NET Core 2.1.11 (CoreCLR 4.6.27617.04, CoreFX 4.6.27617.02), 64bit RyuJIT
          DefaultJob : .NET Core 2.1.11 (CoreCLR 4.6.27617.04, CoreFX 4.6.27617.02), 64bit RyuJIT


                                     Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                                    Compile | 291.735 us | 3.6419 us | 3.4067 us | 22.30 |    0.31 |      1.9531 |      0.9766 |           - |            10.93 KB |
                                CompileFast |  13.104 us | 0.0625 us | 0.0522 us |  1.00 |    0.00 |      0.6866 |      0.3357 |      0.0305 |              3.2 KB |
                 CompileFast_WithoutClosure |  10.585 us | 0.0809 us | 0.0757 us |  0.81 |    0.01 |      0.6714 |      0.3357 |      0.0305 |             3.09 KB |
                CompileFast_LightExpression |   9.829 us | 0.0751 us | 0.0666 us |  0.75 |    0.01 |      0.6866 |      0.3357 |      0.0305 |              3.2 KB |
 CompileFast_LightExpression_WithoutClosure |   9.028 us | 0.0632 us | 0.0560 us |  0.69 |    0.00 |      0.6714 |      0.3357 |      0.0305 |             3.09 KB |

        ## Degradation after adding block / try-catch collection + added WithoutClosure for comparison

                                     Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                                    Compile | 254.680 us | 1.0914 us | 1.0209 us | 13.38 |    0.13 |      1.9531 |      0.9766 |           - |            10.93 KB |
                                CompileFast |  19.031 us | 0.1987 us | 0.1858 us |  1.00 |    0.00 |      0.9766 |      0.4883 |      0.0305 |             4.61 KB |
                 CompileFast_WithoutClosure |   5.373 us | 0.0222 us | 0.0207 us |  0.28 |    0.00 |      0.6256 |           - |           - |             2.91 KB |
                CompileFast_LightExpression |  15.243 us | 0.1002 us | 0.0937 us |  0.80 |    0.01 |      0.9918 |      0.4883 |      0.0458 |             4.61 KB |
 CompileFast_LightExpression_WithoutClosure |   3.882 us | 0.0735 us | 0.0787 us |  0.20 |    0.01 |      0.6294 |           - |           - |             2.91 KB |

        ## Fixed the tests and get back some performance

                                     Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                                    Compile | 289.864 us | 2.7638 us | 2.5853 us | 23.65 |    0.25 |      1.9531 |      0.9766 |           - |            10.93 KB |
                                CompileFast |  12.262 us | 0.0789 us | 0.0699 us |  1.00 |    0.00 |      0.7019 |      0.3510 |      0.0305 |             3.26 KB |
                 CompileFast_WithoutClosure |  10.794 us | 0.0673 us | 0.0562 us |  0.88 |    0.01 |      0.6714 |      0.3357 |      0.0305 |             3.09 KB |
                CompileFast_LightExpression |   9.743 us | 0.0676 us | 0.0632 us |  0.79 |    0.01 |      0.7019 |      0.3510 |      0.0305 |             3.26 KB |
 CompileFast_LightExpression_WithoutClosure |   8.571 us | 0.0464 us | 0.0434 us |  0.70 |    0.01 |      0.6714 |      0.3357 |      0.0305 |             3.09 KB |

        ## Removing the recursion where possible in TryCollectBoundConstants, in-lining in some places

                                     Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                                    Compile | 285.340 us | 1.8488 us | 1.7294 us | 24.17 |    0.20 |      1.9531 |      0.9766 |           - |            10.93 KB |
                                CompileFast |  11.804 us | 0.0780 us | 0.0730 us |  1.00 |    0.00 |      0.7019 |      0.3510 |      0.0305 |             3.23 KB |
                 CompileFast_WithoutClosure |  10.526 us | 0.0959 us | 0.0801 us |  0.89 |    0.01 |      0.6714 |      0.3357 |      0.0305 |             3.09 KB |
                CompileFast_LightExpression |   9.652 us | 0.0787 us | 0.0736 us |  0.82 |    0.01 |      0.7019 |      0.3510 |      0.0305 |             3.23 KB |
 CompileFast_LightExpression_WithoutClosure |   8.686 us | 0.0743 us | 0.0620 us |  0.74 |    0.01 |      0.6714 |      0.3357 |      0.0305 |             3.09 KB |
 
## More in-lining and simplifications - probably the main impact is in-lining in the TryEmit switch; plus remove some string allocation when looking for property setter

                                     Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                                    Compile | 290.499 us | 2.5309 us | 2.3674 us | 25.50 |    0.20 |      1.9531 |      0.9766 |           - |            10.93 KB |
                                CompileFast |  11.391 us | 0.0683 us | 0.0639 us |  1.00 |    0.00 |      0.6714 |      0.3357 |      0.0305 |             3.13 KB |
                 CompileFast_WithoutClosure |  10.037 us | 0.0606 us | 0.0506 us |  0.88 |    0.01 |      0.6409 |      0.3204 |      0.0305 |             2.98 KB |
                CompileFast_LightExpression |   9.498 us | 0.0640 us | 0.0598 us |  0.83 |    0.01 |      0.6714 |      0.3357 |      0.0305 |             3.13 KB |
 CompileFast_LightExpression_WithoutClosure |   8.682 us | 0.0880 us | 0.0735 us |  0.76 |    0.01 |      0.6409 |      0.3204 |      0.0305 |             2.98 KB |

## Changing baseline to CompileFast_LightExpression

                                     Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                                    Compile | 291.325 us | 2.1553 us | 2.0161 us | 30.65 |    0.27 |      1.9531 |      0.9766 |           - |            10.93 KB |
                                CompileFast |  11.548 us | 0.0682 us | 0.0638 us |  1.21 |    0.01 |      0.6714 |      0.3357 |      0.0305 |             3.13 KB |
                CompileFast_LightExpression |   9.510 us | 0.0778 us | 0.0649 us |  1.00 |    0.00 |      0.6714 |      0.3357 |      0.0305 |             3.13 KB |
                 CompileFast_WithoutClosure |  10.453 us | 0.0533 us | 0.0498 us |  1.10 |    0.01 |      0.6409 |      0.3204 |      0.0305 |             2.98 KB |
 CompileFast_LightExpression_WithoutClosure |   8.564 us | 0.0408 us | 0.0341 us |  0.90 |    0.01 |      0.6409 |      0.3204 |      0.0305 |             2.98 KB |

## Changing BlockInfo to struct

                                     Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                                    Compile | 254.985 us | 3.9906 us | 3.7328 us | 28.47 |    0.56 |      1.9531 |      0.9766 |           - |            10.93 KB |
                                CompileFast |  11.032 us | 0.0558 us | 0.0522 us |  1.23 |    0.01 |      0.6256 |      0.3052 |      0.0305 |             2.91 KB |
                CompileFast_LightExpression |   8.961 us | 0.0921 us | 0.0769 us |  1.00 |    0.00 |      0.6256 |      0.3052 |      0.0305 |             2.91 KB |
                 CompileFast_WithoutClosure |   9.530 us | 0.0917 us | 0.0858 us |  1.06 |    0.01 |      0.6256 |      0.3052 |      0.0305 |             2.88 KB |
 CompileFast_LightExpression_WithoutClosure |   8.024 us | 0.0447 us | 0.0349 us |  0.90 |    0.01 |      0.6256 |      0.3052 |      0.0305 |             2.88 KB |

## Updated BDN to 0.11.5
                                     Method |       Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
------------------------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|-------:|-------:|----------:|
                                    Compile | 254.188 us | 1.8270 us | 1.7090 us | 27.01 |    0.35 | 1.9531 | 0.9766 |      - |  10.93 KB |
                                CompileFast |  10.632 us | 0.0604 us | 0.0565 us |  1.13 |    0.02 | 0.6409 | 0.3204 | 0.0305 |   2.98 KB |
                CompileFast_LightExpression |   9.412 us | 0.1053 us | 0.0933 us |  1.00 |    0.00 | 0.6409 | 0.3204 | 0.0305 |   2.98 KB |
                 CompileFast_WithoutClosure |  10.073 us | 0.0732 us | 0.0649 us |  1.07 |    0.01 | 0.6409 | 0.3204 | 0.0305 |   2.95 KB |
 CompileFast_LightExpression_WithoutClosure |   8.062 us | 0.0471 us | 0.0393 us |  0.86 |    0.01 | 0.6409 | 0.3204 | 0.0305 |   2.95 KB |

## V3

|                                     Method |       Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
|------------------------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|-------:|-------:|----------:|
|                                    Compile | 285.602 us | 2.1509 us | 2.0119 us | 27.32 |    0.20 | 1.9531 | 0.9766 |      - |  10.87 KB |
|                                CompileFast |  11.418 us | 0.0519 us | 0.0460 us |  1.09 |    0.00 | 0.6104 | 0.3052 | 0.0305 |   2.84 KB |
|                CompileFast_LightExpression |  10.452 us | 0.0365 us | 0.0342 us |  1.00 |    0.00 | 0.6104 | 0.3052 | 0.0305 |   2.84 KB |
|                 CompileFast_WithoutClosure |  10.437 us | 0.0225 us | 0.0200 us |  1.00 |    0.00 | 0.5951 | 0.2899 | 0.0305 |   2.76 KB |
| CompileFast_LightExpression_WithoutClosure |   9.223 us | 0.0349 us | 0.0310 us |  0.88 |    0.00 | 0.5951 | 0.2899 | 0.0305 |   2.76 KB |

        */
        [MemoryDiagnoser]
        public class Compile_only
        {
            private static readonly Expression<Func<Source, Dest, ResolutionContext, Dest>> _expression = CreateExpression();
            private static readonly LightExpression.Expression<Func<Source, Dest, ResolutionContext, Dest>> _lightExpression = CreateLightExpression();

            [Benchmark]
            public object Compile() => _expression.Compile();

            [Benchmark]
            public object CompileFast() => _expression.CompileFast();

            [Benchmark(Baseline = true)]
            public object CompileFast_LightExpression() =>
                LightExpression.ExpressionCompiler.CompileFast(_lightExpression);

            [Benchmark]
            public object CompileFast_WithoutClosure() =>
                _expression.TryCompileWithoutClosure<Func<Source, Dest, ResolutionContext, Dest>>();

            [Benchmark]
            public object CompileFast_LightExpression_WithoutClosure() => 
                LightExpression.ExpressionCompiler.TryCompileWithoutClosure<Func<Source, Dest, ResolutionContext, Dest>>(_lightExpression);
        }

        /*
        ## Initial results with not yet released v2.1

        BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.765 (1803/April2018Update/Redstone4)
        Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
        Frequency=2156250 Hz, Resolution=463.7681 ns, Timer=TSC
        .NET Core SDK=3.0.100-preview3-010431
          [Host]     : .NET Core 2.1.11 (CoreCLR 4.6.27617.04, CoreFX 4.6.27617.02), 64bit RyuJIT
          DefaultJob : .NET Core 2.1.11 (CoreCLR 4.6.27617.04, CoreFX 4.6.27617.02), 64bit RyuJIT


                                       Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        ------------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                             Create_n_Compile | 314.09 us | 1.6548 us | 1.5479 us | 11.49 |    0.08 |      2.9297 |      1.4648 |           - |            13.82 KB |
                         Create_n_CompileFast |  27.34 us | 0.1675 us | 0.1566 us |  1.00 |    0.00 |      1.3733 |      0.6714 |      0.0305 |             6.38 KB |
         Create_n_CompileFast_LightExpression |  12.53 us | 0.0818 us | 0.0765 us |  0.46 |    0.00 |      1.2512 |      0.6256 |      0.0458 |             5.78 KB |

        ## V3

                                       Method |      Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
        ------------------------------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|-------:|----------:|
                             Create_n_Compile | 313.69 us | 1.5498 us | 1.4497 us | 24.31 |    0.21 | 2.9297 | 1.4648 |      - |   13.7 KB |
                         Create_n_CompileFast |  25.06 us | 0.0842 us | 0.0788 us |  1.94 |    0.01 | 1.2512 | 0.6104 | 0.0305 |   5.77 KB |
         Create_n_CompileFast_LightExpression |  12.90 us | 0.0798 us | 0.0708 us |  1.00 |    0.00 | 1.0071 | 0.5035 | 0.0458 |   4.64 KB |

|                               Method |      Mean |    Error |   StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
|------------------------------------- |----------:|---------:|---------:|------:|--------:|-------:|-------:|-------:|----------:|
|                     Create_n_Compile | 263.63 us | 0.835 us | 0.781 us | 22.21 |    0.10 | 2.4414 | 0.9766 |      - |  13.34 KB |
|                 Create_n_CompileFast |  18.45 us | 0.168 us | 0.157 us |  1.55 |    0.01 | 1.2207 | 0.6104 | 0.0305 |   5.73 KB |
| Create_n_CompileFast_LightExpression |  11.88 us | 0.067 us | 0.059 us |  1.00 |    0.00 | 0.9918 | 0.4883 | 0.0458 |   4.62 KB |


        */
        [MemoryDiagnoser]
        public class Create_and_Compile
        {
            [Benchmark]
            public object Create_n_Compile() => CreateExpression().Compile();

            [Benchmark]
            public object Create_n_CompileFast() => CreateExpression().CompileFast();

            [Benchmark(Baseline = true)]
            public object Create_n_CompileFast_LightExpression() => LightExpression.ExpressionCompiler.CompileFast(CreateLightExpression());
        }

        /*
        |                              Method |     Mean |     Error |    StdDev | Ratio |  Gen 0 | Gen 1 | Gen 2 | Allocated |
        |------------------------------------ |---------:|----------:|----------:|------:|-------:|------:|------:|----------:|
        |                     Invoke_Compiled | 8.027 ns | 0.0938 ns | 0.0878 ns |  0.98 | 0.0051 |     - |     - |      24 B |
        |                 Invoke_CompiledFast | 8.247 ns | 0.0379 ns | 0.0354 ns |  1.00 | 0.0051 |     - |     - |      24 B |
        | Invoke_CompiledFast_LightExpression | 8.225 ns | 0.0318 ns | 0.0298 ns |  1.00 | 0.0051 |     - |     - |      24 B |
        */

        [MemoryDiagnoser]
        public class Invoke_compiled_delegate
        {
            private static readonly Func<Source, Dest, ResolutionContext, Dest> _compiled = CreateExpression().Compile();
            private static readonly Func<Source, Dest, ResolutionContext, Dest> _compiledFast = CreateExpression().CompileFast(true);
            private static readonly Func<Source, Dest, ResolutionContext, Dest> _compiledFastLE = LightExpression.ExpressionCompiler.CompileFast(CreateLightExpression(), true);

            private static readonly Source _source = new Source { Value = 42 };

            [Benchmark(Baseline = true)]
            public Dest Invoke_Compiled() => _compiled(_source, null, null);

            [Benchmark]
            public Dest Invoke_CompiledFast() => _compiledFast(_source, null, null);

            [Benchmark]
            public Dest Invoke_CompiledFast_LightExpression() => _compiledFastLE(_source, null, null);
        }

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        public class ResolutionContext { }

        public class AutoMapperException : Exception
        {
            public AutoMapperException(string message, Exception innerException) : base(message, innerException) { }
        }

        private static Expression<Func<Source, Dest, ResolutionContext, Dest>> CreateExpression()
        {
            var srcParam = Parameter(typeof(Source), "source");
            var destParam = Parameter(typeof(Dest), "dest");

            var typeMapDestVar = Parameter(typeof(Dest), "d");
            var resolvedValueVar = Parameter(typeof(int), "val");
            var exceptionVar = Parameter(typeof(Exception), "ex");

            var expression = Lambda<Func<Source, Dest, ResolutionContext, Dest>>(
                Block(
                    Condition(
                        Equal(srcParam, Constant(null)),
                        Default(typeof(Dest)),
                        Block(typeof(Dest), new[] { typeMapDestVar },
                            Assign(
                                typeMapDestVar,
                                Coalesce(destParam, New(typeof(Dest).GetTypeInfo().DeclaredConstructors.First()))),
                            TryCatch(
                                /* Assign src.Value */
                                Block(typeof(void), new[] { resolvedValueVar },
                                    Block(
                                        Assign(resolvedValueVar,
                                            Condition(Or(Equal(srcParam, Constant(null)), Constant(false)),
                                                Default(typeof(int)),
                                                Property(srcParam, "Value"))
                                        ),
                                        Assign(Property(typeMapDestVar, "Value"), resolvedValueVar)
                                    )
                                ),
                                Catch(exceptionVar,
                                    Throw(New(typeof(AutoMapperException).GetTypeInfo().DeclaredConstructors.First(),
                                        Constant("Error mapping types."),
                                        exceptionVar))) // should skip this, cause does no make sense after the throw
                            ),
                            typeMapDestVar))
                ),
                srcParam, destParam, Parameter(typeof(ResolutionContext), "_")
            );

            return expression;
        }

        private static LightExpression.Expression<Func<Source, Dest, ResolutionContext, Dest>> CreateLightExpression()
        {
            var srcParam  = L.Parameter(typeof(Source), "source");
            var destParam = L.Parameter(typeof(Dest), "dest");

            var exceptionVar     = L.Parameter(typeof(Exception), "ex");
            var typeMapDestVar   = L.Parameter(typeof(Dest), "d");
            var resolvedValueVar = L.Parameter(typeof(int), "val");

            var expression = L.Lambda<Func<Source, Dest, ResolutionContext, Dest>>(
                L.Block(
                    L.Condition(
                        L.Equal(srcParam, L.Constant(null)),
                        L.Default(typeof(Dest)),
                        L.Block(typeof(Dest), new[] { typeMapDestVar },
                            L.Assign(
                                typeMapDestVar,
                                L.Coalesce(destParam, L.New(typeof(Dest).GetTypeInfo().DeclaredConstructors.First()))),
                            L.TryCatch(
                                /* Assign src.Value */
                                L.Block(new[] { resolvedValueVar },
                                    L.Block(
                                        L.Assign(resolvedValueVar,
                                            L.Condition(L.Or(L.Equal(srcParam, L.Constant(null)), L.Constant(false)),
                                                L.Default(typeof(int)),
                                                L.Property(srcParam, "Value"))
                                        ),
                                        L.Assign(L.Property(typeMapDestVar, "Value"), resolvedValueVar)
                                    )
                                ),
                                L.Catch(exceptionVar,
                                    L.Throw(
                                        L.New(typeof(AutoMapperException).GetTypeInfo().DeclaredConstructors.First(),
                                            L.Constant("Error mapping types."),
                                            exceptionVar),
                                        typeof(int))) // should skip this, cause does no make sense after the throw
                            ),
                            typeMapDestVar))
                ),
                srcParam, destParam, L.Parameter(typeof(ResolutionContext), "_")
            );

            return expression;
        }
    }
}
