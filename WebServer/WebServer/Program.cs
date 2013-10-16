using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

class WebServer
{
    HttpListener _listener;
    string _baseFolder;

    public WebServer(string uriPrefix, string baseFolder)
    {
        System.Threading.ThreadPool.SetMaxThreads(50, 1000);
        System.Threading.ThreadPool.SetMinThreads(50, 50);
        _listener = new HttpListener();
        _listener.Prefixes.Add(uriPrefix);
        _baseFolder = baseFolder;
    }

    public void Start()
    {
        _listener.Start();
        while (true)
            try
            {
                HttpListenerContext request = _listener.GetContext();
                ThreadPool.QueueUserWorkItem(ProcessRequest, request);
            }
            catch (HttpListenerException) { break; }
            catch (InvalidOperationException) { break; }
    }

    public void Stop() { _listener.Stop(); }

    void ProcessRequest(object listenerContext)
    {
        try
        {
            var context = (HttpListenerContext)listenerContext;
            string filename = Path.GetFileName(context.Request.RawUrl);
            string path = Path.Combine(_baseFolder, filename);
            byte[] msg;
            if (!File.Exists(path))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                msg = Encoding.UTF8.GetBytes("Sorry, that page does not exist");
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                msg = File.ReadAllBytes(path);
            }
            context.Response.ContentLength64 = msg.Length;
            using (Stream s = context.Response.OutputStream)
                s.Write(msg, 0, msg.Length);
        }
        catch (Exception ex) { Console.WriteLine("Request error: " + ex); }
    }
}

class Program
{
    static void Main(string[] args)
    {
        string path = Directory.GetCurrentDirectory();
        path = path + "/root/";
        //Console.WriteLine(path);
        WebServer ws = new WebServer("http://localhost:8080/", path);
        ws.Start();
        ws.Stop();
    }

}