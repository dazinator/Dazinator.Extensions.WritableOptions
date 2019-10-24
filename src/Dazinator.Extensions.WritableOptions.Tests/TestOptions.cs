using System;

namespace Dazinator.Extensions.WritableOptions.Tests
{
    public class TestOptions
    {
        public bool Enabled { get; set; } = false;
        public int? SomeInt { get; set; }
        public decimal? SomeDecimal { get; set; }
    }
}
