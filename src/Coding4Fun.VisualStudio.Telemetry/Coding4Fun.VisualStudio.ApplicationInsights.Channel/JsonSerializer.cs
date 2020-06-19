using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Platform;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Serializes and compress the telemetry items into a JSON string. Compression will be done using GZIP, for Windows Phone 8 compression will be disabled because there
	/// is API support for it.
	/// </summary>
	internal static class JsonSerializer
	{
		private static readonly UTF8Encoding TransmissionEncoding = new UTF8Encoding(false);

		internal static string CompressionType => "gzip";

		/// <summary>
		/// Serializes and compress the telemetry items into a JSON string. Each JSON object is separated by a new line.
		/// </summary>
		/// <param name="telemetryItems">The list of telemetry items to serialize.</param>
		/// <param name="compress">Should serialization also perform compression.</param>
		/// <returns>The compressed and serialized telemetry items.</returns>
		internal static byte[] Serialize(IEnumerable<ITelemetry> telemetryItems, bool compress = true)
		{
			MemoryStream memoryStream = new MemoryStream();
			using (Stream stream = compress ? CreateCompressedStream(memoryStream) : memoryStream)
			{
				using (StreamWriter streamWriter = new StreamWriter(stream, TransmissionEncoding))
				{
					SeializeToStream(telemetryItems, streamWriter);
				}
			}
			return memoryStream.ToArray();
		}

		/// <summary>
		///  Serialize and compress a telemetry item.
		/// </summary>
		/// <param name="telemetryItem">A telemetry item.</param>
		/// <param name="compress">Should serialization also perform compression.</param>
		/// <returns>The compressed and serialized telemetry item.</returns>
		internal static byte[] Serialize(ITelemetry telemetryItem, bool compress = true)
		{
			return Serialize(new ITelemetry[1]
			{
				telemetryItem
			}, compress);
		}

		/// <summary>
		/// Serializes <paramref name="telemetryItems" /> into a JSON string. Each JSON object is separated by a new line.
		/// </summary>
		/// <param name="telemetryItems">The list of telemetry items to serialize.</param>
		/// <returns>A JSON string of all the serialized items.</returns>
		internal static string SerializeAsString(IEnumerable<ITelemetry> telemetryItems)
		{
			StringBuilder stringBuilder = new StringBuilder();
			using (StringWriter streamWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
			{
				SeializeToStream(telemetryItems, streamWriter);
				return stringBuilder.ToString();
			}
		}

		/// <summary>
		/// Serializes a <paramref name="telemetry" /> into a JSON string.
		/// </summary>
		/// <param name="telemetry">The telemetry to serialize.</param>
		/// <returns>A JSON string of the serialized telemetry.</returns>
		internal static string SerializeAsString(ITelemetry telemetry)
		{
			return SerializeAsString(new ITelemetry[1]
			{
				telemetry
			});
		}

		private static void ConvertExceptionTree(Exception exception, ExceptionDetails parentExceptionDetails, List<ExceptionDetails> exceptions)
		{
			if (exception == null)
			{
				exception = new Exception(Utils.PopulateRequiredStringValue(null, "message", typeof(ExceptionTelemetry).FullName));
			}
			ExceptionDetails exceptionDetails = PlatformSingleton.Current.GetExceptionDetails(exception, parentExceptionDetails);
			exceptions.Add(exceptionDetails);
			AggregateException ex = exception as AggregateException;
			if (ex != null)
			{
				foreach (Exception innerException in ex.InnerExceptions)
				{
					ConvertExceptionTree(innerException, exceptionDetails, exceptions);
				}
			}
			else if (exception.InnerException != null)
			{
				ConvertExceptionTree(exception.InnerException, exceptionDetails, exceptions);
			}
		}

		private static void SerializeExceptions(IEnumerable<ExceptionDetails> exceptions, IJsonWriter writer)
		{
			int num = 0;
			foreach (ExceptionDetails exception in exceptions)
			{
				if (num++ != 0)
				{
					writer.WriteComma();
				}
				writer.WriteStartObject();
				writer.WriteProperty("id", exception.id);
				if (exception.outerId != 0)
				{
					writer.WriteProperty("outerId", exception.outerId);
				}
				writer.WriteProperty("typeName", Utils.PopulateRequiredStringValue(exception.typeName, "typeName", typeof(ExceptionTelemetry).FullName));
				writer.WriteProperty("message", Utils.PopulateRequiredStringValue(exception.message, "message", typeof(ExceptionTelemetry).FullName));
				if (exception.hasFullStack)
				{
					writer.WriteProperty("hasFullStack", exception.hasFullStack);
				}
				writer.WriteProperty("stack", exception.stack);
				if (exception.parsedStack.Count > 0)
				{
					writer.WritePropertyName("parsedStack");
					writer.WriteStartArray();
					int num2 = 0;
					foreach (StackFrame item in exception.parsedStack)
					{
						if (num2++ != 0)
						{
							writer.WriteComma();
						}
						writer.WriteStartObject();
						SerializeStackFrame(item, writer);
						writer.WriteEndObject();
					}
					writer.WriteEndArray();
				}
				writer.WriteEndObject();
			}
		}

		private static void SerializeStackFrame(StackFrame frame, IJsonWriter writer)
		{
			writer.WriteProperty("level", frame.level);
			writer.WriteProperty("method", Utils.PopulateRequiredStringValue(frame.method, "StackFrameMethod", typeof(ExceptionTelemetry).FullName));
			writer.WriteProperty("assembly", frame.assembly);
			writer.WriteProperty("fileName", frame.fileName);
			if (frame.line != 0)
			{
				writer.WriteProperty("line", frame.line);
			}
		}

		/// <summary>
		/// Creates a GZIP compression stream that wraps <paramref name="stream" />. For windows phone 8.0 it returns <paramref name="stream" />.
		/// </summary>
		/// <returns></returns>
		private static Stream CreateCompressedStream(Stream stream)
		{
			return new GZipStream(stream, CompressionMode.Compress);
		}

		private static void SerializeTelemetryItem(ITelemetry telemetryItem, JsonWriter jsonWriter)
		{
			if (telemetryItem is EventTelemetry)
			{
				SerializeEventTelemetry(telemetryItem as EventTelemetry, jsonWriter);
				return;
			}
			if (telemetryItem is ExceptionTelemetry)
			{
				SerializeExceptionTelemetry(telemetryItem as ExceptionTelemetry, jsonWriter);
				return;
			}
			if (telemetryItem is MetricTelemetry)
			{
				SerializeMetricTelemetry(telemetryItem as MetricTelemetry, jsonWriter);
				return;
			}
			if (telemetryItem is PageViewTelemetry)
			{
				SerializePageViewTelemetry(telemetryItem as PageViewTelemetry, jsonWriter);
				return;
			}
			if (telemetryItem is RemoteDependencyTelemetry)
			{
				SerializeRemoteDependencyTelemetry(telemetryItem as RemoteDependencyTelemetry, jsonWriter);
				return;
			}
			if (telemetryItem is RequestTelemetry)
			{
				SerializeRequestTelemetry(telemetryItem as RequestTelemetry, jsonWriter);
				return;
			}
			if (telemetryItem is SessionStateTelemetry)
			{
				SerializeSessionStateTelemetry(telemetryItem as SessionStateTelemetry, jsonWriter);
				return;
			}
			if (telemetryItem is TraceTelemetry)
			{
				SerializeTraceTelemetry(telemetryItem as TraceTelemetry, jsonWriter);
				return;
			}
			if (telemetryItem is PerformanceCounterTelemetry)
			{
				SerializePerformanceCounter(telemetryItem as PerformanceCounterTelemetry, jsonWriter);
				return;
			}
			string message = string.Format(CultureInfo.InvariantCulture, "Unknown telemtry type: {0}", new object[1]
			{
				telemetryItem.GetType()
			});
			CoreEventSource.Log.LogVerbose(message);
		}

		/// <summary>
		/// Serializes <paramref name="telemetryItems" /> and write the response to <paramref name="streamWriter" />.
		/// </summary>
		private static void SeializeToStream(IEnumerable<ITelemetry> telemetryItems, TextWriter streamWriter)
		{
			JsonWriter jsonWriter = new JsonWriter(streamWriter);
			int num = 0;
			foreach (ITelemetry telemetryItem in telemetryItems)
			{
				if (num++ > 0)
				{
					streamWriter.Write(Environment.NewLine);
				}
				SerializeTelemetryItem(telemetryItem, jsonWriter);
			}
		}

		private static void SerializeEventTelemetry(EventTelemetry eventTelemetry, JsonWriter writer)
		{
			writer.WriteStartObject();
			eventTelemetry.WriteTelemetryName(writer, "Event");
			eventTelemetry.WriteEnvelopeProperties(writer);
			writer.WritePropertyName("data");
			writer.WriteStartObject();
			writer.WriteProperty("baseType", eventTelemetry.BaseType);
			writer.WritePropertyName("baseData");
			writer.WriteStartObject();
			writer.WriteProperty("ver", eventTelemetry.Data.ver);
			writer.WriteProperty("name", eventTelemetry.Data.name);
			writer.WriteProperty("measurements", eventTelemetry.Data.measurements);
			writer.WriteProperty("properties", eventTelemetry.Data.properties);
			writer.WriteEndObject();
			writer.WriteEndObject();
			writer.WriteEndObject();
		}

		private static void SerializeExceptionTelemetry(ExceptionTelemetry exceptionTelemetry, JsonWriter writer)
		{
			writer.WriteStartObject();
			exceptionTelemetry.WriteTelemetryName(writer, "Exception");
			exceptionTelemetry.WriteEnvelopeProperties(writer);
			writer.WritePropertyName("data");
			writer.WriteStartObject();
			writer.WriteProperty("baseType", exceptionTelemetry.BaseType);
			writer.WritePropertyName("baseData");
			writer.WriteStartObject();
			writer.WriteProperty("ver", exceptionTelemetry.Data.ver);
			writer.WriteProperty("handledAt", Utils.PopulateRequiredStringValue(exceptionTelemetry.Data.handledAt, "handledAt", typeof(ExceptionTelemetry).FullName));
			writer.WriteProperty("properties", exceptionTelemetry.Data.properties);
			writer.WriteProperty("measurements", exceptionTelemetry.Data.measurements);
			writer.WritePropertyName("exceptions");
			writer.WriteStartArray();
			SerializeExceptions(exceptionTelemetry.Exceptions, writer);
			writer.WriteEndArray();
			if (exceptionTelemetry.Data.severityLevel.HasValue)
			{
				writer.WriteProperty("severityLevel", exceptionTelemetry.Data.severityLevel.Value.ToString());
			}
			writer.WriteEndObject();
			writer.WriteEndObject();
			writer.WriteEndObject();
		}

		private static void SerializeMetricTelemetry(MetricTelemetry metricTelemetry, JsonWriter writer)
		{
			writer.WriteStartObject();
			metricTelemetry.WriteTelemetryName(writer, "Metric");
			metricTelemetry.WriteEnvelopeProperties(writer);
			writer.WritePropertyName("data");
			writer.WriteStartObject();
			writer.WriteProperty("baseType", metricTelemetry.BaseType);
			writer.WritePropertyName("baseData");
			writer.WriteStartObject();
			writer.WriteProperty("ver", metricTelemetry.Data.ver);
			writer.WritePropertyName("metrics");
			writer.WriteStartArray();
			writer.WriteStartObject();
			writer.WriteProperty("name", metricTelemetry.Metric.name);
			writer.WriteProperty("kind", metricTelemetry.Metric.kind.ToString());
			writer.WriteProperty("value", metricTelemetry.Metric.value);
			writer.WriteProperty("count", metricTelemetry.Metric.count);
			writer.WriteProperty("min", metricTelemetry.Metric.min);
			writer.WriteProperty("max", metricTelemetry.Metric.max);
			writer.WriteProperty("stdDev", metricTelemetry.Metric.stdDev);
			writer.WriteEndObject();
			writer.WriteEndArray();
			writer.WriteProperty("properties", metricTelemetry.Data.properties);
			writer.WriteEndObject();
			writer.WriteEndObject();
			writer.WriteEndObject();
		}

		private static void SerializePageViewTelemetry(PageViewTelemetry pageViewTelemetry, JsonWriter writer)
		{
			writer.WriteStartObject();
			pageViewTelemetry.WriteTelemetryName(writer, "PageView");
			pageViewTelemetry.WriteEnvelopeProperties(writer);
			writer.WritePropertyName("data");
			writer.WriteStartObject();
			writer.WriteProperty("baseType", pageViewTelemetry.BaseType);
			writer.WritePropertyName("baseData");
			writer.WriteStartObject();
			writer.WriteProperty("ver", pageViewTelemetry.Data.ver);
			writer.WriteProperty("name", pageViewTelemetry.Data.name);
			writer.WriteProperty("url", pageViewTelemetry.Data.url);
			writer.WriteProperty("duration", pageViewTelemetry.Data.duration);
			writer.WriteProperty("measurements", pageViewTelemetry.Data.measurements);
			writer.WriteProperty("properties", pageViewTelemetry.Data.properties);
			writer.WriteEndObject();
			writer.WriteEndObject();
			writer.WriteEndObject();
		}

		private static void SerializeRemoteDependencyTelemetry(RemoteDependencyTelemetry remoteDependencyTelemetry, JsonWriter writer)
		{
			writer.WriteStartObject();
			remoteDependencyTelemetry.WriteTelemetryName(writer, "RemoteDependency");
			remoteDependencyTelemetry.WriteEnvelopeProperties(writer);
			writer.WritePropertyName("data");
			writer.WriteStartObject();
			writer.WriteProperty("baseType", remoteDependencyTelemetry.BaseType);
			writer.WritePropertyName("baseData");
			writer.WriteStartObject();
			writer.WriteProperty("ver", remoteDependencyTelemetry.Data.ver);
			writer.WriteProperty("name", remoteDependencyTelemetry.Data.name);
			writer.WriteProperty("commandName", remoteDependencyTelemetry.Data.commandName);
			writer.WriteProperty("kind", (int)remoteDependencyTelemetry.Data.kind);
			writer.WriteProperty("value", remoteDependencyTelemetry.Data.value);
			writer.WriteProperty("count", remoteDependencyTelemetry.Data.count);
			writer.WriteProperty("dependencyKind", (int)remoteDependencyTelemetry.Data.dependencyKind);
			writer.WriteProperty("success", remoteDependencyTelemetry.Data.success);
			writer.WriteProperty("async", remoteDependencyTelemetry.Data.async);
			writer.WriteProperty("dependencySource", (int)remoteDependencyTelemetry.Data.dependencySource);
			writer.WriteProperty("properties", remoteDependencyTelemetry.Data.properties);
			writer.WriteEndObject();
			writer.WriteEndObject();
			writer.WriteEndObject();
		}

		private static void SerializeRequestTelemetry(RequestTelemetry requestTelemetry, JsonWriter jsonWriter)
		{
			jsonWriter.WriteStartObject();
			requestTelemetry.WriteTelemetryName(jsonWriter, "Request");
			requestTelemetry.WriteEnvelopeProperties(jsonWriter);
			jsonWriter.WritePropertyName("data");
			jsonWriter.WriteStartObject();
			jsonWriter.WriteProperty("baseType", requestTelemetry.BaseType);
			jsonWriter.WritePropertyName("baseData");
			jsonWriter.WriteStartObject();
			jsonWriter.WriteProperty("ver", requestTelemetry.Data.ver);
			jsonWriter.WriteProperty("id", requestTelemetry.Data.id);
			jsonWriter.WriteProperty("name", requestTelemetry.Data.name);
			jsonWriter.WriteProperty("startTime", requestTelemetry.Timestamp);
			jsonWriter.WriteProperty("duration", requestTelemetry.Duration);
			jsonWriter.WriteProperty("success", requestTelemetry.Data.success);
			jsonWriter.WriteProperty("responseCode", requestTelemetry.Data.responseCode);
			jsonWriter.WriteProperty("url", requestTelemetry.Data.url);
			jsonWriter.WriteProperty("measurements", requestTelemetry.Data.measurements);
			jsonWriter.WriteProperty("httpMethod", requestTelemetry.Data.httpMethod);
			jsonWriter.WriteProperty("properties", requestTelemetry.Data.properties);
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndObject();
		}

		private static void SerializeSessionStateTelemetry(SessionStateTelemetry sessionStateTelemetry, JsonWriter jsonWriter)
		{
			jsonWriter.WriteStartObject();
			sessionStateTelemetry.WriteEnvelopeProperties(jsonWriter);
			sessionStateTelemetry.WriteTelemetryName(jsonWriter, "SessionState");
			jsonWriter.WritePropertyName("data");
			jsonWriter.WriteStartObject();
			jsonWriter.WriteProperty("baseType", typeof(SessionStateData).Name);
			jsonWriter.WritePropertyName("baseData");
			jsonWriter.WriteStartObject();
			jsonWriter.WriteProperty("ver", 2);
			jsonWriter.WriteProperty("state", sessionStateTelemetry.State.ToString());
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndObject();
		}

		private static void SerializeTraceTelemetry(TraceTelemetry traceTelemetry, JsonWriter writer)
		{
			writer.WriteStartObject();
			traceTelemetry.WriteTelemetryName(writer, "Message");
			traceTelemetry.WriteEnvelopeProperties(writer);
			writer.WritePropertyName("data");
			writer.WriteStartObject();
			writer.WriteProperty("baseType", traceTelemetry.BaseType);
			writer.WritePropertyName("baseData");
			writer.WriteStartObject();
			writer.WriteProperty("ver", traceTelemetry.Data.ver);
			writer.WriteProperty("message", traceTelemetry.Message);
			if (traceTelemetry.SeverityLevel.HasValue)
			{
				writer.WriteProperty("severityLevel", traceTelemetry.SeverityLevel.Value.ToString());
			}
			writer.WriteProperty("properties", traceTelemetry.Properties);
			writer.WriteEndObject();
			writer.WriteEndObject();
			writer.WriteEndObject();
		}

		/// <summary>
		/// Serializes this object in JSON format.
		/// </summary>
		private static void SerializePerformanceCounter(PerformanceCounterTelemetry performanceCounter, JsonWriter writer)
		{
			writer.WriteStartObject();
			performanceCounter.WriteTelemetryName(writer, "PerformanceCounter");
			performanceCounter.WriteEnvelopeProperties(writer);
			writer.WritePropertyName("data");
			writer.WriteStartObject();
			writer.WriteProperty("baseType", performanceCounter.BaseType);
			writer.WritePropertyName("baseData");
			writer.WriteStartObject();
			writer.WriteProperty("ver", performanceCounter.Data.ver);
			writer.WriteProperty("categoryName", performanceCounter.Data.categoryName);
			writer.WriteProperty("counterName", performanceCounter.Data.counterName);
			writer.WriteProperty("instanceName", performanceCounter.Data.instanceName);
			writer.WriteProperty("value", performanceCounter.Data.value);
			writer.WriteProperty("properties", performanceCounter.Data.properties);
			writer.WriteEndObject();
			writer.WriteEndObject();
			writer.WriteEndObject();
		}
	}
}
