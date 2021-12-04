using Paspan.Fluent;
using System.Text;
using Xunit;

namespace Paspan.Tests.Json
{
    public class JsonParserTests
    {
        [Theory]
        [InlineData("{\"property\":\"value\"}")]
        [InlineData("{\"property\":[\"value\",\"value\",\"value\"]}")]
        [InlineData("{\"property\":{\"property\":\"value\"}}")]
        public void ShouldParseJson(string json)
        {
            var result = JsonParser.Parse(json);
            Assert.Equal(json, result.ToString());
        }

        [Theory]
        [InlineData("{\"property\":\"value\"}")]
        [InlineData("{\"property\":[\"value\",\"value\",\"value\"]}")]
        [InlineData("{\"property\":{\"property\":\"value\"}}")]
        public void ShouldParseRegionJson(string json)
        {
            var data = Encoding.UTF8.GetBytes(json);
            var result = JsonParserRegion.Parse(data);
            Assert.Equal(json, ((JsonObjectRegion)result).ToString(data));
        }

    }
}
