using System;
using System.Linq;
using System.Net;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.Tools.GitVersion;
using Nuke.Core;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Core.IO.FileSystemTasks;
using static Nuke.Core.IO.PathConstruction;
using static Nuke.Core.EnvironmentInfo;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.BZip2;
using Nuke.Common.Tools.NuGet;

class Build : NukeBuild
{
    private const string Version = "2.1.1";

    private static readonly Uri BaseUri = new Uri("https://bitbucket.org/ariya/phantomjs/downloads/");
    private static readonly Uri NuGetDownloadUri = new Uri("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe");

    // Console application entry. Also defines the default target.
    public static int Main () => Execute<Build>(x => x.Pack);

    // Auto-injection fields:

    // [GitVersion] readonly GitVersion GitVersion;
    // Semantic versioning. Must have 'GitVersion.CommandLine' referenced.

    // [GitRepository] readonly GitRepository GitRepository;
    // Parses origin, branch name and head from git config.
    
    // [Parameter] readonly string MyGetApiKey;
    // Returns command-line arguments and environment variables.

    DriverMetadata[] Drivers = new DriverMetadata[]{
        new DriverMetadata { Platform = "win32", FileName = $"phantomjs-{Version}-windows.zip" },
        new DriverMetadata { Platform = "mac64", FileName = $"phantomjs-{Version}-macosx.zip" },
        new DriverMetadata { Platform = "linux64", FileName = $"phantomjs-{Version}-linux-x86_64.tar.bz2" },
     };

    Target Clean => _ => _
            .Executes(() =>
            {
                EnsureCleanDirectory(RootDirectory / "downloads");
                EnsureCleanDirectory(OutputDirectory);
            });

    Target Download => _ => _
            .DependsOn(Clean)
            .Executes(() => {
                using (var client = new WebClient())
                {
                  foreach(var driver in Drivers)
                  {
                    var downloadedFile = RootDirectory / "downloads" / driver.Platform / driver.FileName;
                    var downloadUrl = new Uri(BaseUri, driver.FileName);

                    EnsureExistingDirectory(Path.GetDirectoryName(downloadedFile));

                    Logger.Info($"Downloading file from url {downloadUrl} to local file {downloadedFile}");
                    client.DownloadFile(downloadUrl, downloadedFile);
                  }
                }
            });
            
    Target Decompress => _ => _
              .DependsOn(Download)
              .Executes(() =>
              {
                foreach (var driver in Drivers)
                {
                  var downloadedFile = RootDirectory / "downloads" / driver.Platform / driver.FileName;
                  var extractPath = RootDirectory / "downloads" / driver.Platform;
                
                  Logger.Info($"Unzip {downloadedFile} to directory {extractPath}");
                  if (Path.GetExtension(downloadedFile) == ".bz2")
                  {
                    ExtractTGZ(downloadedFile, extractPath);
                  }
                  else
                  {
                    FastZip fastZip = new FastZip();
                    fastZip.ExtractZip(downloadedFile, extractPath, null);
                  }
                }
              });

    Target Pack => _ => _
              .DependsOn(Decompress)
              .Executes(() =>
              {
                var nugetExe = TemporaryDirectory / "nuget.exe";
                if (!File.Exists(nugetExe))
                {
                    Logger.Info($"Nuget.exe not found, downloading from {NuGetDownloadUri}");	
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(NuGetDownloadUri, nugetExe);
                    }
                }

                var nuspecPath = SourceDirectory / "Selenium.WebDriver.PhantomJS.CrossPlatform.nuspec";
                NuGetTasks.NuGetPack(nuspecPath, s => NuGetTasks.DefaultNuGetPack.SetBasePath(SourceDirectory).SetVersion(Version));
              });

    private static void ExtractTGZ(string gzArchiveName, string destFolder)
    {
        using (Stream inStream = File.OpenRead(gzArchiveName))
        using (Stream bzip2Stream = new BZip2InputStream(inStream))
        using(TarArchive tarArchive = TarArchive.CreateInputTarArchive(bzip2Stream))
        {
            tarArchive.ExtractContents(destFolder);
        }
    }
}
