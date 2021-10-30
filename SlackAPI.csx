#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
/*
/// <summary>
/// Lineからコンテンツを取得
/// </summary>
/// <returns>Stream</returns>
static async Task<Stream> GetLineContents(string messageId)
{
    Stream responsestream = new MemoryStream();

    // 画像を取得するLine APIを実行
    using (var getContentsClient = new HttpClient())
    {
        //　認証ヘッダーを追加
        getContentsClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("LINE_CHANNEL_ACCESS_TOKEN")}");

        // 非同期でPOST
        var res = await getContentsClient.GetAsync($"https://api-data.line.me/v2/bot/message/{messageId}/content");
        responsestream = await res.Content.ReadAsStreamAsync();
    }

    return responsestream;
}

// <summary>
/// Lineににreplyを送信する
/// </summary>
/// <returns>Stream</returns>
static async Task PutLineReply(Response content, TraceWriter log)
{    
    // JSON形式に変換
    var reqData = JsonConvert.SerializeObject(content);
    
    // レスポンスの作成
    using (var client = new HttpClient())
    {
        // リクエストデータを作成
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.line.me/v2/bot/message/reply");
        request.Content = new StringContent(reqData, Encoding.UTF8, "application/json");

        //　認証ヘッダーを追加
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("LINE_CHANNEL_ACCESS_TOKEN")}");

        // 非同期でPOST
        var res = await client.SendAsync(request);
        log.Info($"{res}");
    }
}
*/
/// <summary>
/// リプライ情報の作成
/// </summary>
/// <param name="token"></param>
/// <param name="translateWord"></param>
/// <param name="log"></param>
/// <returns>response</returns>
static Response CreateResponse(string token,string translateWord,TraceWriter log)
{
    Response res = new Response();
    Messages msg = new Messages();

    // リプライトークンはリクエストに含まれるリプライトークンを使う
    res.replyToken = token;
    res.messages = new List<Messages>();

    // メッセージタイプがtext以外は単一のレスポンス情報とする
    msg.type = "text";
    msg.text = translateWord;
    res.messages.Add(msg);

    return res;
}

// ******************************************************
//　リクエスト
public class Request
{
    public string token { get; set; }
    public string challenge { get; set; }
    public string type { get; set; }
    public Event events { get; set; }
}
public class Event
{
    public string type { get; set; }
    public string channel_id { get; set; }
    public string user_id { get; set; }
    public string file_id { get; set; }
}
public class Source
{
    public string type { get; set; }
    public string userId { get; set; }
}
public class message
{
    public string id { get; set; }
    public string type { get; set; }
    public string text { get; set; }
}
// ******************************************************

// ******************************************************
// レスポンス
public class Response
{
    public string replyToken { get; set; }
    public List<Messages> messages { get; set; }
}

// レスポンスメッセージ
public class Messages
{
    public string type { get; set; }
    public string text { get; set; }
}
// ******************************************************