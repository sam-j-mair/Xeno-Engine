using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace XenoEngine.TextEngine
{
//     public static class Helpers
//     {
//         public static int[] ToIntArray(this int value)
//         {
//             int[] tempArray = new int[32];
// 
//             for (int nStartIndex = 0; nStartIndex < 32; ++nStartIndex)
//             {
//                 Convert.
//             }
//         }
//     }
    enum WordEntryType : byte
    {
        Noun,
        Verb,
        Adjective,

        Invalid_Entry
    }

    public class MultiKeyDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TKey> m_Map = new Dictionary<TKey, TKey>();
        private Dictionary<TKey,  Synonyms<TKey, TValue>> m_nValues = new Dictionary<TKey,  Synonyms<TKey, TValue>>();

        public MultiKeyDictionary(){}

        public virtual void Add(TKey primaryKey, TValue value)
        {
            var entry = new Synonyms<TKey, TValue>(primaryKey, value);

            m_nValues.Add(primaryKey, entry);

            m_Map.Add(primaryKey, primaryKey);
        }

        public virtual void Add(TKey primaryKey, TValue value, params TKey[] aOtherKeys)
        {
            Add(primaryKey, value);
            
            foreach (TKey key in aOtherKeys)
            {
                Associate(primaryKey, key);
            }
        }
        
        public virtual void Associate(TKey primaryKey, TKey secondaryKey)
        {
            Synonyms<TKey, TValue> value;
            if(m_nValues.TryGetValue(primaryKey, out value))
            {
                //we add this the initial values so that we cyclically
                //access the key from the value.
                value.Add(secondaryKey);
                m_Map.Add(secondaryKey, primaryKey);
            }
            else
            {
                Debug.Assert(false, "The primary doesn't exist.");
            }
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                TKey value;
                if (m_Map.TryGetValue(key, out value))
                {
                    return m_nValues[value].Value;
                }
                return default(TValue);
            }
        }

        public virtual void Remove(TKey key)
        {
            TKey value;
            if(m_Map.TryGetValue(key, out value))
            {
                var entry = m_nValues[value];

                foreach (TKey theKey in entry.Entries)
                {
                    m_Map.Remove(theKey);
                }

                m_nValues.Remove(entry.Entries[0]);
            }
        }

        public virtual void Clear()
        {
            m_Map.Clear();
            m_nValues.Clear();
        }

        private class Synonyms<TKeys, TValueType>
        {
            TValueType m_value;
            List<TKeys> m_keys = new List<TKeys>();

            internal Synonyms(TKeys primaryKey, TValueType value)
            {
                m_value = value;
                m_keys.Add(primaryKey);
            }

            internal void Add(TKeys key)
            {
                m_keys.Add(key);
            }

            internal List<TKeys> Entries { get { return m_keys; } }
            internal TValueType Value { get { return m_value;} }
        }
    }

    internal class WordEntry
    {
        public WordAction Action { get; set; }            //4 bytes
        public dynamic StateObject { get; set; }          //4 bytes
        
    }

    internal class WordKeyEntry
    {
        public string Word;
        public int WordIndex;
        public string LookUpKey;
    }

    public delegate dynamic WordAction(dynamic stateObject, dynamic returnValue);


    /// <summary>
    /// Word structure. <Verb>Action to perform</Verb> <Object>The object on which the action is peroformed</Object>
    /// </summary>

    public class XETextParser
    {
        private MultiKeyDictionary<string, WordEntry> m_entries = new MultiKeyDictionary<string, WordEntry>();

        public void AddAction(string szPrimaryWord, WordAction action, dynamic stateObject = null)
        {
            string[] aWords = szPrimaryWord.Split();
            var wordEntry = new WordEntry();
            var key = new WordKeyEntry();
            wordEntry.Action = action;
            wordEntry.StateObject = stateObject;

            if (aWords.Length > 1)
            {
                for (int i = 0; i < aWords.Length; ++i)
                {

                }
            }
            else
            {
                m_entries.Add(szPrimaryWord.ToLower(), wordEntry);
            }
        }

        public void RemoveAction(string szWord)
        {
            m_entries.Remove(szWord.ToLower());
        }

        public void AddSynonym(string szPrimaryWord, string szSecondaryWord)
        {
            m_entries.Associate(szPrimaryWord.ToLower(), szSecondaryWord.ToLower());
        }

        public void ProcessText(string szPhrase)
        {
            string[] aWords = szPhrase.ToLower().Split().Reverse().ToArray();
            //we process the words back to front ....this works for english but...maybe for other languages not so good..
            //var wordsReversed = aWords.Reverse();
            int nItemCount = aWords.Length;
            //aWords = wordsReversed.ToArray();
            dynamic returnValue = null;

            for (int i = 0; i < nItemCount; ++i)
            {
                WordEntry entry = m_entries[aWords[i]];

                if (entry != null)
                {
                    returnValue = entry.Action(entry.StateObject, returnValue);
                }
            }
        }
    }
}
