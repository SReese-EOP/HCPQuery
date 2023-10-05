using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;



class Program
{

    public class HcpRootObject
    {
        public QueryResult queryResult { get; set; }
    }

    public class QueryResult
    {
        public Query query { get; set; }
        public Resultset[] resultSet { get; set; }
        public Status status { get; set; }
    }

    public class Query
    {
        public string expression { get; set; }
    }

    public class Status
    {
        public string code { get; set; }
        public string message { get; set; }
        public int totalResults { get; set; }
        public int results { get; set; }
    }

    public class Resultset
    {
        public string urlName { get; set; }
        public string operation { get; set; }
        public long version { get; set; }
        public string changeTimeMilliseconds { get; set; }
    }

    static async Task Main(string[] args)
    {
        string url = "https://eop46-pra.hcp.dev.pitc.gov/query";
        string authorizationHeader = "c3JlZXNl:b04224edc8f7c9c8177e9cf4c1d09525";
        string requestJson = @"
        {
            ""object"": {
                ""query"": ""+namespace:(EMail-OA.EOP46-PRA) +utf8Name:(ac3bf84b96ef468eb0cbc7b2912a3571@dev.pitc.gov.eml)"",
                ""objectProperties"": ""urlName"",
                ""verbose"": true
            }
        }";

        var reply = await SendHttpRequest(url, authorizationHeader, requestJson);
        string destPath = "c:/temp/" + "ac3bf84b96ef468eb0cbc7b2912a3571@dev.pitc.gov.eml";
        using (WebClient webClient = new WebClient())
        {
            webClient.Headers.Add("Authorization", "HCP " + authorizationHeader);
            webClient.DownloadFile(reply.queryResult.resultSet[0].urlName, destPath);
        }
            Console.WriteLine(reply.queryResult.resultSet);
    }

    static async Task<HcpRootObject> SendHttpRequest(string url, string authorizationHeader, string requestJson)
    {
        using (var httpClient = new HttpClient())
        {
            // Set authorization header
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("HCP", authorizationHeader);

            // Set content type and accept headers
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));

            // Create a StringContent object with the JSON data
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // Send POST request with the JSON data
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            // Check the response status
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Response:");
                Console.WriteLine(responseBody);
                HcpRootObject rootQueryResult = JsonSerializer.Deserialize<HcpRootObject>(responseBody);
                Console.WriteLine("URL PATH: " + rootQueryResult.queryResult.resultSet[0].urlName);
                
                
                return rootQueryResult;
            }
            else
            {
                Console.WriteLine($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
        }
    }
}

