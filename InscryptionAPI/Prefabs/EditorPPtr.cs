using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InscryptionAPI.Prefabs
{
    internal class EditorPPtr<T> : PPtr<T> where T : EditorObject
    {
        public EditorPPtr(T obj)
        {
            this.obj = obj;
        }

        public void PostProcess(List<EditorObject> overallObjects)
        {
            if (!overallObjects.Contains(obj))
            {
                overallObjects.Add(obj);
            }
        }

        public void PostProcess(List<EditorObject> overallObjects, int insertAt)
        {
            if (!overallObjects.Contains(obj))
            {
                overallObjects.Insert(insertAt, obj);
            }
        }

        public PPtr<T> ToPPtr(List<EditorObject> overallObjects, int offset = 0)
        {
            PPtr<T> d = new();
            d.m_FileID = 0;
            d.m_PathID = overallObjects.IndexOf(obj) + 2 + offset;
            return d;
        }

        public EditorObject obj;
    }
}
