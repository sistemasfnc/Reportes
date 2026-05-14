using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace SendCompensarPDF
{
    public class FtpState
    {
        private ManualResetEvent wait;
        private FtpWebRequest request;
        private string fileName;
        private Exception operationException = null;
        string status;

        public FtpState()
        {
            wait = new ManualResetEvent(false);
        }

        public ManualResetEvent OperationComplete
        {
            get { return wait; }
        }

        public FtpWebRequest Request
        {
            get { return request; }
            set { request = value; }
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public Exception OperationException
        {
            get { return operationException; }
            set { operationException = value; }
        }

        public string StatusDescription
        {
            get { return status; }
            set { status = value; }
        }
    }
}
