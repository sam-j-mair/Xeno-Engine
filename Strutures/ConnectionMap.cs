using System;
using System.Collections.Generic;

namespace XenoEngine.Systems
{
    class ConnectionMap<TKey, TInput>
    {
        private Dictionary<TKey, Action<TInput>> m_dictionary;

        public ConnectionMap()
        {
            m_dictionary = new Dictionary<TKey, Action<TInput>>();
        }
        //-------------------------------------------------------------------------------
        //These could be done as an indexer.?
        //-------------------------------------------------------------------------------
        public void AddAction(TKey key)
        {
            m_dictionary.Add(key, new Action<TInput>(InternalHandler));
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void AddHandler(TKey key, Action<TInput> handler)
        {
            m_dictionary[key] += handler;
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public void RemoveHandler(TKey key, Action<TInput> handler)
        {
            m_dictionary[key] -= handler;
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        public Action<TInput> GetConnection(TKey key)
        {
            return m_dictionary[key];
        }
        //-------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------
        private void InternalHandler(TInput data)
        {
            //This is a default handler for the delegate.
        }
    }
}
