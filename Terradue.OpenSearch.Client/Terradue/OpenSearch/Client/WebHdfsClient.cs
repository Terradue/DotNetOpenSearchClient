using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using log4net;

namespace Terradue.OpenSearch.Client {
    /// <summary>
    /// Tool library to connect to HDFS via HTTP
    /// for a complete list of API commands, refer to http://archive.cloudera.com/cdh5/cdh/5/hadoop/hadoop-project-dist/hadoop-hdfs/WebHDFS.html
    /// </summary>
    public class WebHdfsClient {
        
        private HttpWebRequest client;
        private readonly string hdfsUrl;
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET OPERATIONS
        public const string GET_OPERATION_OPEN = "OPEN";
        public const string GET_OPERATION_GETFILESTATUS = "GETFILESTATUS";
        public const string GET_OPERATION_LISTSTATUS = "LISTSTATUS";

        // DEFAULT HTTP WEBHDFS VALUES
        public const string DEFAULT_HTTP_WEBHDFS_PORT = "50075";
        public const string DEFAULT_HTTP_WEBHDFS_VERSION = "v1";


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="version"></param>
        public WebHdfsClient(string host, string port = DEFAULT_HTTP_WEBHDFS_PORT,
            string version = DEFAULT_HTTP_WEBHDFS_VERSION) {
            hdfsUrl = $"http://{host}:{port}/webhdfs/{version}";
        }


        /// <summary>
        /// Returns file content
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="offset"></param>
        /// <param name="contentlength"></param>
        /// <param name="buffersize"></param>
        /// <returns></returns>
        public string Open(string folder, int offset = 0, string contentlength = null, string buffersize = null) {
            var parameters = new Dictionary<string, string>();
            if (offset != 0) parameters.Add("offset", offset.ToString());
            if (contentlength != null) parameters.Add("length", contentlength);
            if (buffersize != null) parameters.Add("buffersize", buffersize);

            return GetRequest(folder, GET_OPERATION_OPEN, parameters);
        }

        /// <summary>
        /// Returns file status
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public string FileStatus(string folder) {
            return GetRequest(folder, GET_OPERATION_GETFILESTATUS);
        }


        /// <summary>
        /// Lists files in folder
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public string ListStatus(string folder) {
            return GetRequest(folder, GET_OPERATION_LISTSTATUS);
        }


        /// <summary>
        /// General Method for GET requests
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="requestType"></param>
        /// <param name="optionalParameters"></param>
        /// <returns></returns>
        private string GetRequest(string folder, string requestType,
            Dictionary<string, string> optionalParameters = null) {
            var optionalparams = "";
            if (optionalParameters != null) {
                optionalparams = string.Join("&", optionalParameters.Select(x => x.Key + "=" + x.Value).ToArray());
            }

            var urlrequest = $"{hdfsUrl}/{folder}/?op={requestType}&{optionalparams}";
            
            log.DebugFormat("WebHdfsClient GetRequest to: {0}" ,urlrequest );
            
            client = (HttpWebRequest) WebRequest.Create(urlrequest);
            var response = client.GetResponse();
            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var content = sr.ReadToEnd();
            return content;
        }
    }
}