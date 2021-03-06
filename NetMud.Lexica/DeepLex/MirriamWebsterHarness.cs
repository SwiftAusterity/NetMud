﻿using NetMud.DataAccess;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace NetMud.Lexica.DeepLex
{
    public class MirriamWebsterHarness
    {
        private string DictionaryEndpoint => "https://www.dictionaryapi.com/api/v3/references/collegiate/json/";
        private string ThesaurusEndpoint => "https://www.dictionaryapi.com/api/v3/references/thesaurus/json/";
        private string DictionaryKey { get; }
        private string ThesaurusKey { get; }
        private JsonSerializer Serializer { get; }

        public int MaxAttempts { get; private set; }
        public int DictionaryAttempts { get; private set; }
        public int ThesaurusAttempts { get; private set; }

        public MirriamWebsterHarness(string dictionaryKey, string thesaurusKey)
        {
            DictionaryKey = dictionaryKey;
            ThesaurusKey = thesaurusKey;
            DictionaryAttempts = 0;
            ThesaurusAttempts = 0;
            MaxAttempts = 1000;
            Serializer = SerializationUtility.GetSerializer();
        }

        public DictionaryEntry GetDictionaryEntry(string word)
        {
            if (DictionaryAttempts >= MaxAttempts)
            {
                return null;
            }

            DictionaryAttempts++;
            var jsonString = GetResponse(DictionaryEndpoint, word, DictionaryKey);

            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                StringReader reader = new StringReader(jsonString);

                var entryCollection = Serializer.Deserialize(reader, typeof(List<DictionaryEntry>)) as List<DictionaryEntry>;

                return entryCollection.FirstOrDefault(entry => entry.meta.id.Strip(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "_", "#" })
                                                                            .Equals(word, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        public ThesaurusEntry GetThesaurusEntry(string word)
        {
            if (ThesaurusAttempts >= MaxAttempts)
            {
                return null;
            }

            ThesaurusAttempts++;
            var jsonString = GetResponse(ThesaurusEndpoint, word, ThesaurusKey);

            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                StringReader reader = new StringReader(jsonString);

                var entryCollection = Serializer.Deserialize(reader, typeof(List<ThesaurusEntry>)) as List<ThesaurusEntry>;

                return entryCollection.FirstOrDefault(entry => entry.meta.id.Strip(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "_", "#" } )
                                                                            .Equals(word, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        private string GetResponse(string baseUri, string searchName, string apiKey)
        {
            var jsonResponse = string.Empty;
            var uriParams = string.Format("?key={0}", apiKey);

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseUri + searchName)
            };

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                // List data response.
                HttpResponseMessage response = client.GetAsync(uriParams).Result;
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body.
                    jsonResponse = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    LoggingUtility.Log(string.Format("Deep Lex Failure: {0} ({1})", (int)response.StatusCode, response.ReasonPhrase), LogChannels.SystemWarnings);
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
            finally
            {
                client.Dispose();
            }

            return jsonResponse;
        }
    }
}
