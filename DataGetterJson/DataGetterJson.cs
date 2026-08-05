#nullable enable
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace cAlgo.Robots;

[Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
public partial class DataGetterJson : Robot
{
    [Parameter("Save folder", DefaultValue = @"C:\Users\vpino\Documents\cTrader\CandleData")]
    public string SaveFolder { get; set; } = string.Empty;

    private StreamWriter? _writer;
    private string _filePath = string.Empty;
    private readonly HashSet<string> _writtenKeys = new(StringComparer.Ordinal);

    protected override void OnStart()
    {
        var folder = string.IsNullOrWhiteSpace(SaveFolder)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "cTrader", "CandleData")
            : SaveFolder.Trim();

        Directory.CreateDirectory(folder);

        var safeSymbol = string.Join("_", SymbolName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        _filePath = Path.Combine(folder, $"{safeSymbol}_{TimeFrame}.jsonl");
        _writer = new StreamWriter(new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read), new System.Text.UTF8Encoding(false))
        {
            AutoFlush = true
        };

        // Export all bars currently available, including historical bars loaded by cTrader.
        for (var index = 0; index < Bars.Count - 1; index++)
            WriteBar(index);

        Print("Writing candle data to: {0}", _filePath);
    }

    protected override void OnBar()
    {
        // OnBar fires when a new candle opens; the previous candle is now complete.
        WriteBar(Bars.Count - 2);
    }

    private void WriteBar(int index)
    {
        if (_writer == null || index < 0 || index >= Bars.Count - 1)
            return;

        var utc = DateTime.SpecifyKind(Bars.OpenTimes[index], DateTimeKind.Utc);
        var key = $"{SymbolName}|{TimeFrame}|{utc.Ticks}";
        if (!_writtenKeys.Add(key))
            return;

        var sgt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
            new DateTimeOffset(utc), "Singapore Standard Time");

        var candle = new CandleRecord
        {
            OpenTimeUtc = utc.ToString("O", CultureInfo.InvariantCulture),
            OpenTimeSgt = sgt.ToString("O", CultureInfo.InvariantCulture),
            Open = Bars.OpenPrices[index],
            High = Bars.HighPrices[index],
            Low = Bars.LowPrices[index],
            Close = Bars.ClosePrices[index],
            TickVolume = Bars.TickVolumes[index]
        };

        _writer.WriteLine(JsonSerializer.Serialize(candle));
    }

    protected override void OnStop()
    {
        _writer?.Flush();
        _writer?.Dispose();
        _writer = null;
    }

    private sealed class CandleRecord
    {
        [JsonPropertyName("open_time_utc")] public string OpenTimeUtc { get; init; } = string.Empty;
        [JsonPropertyName("open_time_sgt")] public string OpenTimeSgt { get; init; } = string.Empty;
        [JsonPropertyName("open")] public double Open { get; init; }
        [JsonPropertyName("high")] public double High { get; init; }
        [JsonPropertyName("low")] public double Low { get; init; }
        [JsonPropertyName("close")] public double Close { get; init; }
        [JsonPropertyName("tick_volume")] public double TickVolume { get; init; }
    }
}
