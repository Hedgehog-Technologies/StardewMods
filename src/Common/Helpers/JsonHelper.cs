using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HedgeTech.Common.Helpers
{
	// Taken from SMAPI's implementation
	// https://github.com/Pathoschild/SMAPI/blob/develop/src/SMAPI.Toolkit/Serialization/JsonHelper.cs
	public class JsonHelper
	{
		public JsonSerializerSettings JsonSettings { get; } = CreateDefaultSettings();

		private static JsonSerializerSettings CreateDefaultSettings()
		{
			return new()
			{
				Formatting = Formatting.Indented,
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				Converters = new List<JsonConverter>
				{
					new StringEnumConverter()
				}
			};
		}

		public bool ReadJsonFileIfExists<TModel>(string fullPath, [NotNullWhen(true)] out TModel? result)
		{
			if (string.IsNullOrWhiteSpace(fullPath))
				throw new ArgumentException("The file path is empty or invalid.", nameof(fullPath));

			string json;
			try
			{
				json = File.ReadAllText(fullPath);
			}
			catch (Exception ex) when (ex is DirectoryNotFoundException or FileNotFoundException)
			{
				result = default;
				return false;
			}

			try
			{
				result = this.Deserialize<TModel>(json);
				return result is not null;
			}
			catch (Exception ex)
			{
				string error = $"Can't parse JSON file at {fullPath}.";

				if (ex is JsonReaderException)
				{
					error += " This doesn't seem to be valid JSON.";
					if (json.Contains("“") || json.Contains("”"))
						error += " Found curly quotes in the text; note that only straight quotes are allowed in JSON.";
				}

				error += $"\nTechnical details: {ex.Message}";
				throw new JsonReaderException(error);
			}
		}

		public void WriteJsonFile<TModel>(string fullPath, TModel model)
			where TModel : class
		{
			if (string.IsNullOrWhiteSpace(fullPath))
				throw new ArgumentException("The file path is empty or invalid.", nameof(fullPath));

			string dir = Path.GetDirectoryName(fullPath)!;
			if (dir is null)
				throw new ArgumentException("The file path is invalid.", nameof(fullPath));
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			string json = this.Serialize(model);
			File.WriteAllText(fullPath, json);
		}

		public TModel? Deserialize<TModel>(string json)
		{
			try
			{
				return JsonConvert.DeserializeObject<TModel>(json, this.JsonSettings);
			}
			catch (JsonReaderException)
			{
				if (json.Contains("“") || json.Contains("”"))
				{
					try
					{
						return JsonConvert.DeserializeObject<TModel>(json.Replace('“', '"').Replace('”', '"'), this.JsonSettings)
							?? throw new InvalidOperationException($"Couldn't deserialize model type '{typeof(TModel)}' from empty or null JSON.");
					}
					catch { }
				}

				throw;
			}
		}

		public string Serialize<TModel>(TModel model, Formatting formatting = Formatting.Indented)
		{
			return JsonConvert.SerializeObject(model, formatting, this.JsonSettings);
		}

		public JsonSerializer GetSerializer()
		{
			return JsonSerializer.CreateDefault(this.JsonSettings);
		}
	}
}
