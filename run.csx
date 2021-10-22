#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"

#load "customvision.csx"
#load "LineAPI.csx"
#load "AzureStorageController.csx"

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.IO;
using System.Xml;
using Newtonsoft.Json;

/// <summary>
/// メインメソッド
/// </summary>
/// <param name="req"></param>
/// <param name="log"></param>
/// <returns></returns>
public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Start");

    // リクエストJSONをパース
    string jsonContent = await req.Content.ReadAsStringAsync();
    Request data = JsonConvert.DeserializeObject<Request>(jsonContent);

    string replyToken = null;
    string messageType = null;
    string messageId = null;

    string fileName = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/"　+ DateTime.Now.Day.ToString() + "/" + Guid.NewGuid().ToString();
    string containerName = "contents";

    Response content;
    Stream responsestream = new MemoryStream();

    // リクエストデータからデータを取得
    foreach (var item in data.events)
    {
        // リプライデータ送付時の認証トークンを取得
        replyToken = item.replyToken.ToString();
        if (item.message != null)
        {
            // メッセージタイプを取得
            messageType = item.message.type.ToString();
            messageId = item.message.id.ToString();
        }
    }
    log.Info(messageType);
    log.Info(messageId);

    if (messageType == "image")
    {    
        // Lineから指定MessageIdの画像を再取得
        responsestream = await GetLineContents(messageId);

        // ComputerVisionAPIにリクエストを送る
        var visionResponse = await GetVisionData(responsestream);

        // 文字列をパース
        var words = await GetParseString(visionResponse,log); 

        // リプライデータの作成
        content = CreateResponse(replyToken, words, log);
//        content = CreateResponse(replyToken, "うけつけました", log);
    }
    else
    {
        // リプライデータの作成
        content = CreateResponse(replyToken, "画像を送信してね!", log);
    }

    // Line ReplyAPIにリクエスト
    await PutLineReply(content, log);

    // ここは失敗してもいいのでtryしとく
    try
    {
        // Lineから指定MessageIdの画像を取得
        responsestream = await GetLineContents(messageId);
        // 取得した画像をAzure Storageに保存
        //await PutLineContentsToStorageAsync(responsestream,containerName, fileName);
    }
    catch{}

    return req.CreateResponse(HttpStatusCode.OK);
}