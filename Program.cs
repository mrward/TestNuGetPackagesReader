using System;
using System.IO;
using System.Linq;
using NuGet;

namespace TestNuGetPackagesReader
{
    /// <summary>
    /// This app will download the response from the url to a response.txt file.
    /// It will also try to get the first 10 NuGet packages from the url.
    /// </summary>
    class MainClass
    {
        public static void Main(string[] args)
        {
            try {
                // VSTS url here looking similar to:
                //
                // https://username.pkgs.visualstudio.com/_packaging/VSTSTest/nuget/v2
                //
                // Note that if DefaultCollection is added to the url then the console app will work:
                // https://username.pkgs.visualstudio.com/DefaultCollection/_packaging/VSTSTest/nuget/v2
                //
                // Credentials should already be stored in the NuGet.Config. See guide for creating
                // VSTS package source:
                //
                // https://www.visualstudio.com/en-us/docs/package/get-started/nuget/consume

                ConfigureCredentialProvider ();

                string url = "https://username.pkgs.visualstudio.com/_packaging/VSTSTest/nuget/v2";
                //url = "https://username.pkgs.visualstudio.com/DefaultCollection/_packaging/VSTSTest/nuget/v2";
                if (args.Length == 1) {
                    url = args[0];
                }

                DownloadData (url);
                ReadPackages (url);
            } catch (Exception ex) {
                Console.WriteLine (ex.ToString ());
            }
        }

        static void ReadPackages (string url)
        {
            var repository = PackageRepositoryFactory.Default.CreateRepository (url);

            var packages = repository
                .Search (null, false)
                .Take(10);

            foreach (var package in packages) {
                Console.WriteLine (package.ToString ());
            }
        }

        static void ConfigureCredentialProvider ()
        {
            ISettings settings = Settings.LoadDefaultSettings (null, null, null);
            var packageSourceProvider = new PackageSourceProvider (settings);

            var credentialProvider = new SettingsCredentialProvider (NullCredentialProvider.Instance, packageSourceProvider);
            HttpClient.DefaultCredentialProvider = credentialProvider;
        }

        static void DownloadData (string url)
        {
            // Hit the main url first to get authenticated.
            var uri = new Uri (url);
            var client = new RedirectedHttpClient (uri);
            using (var response = client.GetResponse ()) {
            }

            // Then run the search and save the results.
            uri = new Uri (url + "/Search()?$top=10&searchTerm=''&targetFramework=''&includePrerelease=false");
            client = new RedirectedHttpClient (uri);

            using (var response = client.GetResponse ()) {
                string data = response.GetResponseStream ().ReadToEnd ();
                File.WriteAllText ("response.txt", data);
            }
        }
    }
}
