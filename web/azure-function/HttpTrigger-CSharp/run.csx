#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Net;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

public static async Task<object> Run(HttpRequestMessage request, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    string requestContentAsString = await request.Content.ReadAsStringAsync();
    dynamic requestContentAsJObject = JsonConvert.DeserializeObject(requestContentAsString);

    string mobileno = (string) requestContentAsJObject.acode + (string) requestContentAsJObject.mobileno;

    string connString = "DefaultEndpointsProtocol=https;AccountName=jciworkshop;AccountKey=;EndpointSuffix=core.windows.net";
    string containerName = "mobile";
    string fileName = "mobile.txt";

    CloudStorageAccount csaccount = CloudStorageAccount.Parse(connString);
    CloudBlobClient serviceClient = csaccount.CreateCloudBlobClient();
    CloudBlobContainer container = serviceClient.GetContainerReference(containerName);
    CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
    string text = blob.DownloadText();

    if (!mobileno.Equals(text))
    {
        return request.CreateResponse<ResponseContent>(
            HttpStatusCode.BadRequest,
            new ResponseContent
            {
                version = "1.0.0",
                status = (int) HttpStatusCode.BadRequest,
                userMessage = $"您的手机号码 {mobileno} 尚未录入！"
            },
            new JsonMediaTypeFormatter(),
            "application/json");
    }

    return request.CreateResponse(HttpStatusCode.OK);
}

public class ResponseContent
{
    public string version { get; set; }

    public int status { get; set; }

    public string userMessage { get; set; }
}
