using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NSTwitterApi.Models.Api
{
    internal class JsonResponse
    {
        private HttpResponseMessage _response;
        public HttpStatusCode StatusCode { get; private set; }
        public string Json { get; private set; }
        public Dictionary<string, IEnumerable<string>> ResponseHeader { get; private set; }
        public JsonResponse(HttpResponseMessage response)
        {
            _response = response;
            StatusCode = _response.StatusCode;
            ResponseHeader = _response.Headers.ToDictionary(p => p.Key, p => p.Value);
        }

        public async Task Read()
        {
            Json = await _response.Content.ReadAsStringAsync();
        }
    }
}
