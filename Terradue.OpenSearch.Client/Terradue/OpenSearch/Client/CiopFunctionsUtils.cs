using System;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using log4net;


namespace Terradue.OpenSearch.Client {
    /// <summary>
    /// Library class 
    /// Similar to /usr/lib/ciop/libexec/ciop-functions.sh
    /// </summary>
    public static class CiopFunctionsUtils {
        
        private static ILog log = LogManager.GetLogger(typeof(CiopFunctionsUtils));
        private const string CIOP_WF_RUN_ROOT_ENV = "ciop_wf_run_root";
        private const string CIOP_WF_JOBS_PARAMS_ENV = "ciop_wf_jobs_params";

        public static NetworkCredential GetT2Credentials() {
            var ciop_wf_run_root = Environment.GetEnvironmentVariable(CIOP_WF_RUN_ROOT_ENV);
            var ciop_wf_jobs_params = Environment.GetEnvironmentVariable(CIOP_WF_JOBS_PARAMS_ENV);
            var workflow_params_path = "";

            if (!string.IsNullOrWhiteSpace(ciop_wf_run_root)) {
                // sandbox
                workflow_params_path = $"{ciop_wf_run_root}/workflow-params.xml";
            }
            else if (!string.IsNullOrWhiteSpace(ciop_wf_jobs_params)) {
                // production center
                workflow_params_path = ciop_wf_jobs_params;
            }
            else {
                // not a production center environment nor sandbox thus return null
                return null;
            }

            string hostname;
            string workflow_params_xml;
            try {
                // retrieving hostname of the machine
                hostname = Dns.GetHostName();

                // instantiating a webhdfs client
                var client = new WebHdfsClient(hostname);

                // retrieving workflow-params.xml from hdfs
                workflow_params_xml = client.Open(workflow_params_path);
            }
            catch (WebException) {
                log.Warn("Could not connect to the webhdfs host");
                return null;
            }

            // parsing workflow-params.xml
            // 1. find node _T2Credentials
            // 2. look for parameter tags with id _T2ApiKey and _T2Username
            NetworkCredential t2ApiNetCreds;
            try {
                var xDoc = XDocument.Parse(workflow_params_xml);
                var workflow = xDoc.Element("workflow");
                var t2Credentials_node = workflow.Elements("node").FirstOrDefault(c => (string) c.Attribute("id") == "_T2Credentials").Elements();
                var t2Credentials_node_parameters = t2Credentials_node.Elements();
                var t2ApiKey = t2Credentials_node_parameters.FirstOrDefault(c => (string) c.Attribute("id") == "_T2ApiKey").Value;
                var t2ApiUsername = t2Credentials_node_parameters.FirstOrDefault(c => (string) c.Attribute("id") == "_T2Username").Value;
                t2ApiNetCreds = new NetworkCredential(t2ApiUsername, t2ApiKey);
            }
            catch (Exception) {
                log.Warn("Could not find _T2Credentials in workflow-params.xml.");
                return null;
            }

            return t2ApiNetCreds;
        }
    }
}