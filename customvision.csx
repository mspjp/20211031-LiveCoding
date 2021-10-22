#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"
#r "System.Web"

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Web;
using Newtonsoft.Json;

/// <summary>
/// Custom Visionで画像を識別
/// </summary>
/// <returns>Stream</returns>
static async Task<HttpResponseMessage> GetVisionData(Stream stream)
{
    var client = new HttpClient();
    var queryString = HttpUtility.ParseQueryString(string.Empty);

    // Request headers
    client.DefaultRequestHeaders.Add("Prediction-key", Environment.GetEnvironmentVariable("CustomVisionPredictionKey"));

    // Request parameters
    //iterationIdとapplicationを指定する場合は使用(デフォルト指定している場合は不要)
    //queryString["iterationId"] = ConfigurationManager.AppSettings["CustomVisionIterationId"];
    //queryString["application"] = ConfigurationManager.AppSettings["CustomVisionApplication"];
    var uri = Environment.GetEnvironmentVariable("CustomVisionUri") + queryString;

    HttpResponseMessage response;

    // Request body
    var content = new StreamContent(stream);
    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
    response = await client.PostAsync(uri, content);

    return response;
}

/// <summary>
/// ComputerVisionAPIのレスポンスメッセージをパース
/// </summary>
/// <returns>Stream</returns>
static async Task<string> GetParseString(HttpResponseMessage visionResponse, TraceWriter log)
{
    var jsonContent = await visionResponse.Content.ReadAsStringAsync();
    log.Info(jsonContent);
    Image_Response image_data = JsonConvert.DeserializeObject<Image_Response>(jsonContent);

    string words = String.Empty;

    if (image_data.predictions.Any())
    {
        foreach (var prediction in image_data.predictions)
        {
            words = words + prediction.TagName + Environment.NewLine + " - " + Convert.ToDouble(prediction.Probability).ToString("P") + Environment.NewLine;
        }
    }
    else
    {
        words = "画像が認識できませんでした";
    }

    return words;
}

// Custom Visionから返却されたデータ
    public class Image_Response
    {
        public string Id { get; set; }
        public string Project { get; set; }
        public string Iteration { get; set; }
        public string Created { get; set; }
        public List<Prediction> predictions { get; set; }
    }

    public class Prediction
    {
        public string TagId { get; set; }
        public string TagName { get; set; }
        public string Probability { get; set; }
    }