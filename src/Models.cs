using System;
using System.Collections.Generic;
using System.Linq;
using Honeycomb.Models;

namespace Honeycomb.AppService;

public class IngestRecords<T>
{
    public List<T> records { get; set; }
}

public class HttpLogRecord
{
    public string category { get; set; }
    public DateTime time { get; set; }
    public string resourceId { get; set; }
    public string properties { get; set; }
    public string EventStampType { get; set; }
    public string EventPrimaryStampName { get; set; }
    public string EventStampName { get; set; }
    public string Host { get; set; }
    public string EventIpAddress { get; set; }
}

public class HttpLogProperties
{
    public string UserAgent { get; set; }
    public string Cookie { get; set; }
    public string ScStatus { get; set; }
    public string CsUsername { get; set; }
    public string Result { get; set; }
    public string CsHost { get; set; }
    public string CsMethod { get; set; }
    public string CsBytes { get; set; }
    public string CsUriQuery { get; set; }
    public string CIp { get; set; }
    public string SPort { get; set; }
    public string Referer { get; set; }
    public string CsUriStem { get; set; }
    public int TimeTaken { get; set; }
    public string ScBytes { get; set; }
    public string ComputerName { get; set; }
}

public static class HoneycombEventExtensions
{
    public static void ApplyLogLineProperties(this HoneycombEvent ev, HttpLogProperties properties)
    {
        var urlBuilder = new UriBuilder("http", properties.CsHost);
        if (!string.IsNullOrEmpty(properties.CsUriQuery))
            urlBuilder.Query = properties.CsUriQuery;
        urlBuilder.Path = properties.CsUriStem;
        if (properties.SPort != "80" &&
            int.TryParse(properties.SPort, out var port))
            urlBuilder.Port = port;

        ev.Data.Add("duration_ms", properties.TimeTaken);
        ev.Data.Add("http.url", urlBuilder.ToString());
        ev.Data.Add("http.port", properties.SPort);
        ev.Data.Add("http.status_code", int.Parse(properties.ScStatus));
        ev.Data.Add("net.peer.ip", properties.CIp);
        ev.Data.Add("http.host", properties.CsHost);
        ev.Data.Add("http.method", properties.CsMethod);
        ev.Data.Add("http.user_agent", properties.UserAgent);
        ev.Data.Add("http.result", properties.Result);
        ev.Data.Add("http.referer", properties.Referer);
    }
    
    public static void ApplyAzureResourceProperties(this HoneycombEvent ev, HttpLogRecord logRecord)
    {
        var segments = logRecord.resourceId.Split("/");
        ev.Data.Add("azure.subscription", segments[2]);
        ev.Data.Add("azure.resource_group", segments[4]);
        ev.Data.Add("azure.appservice_name", segments.Last());
    }
}