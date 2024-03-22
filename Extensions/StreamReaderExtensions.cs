namespace LinguacApi.Extensions
{
	public static class StreamReaderExtensions
	{
		public static async IAsyncEnumerable<string> ReadLinesAsync(this StreamReader reader)
		{
			while (!reader.EndOfStream)
			{
				yield return (await reader.ReadLineAsync())!;
			}
		}

		public class ServerSideEvent
		{
			public string? EventType { get; set; }
			public string? Data { get; set; }
			public string? Id { get; set; }
			public int? Retry { get; set; }
		}

		public static async IAsyncEnumerable<ServerSideEvent> ReadServerSideEvents(this StreamReader reader)
		{
			ServerSideEvent? currentEvent = null;

			await foreach (var line in reader.ReadLinesAsync())
			{
				if (string.IsNullOrEmpty(line))
				{
					if (currentEvent?.Data is not null)
					{
						yield return currentEvent;
						currentEvent = null;
					}
					continue;
				}

				if (line.StartsWith("event: "))
				{
					currentEvent ??= new ServerSideEvent();
					currentEvent.EventType = line[7..];
					continue;
				}

				if (line.StartsWith("data: "))
				{
					currentEvent ??= new ServerSideEvent();
					currentEvent.Data = (currentEvent.Data ?? string.Empty) + line[6..] + Environment.NewLine;
					continue;
				}

				if (line.StartsWith("id: "))
				{
					currentEvent ??= new ServerSideEvent();
					currentEvent.Id = line[4..];
					continue;
				}

				if (line.StartsWith("retry: "))
				{
					currentEvent ??= new ServerSideEvent();
					if (int.TryParse(line[7..], out var retry))
					{
						currentEvent.Retry = retry;
					}
				}
			}

			if (currentEvent?.Data is not null)
			{
				yield return currentEvent;
			}
		}
	}
}
