using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aptos.Exceptions
{
    public class ApiException : BaseException
    {
        private const int SERIALIZED_PAYLOAD_TRIM_TO_MAX_LENGTH = 400;

        public readonly string Url;

        public readonly int Status;

        public readonly string StatusText;

        /// <summary>
        /// The error object returned by the API. This can be a JObject or a string.
        /// </summary>
        public new readonly dynamic Data;

        public readonly AptosRequest Request;

        public ApiException(ApiType type, AptosRequest request, AptosResponse<dynamic> response)
            : base(DeriveErrorMessage(type, request, response))
        {
            Url = request.Url;
            Status = response.Status;
            StatusText = response.StatusText;
            Data = response.Data;
            Request = request;
        }

        private static string DeriveErrorMessage(
            ApiType type,
            AptosRequest request,
            AptosResponse<dynamic> response
        )
        {
            string? traceId = response.Headers.TryGetValue("traceparent", out string? value)
                ? value?.Split('-')[1]
                : null;
            string? traceIdString = traceId != null ? $"(trace_id:{traceId}) " : "";

            var errorPrelude =
                $"Request to [{type}]: {request.Method} {response.Url ?? request.Url} {traceIdString}failed with";

            // TODO: Add more specific error messages

            return $"{errorPrelude} status: {response.StatusText}(code:{response.Status}) and response body: {SerializeAnyPayloadForErrorMessage(response.Data)}";
        }

        private static string SerializeAnyPayloadForErrorMessage(object payload)
        {
            var serializedPayload = JsonConvert.SerializeObject(payload);
            if (serializedPayload.Length <= SERIALIZED_PAYLOAD_TRIM_TO_MAX_LENGTH)
            {
                return serializedPayload;
            }
            return $"truncated(original_size:{serializedPayload.Length}): {serializedPayload.Substring(0, SERIALIZED_PAYLOAD_TRIM_TO_MAX_LENGTH / 2)}...{serializedPayload.Substring(serializedPayload.Length - SERIALIZED_PAYLOAD_TRIM_TO_MAX_LENGTH / 2)}";
        }
    }
}
