using System;
using System.Collections.Generic;
using System.Text;

namespace InscryptionAPI.Helpers
{
    /// <summary>
    /// An equivalent of ref arguments for IEnumerator methods.
    /// </summary>
    /// <typeparam name="T">Type of object this object stores.</typeparam>
    public class Ref<T>
    {
        public Ref() { }

        public Ref(T startval)
        {
            value = startval;
        }

        public T value;
    }
}
