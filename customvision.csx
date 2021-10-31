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

    // リクエストヘッダ
    client.DefaultRequestHeaders.Add("Prediction-key", Environment.GetEnvironmentVariable("CustomVisionPredictionKey"));

    // リクエストパラメータ
    var uri = Environment.GetEnvironmentVariable("CustomVisionUri");

    // リクエストボディ
    var content = new StreamContent(stream);
    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
    HttpResponseMessage response = await client.PostAsync(uri, content);

    return response;
}

/// <summary>
/// Custom Visionのレスポンスメッセージをパース
/// </summary>
/// <returns>Stream</returns>
static async Task<string> GetParseString(HttpResponseMessage visionResponse, TraceWriter log)
{
    var jsonContent = await visionResponse.Content.ReadAsStringAsync();
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

/// <summary>
/// Custom Visionのレスポンスから最大確率のタグを取得
/// </summary>
/// <returns>Stream</returns>
static async Task<string> GetMaxProbability(HttpResponseMessage visionResponse, TraceWriter log)
{
    // Custom VisionのレスポンスからJSONをパースして予測結果を抽出
    var jsonContent = await visionResponse.Content.ReadAsStringAsync();
    log.Info(jsonContent);
    Image_Response image_data = JsonConvert.DeserializeObject<Image_Response>(jsonContent);

    if (image_data.predictions.Any())
    {
        double threshold = 0.8;  // 識別確率の閾値（この値以上の確からしさがあるタグをreturnする）
        double maxProb = 0;  // 現時点の最大確率を保存する一時変数
        string maxTag = "";

        foreach (var prediction in image_data.predictions)
        {
            if (Convert.ToDouble(prediction.Probability) > maxProb)
            {
                maxTag = prediction.TagName;
                maxProb = Convert.ToDouble(prediction.Probability);
            }
        }
        if (maxProb > threshold)
        {
            log.Info(Convert.ToString(maxProb));
            return maxTag;
        }
        else
        {
            return "failed";
        }
    }
    else
    {
        return "error";
    }

    return "error";
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