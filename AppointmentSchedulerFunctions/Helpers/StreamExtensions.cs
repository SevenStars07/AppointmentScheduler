using Newtonsoft.Json;

namespace AppointmentSchedulerFunctions.Helpers;

public static class StreamExtensions
{
    public static async Task<T> Deserialize<T>(this Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var text = await reader.ReadToEndAsync();
        stream.Position = 0;
        return JsonConvert.DeserializeObject<T>(text);
    }
}