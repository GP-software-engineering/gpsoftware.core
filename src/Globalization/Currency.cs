using System;
using System.Collections.Generic;
using GPSoftware.Core.Validation;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace GPSoftware.Core.Globalization {

    public static class Currency {

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

        // Define the dictionary interface based on the framework version
#if NET8_0_OR_GREATER
        // .NET 8+: Use FrozenDictionary for O(1) ultra-fast read-only lookups
        private static readonly FrozenDictionary<string, string> _symbolsMap = GetRawData().ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
#else
        // .NET Standard 2.0 / .NET 6: Use standard Dictionary with case-insensitive comparer
        private static readonly Dictionary<string, string> _symbolsMap = new Dictionary<string, string>(GetRawData(), StringComparer.OrdinalIgnoreCase);
#endif



        /// <summary>
        /// Contains the raw data definitions. 
        /// Used to initialize the optimized dictionary based on the runtime platform.
        /// </summary>
        private static Dictionary<string, string> GetRawData() {
            return new Dictionary<string, string>(200) {
                // Pre-allocate capacity to avoid resizing during init
                { "AED", "د.إ" },
                { "AFN", "؋" },
                { "ALL", "L" },
                { "AMD", "֏" },
                { "ANG", "ƒ" },
                { "AOA", "Kz" },
                { "ARS", "$" },
                { "AUD", "$" },
                { "AWG", "ƒ" },
                { "AZN", "₼" },
                { "BAM", "KM" },
                { "BBD", "$" },
                { "BDT", "৳" },
                { "BGN", "лв" },
                { "BHD", ".د.ب" },
                { "BIF", "FBu" },
                { "BMD", "$" },
                { "BND", "$" },
                { "BOB", "$b" },
                { "BOV", "BOV" },
                { "BRL", "R$" },
                { "BSD", "$" },
                { "BTC", "₿" },
                { "BTN", "Nu." },
                { "BWP", "P" },
                { "BYN", "Br" },
                { "BYR", "Br" },
                { "BZD", "BZ$" },
                { "CAD", "$" },
                { "CDF", "FC" },
                { "CHE", "CHE" },
                { "CHF", "CHF" },
                { "CHW", "CHW" },
                { "CLF", "CLF" },
                { "CLP", "$" },
                { "CNH", "¥" },
                { "CNY", "¥" },
                { "COP", "$" },
                { "COU", "COU" },
                { "CRC", "₡" },
                { "CUC", "$" },
                { "CUP", "₱" },
                { "CVE", "$" },
                { "CZK", "Kč" },
                { "DJF", "Fdj" },
                { "DKK", "kr" },
                { "DOP", "RD$" },
                { "DZD", "دج" },
                { "EEK", "kr" },
                { "EGP", "£" },
                { "ERN", "Nfk" },
                { "ETB", "Br" },
                { "ETH", "Ξ" },
                { "EUR", "€" },
                { "FJD", "$" },
                { "FKP", "£" },
                { "GBP", "£" },
                { "GEL", "₾" },
                { "GGP", "£" },
                { "GHC", "₵" },
                { "GHS", "GH₵" },
                { "GIP", "£" },
                { "GMD", "D" },
                { "GNF", "FG" },
                { "GTQ", "Q" },
                { "GYD", "$" },
                { "HKD", "$" },
                { "HNL", "L" },
                { "HRK", "kn" },
                { "HTG", "G" },
                { "HUF", "Ft" },
                { "IDR", "Rp" },
                { "ILS", "₪" },
                { "IMP", "£" },
                { "INR", "₹" },
                { "IQD", "ع.د" },
                { "IRR", "﷼" },
                { "ISK", "kr" },
                { "JEP", "£" },
                { "JMD", "J$" },
                { "JOD", "JD" },
                { "JPY", "¥" },
                { "KES", "KSh" },
                { "KGS", "лв" },
                { "KHR", "៛" },
                { "KMF", "CF" },
                { "KPW", "₩" },
                { "KRW", "₩" },
                { "KWD", "KD" },
                { "KYD", "$" },
                { "KZT", "₸" },
                { "LAK", "₭" },
                { "LBP", "£" },
                { "LKR", "₨" },
                { "LRD", "$" },
                { "LSL", "M" },
                { "LTC", "Ł" },
                { "LTL", "Lt" },
                { "LVL", "Ls" },
                { "LYD", "LD" },
                { "MAD", "MAD" },
                { "MDL", "lei" },
                { "MGA", "Ar" },
                { "MKD", "ден" },
                { "MMK", "K" },
                { "MNT", "₮" },
                { "MOP", "MOP$" },
                { "MRO", "UM" },
                { "MRU", "UM" },
                { "MUR", "₨" },
                { "MVR", "Rf" },
                { "MWK", "MK" },
                { "MXN", "$" },
                { "MXV", "MXV" },
                { "MYR", "RM" },
                { "MZN", "MT" },
                { "NAD", "$" },
                { "NGN", "₦" },
                { "NIO", "C$" },
                { "NOK", "kr" },
                { "NPR", "₨" },
                { "NZD", "$" },
                { "OMR", "﷼" },
                { "PAB", "B/." },
                { "PEN", "S/." },
                { "PGK", "K" },
                { "PHP", "₱" },
                { "PKR", "₨" },
                { "PLN", "zł" },
                { "PYG", "Gs" },
                { "QAR", "﷼" },
                { "RMB", "￥" },
                { "RON", "lei" },
                { "RSD", "Дин." },
                { "RUB", "₽" },
                { "RWF", "R₣" },
                { "SAR", "﷼" },
                { "SBD", "$" },
                { "SCR", "₨" },
                { "SDG", "ج.س." },
                { "SEK", "kr" },
                { "SGD", "S$" },
                { "SHP", "£" },
                { "SLL", "Le" },
                { "SOS", "S" },
                { "SRD", "$" },
                { "SSP", "£" },
                { "STD", "Db" },
                { "STN", "Db" },
                { "SVC", "$" },
                { "SYP", "£" },
                { "SZL", "E" },
                { "THB", "฿" },
                { "TJS", "SM" },
                { "TMT", "T" },
                { "TND", "د.ت" },
                { "TOP", "T$" },
                { "TRL", "₤" },
                { "TRY", "₺" },
                { "TTD", "TT$" },
                { "TVD", "$" },
                { "TWD", "NT$" },
                { "TZS", "TSh" },
                { "UAH", "₴" },
                { "UGX", "USh" },
                { "USD", "$" },
                { "UYI", "UYI" },
                { "UYU", "$U" },
                { "UYW", "UYW" },
                { "UZS", "лв" },
                { "VEF", "Bs" },
                { "VES", "Bs.S" },
                { "VND", "₫" },
                { "VUV", "VT" },
                { "WST", "WS$" },
                { "XAF", "FCFA" },
                { "XBT", "Ƀ" },
                { "XCD", "$" },
                { "XOF", "CFA" },
                { "XPF", "₣" },
                { "XSU", "Sucre" },
                { "XUA", "XUA" },
                { "YER", "﷼" },
                { "ZAR", "R" },
                { "ZMW", "ZK" },
                { "ZWD", "Z$" },
                { "ZWL", "$" },
            };
        }
    }
}
