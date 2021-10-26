#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using MinecraftConnection;

static string address = "hoge.japaneast.cloudapp.azure.com";
static ushort port = 25575;
static string pass = "huga";

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    log.LogInformation("C# HTTP trigger function processed a request.");

    string result = req.Query["result"];

    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    result = result ?? data?.result;

    string responseMessage = null;
    // マイクラコマンドを使うためのインスタンス
    var command = new MinecraftCommands(address, port, pass);
    var rnd = new Random();

    // かぼちゃ設置のための基準座標
    int x = -1605, y = 70, z = 265;
    // 乱数で座標を決める
    x = x + rnd.Next(-20, 21);
    y = y + rnd.Next(0, 21);
    z = z + rnd.Next(-20, 21);
    // ジャックオーランタンの顔の向きを決める
    string facing = RndFacing(rnd.Next(0, 4));

    if(result.Contains("pumpkin"))
    {
        responseMessage = "pumpkin";
        command.SendCommand($"setblock {x} {y} {z} pumpkin destroy");
    }
    else if(result.Contains("jackolantern"))
    {
        responseMessage = "jackolantern";
        command.SendCommand($"setblock {x} {y} {z} jack_o_lantern[facing={facing}] destroy");
    }
    else
    {
        responseMessage = "else";
        command.SendCommand($"setblock {x} {y} {z} air");
    }

    return new OkObjectResult(responseMessage);
}

public static string RndFacing(int val)
{
    switch(val)
    {
        case 0:
            return "north";
        case 1:
            return "south";
        case 2:
            return "west";
        default:
            return "east";
    }
}