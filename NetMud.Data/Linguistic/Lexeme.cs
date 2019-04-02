﻿using NetMud.Communication.Lexical;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.ActorBase;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Linguistic
{
    public class Lexeme : ConfigData, ILexeme, IComparable<ILexeme>, IEquatable<ILexeme>, IEqualityComparer<ILexeme>
    {
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.ReviewOnly;

        /// <summary>
        /// The unique key used to identify, store and retrieve data
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string UniqueKey => string.Format("{0}_{1}", Language.Name, Name);

        /// <summary>
        /// Type of configuation data this is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.Dictionary;

        /// <summary>
        /// The unique name of this configuration data
        /// </summary>
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 1)]
        [Display(Name = "Word", Description = "The actual word or phrase at hand.")]
        [DataType(DataType.Text)]
        [Required]
        public override string Name { get; set; }

        /// <summary>
        /// Has this been mapped by the synset already
        /// </summary>
        [Display(Name = "Mapped", Description = "Has this word been SynSet mapped? (changing this can be damagaing to the synonym network)")]
        [UIHint("Boolean")]
        public bool IsSynMapped { get; set; }

        [JsonProperty("Language")]
        private ConfigDataCacheKey _language { get; set; }

        /// <summary>
        /// The language this is derived from
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Language", Description = "The language this is in.")]
        [UIHint("LanguageList")]
        [LanguageDataBinder]
        [Required]
        public ILanguage Language
        {
            get
            {
                if (_language == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<ILanguage>(_language);
            }
            set
            {
                if (value == null)
                {
                    _language = null;
                    return;
                }

                _language = new ConfigDataCacheKey(value);
            }
        }

        /// <summary>
        /// Individual meanings and types under this
        /// </summary>
        [DictataCollectionDataBinder]
        public HashSet<IDictata> WordForms { get; set; }

        [JsonConstructor]
        public Lexeme()
        {
            WordForms = new HashSet<IDictata>();
            Name = string.Empty;

            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            if (globalConfig?.BaseLanguage == null)
            {
                Language = ConfigDataCache.GetAll<ILanguage>().FirstOrDefault();
            }
            else
            {
                Language = globalConfig.BaseLanguage;
            }
        }

        /// <summary>
        /// What types exist within the valid wordforms
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LexicalType> ContainedTypes()
        {
            return WordForms.Select(form => form.WordType).Distinct();
        }


        /// <summary>
        /// Add a new word form to this lexeme
        /// </summary>
        /// <param name="newWord">The word</param>
        /// <returns>the word with changes</returns>
        public IDictata AddNewForm(IDictata newWord)
        {
            var existingWords = WordForms;

            //Easy way - we dont have one with this type at all
            //Hard way - reject if our semantics are similar by count and the semantics lists match
            if (!existingWords.Any(form => form.WordType == newWord.WordType) 
             || (newWord.Semantics.Count() > 0 && !existingWords.Where(form => form.WordType == newWord.WordType)
                            .Any(form => form.Semantics.Count() == newWord.Semantics.Count() && form.Semantics.All(semantic => newWord.Semantics.Contains(semantic)))))
            {
                var maxForm = 0;

                if (existingWords.Any())
                {
                    maxForm = existingWords.Max(form => form.FormGroup);
                }

                newWord.FormGroup = (short)(maxForm + 1);
                existingWords.Add(newWord);
                WordForms = existingWords;
                SystemSave();
                PersistToCache();
            }

            return newWord;
        }

        /// <summary>
        /// Add language translations for this
        /// </summary>
        public void FillLanguages()
        {
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            //Don't do this if: we have no config, translation is turned off or lacking in the azure key, the language is not a human-ui language
            //it isn't an approved language, the word is a proper noun or the language isnt the base language at all
            if (globalConfig == null || !globalConfig.TranslationActive || string.IsNullOrWhiteSpace(globalConfig.AzureTranslationKey)
                || !Language.UIOnly || !Language.SuitableForUse || ContainedTypes().Contains(LexicalType.ProperNoun) || Language != globalConfig.BaseLanguage)
            {
                return;
            }

            var otherLanguages = ConfigDataCache.GetAll<ILanguage>().Where(lang => lang != Language && lang.SuitableForUse && lang.UIOnly);

            foreach (var language in otherLanguages)
            {
                short formGrouping = -1;
                var newName = string.Empty;

                var newLexeme = new Lexeme()
                {
                    Language = language
                };

                foreach (var word in WordForms)
                {
                    var context = new LexicalContext(null)
                    {
                        Language = language,
                        Perspective = word.Perspective,
                        Tense = word.Tense,
                        Position = word.Positional,
                        Determinant = word.Determinant,
                        Plural = word.Plural,
                        Possessive = word.Possessive,
                        Elegance = word.Elegance,
                        Quality = word.Quality,
                        Semantics = word.Semantics,
                        Severity = word.Severity,
                        GenderForm = new Gender() { Feminine = word.Feminine }
                    };

                    var translatedWord = Thesaurus.GetSynonym(word, context);

                    //no linguistic synonym
                    if (translatedWord == this)
                    {
                        var newWord = Thesaurus.GetTranslatedWord(globalConfig.AzureTranslationKey, Name, Language, language);

                        if (!string.IsNullOrWhiteSpace(newWord))
                        {
                            newName = newWord;
                            newLexeme.Name = newName;

                            var newDictata = new Dictata(newWord, formGrouping++)
                            {
                                Elegance = word.Elegance,
                                Severity = word.Severity,
                                Quality = word.Quality,
                                Determinant = word.Determinant,
                                Plural = word.Plural,
                                Perspective = word.Perspective,
                                Feminine = word.Feminine,
                                Positional = word.Positional,
                                Possessive = word.Possessive,
                                Semantics = word.Semantics,
                                Antonyms = word.Antonyms,
                                Synonyms = word.Synonyms,
                                PhraseAntonyms = word.PhraseAntonyms,
                                PhraseSynonyms = word.PhraseSynonyms,
                                Tense = word.Tense,
                                WordType = word.WordType
                            };

                            newDictata.Synonyms = new HashSet<IDictata>(word.Synonyms) { word };
                            word.Synonyms = new HashSet<IDictata>(word.Synonyms) { newDictata };
                            newLexeme.AddNewForm(newDictata);
                        }
                    }
                }

                if(newLexeme.WordForms.Count() > 0)
                {
                    newLexeme.SystemSave();
                    newLexeme.PersistToCache();
                }
            }

            SystemSave();
            PersistToCache();
        }


        /// <summary>
        /// Get a wordform by grouping id
        /// </summary>
        /// <param name="formGroup">the form grouping id</param>
        /// <returns>the word</returns>
        public IDictata GetForm(short formGroup)
        {
            return WordForms.FirstOrDefault(form => form.FormGroup == formGroup);
        }

        /// <summary>
        /// Get a wordform by grouping id
        /// </summary>
        /// <param name="wordType">the lexical type of the word</param>
        /// <param name="formGroup">the form grouping id</param>
        /// <returns>the word</returns>
        public IDictata GetForm(LexicalType wordType, short formGroup = -1)
        {
            return WordForms.FirstOrDefault(form => form.WordType == wordType && (formGroup < 0 || form.FormGroup == formGroup));
        }

        /// <summary>
        /// Map the synnet of this word
        /// </summary>
        public void MapSynNet(bool cascade = false)
        {
            //Not a whole lot of point here
            if (IsSynMapped)
            {
                return;
            }

            foreach (var dict in WordForms)
            {
                LexicalProcessor.GetSynSet(dict);
            }

            //We've been mapped, set it and save the state
            //IsSynMapped = true;
            //PersistToCache();
            //SystemSave();
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
                return CompareTo(other as ILexeme);
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
        public int CompareTo(ILexeme other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                    {
                        return -1;
                    }

                    if (other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return 1;
                    }

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
        public bool Equals(ILexeme other)
        {
            if (other != default(ILexeme))
            {
                try
                {
                    return other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILexeme x, ILexeme y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(ILexeme obj)
        {
            return obj.GetType().GetHashCode() + obj.Language.Name.GetHashCode() + obj.Name.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + Language.Name.GetHashCode() + Name.GetHashCode();
        }

        public override object Clone()
        {
            return new Lexeme
            {
                Language = Language,
                Name = Name,
                WordForms = new HashSet<IDictata>(WordForms.Select(form => form.Clone()))
            };
        }
        #endregion

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Forms", string.Join(", ", ContainedTypes()));

            return returnList;
        }

        #region Data persistence functions
        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public override bool Remove(IAccount remover, StaffRank rank)
        {
            var removalState = base.Remove(remover, rank);

            if (removalState)
            {
                var synonyms = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.Synonyms.Any(syn => syn.Equals(this)));
                var antonyms = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.Antonyms.Any(ant => ant.Equals(this)));

                foreach (var word in synonyms)
                {
                    var syns = new HashSet<IDictata>(word.Synonyms);
                    syns.RemoveWhere(syn => syn.Equals(this));
                    word.Synonyms = syns;
                }

                foreach (var word in antonyms)
                {
                    var ants = new HashSet<IDictata>(word.Antonyms);
                    ants.RemoveWhere(syn => syn.Equals(this));
                    word.Antonyms = ants;
                }

                var synonymPhrases = ConfigDataCache.GetAll<IDictataPhrase>().Where(dict => dict.Synonyms.Any(syn => syn.Equals(this)));
                var antonymPhrases = ConfigDataCache.GetAll<IDictataPhrase>().Where(dict => dict.Antonyms.Any(ant => ant.Equals(this)));

                foreach (var phrase in synonymPhrases)
                {
                    var syns = new HashSet<IDictata>(phrase.Synonyms);
                    syns.RemoveWhere(syn => syn.Equals(this));
                    phrase.Synonyms = syns;
                    phrase.Save(remover, rank);
                }

                foreach (var phrase in antonymPhrases)
                {
                    var ants = new HashSet<IDictata>(phrase.Antonyms);
                    ants.RemoveWhere(syn => syn.Equals(this));
                    phrase.Antonyms = ants;
                    phrase.Save(remover, rank);
                }

                var containedPhrases = ConfigDataCache.GetAll<IDictataPhrase>().Where(dict => dict.Words.Any(syn => syn.Equals(this)));

                foreach (var phrase in containedPhrases)
                {
                    phrase.Remove(remover, rank);
                }
            }

            return removalState;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool Save(IAccount editor, StaffRank rank)
        {
            var removalState = base.Remove(editor, rank);

            if (removalState)
            {
                return base.Save(editor, rank);
            }

            return false;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool SystemSave()
        {
            var removalState = base.SystemRemove();

            if (removalState)
            {
                return base.SystemSave();
            }

            return false;
        }

        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public override bool SystemRemove()
        {
            var removalState = base.SystemRemove();

            if (removalState)
            {
                var synonyms = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.Synonyms.Any(syn => syn.Equals(this)));
                var antonyms = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.Antonyms.Any(ant => ant.Equals(this)));
                foreach (var word in synonyms)
                {
                    var syns = new HashSet<IDictata>(word.Synonyms);
                    syns.RemoveWhere(syn => syn.Equals(this));
                    word.Synonyms = syns;
                }

                foreach (var word in antonyms)
                {
                    var ants = new HashSet<IDictata>(word.Antonyms);
                    ants.RemoveWhere(syn => syn.Equals(this));
                    word.Antonyms = ants;
                }

                var synonymPhrases = ConfigDataCache.GetAll<IDictataPhrase>().Where(dict => dict.Synonyms.Any(syn => syn.Equals(this)));
                var antonymPhrases = ConfigDataCache.GetAll<IDictataPhrase>().Where(dict => dict.Antonyms.Any(ant => ant.Equals(this)));

                foreach (var phrase in synonymPhrases)
                {
                    var syns = new HashSet<IDictata>(phrase.Synonyms);
                    syns.RemoveWhere(syn => syn.Equals(this));
                    phrase.Synonyms = syns;
                    phrase.SystemSave();
                }

                foreach (var phrase in antonymPhrases)
                {
                    var ants = new HashSet<IDictata>(phrase.Antonyms);
                    ants.RemoveWhere(syn => syn.Equals(this));
                    phrase.Antonyms = ants;
                    phrase.SystemSave();
                }

                var containedPhrases = ConfigDataCache.GetAll<IDictataPhrase>().Where(dict => dict.Words.Any(syn => syn.Equals(this)));

                foreach (var phrase in containedPhrases)
                {
                    phrase.SystemRemove();
                }
            }

            return removalState;
        }
        #endregion

    }
}