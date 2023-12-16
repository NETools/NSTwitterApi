using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NSTwitterApi.Models.Api
{
    public enum ApiStatusCode
    {
        Ok,
        Failed,
        NotDone,
        AuthFailed,
    }
    public class APIResponse
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public ApiStatusCode ApiStatus { get; set; }
        public string? Data { get; set; }
    }
}
