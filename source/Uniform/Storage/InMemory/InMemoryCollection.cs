using System;
using System.Collections;
using System.Collections.Generic;

namespace Uniform.Storage.InMemory
{
    public class InMemoryCollection : ICollection
    {
        private readonly Dictionary<string, object> _documents = new Dictionary<string, object>();

        private readonly Dictionary<String, Dictionary<int, List<object>>> _indexes =
            new Dictionary<string, Dictionary<int, List<object>>>();

        public Dictionary<string, object> Documents
        {
            get { return _documents; }
        }

        public Dictionary<int, List<Object>> GetIndex(String name)
        {
            Dictionary<int, List<object>> index;
            if (!_indexes.TryGetValue(name, out index))
                _indexes[name] = index = new Dictionary<int, List<object>>();

            return index;
        }


        public Object GetById(String key)
        {
            Object value;
            if (!_documents.TryGetValue(key, out value))
                throw new Exception("Document not available");

            return value;
        }

        public void Update(String key, Action<Object> updater)
        {
            var obj = GetById(key);
            updater(obj);
            Save(key, obj);
        }

        public void Save(String key, Object obj)
        {
            CheckForIndexes(key, obj);
            _documents[key] = obj;
        }

        private Dictionary<Type, IIndexDefinition> _indexDefinitions = new Dictionary<Type, IIndexDefinition>();

        public void CheckForIndexes(String key, Object obj)
        {
            IIndexDefinition definition;
            if (_indexDefinitions.TryGetValue(obj.GetType(), out definition))
                return;

            var type = obj.GetType();
            var defType = typeof (IndexDefinition<>).MakeGenericType(type);
            var def = (IIndexDefinition) Activator.CreateInstance(defType);

            var mthd = type.GetMethod("DefineIndexes");

            if (mthd != null)
            {
                mthd.Invoke(obj, new object[] { def });
            }

            _indexDefinitions[type] = def;
        }
    }
}