using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InscryptionAPI.Prefabs
{
    internal sealed class EditorGameObject : EditorExtension
    {
        public PPtr<EditorComponent>[] m_Components;
        public string m_Name;
        public int m_Layer;

        public EditorTransform m_Transform;

        public EditorGameObject()
        {
        }

        public EditorTransform TryGetTransform(List<EditorObject> manualObjects)
        {
            if(m_Transform != null)
            {
                return m_Transform;
            }
            if (m_Components != null)
            {
                foreach (PPtr<EditorComponent> pptr in m_Components)
                {
                    if(pptr != null && pptr.TryGet(manualObjects, out var comp))
                    {
                        if(comp != null && comp is EditorTransform transform)
                        {
                            m_Transform = transform;
                            return transform;
                        }
                    }
                }
            }
            return null;
        }

        public override void Write(BinaryWriter write)
        {
            base.Write(write);
            write.Write(m_Components.Length);
            foreach(PPtr<EditorComponent> p in m_Components)
            {
                p.Write(write);
            }
            write.Write(m_Layer);
            write.WriteAlignedString(m_Name);
        }
    }
}
