namespace GPSoftware.core.Tests.Currency {

    public class Currency_Tests {

        public Currency_Tests() {
        }

        [Theory]
        [InlineData("EUR", "€")]
        [InlineData("USD", "$")]
        [InlineData("DZD", "دج")]
        [InlineData("XYZ", null)]
        public void GetSymbolFromCurrency(string currency, string? symbol) {
            // run
            string? output = GPSoftware.Core.Globalization.Currency.GetSymbolFromCurrency(currency);

            // assert
            output.ShouldBe(symbol);
        }

    }
}
