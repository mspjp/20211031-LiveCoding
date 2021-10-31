#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

// ******************************************************
//　リクエスト
public class Request
{
    public string token { get; set; }
    public string challenge { get; set; }
    public string type { get; set; }
    public Event @event { get; set; }
}
public class Event
{
    public string type { get; set; }
    public string file_id { get; set; }
}
public class File
{
    public string filetype { get; set; }
    public string url_private { get; set; }
    public string permalink_public { get; set; }
}
public class FileResponse
{
    public bool ok { get; set; }
    public File file { get; set; }
}
// ******************************************************

// ******************************************************
// レスポンス
public class Payload
{
    public string text { get; set; }
}
// ******************************************************