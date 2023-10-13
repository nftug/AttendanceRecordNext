using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Text.Json;

namespace Presentation.Models;

// Reference: https://qiita.com/kobayashi_stmn/items/7de42805eba009deebaa

internal abstract class NamedPipeBase
{
    public static readonly string PipeName = "1809e0d4-4692-4c25-abd1-81b4766328d6";
}

internal class NamedPipeServer : NamedPipeBase
{
    public static async Task ReceiveMessageAsync(Action<NamedPipeMessage?> action)
    {
        while (true)
        {
            using var stream = new NamedPipeServerStream(PipeName);
            await stream.WaitForConnectionAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            System.Windows.Application.Current.Dispatcher.Invoke(() => action(NamedPipeMessage.Deserialize(json)));
        }
    }
}

internal class NamedPipeClient : NamedPipeBase
{
    public static async Task<bool> SendMessageAsync(NamedPipeMessage message)
    {
        using var stream = new NamedPipeClientStream(PipeName);
        await stream.ConnectAsync(3000);

        using var writer = new StreamWriter(stream);

        var json = message.Serialize();
        Debug.WriteLine($"SendMessageAsync: {json}");
        await writer.WriteAsync(json);

        return true;
    }
}

internal record NamedPipeMessage
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public string Sender { get; private set; }
    public string Text { get; private set; }

    public NamedPipeMessage(string sender, string text)
    {
        Sender = sender;
        Text = text;
    }

    public string Serialize() => JsonSerializer.Serialize(this, JsonOptions);

    public static NamedPipeMessage? Deserialize(string? serialized)
    {
        try
        {
            if (serialized is not { Length: > 0 })
                return null;

            return JsonSerializer.Deserialize<NamedPipeMessage>(serialized, JsonOptions);
        }
        catch (JsonException e)
        {
            Debug.WriteLine(e);
        }
        return null;
    }
}