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

    Stream responseStream = new MemoryStream();

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
                getContentsClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("SLACK_API_TOKEN")}");
                var url = "https://slack.com/api/files.sharedPublicURL?file=" + file_id;

                // 非同期でGET
                var res = await getContentsClient.PostAsync(url, new StringContent(""));
                string str = await res.Content.ReadAsStringAsync();

                var body = JsonConvert.DeserializeObject<FileResponse>(str);

                if (body.ok)
                {
                    log.Info(body.file.url_private);
                    var imageResponse = await getContentsClient.GetAsync(body.file.url_private);
                    responseStream = await imageResponse.Content.ReadAsStreamAsync();

                    // ComputerVisionAPIにリクエストを送る
                    var visionResponse = await GetVisionData(responseStream);

                    // 文字列をパース
                    var tag = await GetMaxProbability(visionResponse,log);

                    if (!tag.Equals("error"))
                    {
                        var minecraftUrl = Environment.GetEnvironmentVariable("MINECRAFT_FUNCTIONS_URL") + tag;
                        var minecraftResponse = await getContentsClient.GetAsync(minecraftUrl);
                        string result = await minecraftResponse.Content.ReadAsStringAsync();
                        log.Info(result);
                        
                        var webhookUrl = Environment.GetEnvironmentVariable("SLACK_WEBHOOK_URL");
                        var payload = new Payload
                        {
                            text = result,
                        };
                        var json = JsonConvert.SerializeObject(payload);
                        var client = new HttpClient();
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        
                        var r = await client.PostAsync(webhookUrl, content);
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

    return req.CreateResponse(HttpStatusCode.OK);
}