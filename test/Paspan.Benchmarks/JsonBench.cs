using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Paspan.Tests.Json;
using Parlot.Benchmarks.SpracheParsers;
//using Parlot.Benchmarks.SuperpowerParsers;
using Parlot.Benchmarks.PidginParsers;
using Parlot.Fluent;
using System.Text;

namespace Paspan.Benchmarks
{
    [MemoryDiagnoser, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), ShortRunJob]
    public class JsonBench
    {
#nullable disable
        private string _bigJson;
        private string _longJson;
        private string _wideJson;
        private string _deepJson;
        private byte[] _bigJsonBytes;
        private byte[] _longJsonBytes;
        private byte[] _wideJsonBytes;
        private byte[] _deepJsonBytes;
        private Parser<IJson> _compiled;
#nullable restore

        private static readonly Random _random = new();

        [GlobalSetup]
        public void Setup()
        {
            _bigJson = BuildJson(4, 4, 3).ToString()!;
            _longJson = BuildJson(256, 1, 1).ToString()!;
            _wideJson = BuildJson(1, 1, 256).ToString()!;
            // .Net 6 System.Text.Json max depth = 64
            _deepJson = BuildJson(1, 60, 1).ToString()!;

            _bigJsonBytes = Encoding.UTF8.GetBytes(_bigJson);
            _longJsonBytes = Encoding.UTF8.GetBytes(_longJson);
            _wideJsonBytes = Encoding.UTF8.GetBytes(_wideJson);
            _deepJsonBytes = Encoding.UTF8.GetBytes(_deepJson);

            _compiled = Parlot.Tests.Json.JsonParser.Json.Compile();
        }

        [Benchmark(Baseline = true), BenchmarkCategory("Big")]
        public IJson BigJson_Paspan()
        {
            return Paspan.Tests.Json.JsonParser.Parse(_bigJson);
        }

        [Benchmark, BenchmarkCategory("Big")]
        public IJson BigJson_PaspanUtf8()
        {
            return Paspan.Tests.Json.JsonParser.Parse(_bigJsonBytes);
        }

        [Benchmark, BenchmarkCategory("Big")]
        public IJson BigJson_PaspanUtf8Region()
        {
            return Paspan.Tests.Json.JsonParserRegion.Parse(_bigJsonBytes);
        }
        class DotNetJsonDocument : IJson
        {
            public System.Text.Json.JsonDocument? Doc { get; set; }
        }

        [Benchmark, BenchmarkCategory("Big")]
        public IJson BigJson_DotNetJsonDocument()
        {
            return new DotNetJsonDocument { Doc = System.Text.Json.JsonDocument.Parse(_bigJsonBytes) };
        }

        [Benchmark, BenchmarkCategory("Big")]
        public IJson BigJson_Parlot()
        {
            return Parlot.Tests.Json.JsonParser.Parse(_bigJson);
        }

        [Benchmark, BenchmarkCategory("Big")]
        public IJson BigJson_ParlotCompiled()
        {
            return _compiled.Parse(_bigJson);
        }

        [Benchmark, BenchmarkCategory("Big")]
        public IJson BigJson_Pidgin()
        {
            return PidginJsonParser.Parse(_bigJson).Value;
        }

        [Benchmark, BenchmarkCategory("Big")]
        public IJson BigJson_Sprache()
        {
            return SpracheJsonParser.Parse(_bigJson).Value;
        }

        //[Benchmark, BenchmarkCategory("Big")]
        //public IJson BigJson_Superpower()
        //{
        //    return SuperpowerJsonParser.Parse(_bigJson);
        //}

        [Benchmark(Baseline = true), BenchmarkCategory("Long")]
        public IJson LongJson_Paspan()
        {
            return Paspan.Tests.Json.JsonParser.Parse(_longJson);
        }

        [Benchmark, BenchmarkCategory("Long")]
        public IJson LongJson_PaspanUtf8()
        {
            return Paspan.Tests.Json.JsonParser.Parse(_longJsonBytes);
        }

        [Benchmark, BenchmarkCategory("Long")]
        public IJson LongJson_PaspanUtf8Region()
        {
            return Paspan.Tests.Json.JsonParserRegion.Parse(_longJsonBytes);
        }


        [Benchmark, BenchmarkCategory("Long")]
        public IJson LongJson_DotNetJsonDocument()
        {
            return new DotNetJsonDocument { Doc = System.Text.Json.JsonDocument.Parse(_longJsonBytes) };
        }

        [Benchmark, BenchmarkCategory("Long")]
        public IJson LongJson_Parlot()
        {
            return Parlot.Tests.Json.JsonParser.Parse(_longJson);
        }

        [Benchmark, BenchmarkCategory("Long")]
        public IJson LongJson_ParlotCompiled()
        {
            return _compiled.Parse(_longJson);
        }

        [Benchmark, BenchmarkCategory("Long")]
        public IJson LongJson_Pidgin()
        {
            return PidginJsonParser.Parse(_longJson).Value;
        }

        [Benchmark, BenchmarkCategory("Long")]
        public IJson LongJson_Sprache()
        {
            return SpracheJsonParser.Parse(_longJson).Value;
        }

        //[Benchmark, BenchmarkCategory("Long")]
        //public IJson LongJson_Superpower()
        //{
        //    return SuperpowerJsonParser.Parse(_longJson);
        //}

        [Benchmark(Baseline = true), BenchmarkCategory("Deep")]
        public IJson DeepJson_Paspan()
        {
            return Paspan.Tests.Json.JsonParser.Parse(_deepJson);
        }

        [Benchmark, BenchmarkCategory("Deep")]
        public IJson DeepJson_PaspanUtf8()
        {
            return Paspan.Tests.Json.JsonParser.Parse(_deepJsonBytes);
        }

        [Benchmark, BenchmarkCategory("Deep")]
        public IJson DeepJson_PaspanUtf8Region()
        {
            return Paspan.Tests.Json.JsonParserRegion.Parse(_deepJsonBytes);
        }

        [Benchmark, BenchmarkCategory("Deep")]
        public IJson DeepJson_DotNetJsonDocument()
        {
            return new DotNetJsonDocument { Doc = System.Text.Json.JsonDocument.Parse(_deepJsonBytes) };
        }

        [Benchmark, BenchmarkCategory("Deep")]
        public IJson DeepJson_Parlot()
        {
            return Parlot.Tests.Json.JsonParser.Parse(_deepJson);
        }

        [Benchmark, BenchmarkCategory("Deep")]
        public IJson DeepJson_ParlotCompiled()
        {
            return _compiled.Parse(_deepJson);
        }

        [Benchmark, BenchmarkCategory("Deep")]
        public IJson DeepJson_Pidgin()
        {
            return PidginJsonParser.Parse(_deepJson).Value;
        }

        [Benchmark, BenchmarkCategory("Deep")]
        public IJson DeepJson_Sprache()
        {
            return SpracheJsonParser.Parse(_deepJson).Value;
        }

        //this one blows the stack
        //[Benchmark, BenchmarkCategory("Deep")]
        //public IJson DeepJson_Superpower()
        //{
        //    return SuperpowerJsonParser.Parse(_deepJson);
        //}

        [Benchmark(Baseline = true), BenchmarkCategory("Wide")]
        public IJson WideJson_Paspan()
        {
            return Paspan.Tests.Json.JsonParser.Parse(_wideJson);
        }

        [Benchmark, BenchmarkCategory("Wide")]
        public IJson WideJson_PaspanUtf8()
        {
            return Paspan.Tests.Json.JsonParser.Parse(_wideJsonBytes);
        }

        [Benchmark, BenchmarkCategory("Wide")]
        public IJson WideJson_PaspanUtf8Region()
        {
            return Paspan.Tests.Json.JsonParserRegion.Parse(_wideJsonBytes);
        }

        [Benchmark, BenchmarkCategory("Wide")]
        public IJson WideJson_DotNetJsonDocument()
        {
            return new DotNetJsonDocument { Doc = System.Text.Json.JsonDocument.Parse(_wideJsonBytes) };
        }

        [Benchmark, BenchmarkCategory("Wide")]
        public IJson WideJson_Parlot()
        {
            return Parlot.Tests.Json.JsonParser.Parse(_wideJson);
        }

        [Benchmark, BenchmarkCategory("Wide")]
        public IJson WideJson_ParlotCompiled()
        {
            return _compiled.Parse(_wideJson);
        }

        [Benchmark, BenchmarkCategory("Wide")]
        public IJson WideJson_Pidgin()
        {
            return PidginJsonParser.Parse(_wideJson).Value;
        }

        [Benchmark, BenchmarkCategory("Wide")]
        public IJson WideJson_Sprache()
        {
            return SpracheJsonParser.Parse(_wideJson).Value;
        }

        //[Benchmark, BenchmarkCategory("Wide")]
        //public IJson WideJson_Superpower()
        //{
        //    return SuperpowerJsonParser.Parse(_wideJson);
        //}

        private static IJson BuildJson(int length, int depth, int width)
            => new JsonArray(
                Enumerable.Repeat(1, length)
                    .Select(_ => BuildObject(depth, width))
                    .ToImmutableArray()
            );

        private static IJson BuildObject(int depth, int width)
        {
            if (depth == 0)
            {
                return new JsonString(RandomString(6));
            }
            return new JsonObject(
                Enumerable.Repeat(1, width)
                    .Select(_ => new KeyValuePair<string, IJson>(RandomString(5), BuildObject(depth - 1, width)))
                    .ToImmutableDictionary()
            );
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
