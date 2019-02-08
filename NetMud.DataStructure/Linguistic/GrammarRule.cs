﻿namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Relational rule From sentence construction
    /// </summary>
    public interface IGrammarRule
    {
        /// <summary>
        /// Rule applies when sentence is in this tense
        /// </summary>
        LexicalTense Tense { get; set; }

        /// <summary>
        /// Rule applies when sentence is in this perspective
        /// </summary>
        NarrativePerspective Perspective { get; set; }

        /// <summary>
        /// When the From word is this or a synonym of this (only native synonyms) this rule applies
        /// </summary>
        IDictata SpecificWord { get; set; }

        /// <summary>
        /// Applies when this type of word is the primary one
        /// </summary>
        LexicalType FromType { get; set; }

        /// <summary>
        /// This rule applies when the word is this role
        /// </summary>
        GrammaticalType FromRole { get; set; }

        /// <summary>
        /// Applies when we're trying to figure out where to put this type of word
        /// </summary>
        LexicalType ToType { get; set; }

        /// <summary>
        /// This rule applies when the adjunct word is this role
        /// </summary>
        GrammaticalType ToRole { get; set; }

        /// <summary>
        /// Can be made into a list
        /// </summary>
        bool Listable { get; set; }

        /// <summary>
        /// Place the "to" word before the From word, false means after
        /// </summary>
        bool Precedes { get; set; }

        /// <summary>
        /// The presence of these criteria changes the sentence type
        /// </summary>
        SentenceType AltersSentence { get; set; }
    }
}
