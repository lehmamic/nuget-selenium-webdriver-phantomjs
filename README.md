# NuGet package - Selenium WebDriver PhantomJS

[![NuGet Package](https://img.shields.io/nuget/v/Selenium.WebDriver.PhantomJS.svg)](https://www.nuget.org/packages/Selenium.WebDriver.PhantomJS/)

## What's this?

This NuGet package install PhantomJS(Win32, macOS, and Linux64) for Selenium WebDriver into your Unit Test Project.

"phantomjs(.exe)" does not appear in Solution Explorer, but it is copied to bin folder from package folder when the build process.

NuGet package restoring ready, and no need to commit "phantomjs(.exe)" binary into source code control repository.

## How to install?

For example, at the package manager console on Visual Studio, enter following command.

    PM> Install-Package Selenium.WebDriver.phantomjs

## Detail

### Where is phantomjs.exe saved to?

phantomjs(.exe) exists at  
" _{solution folder}_ /packages/Selenium.WebDriver.PhantomJS. _{ver}_ /**driver**/ _{platform}_"
folder.

     {Solution folder}/
      +-- packages/
      |   +-- Selenium.WebDriver.PhantomJS.{version}/
      |       +-- driver/
      |       |   +-- win32
      |       |       +-- phantomjs.exe
      |       |   +-- mac64
      |       |       +-- phantomjs
      |       |   +-- linux64
      |       |       +-- phantomjs
      |       +-- build/
      +-- {project folder}/
          +-- bin/
              +-- Debug/
              |   +-- phantomjs(.exe) (copy from above by build process)
              +-- Release/
                  +-- phantomjs(.exe) (copy from above by build process)

 And package installer configure msbuild task such as .csproj to
 copy phantomjs(.exe) into output folder during build process.

### How to include the driver file into published files?

"phantomjs(.exe)" isn't included in published files on default configuration. This behavior is by design.

If you want to include "phantomjs(.exe)" into published files, please define `_PUBLISH_PHANTOMJS` compilation symbol.

![define _PUBLISH_PHANTOMJS compilation symbol](.asset/define_PUBLISH_PHANTOMJS_compilation_symbol.png)

Anoter way, you can define `PublishPhantomJSr` property with value is "true" in MSBuild file (.csproj, .vbproj, etc...) to publish the driver file instead of define compilation symbol.

```xml
  <Project ...>
    ...
    <PropertyGroup>
      ...
      <PublishPhantomJS>true</PublishPhantomJS>
      ...
    </PropertyGroup>
...
</Project>
```

#### Note

`PublishPhantomJS` MSBuild property always override the condition of define `_PUBLISH_PHANTOMJS` compilation symbol or not. If you define `PublishPhantomJS` MSBuild property with false, then the driver file isn't included in publish files whenever define `_PUBLISH_PHANTOMJS` compilation symbol or not.
