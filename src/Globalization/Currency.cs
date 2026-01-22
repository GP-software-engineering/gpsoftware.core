using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GPSoftware.Core.Validation;

#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace GPSoftware.Core.Globalization {

    /// <summary>
    /// Provides utilities for handling currency symbols and formatting.
    /// Supports .NET Framework 4.7.2, .NET Standard 2.0, and modern .NET versions.
    /// </summary>
    public static class Currency {

        // We must keep the #if directive here because FrozenDictionary is a BCL type, 
        // not a language feature covered by PolySharp.
#if NET8_0_OR_GREATER
        // .NET 8+: Use FrozenDictionary for O(1) ultra-fast read-only lookups
        private static readonly FrozenDictionary<string, string> _symbolsMap = 
            GetRawData().ToFrozenDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
#else
        // .NET Framework 4.7.2 / .NET Standard: Use standard Dictionary with case-insensitive comparer
        private static readonly Dictionary<string, string> _symbolsMap = 
            GetRawData().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
#endif

        /// <summary>
        ///      Return the currency symbol from the three letters ISO 4217 currency code.
        /// </summary>
        /// <param name="currencyCode">The three letters ISO 4217 currency code (e.g. "USD", "eur")</param>
        /// <returns>The symbol (e.g. "$", "€") or null if not found</returns>
        public static string? GetSymbolFromCurrency(string currencyCode) {
            Check.NotNullOrEmpty(currencyCode, nameof(currencyCode));

            if (_symbolsMap.TryGetValue(currencyCode, out string? symbol)) {
                return symbol;
            }
            return null;
        }

        /// <summary>
        /// Formats a decimal amount as a currency string using the symbol associated with the provided ISO 4217 code.
        /// It preserves the current culture's number formatting (decimal and group separators) but overrides the currency symbol.
        /// </summary>
        /// <param name="amount">The numeric amount to format.</param>
        /// <param name="currencyCode">The three-letter ISO 4217 currency code (e.g. "USD", "EUR").</param>
        /// <param name="format">The format string to use (default is "C" for currency).</param>
        /// <returns>A formatted currency string (e.g. "€ 1.250,50" or "$ 1,250.50" depending on current culture).</returns>
        public static string FormatAmount(decimal amount, string currencyCode, string format = "C") {
            Check.NotNullOrEmpty(currencyCode, nameof(currencyCode));

            // Try to get the symbol, fallback to the code itself if not found (e.g. "100 XYZ")
            string symbol = GetSymbolFromCurrency(currencyCode) ?? currencyCode;

            // Clone the current culture to preserve the user's number format preferences (comma vs dot)
            // but inject the specific symbol for the requested currency.
            var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.CurrencySymbol = symbol;

            // Use the "C" format specifier with the modified culture
            return amount.ToString(format, culture);
        }

        /// <summary>
        ///     Formats a double amount as a currency string using the symbol associated with the provided ISO 4217 code.
        /// </summary>
        /// <param name="amount">The numeric amount to format.</param>
        /// <param name="currencyCode">The three-letter ISO 4217 currency code (e.g. "USD", "EUR").</param>
        /// <param name="format">The format string to use (default is "C" for currency).</param>
        /// <returns>A formatted currency string (e.g. "€ 1.250,50" or "$ 1,250.50" depending on current culture).</returns>
        public static string FormatAmount(double amount, string currencyCode, string format = "C") {
            return FormatAmount((decimal)amount, currencyCode, format);
        }

        #region Extensions

        /// <summary>
        ///     Extension method to format a decimal amount as a currency string using the symbol associated with the provided ISO 4217 code.
        /// </summary>
        /// <param name="format">The format string to use.</param>
        /// <param name="currencyCode">The three-letter ISO 4217 currency code (e.g. "USD", "EUR").</param>
        /// <returns>A formatted currency string (e.g. "€ 1.250,50" or "$ 1,250.50" depending on current culture).</returns>
        public static string ToString(this decimal amount, string format, string currencyCode) {
            return FormatAmount(amount, currencyCode, format);
        }

        #endregion

        /// <summary>
        /// Contains the raw data definitions. 
        /// Used to initialize the optimized dictionary based on the runtime platform.
        /// </summary>
        private static IEnumerable<KeyValuePair<string, string>> GetRawData() {
            yield return new KeyValuePair<string, string>("AED", "د.إ");
            yield return new KeyValuePair<string, string>("AFN", "؋");
            yield return new KeyValuePair<string, string>("ALL", "L");
            yield return new KeyValuePair<string, string>("AMD", "֏");
            yield return new KeyValuePair<string, string>("ANG", "ƒ");
            yield return new KeyValuePair<string, string>("AOA", "Kz");
            yield return new KeyValuePair<string, string>("ARS", "$");
            yield return new KeyValuePair<string, string>("AUD", "$");
            yield return new KeyValuePair<string, string>("AWG", "ƒ");
            yield return new KeyValuePair<string, string>("AZN", "₼");
            yield return new KeyValuePair<string, string>("BAM", "KM");
            yield return new KeyValuePair<string, string>("BBD", "$");
            yield return new KeyValuePair<string, string>("BDT", "৳");
            yield return new KeyValuePair<string, string>("BGN", "лв");
            yield return new KeyValuePair<string, string>("BHD", ".د.ب");
            yield return new KeyValuePair<string, string>("BIF", "FBu");
            yield return new KeyValuePair<string, string>("BMD", "$");
            yield return new KeyValuePair<string, string>("BND", "$");
            yield return new KeyValuePair<string, string>("BOB", "$b");
            yield return new KeyValuePair<string, string>("BOV", "BOV");
            yield return new KeyValuePair<string, string>("BRL", "R$");
            yield return new KeyValuePair<string, string>("BSD", "$");
            yield return new KeyValuePair<string, string>("BTC", "₿");
            yield return new KeyValuePair<string, string>("BTN", "Nu.");
            yield return new KeyValuePair<string, string>("BWP", "P");
            yield return new KeyValuePair<string, string>("BYN", "Br");
            yield return new KeyValuePair<string, string>("BYR", "Br");
            yield return new KeyValuePair<string, string>("BZD", "BZ$");
            yield return new KeyValuePair<string, string>("CAD", "$");
            yield return new KeyValuePair<string, string>("CDF", "FC");
            yield return new KeyValuePair<string, string>("CHE", "CHE");
            yield return new KeyValuePair<string, string>("CHF", "CHF");
            yield return new KeyValuePair<string, string>("CHW", "CHW");
            yield return new KeyValuePair<string, string>("CLF", "CLF");
            yield return new KeyValuePair<string, string>("CLP", "$");
            yield return new KeyValuePair<string, string>("CNH", "¥");
            yield return new KeyValuePair<string, string>("CNY", "¥");
            yield return new KeyValuePair<string, string>("COP", "$");
            yield return new KeyValuePair<string, string>("COU", "COU");
            yield return new KeyValuePair<string, string>("CRC", "₡");
            yield return new KeyValuePair<string, string>("CUC", "$");
            yield return new KeyValuePair<string, string>("CUP", "₱");
            yield return new KeyValuePair<string, string>("CVE", "$");
            yield return new KeyValuePair<string, string>("CZK", "Kč");
            yield return new KeyValuePair<string, string>("DJF", "Fdj");
            yield return new KeyValuePair<string, string>("DKK", "kr");
            yield return new KeyValuePair<string, string>("DOP", "RD$");
            yield return new KeyValuePair<string, string>("DZD", "دج");
            yield return new KeyValuePair<string, string>("EEK", "kr");
            yield return new KeyValuePair<string, string>("EGP", "£");
            yield return new KeyValuePair<string, string>("ERN", "Nfk");
            yield return new KeyValuePair<string, string>("ETB", "Br");
            yield return new KeyValuePair<string, string>("ETH", "Ξ");
            yield return new KeyValuePair<string, string>("EUR", "€");
            yield return new KeyValuePair<string, string>("FJD", "$");
            yield return new KeyValuePair<string, string>("FKP", "£");
            yield return new KeyValuePair<string, string>("GBP", "£");
            yield return new KeyValuePair<string, string>("GEL", "₾");
            yield return new KeyValuePair<string, string>("GGP", "£");
            yield return new KeyValuePair<string, string>("GHC", "₵");
            yield return new KeyValuePair<string, string>("GHS", "GH₵");
            yield return new KeyValuePair<string, string>("GIP", "£");
            yield return new KeyValuePair<string, string>("GMD", "D");
            yield return new KeyValuePair<string, string>("GNF", "FG");
            yield return new KeyValuePair<string, string>("GTQ", "Q");
            yield return new KeyValuePair<string, string>("GYD", "$");
            yield return new KeyValuePair<string, string>("HKD", "$");
            yield return new KeyValuePair<string, string>("HNL", "L");
            yield return new KeyValuePair<string, string>("HRK", "kn");
            yield return new KeyValuePair<string, string>("HTG", "G");
            yield return new KeyValuePair<string, string>("HUF", "Ft");
            yield return new KeyValuePair<string, string>("IDR", "Rp");
            yield return new KeyValuePair<string, string>("ILS", "₪");
            yield return new KeyValuePair<string, string>("IMP", "£");
            yield return new KeyValuePair<string, string>("INR", "₹");
            yield return new KeyValuePair<string, string>("IQD", "ع.د");
            yield return new KeyValuePair<string, string>("IRR", "﷼");
            yield return new KeyValuePair<string, string>("ISK", "kr");
            yield return new KeyValuePair<string, string>("JEP", "£");
            yield return new KeyValuePair<string, string>("JMD", "J$");
            yield return new KeyValuePair<string, string>("JOD", "JD");
            yield return new KeyValuePair<string, string>("JPY", "¥");
            yield return new KeyValuePair<string, string>("KES", "KSh");
            yield return new KeyValuePair<string, string>("KGS", "лв");
            yield return new KeyValuePair<string, string>("KHR", "៛");
            yield return new KeyValuePair<string, string>("KMF", "CF");
            yield return new KeyValuePair<string, string>("KPW", "₩");
            yield return new KeyValuePair<string, string>("KRW", "₩");
            yield return new KeyValuePair<string, string>("KWD", "KD");
            yield return new KeyValuePair<string, string>("KYD", "$");
            yield return new KeyValuePair<string, string>("KZT", "₸");
            yield return new KeyValuePair<string, string>("LAK", "₭");
            yield return new KeyValuePair<string, string>("LBP", "£");
            yield return new KeyValuePair<string, string>("LKR", "₨");
            yield return new KeyValuePair<string, string>("LRD", "$");
            yield return new KeyValuePair<string, string>("LSL", "M");
            yield return new KeyValuePair<string, string>("LTC", "Ł");
            yield return new KeyValuePair<string, string>("LTL", "Lt");
            yield return new KeyValuePair<string, string>("LVL", "Ls");
            yield return new KeyValuePair<string, string>("LYD", "LD");
            yield return new KeyValuePair<string, string>("MAD", "MAD");
            yield return new KeyValuePair<string, string>("MDL", "lei");
            yield return new KeyValuePair<string, string>("MGA", "Ar");
            yield return new KeyValuePair<string, string>("MKD", "ден");
            yield return new KeyValuePair<string, string>("MMK", "K");
            yield return new KeyValuePair<string, string>("MNT", "₮");
            yield return new KeyValuePair<string, string>("MOP", "MOP$");
            yield return new KeyValuePair<string, string>("MRO", "UM");
            yield return new KeyValuePair<string, string>("MRU", "UM");
            yield return new KeyValuePair<string, string>("MUR", "₨");
            yield return new KeyValuePair<string, string>("MVR", "Rf");
            yield return new KeyValuePair<string, string>("MWK", "MK");
            yield return new KeyValuePair<string, string>("MXN", "$");
            yield return new KeyValuePair<string, string>("MXV", "MXV");
            yield return new KeyValuePair<string, string>("MYR", "RM");
            yield return new KeyValuePair<string, string>("MZN", "MT");
            yield return new KeyValuePair<string, string>("NAD", "$");
            yield return new KeyValuePair<string, string>("NGN", "₦");
            yield return new KeyValuePair<string, string>("NIO", "C$");
            yield return new KeyValuePair<string, string>("NOK", "kr");
            yield return new KeyValuePair<string, string>("NPR", "₨");
            yield return new KeyValuePair<string, string>("NZD", "$");
            yield return new KeyValuePair<string, string>("OMR", "﷼");
            yield return new KeyValuePair<string, string>("PAB", "B/.");
            yield return new KeyValuePair<string, string>("PEN", "S/.");
            yield return new KeyValuePair<string, string>("PGK", "K");
            yield return new KeyValuePair<string, string>("PHP", "₱");
            yield return new KeyValuePair<string, string>("PKR", "₨");
            yield return new KeyValuePair<string, string>("PLN", "zł");
            yield return new KeyValuePair<string, string>("PYG", "Gs");
            yield return new KeyValuePair<string, string>("QAR", "﷼");
            yield return new KeyValuePair<string, string>("RMB", "￥");
            yield return new KeyValuePair<string, string>("RON", "lei");
            yield return new KeyValuePair<string, string>("RSD", "Дин.");
            yield return new KeyValuePair<string, string>("RUB", "₽");
            yield return new KeyValuePair<string, string>("RWF", "R₣");
            yield return new KeyValuePair<string, string>("SAR", "﷼");
            yield return new KeyValuePair<string, string>("SBD", "$");
            yield return new KeyValuePair<string, string>("SCR", "₨");
            yield return new KeyValuePair<string, string>("SDG", "ج.س.");
            yield return new KeyValuePair<string, string>("SEK", "kr");
            yield return new KeyValuePair<string, string>("SGD", "S$");
            yield return new KeyValuePair<string, string>("SHP", "£");
            yield return new KeyValuePair<string, string>("SLL", "Le");
            yield return new KeyValuePair<string, string>("SOS", "S");
            yield return new KeyValuePair<string, string>("SRD", "$");
            yield return new KeyValuePair<string, string>("SSP", "£");
            yield return new KeyValuePair<string, string>("STD", "Db");
            yield return new KeyValuePair<string, string>("STN", "Db");
            yield return new KeyValuePair<string, string>("SVC", "$");
            yield return new KeyValuePair<string, string>("SYP", "£");
            yield return new KeyValuePair<string, string>("SZL", "E");
            yield return new KeyValuePair<string, string>("THB", "฿");
            yield return new KeyValuePair<string, string>("TJS", "SM");
            yield return new KeyValuePair<string, string>("TMT", "T");
            yield return new KeyValuePair<string, string>("TND", "د.ت");
            yield return new KeyValuePair<string, string>("TOP", "T$");
            yield return new KeyValuePair<string, string>("TRL", "₤");
            yield return new KeyValuePair<string, string>("TRY", "₺");
            yield return new KeyValuePair<string, string>("TTD", "TT$");
            yield return new KeyValuePair<string, string>("TVD", "$");
            yield return new KeyValuePair<string, string>("TWD", "NT$");
            yield return new KeyValuePair<string, string>("TZS", "TSh");
            yield return new KeyValuePair<string, string>("UAH", "₴");
            yield return new KeyValuePair<string, string>("UGX", "USh");
            yield return new KeyValuePair<string, string>("USD", "$");
            yield return new KeyValuePair<string, string>("UYI", "UYI");
            yield return new KeyValuePair<string, string>("UYU", "$U");
            yield return new KeyValuePair<string, string>("UYW", "UYW");
            yield return new KeyValuePair<string, string>("UZS", "лв");
            yield return new KeyValuePair<string, string>("VEF", "Bs");
            yield return new KeyValuePair<string, string>("VES", "Bs.S");
            yield return new KeyValuePair<string, string>("VND", "₫");
            yield return new KeyValuePair<string, string>("VUV", "VT");
            yield return new KeyValuePair<string, string>("WST", "WS$");
            yield return new KeyValuePair<string, string>("XAF", "FCFA");
            yield return new KeyValuePair<string, string>("XBT", "Ƀ");
            yield return new KeyValuePair<string, string>("XCD", "$");
            yield return new KeyValuePair<string, string>("XOF", "CFA");
            yield return new KeyValuePair<string, string>("XPF", "₣");
            yield return new KeyValuePair<string, string>("XSU", "Sucre");
            yield return new KeyValuePair<string, string>("XUA", "XUA");
            yield return new KeyValuePair<string, string>("YER", "﷼");
            yield return new KeyValuePair<string, string>("ZAR", "R");
            yield return new KeyValuePair<string, string>("ZMW", "ZK");
            yield return new KeyValuePair<string, string>("ZWD", "Z$");
            yield return new KeyValuePair<string, string>("ZWL", "$");
        }
    }
}
