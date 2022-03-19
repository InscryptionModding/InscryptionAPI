using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InscryptionAPI.Prefabs
{
    internal static class BinaryWriterExtensions
    {
        /*public static void AlignStream(this BinaryReader reader)
        {
            reader.AlignStream(4);
        }

        public static void AlignStream(this BinaryReader reader, int alignment)
        {
            var pos = reader.BaseStream.Position;
            var mod = pos % alignment;
            if (mod != 0)
            {
                reader.BaseStream.Position += alignment - mod;
            }
        }

        public static string WriteAlignedString(this BinaryWriter reader, string stringData, int length)
        {
            if (length > 0 && length <= reader.BaseStream.Length - reader.BaseStream.Position)
            {
                var result = Encoding.UTF8.GetBytes(stringData);
                var stringData = reader.ReadBytes(length);
                reader.AlignStream(4);
                return result;
            }
            return "";
        }

        public static void WriteStringArray(this BinaryWriter reader, string[] array)
        {
            WriteArray(reader.WriteAlignedString, array);
        }
        */

        public static void AlignStream(this BinaryWriter write)
        {
            write.AlignStream(4);
        }

        public static void AlignStream(this BinaryWriter write, int alignment)
        {
            var pos = write.BaseStream.Position;
            var mod = pos % alignment;
            if (mod != 0)
            {
                write.BaseStream.Position += alignment - mod;
            }
        }

        public static void Write(this BinaryWriter write, EditorQuaternion q)
        {
            write.Write(q.X);
            write.Write(q.Y);
            write.Write(q.Z);
            write.Write(q.W);
        }

        /*public static void Write(this BinaryWriter write, Vector2 v)
        {
            write.Write(v.X);
            write.Write(v.Y);
        }*/

        public static void Write(this BinaryWriter write, EditorVector3 v)
        {
            write.Write(v.X);
            write.Write(v.Y);
            write.Write(v.Z);
        }

        public static void WriteStringToNull(this BinaryWriter bw, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            foreach (byte b in bytes)
            {
                bw.Write(b);
            }
            bw.Write((byte)0);
        }

        public static void WriteString(this BinaryWriter bw, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            foreach (byte b in bytes)
            {
                bw.Write(b);
            }
        }

        public static void WriteAlignedString(this BinaryWriter write, string toWrite, int overrideAlignment = -1, bool verbose = false)
        {
            var result = Encoding.UTF8.GetBytes(toWrite);
            write.Write(result.Length);
            write.Write(result);
            if (verbose)
            {
                Console.WriteLine("before al: " + write.BaseStream.Position);
            }
            write.AlignStream(overrideAlignment > 0 ? overrideAlignment : 4);
            if (verbose)
            {
                Console.WriteLine("after al: " + write.BaseStream.Position);
            }
        }

        public static void Insert(this BinaryWriter bw, byte[] bytes, long pos)
        {
            byte[] streambytes = bw.BaseStream.ReadToEnd(pos);
            if (bw.BaseStream.CanSeek)
            {
                bw.BaseStream.Position = pos;
            }
            bw.Write(bytes);
            bw.Write(streambytes);
            bw.BaseStream.Position = bw.BaseStream.ReadToEnd(pos).Length;
        }

        public static void Insert(this BinaryWriter bw, string val, long pos)
        {
            bw.Insert(Encoding.UTF8.GetBytes(val), pos);
        }

        public static void Insert(this BinaryWriter bw, int val, long pos)
        {
            var buff = BitConverter.GetBytes(val);
            bw.Insert(buff, pos);
        }

        public static void Insert(this BinaryWriter bw, long val, long pos)
        {
            var buff = BitConverter.GetBytes(val);
            bw.Insert(buff, pos);
        }

        public static void Insert(this BinaryWriter bw, short val, long pos)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            bw.Insert(buff, pos);
        }

        public static byte[] ReadToEnd(this Stream stream, long startFrom = 0)
        {
            long originalPosition = 0;
            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = startFrom;
            }
            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public static FieldInfo inf = typeof(FileStream).GetField("access", BindingFlags.NonPublic | BindingFlags.Instance);
    }
}
