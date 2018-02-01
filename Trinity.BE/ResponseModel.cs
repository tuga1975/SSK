using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trinity.BE
{
    public class Response<T> : Response
    {
        public T Data { get; set; }

        public Response(int responseCode, string message = null, T data = default(T))
            : base(responseCode, message)
        {
            Data = data;
        }
    }

    public class Response
    {
        public int ResponseCode { get; set; }

        public string ResponseMessage { get; set; }

        public Response(int responseCode, string message = null)
        {
            ResponseCode = responseCode;
            ResponseMessage = message;
        }
    }
    public class ResponseModel
    {
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public object Data { get; set; }
        public int CountData { get; set; }

        public ResponseModel()
        {
            this.ResponseCode = (int)EnumResponseStatuses.ErrorSystem;
            this.ResponseMessage = EnumResponseMessage.ErrorSystem;
            this.Data = null;
            this.CountData = 0;
        }

        public ResponseModel(string responseMessage)
        {
            this.ResponseCode = (int)EnumResponseStatuses.ErrorRequestParam;
            this.ResponseMessage = responseMessage;
            this.Data = null;
            this.CountData = 0;
        }

        public ResponseModel(int responseCode, string responseMessage)
        {
            this.ResponseCode = responseCode;
            this.ResponseMessage = responseMessage;
            this.Data = null;
            this.CountData = 0;
        }
    }

    public class ResponseTypeModel<ObjectModel>
        where ObjectModel : class
    {
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public ObjectModel Data { get; set; }
        public int CountData { get; set; }
    }
}