using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InscryptionAPI.Prefabs
{
    internal class NamedObject : EditorExtension
    {
        public string m_Name;

        public NamedObject()
        {
        }

        public override void Write(BinaryWriter write)
        {
            base.Write(write);
            write.WriteAlignedString(m_Name);
        }
    }
}
