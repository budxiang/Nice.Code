﻿
using Nice.Core.Log;
using Nice.Network.Web.Models;
using Nice.Network.Web.Session;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nice.Network.Web
{
    public class HttpListenerHandler
    {
        private readonly string rootPath = null;
        private readonly HttpSessionStore sessionStore = null;
        private const string sessionCookieName = "__nice_sessionid";

        public HttpListenerHandler(string physicalPath)
        {
            rootPath = physicalPath;
            sessionStore = new HttpSessionStore();
        }
        public void ProcessRequest(HttpListenerContext context)
        {
            SetSession(context);
            Task.Run(() =>
            {
                OnProcessRequest(context);
            });
        }

        private void SetSession(HttpListenerContext context)
        {
            if (context.Request.Cookies[sessionCookieName] == null)
            {
                Cookie cookie = new Cookie();
                cookie.Name = sessionCookieName;
                cookie.Value = Guid.NewGuid().ToString("N");
                cookie.Expires = DateTime.Now.AddSeconds(WebSettings.SessionTimeout);
                context.Response.SetCookie(cookie);
                sessionStore.Add(cookie);
                Logging.Info(string.Format("新的HTTP会话{0}", cookie.Value));
            }
        }
        private void OnProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            try
            {
                string absolutePath = request.Url.AbsolutePath;
                if (absolutePath == "/")
                {
                    ResponseDefault(response);
                    return;
                }
                string[] dirs = absolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (dirs.Length == 0)
                {
                    ResponseDefault(response);
                    return;
                }
                string lastStr = dirs.Last();
                int idxext = lastStr.LastIndexOf('.');
                if (idxext > 0)//文件
                {
                    string extension = lastStr.Substring(idxext);
                    string contentType = MimeMapping.Get(extension);
                    string filepath = null;
                    if (contentType == MimeMapping.Html)
                        filepath = FileHelper.PathCombine(rootPath + "\\" + WebSettings.ViewsPath, absolutePath);
                    else
                        filepath = FileHelper.PathCombine(rootPath, absolutePath);
                    if (contentType == null)
                    {
                        HttpResponseExtensions.Write(response, StatusCode.ServiceUnavailable, "不接受当前Mime类型");
                        return;
                    }
                    HttpResponseExtensions.WriteFile(response, filepath, contentType);
                }
                else
                {
                    if (dirs.Length == 1)
                    {
                        HttpResponseExtensions.Write(response, StatusCode.ServiceUnavailable, "路径格式错误");
                        return;
                    }
                    string controllerName = null;
                    if (dirs.Length == 2)
                    {
                        controllerName = dirs[0];
                    }
                    else
                    {
                        StringBuilder route = new StringBuilder(32);
                        for (int i = 0; i < dirs.Length - 1; i++)
                        {
                            route.Append(dirs[i] + "/");
                        }
                        controllerName = route.Remove(route.Length - 1, 1).ToString();
                    }
                    HttpContext httpContext = new HttpContext();
                    Cookie cookie = request.Cookies[sessionCookieName];
                    if (cookie != null)
                    {
                        httpContext.Session = sessionStore.Get(cookie.Value);
                    }
                    httpContext.RemoteEndPoint = request.RemoteEndPoint;
                    ControllerFactory.ExecuteAction(context, httpContext, controllerName, dirs.Last());
                }
            }
            catch (InvalidOperationException ex)
            {
                ResponseException(response, ex);
                Logging.Error(ex);
            }
            catch (Exception ex)
            {
                ResponseException(response, ex);
                Logging.Error(ex);
            }
        }
        private void ResponseDefault(HttpListenerResponse response)
        {
            string filepath = FileHelper.PathCombine(rootPath + "\\" + WebSettings.ViewsPath, WebSettings.DefaultPage);
            HttpResponseExtensions.WriteFile(response, filepath, MimeMapping.Html);
        }
        private void ResponseException(HttpListenerResponse response, Exception ex)
        {
            try
            {
                HttpResponseExtensions.Write(response, StatusCode.InternalServerError, string.Format("调用方法错误{0}/r/nStackTrace:{1}", ex.Message, ex.StackTrace));
            }
            catch (ObjectDisposedException e)
            {
                Logging.Error(e);
            }
            catch (InvalidOperationException e)
            {
                Logging.Error(e);
            }
            catch (Exception e)
            {
                Logging.Error(e);
            }
        }
        public void Close()
        {
            if (sessionStore != null)
                sessionStore.Close();
        }
    }
}