using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace InscryptionAPI.Prefabs
{
    internal class EditorObject
    {
        public long m_PathID;
        public int[] version;
        public EditorBuildTarget platform;
        public ClassIDType type;
        public uint byteSize;

        public EditorObject()
        {
        }

        public virtual void Write(BinaryWriter write)
        {
        }
    }
}
