using System;
using System.Net.Http;
using System.Threading.Tasks;

private static HttpClient httpClient = new HttpClient();
private static string Url = "https://func-mcrcon-mstc1staniv01.azurewebsites.net/api/SendCommand";

public static async Task Run(TimerInfo myTimer, ILogger log)
{
    // HTTPリクエストと body の取得
    var responce = await httpClient.GetAsync($"{Url}?result=jackolantern");
    var content = await responce.Content.ReadAsStringAsync();

    if(content.Contains("jackolantern"))
    {
        log.LogInformation("ジャック・オー・ランタンを設置したよ！");
    }
    else if(content.Contains("pumpkin"))
    {
        log.LogInformation("かぼちゃを設置したよ！");
    }
    else
    {
        log.LogInformation("それは...かぼちゃではないのでは？");
    }
}