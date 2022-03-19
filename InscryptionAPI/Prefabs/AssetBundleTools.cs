using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InscryptionAPI.Prefabs
{
    internal static class AssetBundleTools
    {
        public static ClassIDType GetClassFromType<T>() where T : EditorObject
        {
            return GetClassFromType(typeof(T));
        }

        public static string ToStringBetter<T>(this IEnumerable<T> enumerable)
        {
            string s = "{ ";
            bool firstTime = true;
            foreach(T val in enumerable)
            {
                if (!firstTime)
                {
                    s += ", ";
                }
                s += val;
                firstTime = false;
            }
            s += " }";
            return s;
        }

        public static ClassIDType GetClassFromType(Type t)
        {
            if (t == typeof(EditorGameObject))
                return ClassIDType.GameObject;
            else if (t == typeof(EditorTransform))
                return ClassIDType.Transform;
            else if (t == typeof(EditorAssetBundle))
                return ClassIDType.AssetBundle;
            else if (t == typeof(EditorTextAsset))
                return ClassIDType.TextAsset;
            return ClassIDType.Object;
        }

        public static List<FieldInfo> GetFieldsOfType<T>(this object anything, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            Type t = anything.GetType();
            Type tt = typeof(T);
            FieldInfo[] f = t.GetFields(flags);
            List<FieldInfo> rf = new List<FieldInfo>();
            foreach(FieldInfo i in f)
            {
                if (i.FieldType == tt)
                {
                    rf.Add(i);
                }
            }
            return rf;
        }

        public static List<FieldInfo> GetFieldsOfType(this object anything, Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            Type t = anything.GetType();
            Type tt = type;
            FieldInfo[] f = t.GetFields(flags);
            List<FieldInfo> rf = new List<FieldInfo>();
            foreach (FieldInfo i in f)
            {
                if (i.FieldType == tt)
                {
                    rf.Add(i);
                }
            }
            return rf;
        }

        public static List<FieldInfo> GetFieldsOfType(this object anything, IEnumerable<Type> types, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            Type t = anything.GetType();
            FieldInfo[] f = t.GetFields(flags);
            List<FieldInfo> rf = new List<FieldInfo>();
            foreach (FieldInfo i in f)
            {
                if (types.Contains(i.FieldType))
                {
                    rf.Add(i);
                }
            }
            return rf;
        }

        public static List<FieldInfo> GetFieldsOfType(this object anything, string typeName, TypeNameComprareType comprareType, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            Type t = anything.GetType();
            FieldInfo[] f = t.GetFields(flags);
            List<FieldInfo> rf = new List<FieldInfo>();
            foreach (FieldInfo i in f)
            {
                if (comprareType == TypeNameComprareType.Contains ? i.FieldType.ToString().Contains(typeName) : (i.FieldType.ToString() == typeName))
                {
                    rf.Add(i);
                }
            }
            return rf;
        }

        /*public static void TypeTreeBlobRead(BinaryReader reader, List<TypeTreeNode> typeTree, List<byte> stringbuffer)
        {
            int numberOfNodes = reader.ReadInt32();
            int stringBufferSize = reader.ReadInt32();
            for (int i = 0; i < numberOfNodes; i++)
            {
                var typeTreeNode = new TypeTreeNode();
                typeTree.Add(typeTreeNode);
                typeTreeNode.m_Version = reader.ReadUInt16();
                typeTreeNode.m_Level = reader.ReadByte();
                typeTreeNode.m_IsArray = reader.ReadBoolean() ? 1 : 0;
                typeTreeNode.m_TypeStrOffset = reader.ReadUInt32();
                typeTreeNode.m_NameStrOffset = reader.ReadUInt32();
                typeTreeNode.m_ByteSize = reader.ReadInt32();
                typeTreeNode.m_Index = reader.ReadInt32();
                typeTreeNode.m_MetaFlag = reader.ReadInt32();
            }
            var m_StringBuffer = reader.ReadBytes(stringBufferSize);
            stringbuffer.AddRange(m_StringBuffer);

            using (var stringBufferReader = new BinaryReader(new MemoryStream(m_StringBuffer)))
            {
                for (int i = 0; i < numberOfNodes; i++)
                {
                    var typeTreeNode = typeTree[i];
                    typeTreeNode.m_Type = ReadString(stringBufferReader, typeTreeNode.m_TypeStrOffset);
                    typeTreeNode.m_Name = ReadString(stringBufferReader, typeTreeNode.m_NameStrOffset);
                }
            }

            string ReadString(BinaryReader stringBufferReader, uint value)
            {
                var isOffset = (value & 0x80000000) == 0;
                if (isOffset)
                {
                    stringBufferReader.BaseStream.Position = value;
                    return stringBufferReader.ReadStringToNull();
                }
                var offset = value & 0x7FFFFFFF;
                if (CommonString.StringBuffer.TryGetValue(offset, out var str))
                {
                    return str;
                }
                return offset.ToString();
            }
        }*/

        public static byte[] ToBytes(this short val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this int val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this long val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this ushort val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this uint val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this ulong val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this float val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this double val)
        {
            var buff = BitConverter.GetBytes(val);
            Array.Reverse(buff);
            return buff;
        }

        public static byte[] ToBytes(this bool val)
        {
            return new byte[] { (byte)(val ? 1 : 0) };
        }

        public static byte[] ToBytesLittle(this short val)
        {
            byte[] buff = new byte[2];
            buff[0] = (byte)val;
            buff[1] = (byte)(val >> 8);
            return buff;
        }

        public static byte[] ToBytesLittle(this int val)
        {
            byte[] buff = new byte[4];
            buff[0] = (byte)val;
            buff[1] = (byte)(val >> 8);
            buff[2] = (byte)(val >> 16);
            buff[3] = (byte)(val >> 24);
            return buff;
        }

        public static byte[] ToBytesLittle(this long val)
        {
            byte[] buff = new byte[8];
            int i = 0;
            int num = 0;
            while (i < 8)
            {
                buff[i] = (byte)(val >> num);
                i++;
                num += 8;
            }
            return buff;
        }

        public static byte[] ToBytesLittle(this ushort val)
        {
            byte[] buff = new byte[2];
            buff[0] = (byte)val;
            buff[1] = (byte)(val >> 8);
            return buff;
        }

        public static byte[] ToBytesLittle(this uint val)
        {
            byte[] buff = new byte[4];
            buff[0] = (byte)val;
            buff[1] = (byte)(val >> 8);
            buff[2] = (byte)(val >> 16);
            buff[3] = (byte)(val >> 24);
            return buff;
        }

        public static byte[] ToBytesLittle(this ulong val)
        {
            byte[] buff = new byte[8];
            int i = 0;
            int num = 0;
            while (i < 8)
            {
                buff[i] = (byte)(val >> num);
                i++;
                num += 8;
            }
            return buff;
        }

        public static byte[] ToBytesLittle(this float val)
        {
            using(BinaryWriter stream = new BinaryWriter(new MemoryStream()))
            {
                stream.Write(val);
                return stream.BaseStream.ReadToEnd();
            }
        }

        public static byte[] ToBytesLittle(this double val)
        {
            using (BinaryWriter stream = new BinaryWriter(new MemoryStream()))
            {
                stream.Write(val);
                return stream.BaseStream.ReadToEnd();
            }
        }

        public static byte[] ToBytesLittle(this bool val)
        {
            return new byte[] { (byte)(val ? 1 : 0) };
        }


        public static byte[] ToBytes(this string val)
        {
            return Encoding.UTF8.GetBytes(val);
        }

        public static byte[] ToBytesPlusNull(this string val)
        {
            return val.ToBytes().Concat(new byte[] { 0 }).ToArray();
        }

        /*public static byte[] ToBytes(this SerializedType type, bool enableTypeTree)
        {
            List<byte> result = new List<byte>();
            result.AddRange(type.classID.ToBytesLittle());
            result.AddRange(type.m_IsStrippedType.ToBytesLittle());
            result.AddRange(type.m_ScriptTypeIndex.ToBytesLittle());
            if (type.classID == 114)
            {
                result.AddRange(type.m_ScriptID); //Hash128
            }
            result.AddRange(type.m_OldTypeHash); //Hash128
            if (enableTypeTree)
            {
                result.AddRange(type.m_Nodes.ToBytes(type.stringBuffer));
            }
            return result.ToArray();
        }*/


        public static List<T2> Convert<T, T2>(this List<T> self, Func<T, T2> convertor)
        {
            List<T2> result = new List<T2>();
            foreach (T element in self)
            {
                result.Add(convertor(element));
            }
            return result;
        }

        /*public static byte[] ToBytes(this List<TypeTreeNode> nodes, byte[] stringBuffer)
        {
            List<byte> result = new List<byte>();
            result.AddRange(nodes.Count.ToBytesLittle());
            result.AddRange(stringBuffer.Length.ToBytesLittle());
            foreach(TypeTreeNode typeTreeNode in nodes)
            {
                result.AddRange(((ushort)typeTreeNode.m_Version).ToBytesLittle());
                result.Add((byte)typeTreeNode.m_Level);
                result.Add((byte)typeTreeNode.m_IsArray);
                result.AddRange(typeTreeNode.m_TypeStrOffset.ToBytesLittle());
                result.AddRange(typeTreeNode.m_NameStrOffset.ToBytesLittle());
                result.AddRange(typeTreeNode.m_ByteSize.ToBytesLittle());
                result.AddRange(typeTreeNode.m_Index.ToBytesLittle());
                result.AddRange(typeTreeNode.m_MetaFlag.ToBytesLittle());
            }

            result.AddRange(stringBuffer);

            return result.ToArray();
        }*/

        public enum TypeNameComprareType
        {
            Equals,
            Contains
        }
    }
}
