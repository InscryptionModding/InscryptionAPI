using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InscryptionAPI.Prefabs
{
    internal class EditorAssetInfo
    {
        public int preloadIndex;
        public int preloadSize;
        public PPtr<EditorObject> asset;

        public EditorAssetInfo()
        {
        }

        public void Write(BinaryWriter write)
        {
            write.Write(preloadIndex);
            write.Write(preloadSize);
            asset.Write(write);
        }
    }

    internal sealed class EditorAssetBundle : NamedObject
    {
        public PPtr<EditorObject>[] m_PreloadTable;
        public KeyValuePair<string, EditorAssetInfo>[] m_Container;

        public EditorAssetBundle()
        {
        }

        public override void Write(BinaryWriter write)
        {
            base.Write(write);
            write.Write(m_PreloadTable.Length);
            foreach(PPtr<EditorObject> pptr in m_PreloadTable)
            {
                pptr.Write(write);
            }
            write.Write(m_Container.Length);
            foreach(KeyValuePair<string, EditorAssetInfo> container in m_Container)
            {
                write.WriteAlignedString(container.Key);
                container.Value.Write(write);
            }
            write.Write(new byte[20]);
            write.Write((byte)1);
            write.Write(new byte[3]);
            write.WriteAlignedString(m_Name);
            var mod = write.BaseStream.Position % 4;
            write.Write(new byte[12 - mod]);
            write.Write((byte)7);
            write.Write(new byte[7]);
        }
    }
}
