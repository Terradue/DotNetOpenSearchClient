using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace Terradue.OpenSearch.Client.Test {

    public class TestBase {

        private static Dictionary<string, Credential> credentials;

        public TestBase() {
            LoadCredentials();
        }

        public OpenSearchClient CreateTestClient(string baseUrl = null, string metadataPath = null) {
            OpenSearchClient client = new OpenSearchClient();
            client.BaseUrls = new List<string>();
            if (baseUrl != null) client.BaseUrls.Add(baseUrl);
            client.MetadataPaths = new List<string>();
            if (metadataPath != null) client.MetadataPaths.Add(metadataPath);
            client.Timeout = 60000;
            client.Initialize();

            return client;
        }

        protected void LoadCredentials() {
            if (credentials != null) return;

            credentials = new Dictionary<string, Credential>();

            string authFile = String.Format("{0}/auth.txt", Regex.Replace(this.GetType().Assembly.Location, "/bin/.+$", String.Empty));
            Console.WriteLine("Trying to load credentials from file: {0}", authFile);

            if (File.Exists(authFile)) {
                using (StreamReader sr = new StreamReader(authFile)) {
                    string line = sr.ReadLine();
                    MatchCollection matches = Regex.Matches(line, "([^ =:]+)=([^ :]+):([^ ]+)");
                    foreach (Match match in matches) {
                        credentials[match.Groups[1].Value] = new Credential(match.Groups[2].Value, match.Groups[3].Value);
                    }
                    sr.Close();
                }
                Console.WriteLine("Credentials loaded");

            } else {
                Console.WriteLine("File not found (continuing without credentials).");
            }

        }



        public Credential GetCredential(string name, bool throwOnError = false) {
            if (credentials != null && credentials.ContainsKey(name)) {
                return credentials[name];
            }

            if (throwOnError) throw new UnauthorizedAccessException(String.Format("No credentials with name {0}", name));

            return null;
        }



        public string GetCredentialString(string name, bool throwOnError = false) {
            if (credentials != null && credentials.ContainsKey(name)) {
                return String.Format("{0}:{1}", credentials[name].Username, credentials[name].Password);
            }

            if (throwOnError) throw new UnauthorizedAccessException(String.Format("No credentials with name {0}", name));

            return null;
        }

    }



    public class Credential {
        public string Username { get; set; }
        public string Password { get; set; }

        public Credential(string username, string password) {
            this.Username = username;
            this.Password = password;
        }
    }

}
