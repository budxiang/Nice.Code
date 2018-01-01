using Nice.Network.Web.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace Nice.Network.Web
{
    public class ControllerFactory
    {
        private readonly static Dictionary<string, ClassController> controllers = new Dictionary<string, ClassController>();
        private readonly static Dictionary<string, MethodAction> methodInfos = new Dictionary<string, MethodAction>();
       
        private static Assembly assembly;
        public static void RegisterRoutes()
        {
            string assemblyName = NiceWebSettings.WebControllerAssembly;
            string namespaceName = NiceWebSettings.WebControllerNamespace;
            IEnumerable<Type> types = null;
            assembly = Assembly.Load(assemblyName);
            if (string.IsNullOrEmpty(namespaceName))
                types = assembly.GetTypes().Where(t => t.IsClass);
            else
                types = assembly.GetTypes().Where(t => t.IsClass && t.Namespace == namespaceName);
            ControllerAttribute attr = null;
            foreach (Type t in types)
            {
                attr = t.GetCustomAttribute<ControllerAttribute>();
                if (attr != null)
                {
                    string path = null;
                    if (string.IsNullOrEmpty(attr.Route))
                        path = t.Name.Substring(0, t.Name.Length - "Controller".Length);
                    else
                        path = attr.Route;
                    FieldInfo context = t.GetField("context", BindingFlags.NonPublic | BindingFlags.Instance);
                    controllers.Add(path, new ClassController(t.FullName, context));
                    SetAction(t, path);
                }
            }
        }
        private static void SetAction(Type t, string route)
        {
            MethodInfo[] methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (MethodInfo method in methods)
            {
                ActionAttribute attr = method.GetCustomAttribute<ActionAttribute>();
                if (attr != null)
                {
                    string actionName = route + "_" + method.Name;
                    if (!string.IsNullOrEmpty(attr.Name))
                        actionName = route + "_" + attr.Name;
                    methodInfos.Add(actionName, new MethodAction(attr.MimeType, method));
                }
            }
        }
        internal static void ExecuteAction(HttpListenerContext context, HttpContext httpContext, string controllerName, string actionName)
        {
            string key = controllerName + "_" + actionName;
            MethodAction action = null;
            if (methodInfos.ContainsKey(key))
                action = methodInfos[key];

            if (action != null)
            {
                ParameterInfo[] pi = action.Method.GetParameters();
                object[] parms = null;
                if (pi.Length > 0)
                    GetParameters(pi, context.Request, ref parms);
                try
                {
                    ClassController clc = controllers[controllerName];
                    object instance = assembly.CreateInstance(clc.FullName);
                    clc.Context.SetValue(instance, httpContext);
                    object result = action.Method.Invoke(instance, parms);
                    HttpResponseExtensions.Write(context.Response, result, action.ContentType);
                }
                catch (TargetInvocationException ex)
                {
                    HttpResponseExtensions.Write(context.Response, StatusCode.ServiceUnavailable, string.Format("调用方法错误{0}.{1}=>{2}/r/nStackTrace:{3}", controllerName, actionName, ex.Message, ex.StackTrace));
                }
            }
            else
            {
                HttpResponseExtensions.Write(context.Response, StatusCode.NotFound, string.Format("未找到方法{0}.{1}", controllerName, actionName));
            }
        }
        private static void GetParameters(ParameterInfo[] parmInfos, HttpListenerRequest request, ref object[] parms)
        {
            Type t = null;
            ParameterInfo pi = null;
            NameValueCollection httpParms = new NameValueCollection();
            GetHttpParameters(request, httpParms);
            parms = new object[parmInfos.Length];
            for (int i = 0; i < parmInfos.Length; i++)
            {
                pi = parmInfos[i];
                string pvalue = httpParms[pi.Name];
                t = pi.ParameterType;
                if (t.Equals(typeof(string)) || (!t.IsInterface && !t.IsClass))//如果它是值类型,或者String   
                {
                    if (string.IsNullOrWhiteSpace(pvalue))
                        parms[i] = t.IsValueType ? Activator.CreateInstance(t) : null;
                    else
                        parms[i] = Convert.ChangeType(pvalue, t);
                }
                else if (t.IsClass)
                {
                    parms[i] = RequestConvertModel(t, httpParms);
                }
            }
        }
        private static void GetHttpParameters(HttpListenerRequest request, NameValueCollection parms)
        {
            parms.Add(request.QueryString);
            if (request.HttpMethod == HttpMethods.Post)
            {
                Stream SourceStream = request.InputStream;
                byte[] currentChunk = ReadLineAsBytes(SourceStream);
                if (currentChunk == null)
                    return;
                string postDatas = Encoding.UTF8.GetString(currentChunk);//.Replace('�', ' ');
                string[] arr = postDatas.Substring(0, postDatas.Length - 1).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in arr)
                {
                    int idx = item.IndexOf('=');
                    string key = item.Substring(0, idx);
                    string val = item.Substring(idx + 1);
                    parms.Add(key, WebUtility.UrlDecode(val));
                }
            }
        }
        private static object RequestConvertModel(Type type, NameValueCollection httpParms)
        {
            PropertyInfo[] properties = type.GetProperties();
            object obj = Activator.CreateInstance(type);
            foreach (PropertyInfo pi in properties)
            {
                string name = pi.Name;
                if (pi.PropertyType.IsArray)
                {
                    Type tele = pi.PropertyType.GetElementType();
                    PropertyInfo[] tprop = tele.GetProperties();
                    int idxArr = 0;
                    int idxp = 0;
                    IList arrList = MakeListOfType(tele);
                    PropertyInfo tp = tprop[idxp];
                    string firstName = tp.Name;
                    string value = null;
                    while (true)
                    {
                        value = httpParms[name + "[" + idxArr + "][" + firstName + "]"];
                        if (value == null) break;
                        object pobj = Activator.CreateInstance(tele);
                        tp = tprop[idxp];
                        SetValueType(pobj, tp, value);
                        idxp++;
                        for (; idxp < tprop.Length; idxp++)
                        {
                            tp = tprop[idxp];
                            if (tp.PropertyType.IsClass && tp.PropertyType != typeof(string))
                            {
                                SetClassPropertyValue(tp, pobj, httpParms, name + "[" + idxArr + "]");
                            }
                            else
                            {
                                value = httpParms[name + "[" + idxArr + "][" + tp.Name + "]"];
                                SetValueType(pobj, tp, value);
                            }
                        }
                        arrList.Add(pobj);
                        idxp = 0;
                        idxArr++;
                    }
                    Array array = Array.CreateInstance(tele, arrList.Count);
                    arrList.CopyTo(array, 0);
                    pi.SetValue(obj, array, null);
                }
                else if (pi.PropertyType.IsClass && pi.PropertyType != typeof(string))
                {
                    SetClassPropertyValue(pi, obj, httpParms, name);
                }
                else
                {
                    string result = httpParms[name];
                    if (!string.IsNullOrEmpty(result))
                    {
                        SetValueType(obj, pi, result);
                    }
                }
            }
            return obj;
        }
        private static void SetValueType(object obj, PropertyInfo pi, string value)
        {
            Type t = Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType;
            object safeValue = (value == null) ? null : Convert.ChangeType(value, t);
            pi.SetValue(obj, safeValue, null);
        }
        private static IList MakeListOfType(Type type)
        {
            return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
        }

        private static void SetClassPropertyValue(PropertyInfo pi, object obj, NameValueCollection httpParms, string name)
        {
            Type tele = pi.PropertyType;
            object pobj = Activator.CreateInstance(tele);
            PropertyInfo[] tprop = tele.GetProperties();
            foreach (PropertyInfo tp in tprop)
            {
                string result = httpParms[name + "[" + pi.Name + "][" + tp.Name + "]"];
                SetValueType(pobj, tp, result);
            }
            pi.SetValue(obj, pobj, null);
        }

        private static byte[] ReadLineAsBytes(Stream SourceStream)
        {
            var resultStream = new MemoryStream();
            while (true)
            {
                int data = SourceStream.ReadByte();
                resultStream.WriteByte((byte)data);
                if (data <= 10)
                    break;
            }
            resultStream.Position = 0;
            byte[] dataBytes = new byte[resultStream.Length];
            resultStream.Read(dataBytes, 0, dataBytes.Length);
            return dataBytes;
        }
    }
    internal class MethodAction
    {
        public MethodAction(MimeType contentType, MethodInfo method)
        {
            this.contentType = contentType;
            this.method = method;
        }
        private MimeType contentType;
        public MimeType ContentType
        {
            get { return contentType; }
            set { contentType = value; }
        }
        private MethodInfo method;
        public MethodInfo Method
        {
            get { return method; }
            set { method = value; }
        }
    }

    internal class ClassController
    {
        private string fullName;

        public string FullName
        {
            get { return fullName; }
            set { fullName = value; }
        }
        private FieldInfo context;

        public FieldInfo Context
        {
            get { return context; }
            set { context = value; }
        }

        public ClassController(string fullName, FieldInfo context)
        {
            this.fullName = fullName;
            this.context = context;
        }
    }

}
