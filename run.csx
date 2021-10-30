#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"

#load "customvision.csx"
#load "SlackAPI.csx"
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

    Stream responsestream = new MemoryStream();

    if (data.type.Equals("url_verification"))
    {
        var challengeResponse = req.CreateResponse(HttpStatusCode.OK);
        challengeResponse.Content = new StringContent(data.challenge);
        return challengeResponse;
    }
    else if (data.type.Equals("event_callback"))
    {
        if (data.@event.type.Equals("file_shared"))
        {
            string file_id = data.@event.file_id;
            log.Info(file_id);
            using (var getContentsClient = new HttpClient())
            {
                //　認証ヘッダーを追加
                //
                getContentsClient.DefaultRequestHeaders.Add("Authorization", "Bearer xoxp-1369928543665-1356135721381-2680199851281-31f824e31c4e59c21a6d3a0eacd8f073");
                var url = "https://slack.com/api/files.sharedPublicURL?file=" + file_id;
                
                //var message = new HttpRequestMessage(HttpMethod.Post, url);
                //message.Headers.Add("Authorization", "Bearer xoxp-1369928543665-1356135721381-2680199851281-31f824e31c4e59c21a6d3a0eacd8f073");
                //message.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                // 非同期でGET
                var res = await getContentsClient.PostAsync(url, new StringContent(""));
                string str = await res.Content.ReadAsStringAsync();

                //var res = await getContentsClient.GetAsync(url)
                var body = JsonConvert.DeserializeObject<FileResponse>(str);

                if (body.ok)
                {
                    log.Info(body.file.url_private);
                    var imageResponse = await getContentsClient.GetAsync(body.file.url_private);
                    responsestream = await imageResponse.Content.ReadAsStreamAsync();

                    // ComputerVisionAPIにリクエストを送る
                    var visionResponse = await GetVisionData(responsestream);

                    // 文字列をパース
                    var tag = await GetMaxProbability(visionResponse,log);

                    if (!tag.Equals("error"))
                    {
                        var minecraftUrl = "https://func-mcrcon-mstc1staniv01.azurewebsites.net/api/SendCommand?result=" + tag;
                        var minecraftResponse = await getContentsClient.GetAsync(minecraftUrl);
                        string result = await minecraftResponse.Content.ReadAsStringAsync();
                        log.Info(result);
                        
                        url = "https://hooks.slack.com/services/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";

                        var payload = new Payload
                        {
                            text = result,
                        };

                        var json = JsonConvert.SerializeObject(payload);

                        var client = new HttpClient();
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var r = await client.PostAsync(url, content);
                        string s = await r.Content.ReadAsStringAsync();
                        log.Info(s);
                    }
                }
            }
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
    else
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.Content = new StringContent("Bad request");
        return response;
    }
/*
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
*/
    return req.CreateResponse(HttpStatusCode.OK);
}