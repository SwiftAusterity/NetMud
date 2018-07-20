﻿using NetMud.Communication.Lexicon;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Linguistic;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMud.Communication.Messaging
{
    /// <summary>
    /// A gramatical element
    /// </summary>
    [Serializable]
    public class Lexica : ILexica
    {
        /// <summary>
        /// The type of word this is to the sentence
        /// </summary>
        public GrammaticalType Role { get; set; }

        /// <summary>
        /// The type of word this is in general
        /// </summary>
        public LexicalType Type { get; set; }

        /// <summary>
        /// The actual word/phrase
        /// </summary>
        public string Phrase { get; set; }

        /// <summary>
        /// Modifiers for this lexica
        /// </summary>
        public HashSet<ILexica> Modifiers { get; set;  }

        public Lexica()
        {
            Modifiers = new HashSet<ILexica>();
        }

        public Lexica(LexicalType type, GrammaticalType role, string phrase)
        {
            Type = type;
            Phrase = phrase;
            Role = role;

            Modifiers = new HashSet<ILexica>();

            LexicalProcessor.VerifyDictata(this);
        }

        /// <summary>
        /// Get the dictata from this lexica
        /// </summary>
        /// <returns>A dictata</returns>
        public IDictata GetDictata()
        {
            return ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(typeof(IDictata), string.Format("{0}_{1}", Type.ToString(), Phrase)));
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(ILexica modifier)
        {
            if (!Modifiers.Contains(modifier))
                Modifiers.Add(modifier);

            return modifier;
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(LexicalType type, GrammaticalType role, string phrase)
        {
            var modifier = new Lexica(type, role, phrase);
            if (!Modifiers.Contains(modifier))
                Modifiers.Add(modifier);

            return modifier;
        }

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <param name="normalization">How much sentence splitting should be done</param>
        /// <param name="verbosity">A measure of how much flourish should be added as well as how far words get synonym-upgraded by "finesse". (0 to 100)</param>
        /// <param name="chronology">The time tensing of the sentence structure</param>
        /// <param name="perspective">The personage of the sentence structure</param>
        /// <param name="omitName">Should we omit the proper name of the initial subject entirely (and only resort to pronouns)</param>
        /// <returns>A long description</returns>
        public string Describe(NarrativeNormalization normalization, int verbosity, NarrativeChronology chronology = NarrativeChronology.Present, 
            NarrativePerspective perspective = NarrativePerspective.SecondPerson, bool omitName = true)
        {
            var sentences = new List<Tuple<SentenceType, ILexica>>();

            var subjects = new List<ILexica>
            {
                this
            };
            subjects.AddRange(Modifiers.Where(mod => mod.Role == GrammaticalType.Subject));

            foreach(var subject in subjects)
            {
                var lexicas = new List<ILexica>();
                switch(normalization)
                {
                    case NarrativeNormalization.Hemmingway:
                        //Don't just add the name in as its own sentence that's cray
                        if (subject.Modifiers.Any(mod => mod.Role != GrammaticalType.Descriptive && mod.Role != GrammaticalType.Subject))
                        {
                            var newSubject = new Lexica(subject.Type, subject.Role, subject.Phrase)
                            {
                                Modifiers = new HashSet<ILexica>(subject.Modifiers.Where(mod => mod.Role != GrammaticalType.Descriptive))
                            };

                            lexicas.Add(newSubject);
                        }

                        foreach(var adj in subject.Modifiers.Where(mod => mod.Role == GrammaticalType.Descriptive))
                        {
                            var newSplitSubject = new Lexica(subject.Type, subject.Role, subject.Phrase);
                            newSplitSubject.TryModify(adj).TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "is");
                            lexicas.Add(newSplitSubject);
                        }
                        break;
                    case NarrativeNormalization.Runon: //todo: figure this one out
                    case NarrativeNormalization.Normal:
                        lexicas.Add(subject);
                        break;
                }

                foreach (var lex in lexicas)
                    sentences.Add(new Tuple<SentenceType, ILexica>(SentenceType.Statement, lex));
            }

            //join the sentences together with a space and add punctuation
            var finalOutput = new List<string>();

            foreach(var sentence in sentences)
            {
                if (sentence.Item2.Equals(this))
                    finalOutput.Add(sentence.Item2.ToString() + LexicalProcessor.GetPunctuationMark(sentence.Item1));
                else
                    finalOutput.Add(sentence.Item2.Describe(normalization, verbosity, chronology, perspective, omitName));
            }

            return string.Join(" ", finalOutput);
        }

        /// <summary>
        /// Render this lexica to a sentence fragment (or whole sentence if it's a Subject role)
        /// </summary>
        /// <returns>a sentence fragment</returns>
        public override string ToString()
        {
            //short circuit empty lexica
            if (string.IsNullOrWhiteSpace(Phrase))
                return string.Empty;

            var sb = new StringBuilder();
            var adjectives = Modifiers.Where(mod => mod.Role == GrammaticalType.Descriptive);

            switch (Role)
            {
                case GrammaticalType.Descriptive:
                    if (adjectives.Count() > 0)
                    {
                        if (Type == LexicalType.Conjunction || Type == LexicalType.Interjection)
                        {
                            sb.AppendFormat("{0} {1}", Phrase, adjectives.Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma));
                        }
                        else
                        {
                            sb.AppendFormat("{1} {0}", Phrase, adjectives.Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma));
                        }
                    }
                    else
                    {
                        sb.Append(Phrase);
                    }
                    break;
                case GrammaticalType.IndirectObject:
                    sb.Append(AppendDescriptors(adjectives, Phrase));
                    break;
                case GrammaticalType.DirectObject:
                    var describedNoun = AppendDescriptors(adjectives, Phrase);

                    if (Modifiers.Any(mod => mod.Role == GrammaticalType.IndirectObject))
                    {
                        var iObj = Modifiers.Where(mod => mod.Role == GrammaticalType.IndirectObject)
                                            .Select(mod => mod.ToString()).CommaList(RenderUtility.SplitListType.AllAnd);

                        sb.AppendFormat("{0} {1}", iObj, describedNoun);
                    }
                    else
                        sb.Append(describedNoun);

                    break;
                case GrammaticalType.Verb:
                    var adverbString = adjectives.Where(adj => adj.Type == LexicalType.Adverb)
                                 .Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma);

                    var adjectiveString = adjectives.Where(adj => adj.Type == LexicalType.Adjective)
                                     .Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.OxfordComma);

                    if(Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                    {
                        var dObj = Modifiers.Where(mod => mod.Role == GrammaticalType.DirectObject)
                                            .Select(mod =>  mod.ToString()).CommaList(RenderUtility.SplitListType.OxfordComma);

                        sb.AppendFormat("{2} {0} {1} {3}", Phrase, dObj, adverbString, adjectiveString);
                    }
                    else
                        sb.AppendFormat("{1} {0} {2}", Phrase, adverbString, adjectiveString);
                    break;
                case GrammaticalType.Subject:
                    var describedSubject = AppendDescriptors(adjectives, Phrase);

                    if (Modifiers.Any(mod => mod.Role == GrammaticalType.Verb))
                    {
                        var vObj = Modifiers.Where(mod => mod.Role == GrammaticalType.Verb)
                                            .Select(mod => mod.ToString()).CommaList(RenderUtility.SplitListType.AllAnd);

                        sb.AppendFormat("{0} {1}", describedSubject, vObj);
                    }
                    else
                        sb.Append(describedSubject);
                    break;
            }

            return sb.ToString();
        }

        private string AppendDescriptors(IEnumerable<ILexica> adjectives, string phrase)
        {
            var described = phrase;

            if (adjectives.Count() > 0)
            {
                var decorativeString = adjectives.Where(adj => adj.Type != LexicalType.Conjunction && adj.Type != LexicalType.Interjection)
                                                 .Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma);

                var conjunctive = adjectives.FirstOrDefault(adj => adj.Type == LexicalType.Conjunction || adj.Type == LexicalType.Interjection);
                var conjunctiveString = conjunctive != null ? conjunctive.ToString() + " " : string.Empty;

                described = string.Format("{0} {1}{2}", phrase, conjunctiveString, decorativeString);
            }

            return described;
        }

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
            try
            {
                return CompareTo(other as ILexica);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return -99;
        }

        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(ILexica other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                        return -1;

                    if (other.Phrase.Equals(Phrase) && other.Type == Type)
                        return 1;

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILexica other)
        {
            if (other != default(ILexica))
            {
                try
                {
                    return other.Phrase.Equals(Phrase) && other.Type == Type;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }
        #endregion
    }
}
