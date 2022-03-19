using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace InscryptionAPI.Prefabs
{
    internal sealed class EditorTextAsset : NamedObject
    {
        public byte[] m_Script;

        /*public EditorTextAsset(ObjectReader reader) : base(reader)
        {
            m_Script = reader.ReadUInt8Array();
        }*/ //maybe ill add support for this later

    }
}
