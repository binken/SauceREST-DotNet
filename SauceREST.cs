

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ETFSeleniumTestCommon
{

    public class SauceREST
    {
        private string _username;
        private string _accessKey;
        private readonly static string RESTURL = "https://saucelabs.com/rest/v1/{0}";
        private static readonly string RESTURL_DOWNLOAD = "https://saucelabs.com/rest/{0}/jobs/{1}";
        private readonly static string USER_RESULT_FORMAT = RESTURL + "/{1}";
        private readonly static string JOB_RESULT_FORMAT = RESTURL + "/jobs/{1}";
        private readonly static string DOWNLOAD_VIDEO_FORMAT = RESTURL_DOWNLOAD + "/results/video.flv";
        private readonly static string DOWNLOAD_LOG_FORMAT = RESTURL_DOWNLOAD + "/results/selenium-server.log";
        public SauceREST(string username, string accessKey)
        {
            _username = username;
            _accessKey = accessKey;
        }
            /**
         * Marks a Sauce Job as 'passed'.
         *
         * @param jobId the Sauce Job Id, typically equal to the Selenium/WebDriver sessionId
         * @throws IOException thrown if an error occurs invoking the REST request
         */
        public void JobPassed(string jobId)
        {
            IDictionary<String, Object> updates = new Dictionary<String, Object>();
            updates.Add("passed", true);
            UpdateJobInfo(jobId, updates);
        }
            /**
         * Marks a Sauce Job as 'failed'.
         *
         * @param jobId the Sauce Job Id, typically equal to the Selenium/WebDriver sessionId
         * @throws IOException thrown if an error occurs invoking the REST request
         */
        public void JobFailed(string jobId)
        {
            IDictionary<String, Object> updates = new Dictionary<String, Object>();
            updates.Add("passed", false);
            UpdateJobInfo(jobId, updates);
        }
            /**
         * Downloads the video for a Sauce Job to the filesystem.  The file will be stored in
         * a directory specified by the <code>location</code> field.
         *
         * @param jobId    the Sauce Job Id, typically equal to the Selenium/WebDriver sessionId
         * @param location
         * @throws IOException thrown if an error occurs invoking the REST request
         */
        public void DownloadVideo(string jobId, string location)
        {
            Uri restEndpoint = new Uri(String.Format(DOWNLOAD_VIDEO_FORMAT,_username, jobId));
            DownloadFile(location, restEndpoint);
        }
            /**
        * Downloads the log file for a Sauce Job to the filesystem.  The file will be stored in
        * a directory specified by the <code>location</code> field.
        *
        * @param jobId    the Sauce Job Id, typically equal to the Selenium/WebDriver sessionId
        * @param location
        * @throws IOException thrown if an error occurs invoking the REST request
        */
        public void DownloadLog(string jobId, string location)
        {
            Uri restEndpoint = new Uri(String.Format(DOWNLOAD_LOG_FORMAT, _username, jobId));
            DownloadFile(location, restEndpoint);
        }
        public String RetrieveResults(string path)
        {
            Uri restEndpoint =  new Uri(String.Format(USER_RESULT_FORMAT, _username, path));
            return RetrieveResults(restEndpoint);
        }
        public String GetJobInfo(string jobId)
        {
            Uri restEndpoint = new Uri(String.Format(JOB_RESULT_FORMAT, _username, jobId));
            return RetrieveResults(restEndpoint);
        }

        public String RetrieveResults(Uri restEndpoint)
        {
            StringBuilder builder = new StringBuilder();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(restEndpoint);
            request.Method = "GET";
            request.ContentType = "application/text";
            string usernamePassword = _username + ":" + _accessKey;
            CredentialCache mycache = new CredentialCache();
            mycache.Add(restEndpoint, "Basic", new NetworkCredential(_username, _accessKey));
            request.Credentials = mycache;
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(new ASCIIEncoding().GetBytes(usernamePassword)));

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string inputLine;
                    while ((inputLine = reader.ReadLine()) != null)
                    {
                        builder.Append(inputLine);
                    }
                }
            }
            return builder.ToString();
        }

        public void UpdateJobInfo(String jobId, IDictionary<String, Object> updates)
        {
            String url = string.Format(JOB_RESULT_FORMAT, _username, jobId);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.ContentType = "application/json";
            request.Method = "PUT";
            string usernamePassword = _username + ":" + _accessKey;
            CredentialCache mycache = new CredentialCache();
            mycache.Add(new Uri(url), "Basic", new NetworkCredential(_username, _accessKey));
            request.Credentials = mycache;
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(new ASCIIEncoding().GetBytes(usernamePassword)));

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(updates));
            }

            WebResponse response = request.GetResponse();
            response.Close();

        }



        private void DownloadFile(string location, Uri restEndpoint)
        {
             
            using(var webClient = new WebClient()){
            webClient.Credentials = new NetworkCredential(_username, _accessKey);
            webClient.DownloadFile(restEndpoint, location);
            }
        }
    }
}