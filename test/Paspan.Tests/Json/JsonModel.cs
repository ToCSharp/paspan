using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Paspan.Tests.Json
{
    public interface IJson
    {
    }

    public class JsonArray : IJson
    {
        public ImmutableArray<IJson> Elements { get; }
        public JsonArray(ImmutableArray<IJson> elements)
        {
            Elements = elements;
        }
        public override string ToString()
            => $"[{string.Join(",", Elements.Select(e => e.ToString()))}]";
    }

    public class JsonArrayRegion : IJson
    {
        public ImmutableArray<IJson> Elements { get; }
        public JsonArrayRegion(ImmutableArray<IJson> elements)
        {
            Elements = elements;
        }
        public string ToString(ReadOnlySpan<byte> data)
        {
            var list = new List<string>();
            foreach (var el in Elements)
            {
                switch (el)
                {
                    case JsonStringRegion s:
                        list.Add(s.ToString(data));
                        break;
                    case JsonObjectRegion o:
                        list.Add($"\"{o.ToString(data)}");
                        break;
                    case JsonArrayRegion a:
                        list.Add($"\"{a.ToString(data)}");
                        break;
                    default:
                        throw new NotImplementedException();

                };
            }
            return $"[{string.Join(",", list)}]";
        }
    }

    public class JsonObject : IJson
    {
        public IImmutableDictionary<string, IJson> Members { get; }
        public JsonObject(IImmutableDictionary<string, IJson> members)
        {
            Members = members;
        }
        public override string ToString()
            => $"{{{string.Join(",", Members.Select(kvp => $"\"{kvp.Key}\":{kvp.Value}"))}}}";
    }

    public class JsonObjectRegion : IJson
    {
        public IImmutableDictionary<Region, IJson> Members { get; }
        public JsonObjectRegion(IImmutableDictionary<Region, IJson> members)
        {
            Members = members;
        }
        public override string ToString()
           => $"{{{string.Join(",", Members.Select(kvp => $"\"{kvp.Key}\":{kvp.Value}"))}}}";
        public string ToString(ReadOnlySpan<byte> data)
        {
            var list = new List<string>();
            foreach (var kvp in Members)
            {
                switch (kvp.Value)
                {
                    //case Region r:
                    //    list.Add($"\"{kvp.Key.ToString(data)}\":{r.ToString(data)}");
                    //    break;
                    case JsonStringRegion s:
                        list.Add($"\"{kvp.Key.ToString(data)}\":{s.ToString(data)}");
                        break;
                    case JsonObjectRegion o:
                        list.Add($"\"{kvp.Key.ToString(data)}\":{o.ToString(data)}");
                        break;
                    case JsonArrayRegion a:
                        list.Add($"\"{kvp.Key.ToString(data)}\":{a.ToString(data)}");
                        break;
                    default:
                        throw new NotImplementedException();

                };
            }
            return $"{{{string.Join(",", list)}}}";
        }
    }

    public class JsonString : IJson
    {
        public string Value { get; }
        public JsonString(string value)
        {
            Value = value;
        }

        public override string ToString()
            => $"\"{Value}\"";
    }

    public class JsonStringRegion : IJson
    {
        public Region Value { get; }
        public JsonStringRegion(Region value)
        {
            Value = value;
        }

        public string ToString(ReadOnlySpan<byte> data)
            => $"\"{Value.ToString(data)}\"";
    }

}
