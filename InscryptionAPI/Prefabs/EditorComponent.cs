using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InscryptionAPI.Prefabs
{
    internal abstract class EditorComponent : EditorExtension
    {
        public PPtr<EditorGameObject> m_GameObject;

        public EditorComponent()
        {
        }

        public override void Write(BinaryWriter write)
        {
            base.Write(write);
            m_GameObject.Write(write);
        }
    }
}
