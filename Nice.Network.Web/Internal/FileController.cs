using System;
using System.IO;
using System.Text;

namespace Nice.Network.Web.Internal
{
    internal class FileController
    {
        internal static void SaveFile(Encoding encoding, string boundary, Stream input)
        {
            byte[] boundarybytes = encoding.GetBytes(boundary);
            int boundaryLen = boundarybytes.Length;
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            using (FileStream output = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[1024];
                int len = input.Read(buffer, 0, 1024);
                int startPos = -1;

                // Find start boundary
                while (true)
                {
                    if (len == 0)
                    {
                        throw new Exception("Start Boundaray Not Found");
                    }

                    startPos = IndexOf(buffer, len, boundarybytes);
                    if (startPos >= 0)
                    {
                        break;
                    }
                    else
                    {
                        Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen);
                        len = input.Read(buffer, boundaryLen, 1024 - boundaryLen);
                    }
                }

                // Skip four lines (Boundary, Content-Disposition, Content-Type, and a blank)
                for (int i = 0; i < 4; i++)
                {
                    while (true)
                    {
                        if (len == 0)
                        {
                            throw new Exception("Preamble not Found.");
                        }

                        startPos = Array.IndexOf(buffer, encoding.GetBytes("\n")[0], startPos);
                        if (startPos >= 0)
                        {
                            startPos++;
                            break;
                        }
                        else
                        {
                            len = input.Read(buffer, 0, 1024);
                        }
                    }
                }

                Array.Copy(buffer, startPos, buffer, 0, len - startPos);
                len = len - startPos;

                while (true)
                {
                    int endPos = IndexOf(buffer, len, boundarybytes);
                    if (endPos >= 0)
                    {
                        if (endPos > 0) output.Write(buffer, 0, endPos - 2);
                        break;
                    }
                    else if (len <= boundaryLen)
                    {
                        throw new Exception("End Boundaray Not Found");
                    }
                    else
                    {
                        output.Write(buffer, 0, len - boundaryLen);
                        Array.Copy(buffer, len - boundaryLen, buffer, 0, boundaryLen);
                        len = input.Read(buffer, boundaryLen, 1024 - boundaryLen) + boundaryLen;
                    }
                }
            }
        }

        internal static int IndexOf(byte[] buffer, int len, byte[] boundarybytes)
        {
            for (int i = 0; i <= len - boundarybytes.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < boundarybytes.Length && match; j++)
                {
                    match = buffer[i + j] == boundarybytes[j];
                }

                if (match)
                {
                    return i;
                }
            }

            return -1;
        }

        internal static string GetBoundary(string ctype)
        {
            return "--" + ctype.Split(';')[1].Split('=')[1];
        }
    }
}
