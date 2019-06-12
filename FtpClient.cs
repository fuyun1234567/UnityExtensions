using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

public class FtpClient
{
    private string _host;
    private string _userId;
    private string _password;
    private int _port= 21;
    private IWebProxy _proxy;
    private bool _enableSsl;
    private bool _useBinary;
    private bool _usePassive;

    private long _curUploadLength=0;
    /// <summary>
    /// 创建FTP工具
    /// </summary>
    /// <param name="host">主机名</param>
    /// <param name="userId">账户</param>
    /// <param name="password">密码</param>
    public FtpClient(string host, string userId, string password)
        : this(host, userId, password, 21, null, false, true, true)
    {
    }
    /// <summary>
    /// 创建FTP工具
    /// </summary>
    /// <param name="host">主机名</param>
    /// <param name="userId">账户</param>
    /// <param name="pwd">密码</param>
    /// <param name="port">端口号</param>
    /// <param name="proxy">代理</param>
    /// <param name="enableSsl">开启ssl</param>
    /// <param name="useBinary">是否使用二进制</param>
    /// <param name="usePassive">是否允许被动模式</param>
    public FtpClient(string host,string userId,string pwd,int port,IWebProxy proxy,bool enableSsl,bool useBinary,bool usePassive)
    {
        if (host.StartsWith("ftp://"))
        {
            _host = host;
        }
        else
        {
            _host = "ftp://" + host;
        }
        _userId = userId;
        _password = pwd;
        _port = port;
        _proxy = proxy;
        _enableSsl = enableSsl;
        _useBinary = useBinary;
        _usePassive = usePassive;
    }
    /// <summary>
    /// 主机名
    /// </summary>
    public string Host
    {
        get { return _host?? String.Empty; }
    }
    /// <summary>
    /// 账号
    /// </summary>
    public string UserId
    {
        get { return _userId; }
    }
    /// <summary>
    /// 密码
    /// </summary>
    public string Password
    {
        get { return _password; }
    }
    /// <summary>
    /// 端口号
    /// </summary>
    public int Port
    {
        get { return _port; }
        set { _port = value; }
    }
    /// <summary>
    /// 代理
    /// </summary>
    public IWebProxy Proxy
    {
        get { return _proxy; }
        set { _proxy = value; }
    }
    /// <summary>
    /// 开启ssl
    /// </summary>
    public bool EnableSsl
    {
        get { return _enableSsl; }
    }
    /// <summary>
    /// 使用二进制
    /// </summary>
    public bool UseBinary
    {
        get { return _useBinary; }
        set { _useBinary = value; }
    }
    /// <summary>
    /// 开启被动模式
    /// </summary>
    public bool UsePassive
    {
        get { return _usePassive; }
        set { _usePassive = value; }
    }
    private string remotePath = "/";
    /// <summary>
    /// 远端路径
    /// <para>
    /// 返回FTP服务器上的当前路径(可以是 / 或 /a/../ 的形式)
    /// </para>
    /// </summary>
    public string RemotePath
    {
        get
        {
            return remotePath;
        }
        set
        {
            string result = "/";
            if (!string.IsNullOrEmpty(value) && value != "/")
            {
                result = "/" + value.TrimStart('/').TrimEnd('/') + "/";
            }
            this.remotePath = result;
        }
    }
    /// <summary>
    /// 获取FTP服务器上的当前路径
    /// </summary>
    public string CurrentDirectory
    {
        get
        {
            string result = string.Empty;
            string url = Host.TrimEnd('/') + remotePath;
            FtpWebRequest request = CreateRequest(url, WebRequestMethods.Ftp.PrintWorkingDirectory);
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                string temp = response.StatusDescription;
                int start = temp.IndexOf('"') + 1;
                int end = temp.LastIndexOf('"');
                if (end >= start)
                {
                    result = temp.Substring(start, end - start);
                }
            }
            return result;

        }
    }
    /// <summary>
    /// 创建ftp连接
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <param name="method">请求方法</param>
    /// <returns></returns>
    FtpWebRequest CreateRequest(string url,string method)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);  //创建连接对象
        
        request.Credentials=new NetworkCredential(_userId,_password);  //创建连接通信凭据
        
        if (_proxy!=null)
        {
            request.Proxy = _proxy;
        }
        request.UseBinary = _useBinary;
        request.UsePassive = _usePassive;
        request.EnableSsl = _enableSsl;
        request.Method = method;

        return request;
    }
    /// <summary>
    /// 从当前目录下下载文件
    /// <para>
    /// 如果本地文件存在,则从本地文件结束的位置开始下载.
    /// </para>
    /// </summary>
    /// <param name="serverName">服务器上的文件名称</param>
    /// <param name="localName">本地文件名称</param>
    /// <returns>返回一个值,指示是否下载成功</returns>
    public bool Download(string serverName, string localName)
    {
        bool result = false;
        using (FileStream fs = new FileStream(localName, FileMode.OpenOrCreate)) //创建或打开本地文件
        {
            //建立连接
            string url = Host.TrimEnd('/') + RemotePath + serverName;
            FtpWebRequest request = CreateRequest(url, WebRequestMethods.Ftp.DownloadFile);
            request.ContentOffset = fs.Length;
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                fs.Position = fs.Length;
                byte[] buffer = new byte[4096];//4K
                int count = response.GetResponseStream().Read(buffer, 0, buffer.Length);
                while (count > 0)
                {
                    fs.Write(buffer, 0, count);
                    count = response.GetResponseStream().Read(buffer, 0, buffer.Length);
                }
                response.GetResponseStream().Close();
            }
            result = true;
        }
        return result;
    }
    /// <summary>
    /// 把文件上传到FTP服务器的RemotePath下  先设置RemotePath
    /// </summary>
    /// <param name="localFile">本地文件信息</param>
    /// <param name="remoteFileName">要保存到FTP文件服务器上的名称</param>
    public bool Upload(FileInfo fileInfo,string remoteFileName)
    {
        bool result = false;
        if (fileInfo.Exists)
        {
            string url = Host.TrimEnd('/') + RemotePath + remoteFileName;
            FtpWebRequest request = CreateRequest(url, WebRequestMethods.Ftp.UploadFile);
            using (Stream rs = request.GetRequestStream())
            using (FileStream fs=fileInfo.OpenRead())
            {
                _curUploadLength = 0;
                long fileLenth = fileInfo.Length;
                int onceLenth;
                byte[] buffer = new byte[1024 * 4];
                while (_curUploadLength < fileLenth)
                {
                    onceLenth = fs.Read(buffer, 0, buffer.Length);
                    _curUploadLength += onceLenth;
                    rs.Write(buffer, 0, onceLenth);
                }
                fs.Close();
                result = true;
            }

            return result;
        }
        else
        {
            throw new Exception(string.Format("本地文件不存在,文件路径:{0}", fileInfo.FullName));
        }
    }
    /// <summary>
    /// 文件更名
    /// </summary>
    /// <param name="oldFileName">原文件名</param>
    /// <param name="newFileName">新文件名</param>
    /// <returns>返回一个值,指示更名是否成功</returns>
    public bool Rename(string oldFileName, string newFileName)
    {
        bool result = false;
        //建立连接
        string url = Host.TrimEnd('/') + remotePath + oldFileName;
        FtpWebRequest request = CreateRequest(url, WebRequestMethods.Ftp.Rename);
        request.RenameTo = newFileName;
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
            result = true;
        }
        return result;
    }
    /// <summary>
    /// 获取当前目录下文件列表
    /// </summary>
    /// <returns></returns>
    public List<string> GetFileList()
    {
        List<string> result = new List<string>();
        //建立连接
        string url = Host.TrimEnd('/') + remotePath;
        FtpWebRequest request = CreateRequest(url, WebRequestMethods.Ftp.ListDirectory);
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
            StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default);//中文文件名
            string line = reader.ReadLine();
            while (line != null)
            {
                result.Add(line);
                line = reader.ReadLine();
            }
        }
        return result;
    }
    /// <summary>
    /// 获取详细列表 从FTP服务器上获取文件和文件夹列表
    /// </summary>
    /// <returns></returns>
    public List<string> GetFileDetails()
    {
        List<string> result = new List<string>();
        //建立连接
        string url = Host.TrimEnd('/') + RemotePath;
        FtpWebRequest request = CreateRequest(url, WebRequestMethods.Ftp.ListDirectoryDetails);
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
            StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default);//中文文件名
            string line = reader.ReadLine();
            while (line != null)
            {
                result.Add(line);
                line = reader.ReadLine();
            }
        }
        return result;
    }
    /// <summary>
    /// 删除FTP服务器上的文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns>是否删除成功</returns>
    public bool DeleteFile(string fileName)
    {
        bool result = false;
        //建立连接
        string url = Host.TrimEnd('/') + remotePath + fileName;
        FtpWebRequest request = CreateRequest(url, WebRequestMethods.Ftp.DeleteFile);
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
            result = true;
        }

        return result;
    }
    /// <summary>
    /// 删除文件夹
    /// </summary>
    /// <param name="dirName">文件夹名称</param>
    /// <returns>返回一个值,指示是否删除成功</returns>
    public bool DeleteDirectory(string dirName)
    {
        bool result = false;
        //建立连接
        string url = Host.TrimEnd('/') + remotePath + dirName;
        FtpWebRequest request = CreateRequest(url, WebRequestMethods.Ftp.RemoveDirectory);
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
            result = true;
        }
        return result;
    }
    /// <summary>
    /// 在当前目录下创建文件夹
    /// </summary>
    /// <param name="dirName">文件夹名称</param>
    /// <returns>是否创建成功</returns>
    public bool MakeDirectory(string dirName)
    {
        bool result = false;
        //建立连接
        string url = Host.TrimEnd('/') + remotePath + dirName;
        FtpWebRequest request = CreateRequest(url, WebRequestMethods.Ftp.MakeDirectory);
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
            result = true;
        }
        return result;
    }
    /// <summary>
    /// 获取文件大小
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns></returns>
    public long GetFileSize(string filepath)
    {
        long result = 0;
        //建立连接
        string url = _host.TrimEnd('/')+remotePath + filepath;
        FtpWebRequest request = CreateRequest(url, WebRequestMethods.Ftp.GetFileSize);
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        {
            result = response.ContentLength;
        }

        return result;
    }

    /// <summary>
    /// 给FTP服务器上的文件追加内容
    /// </summary>
    /// <param name="localFile">本地文件</param>
    /// <param name="remoteFileName">FTP服务器上的文件</param>
    /// <returns>返回一个值,指示是否追加成功</returns>
    public bool Append(FileInfo localFile, string remoteFileName)
    {
        if (localFile.Exists)
        {
            using (FileStream fs = new FileStream(localFile.FullName, FileMode.Open))
            {
                return Append(fs, remoteFileName);
            }
        }
        throw new Exception(string.Format("本地文件不存在,文件路径:{0}", localFile.FullName));
    }

    /// <summary>
    /// 给FTP服务器上的文件追加内容
    /// </summary>
    /// <param name="stream">数据流(可通过设置偏移来实现从特定位置开始上传)</param>
    /// <param name="remoteFileName">FTP服务器上的文件</param>
    /// <returns>返回一个值,指示是否追加成功</returns>
    public bool Append(Stream stream, string remoteFileName)
    {
        bool result = false;
        if (stream != null && stream.CanRead)
        {
            //建立连接
            string url = Host.TrimEnd('/') + remotePath + remoteFileName;
            FtpWebRequest request = CreateRequest(url, WebRequestMethods.Ftp.AppendFile);
            using (Stream rs = request.GetRequestStream())
            {
                //上传数据
                byte[] buffer = new byte[4096];//4K
                int count = stream.Read(buffer, 0, buffer.Length);
                while (count > 0)
                {
                    rs.Write(buffer, 0, count);
                    count = stream.Read(buffer, 0, buffer.Length);
                }
                result = true;
            }
        }
        return result;
    }

   
    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="fileName">要检查的文件名</param>
    /// <returns>返回一个值,指示要检查的文件是否存在</returns>
    public bool CheckFileExist(string fileName)
    {
        bool result = false;
        if (fileName != null && fileName.Trim().Length > 0)
        {
            fileName = fileName.Trim();
            List<string> files = GetFileList();
            if (files != null && files.Count > 0)
            {
                foreach (string file in files)
                {
                    if (file.ToLower() == fileName.ToLower())
                    {
                        result = true;
                        break;
                    }
                }
            }
        }
        return result;
    }
}
