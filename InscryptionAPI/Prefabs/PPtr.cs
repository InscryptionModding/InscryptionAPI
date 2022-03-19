using System;
using System.Reflection;
using System.IO;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace InscryptionAPI.Prefabs
{
    internal class PPtr<T> where T : EditorObject
    {
        public int m_FileID;
        public long m_PathID;
        private int index = -2; //-2 - Prepare, -1 - Missing
        
        public PPtr()
        {
        }

        public static PPtr<T> CreatePPtr(EditorObject obj, IEnumerable<EditorObject> allObjects, int pathOffset = 0)
        {
            return new PPtr<T>() { m_FileID = 0, m_PathID = allObjects.ToList().IndexOf(obj) + 2 + pathOffset };
        }

        public void Write(BinaryWriter write)
        {
            write.Write(m_FileID);
            write.Write(m_PathID);
        }

        public byte[] ToBytes(EndianType endian)
        {
            using (EndianBinaryWriter writer = new EndianBinaryWriter(new MemoryStream(), endian))
            {
                writer.Write(m_FileID);
                writer.Write(m_PathID);
                return writer.BaseStream.ReadToEnd();
            }
        }

        public static PPtr<T> Empty
        {
            get
            {
                return new PPtr<T> { m_FileID = 0, m_PathID = 0 };
            }
        }

        public bool TryGet(List<EditorObject> manualObjects, out T result)
        {
            if (manualObjects.Count <= m_PathID)
            {
                var obj = manualObjects[(int)m_PathID - 2];
                if (obj is T variable)
                {
                    result = variable;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public T Get(List<EditorObject> manualObjects)
        {
            if (m_PathID <= manualObjects.Count)
            {
                var obj = manualObjects[(int)m_PathID - 2];
                if (obj is T variable)
                {
                    return variable;
                }
            }

            return null;
        }

        public bool IsNull => m_PathID == 0 || m_FileID < 0;
    }
}
