using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;



using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Speech.Recognition;

namespace XenoEngine.Systems
{
    struct KeyValue
    {
        string m_szKey;
        object m_value;

        public KeyValue(string szKey, object value)
        {
            m_szKey = szKey;
            m_value = value;
        }

        public string Key { get { return m_szKey; } }
        public object Value { get { return m_value; } }
    }

    class CommandSegment
    {
        private GrammarBuilder      m_grammerBuilder;
        private GrammarBuilder[]    m_aBuilders;
        private KeyValue[]          m_aGrammars;
        private string              m_szStorageKey;

        //-----------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------
        #region Constructors
        public CommandSegment(string szStorageKey, params KeyValue[] aGrammers)
        {
            m_grammerBuilder = ConstructGrammarBuilder(szStorageKey, aGrammers);

            //This is for debug purposes
            m_aGrammars = aGrammers;
            m_szStorageKey = szStorageKey;
        }
        //-----------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------
        public CommandSegment(string szStorageKey, params GrammarBuilder[] grammars)
        {
            m_grammerBuilder = ConstructGrammarBuilder(szStorageKey, grammars);

            m_aBuilders = grammars;
            m_szStorageKey = szStorageKey;
        }
        #endregion
        //-----------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------
        #region Properties
        public GrammarBuilder Builder { get { return m_grammerBuilder; } }
        #endregion
        //-----------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------
        #region Private Methods
        private GrammarBuilder ConstructGrammarBuilder(string szStorageKey,  params KeyValue[] aGrammers)
        {
	        Choices choice = new Choices();

            foreach (KeyValue keyValue in aGrammers)
            {
                GrammarBuilder grammarBuilder = new GrammarBuilder(keyValue.Key);
                SemanticResultValue rsltValue = new SemanticResultValue(grammarBuilder, keyValue.Value);
                
                choice.Add(rsltValue.ToGrammarBuilder());
            }

            SemanticResultKey rsltKey = new SemanticResultKey(szStorageKey, choice.ToGrammarBuilder());

            return rsltKey.ToGrammarBuilder();
        }
        //-----------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------
        private GrammarBuilder ConstructGrammarBuilder(string szStorageKey, params GrammarBuilder[] aGrammers)
        {
            Choices choice = new Choices();

            foreach (GrammarBuilder builder in aGrammers)
            {
                //SemanticResultValue rsltValue = new SemanticResultValue(grammarBuilder);

                choice.Add(builder);
            }

            SemanticResultKey rsltKey = new SemanticResultKey(szStorageKey, choice.ToGrammarBuilder());

            return rsltKey.ToGrammarBuilder();
        }
        #endregion
    }

    class GrammarGenerator
    {
        private Grammar             m_grammarObject;
        private Choices             m_choices;

        public GrammarGenerator(params CommandObject[] commands)
        {
            m_choices = new Choices();

            foreach(CommandObject command in commands)
            {
                m_choices.Add(command.Builder);
            }
        }

        public void AddCommand(CommandObject command)
        {
            m_choices.Add(command.Builder);
        }

        public Grammar GenerateGrammarObject()
        {
            return m_grammarObject = new Grammar(m_choices.ToGrammarBuilder());
        }
    }

    class CommandObject
    {
        private GrammarBuilder       m_grammarBuilder;

        public CommandObject(params CommandSegment[] aCommandSegments)
        {
            m_grammarBuilder = CreateGrammar(aCommandSegments);
        }
        
        private GrammarBuilder CreateGrammar(params CommandSegment[] aCommandSegments)
        {
            GrammarBuilder builder = new GrammarBuilder();

            foreach (CommandSegment segment in aCommandSegments)
            {
                builder.Append(segment.Builder);
            }

            return builder;
        }

        public void AddCommandSegment(CommandSegment commandSegment)
        {
            m_grammarBuilder.Append(commandSegment.Builder);
        }

        public GrammarBuilder Builder { get { return m_grammarBuilder; } }

    }
}
