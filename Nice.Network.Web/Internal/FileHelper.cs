using System.IO;

namespace Nice.Network.Web
{
    internal class FileHelper
    {
        internal static byte[] GetFile(string file)
        {
            if (!File.Exists(file)) return null;
            FileStream readIn = new FileStream(file, FileMode.Open, FileAccess.Read);
            int totalLen = (int)readIn.Length;
            byte[] buffer = new byte[totalLen];
            int bytesReadLen = 10240;
            if (totalLen < bytesReadLen)
                bytesReadLen = totalLen;
            int readLen = 0;
            int nRead = readIn.Read(buffer, readLen, bytesReadLen);
            readLen += nRead;
            while (nRead > 0 && readLen < totalLen)
            {
                if (totalLen - readLen < bytesReadLen)
                    bytesReadLen = totalLen - readLen;
                nRead = readIn.Read(buffer, readLen, bytesReadLen);
                readLen += nRead;
            }
            readIn.Close();
            return buffer;
        }
        internal static string PathCombine(string path1, string path2)
        {
            if (path1.Length == 0)
                return path2;
            if (path2.Length == 0)
                return path1;
            path2 = path2.Replace('/', '\\');
            if (path2[0] == Path.AltDirectorySeparatorChar || (path2.Length >= 2 && path2[1] == Path.VolumeSeparatorChar))
            {
                return path2;
            }
            char c = path1[path1.Length - 1];
            if (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar && c != Path.VolumeSeparatorChar && path2[0] != Path.DirectorySeparatorChar)
            {
                return path1 + "\\" + path2;
            }
            return path1 + path2;
        }
    }
}
