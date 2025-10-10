using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Order.Common.Models.Responses
{
    public class InternalServiceResponse<T>
    {
        [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
        [JsonPropertyName("data")] public T? Data { get; set; }
    }
}
