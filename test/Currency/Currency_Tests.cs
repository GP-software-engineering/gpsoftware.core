using System.Globalization;
using GPSoftware.Core.Globalization;

namespace GPSoftware.Core.Tests.Currency;

public class Currency_Tests {

    [Theory]
    [InlineData("EUR", "€")]
    [InlineData("USD", "$")]
    [InlineData("GBP", "£")]
    [InlineData("DZD", "دج")]
    [InlineData("XYZ", null)] // Unknown currency returns null
    public void GetSymbolFromCurrency_ShouldReturnCorrectSymbol(string currency, string? expectedSymbol) {
        // Act
        string? result = GPSoftware.Core.Globalization.Currency.GetSymbolFromCurrency(currency);

        // Assert
        result.ShouldBe(expectedSymbol);
    }

    [Fact]
    public void FormatAmount_Extension_ShouldFormatCorrectly_WithDifferentCultures() {
        // Arrange
        decimal amount = 1250.50m;

        // Save current culture to restore it later
        var originalCulture = CultureInfo.CurrentCulture;

        try {
            // CASE 1: Test with Italian Culture (Comma decimal separator)
            // -----------------------------------------------------------
            CultureInfo.CurrentCulture = new CultureInfo("it-IT");

            // Even if we are in Italy, asking for USD should show Italian formatting numbers but Dollar symbol
            // Expected: "$ 1.250,50" (Note: placement of symbol depends on culture pattern, 
            // but usually cloning culture keeps the pattern. In IT, positive currency pattern is usually "€ n")
            // Let's rely on the fact that we swap the symbol.

            string formattedItUsd = amount.FormatAmount("USD", "C");

            // Assert: Symbol is $, separator is comma
            formattedItUsd.ShouldContain("$");
            formattedItUsd.ShouldContain("1.250,50");


            // CASE 2: Test with US Culture (Dot decimal separator)
            // ----------------------------------------------------
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            string formattedUsEur = amount.FormatAmount("EUR", "C");

            // Assert: Symbol is €, separator is dot
            formattedUsEur.ShouldContain("€");
            formattedUsEur.ShouldContain("1,250.50");

        }
        finally {
            // Cleanup: Restore original culture
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void ToString_Extension_ShouldFormatCorrectly_WithDifferentCultures() {
        // Arrange
        decimal amount = 1250.50m;

        // Save current culture to restore it later
        var originalCulture = CultureInfo.CurrentCulture;

        try {
            // CASE 1: Test with Italian Culture (Comma decimal separator)
            // -----------------------------------------------------------
            CultureInfo.CurrentCulture = new CultureInfo("it-IT");

            // Even if we are in Italy, asking for USD should show Italian formatting numbers but Dollar symbol
            // Expected: "$ 1.250,50" (Note: placement of symbol depends on culture pattern, 
            // but usually cloning culture keeps the pattern. In IT, positive currency pattern is usually "€ n")
            // Let's rely on the fact that we swap the symbol.

            string formattedItUsd = amount.ToString("C", "USD");

            // Assert: Symbol is $, separator is comma
            formattedItUsd.ShouldContain("$");
            formattedItUsd.ShouldContain("1.250,50");


            // CASE 2: Test with US Culture (Dot decimal separator)
            // ----------------------------------------------------
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            string formattedUsEur = amount.ToString("C", "EUR");

            // Assert: Symbol is €, separator is dot
            formattedUsEur.ShouldContain("€");
            formattedUsEur.ShouldContain("1,250.50");

        }
        finally {
            // Cleanup: Restore original culture
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData(100, "USD", "$")]
    [InlineData(100, "EUR", "€")]
    [InlineData(100, "JPY", "¥")]
    public void ToString_Extension_ShouldUseCorrectSymbol(decimal amount, string currencyCode, string expectedSymbol) {
        // Act
        var result = amount.ToString("C", currencyCode);

        // Assert
        result.ShouldContain(expectedSymbol);
    }

    [Fact]
    public void ToString_Extension_ShouldFallbackToCode_IfSymbolNotFound() {
        // Arrange
        decimal amount = 100m;
        string unknownCurrency = "XXX";

        // Act
        var result = amount.ToString("C", unknownCurrency);

        // Assert
        // Should contain "XXX" because symbol is not found
        result.ShouldContain("XXX");
    }
}
