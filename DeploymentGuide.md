# GPSoftware Core library Deployment guide
[![Nuget](https://img.shields.io/nuget/v/GPSoftware.Core.svg)](https://www.nuget.org/packages/GPSoftware.Core)
[![Nuget Downloads](https://img.shields.io/nuget/dt/GPSoftware.Core)](https://www.nuget.org/packages/GPSoftware.Core)
<hr />

#### Step 1: Pre-flight Check

Before running any commands, ensure the required static files defined in your .csproj are present in the project root folder 
(where the .csproj file is located):

1. `README.md`: The markdown file for the package description.
2. `package_icon.png`: The icon image for the gallery.

#### Step 2: Upadete the package release notes

Open the .csproj file and update the content into the tag 
```
<PackageReleaseNotes>
  ...
</PackageReleaseNotes>
```

#### Step 3: Create the Package

Open your terminal/command prompt in the project folder.
Run the following command:

```
dotnet pack -c Release -o "YOUR_FOLDER"
```
