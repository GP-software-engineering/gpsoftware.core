# GPSoftware.Core

![NuGet Version](https://img.shields.io/nuget/v/GPSoftware.Core)
![Target Framework](https://img.shields.io/badge/.NET-Standard%202.0%20%7C%206.0%20%7C%208.0-blue)
![License](https://img.shields.io/github/license/GP-software-engineering/gpsoftware.core)

A lightweight, versatile, high-performance .NET library providing essential utilities and extension methods.

Built with **cross-compatibility** in mind, it supports modern .NET runtimes while remaining fully functional for legacy systems.

### Key Features

* **🌍 Localization & Finance:** Advanced `Currency` management with culture-aware formatting.
* **✅ Validation:** Specialized attributes for international Social Security Numbers (IT, CH, AT, FR).
* **📅 Date & Time:** Modern extensions for `DateOnly` and `DateTime` (optimized for .NET 6+).
* **🛠 Utility Extensions:** Optimized methods for string manipulation, collection handling, and data processing.

---

## 🛠 Compatibility & Support

The library targets **.NET Standard 2.0**, ensuring it can be used across a wide range of platforms:

* **Modern .NET**: Fully optimized for **.NET 8.0**, .NET 7.0, and .NET 6.0.
* **Legacy .NET Framework**: Compatible with **.NET Framework 4.6.2 and above**.
* **Cross-Platform**: Works seamlessly on Windows, Linux, and macOS via .NET Core / .NET 5+.

> [!NOTE]
> While the library is compatible with .NET Standard 2.0, some specific features (like `DateOnly` extensions) are optimized for the runtimes that natively support them.


### Quick Examples

```csharp
using GPSoftware.Core.Extensions;

// --- Culture-aware Currency (v3.4.0 feature)
var price = new Currency(1250.50m);
string formatted = price.ToString(new CultureInfo("it-IT")); // "1.250,50 €"


// --- International SSN Validation
[SocialSecurityNumber(Country.Italy)]
public string MyTaxCode { get; set; }


// --- International SSN Validation
string ssnCode = "CRTGPR68H21G839X";
bool isValid = SocialSecurityValidator.Validate(ssnCode, Country.Italy);
if (!isValid)
{
    throw new Exception("Invalid Italian Fiscal code");
}


// --- Quickly check strings for multiple conditions
string? myData = "  ";
if (myData.IsNotNullOrWhiteSpace()) 
{
    // Do something
}


// --- Fluent conversions with fallback values
string input = "123";
int result = input.ToInt(defaultValue: 0);


// --- Slug generation for URLs
string title = "Hello World! This is a Test";
string slug = title.ToSlug(); // "hello-world-this-is-a-test"


// --- Collection & Mapping Helpers
var items = new List<string> { "A", "B", "C" };
var item = items.GetElementAtOrDefault(5); // Returns null instead of throwing exception
var batches = items.Chunk(2); // Splitting into smaller chunks of 2
```

## How to install

Install the package from [NuGet](https://www.nuget.org/) or from the `Package Manager Console` :

```powershell
PM> Install-Package GPSoftware.Core
```

## Disclaimer

This library has been developed initialy for a personal project of mine which suits my use case.
It provides a simple way to do useful software development stuff.

We **do not** take responsability if you use/deploy this in a production environment.

