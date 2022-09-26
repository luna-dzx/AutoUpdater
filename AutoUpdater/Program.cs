using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using Octokit;
using FileMode = System.IO.FileMode;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace AutoUpdater;

public class Program
{
    private const string Location = "DemoProject/";
    private const string DllName = "DemoProject.dll";
    private const string TargetFile = "linux-x64.zip";
    private const string AppName = "LunaAutoUpdate";
    private const long RepoId = 541505447;
    
    private static GitHubClient githubClient;


    public static void Main(string[] args)
    {
        #region check current version
        
            githubClient = new GitHubClient(new ProductHeaderValue(AppName));
            
            Release latestRelease = githubClient.Repository.Release.GetLatest(RepoId).Result;
            
            string latestVersion = latestRelease.TagName;
            Console.WriteLine("Latest Version: "+latestVersion);
            
            string? fileVersion = FileVersionInfo.GetVersionInfo(Location+DllName).FileVersion;
            Console.WriteLine("Downloaded Version: "+fileVersion);

            if (fileVersion == latestVersion)
            {
                Console.WriteLine("Up to date!");
                return;
            }
            
            Console.WriteLine("Out of date, Updating...");
            
            string url = latestRelease.Assets.First(a => a.Name == TargetFile).BrowserDownloadUrl;
            
        #endregion


        #region download file
        
            var fileStream = new FileStream(TargetFile, FileMode.Create);
            var webClient = new HttpClient();

            var message = new HttpRequestMessage(HttpMethod.Get, url);
            message.Headers.UserAgent.Add(ProductInfoHeaderValue.Parse(AppName));
            
            webClient.Send(message).Content.ReadAsStream().CopyTo(fileStream);
            fileStream.Dispose();
            
            Console.WriteLine("Download Complete");
            
        #endregion


        #region unzip and delete zip

            ZipArchive archive = ZipFile.OpenRead(TargetFile);
            
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(Location, file.FullName);
                string? directory = Path.GetDirectoryName(completeFileName);

                if (directory != null && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);
            }

            Console.WriteLine("Extracted");
            
            archive.Dispose();
            
            File.Delete(TargetFile);
            Console.WriteLine("Deleted Zip");

        #endregion


        
        
    }

}