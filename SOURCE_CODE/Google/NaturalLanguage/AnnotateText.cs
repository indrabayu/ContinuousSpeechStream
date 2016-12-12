// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace App2.Google.NaturalLanguage.AnnotateText
{

    public class Text
    {

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("beginOffset")]
        public int BeginOffset { get; set; }
    }

    public class Sentiment
    {

        [JsonProperty("polarity")]
        public int Polarity { get; set; }

        [JsonProperty("magnitude")]
        public double Magnitude { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }
    }

    public class Sentence
    {

        [JsonProperty("text")]
        public Text Text { get; set; }

        [JsonProperty("sentiment")]
        public Sentiment Sentiment { get; set; }
    }

    public class Text2
    {

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("beginOffset")]
        public int BeginOffset { get; set; }
    }

    public class PartOfSpeech
    {

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("aspect")]
        public string Aspect { get; set; }

        [JsonProperty("case")]
        public string Case { get; set; }

        [JsonProperty("form")]
        public string Form { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("mood")]
        public string Mood { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("person")]
        public string Person { get; set; }

        [JsonProperty("proper")]
        public string Proper { get; set; }

        [JsonProperty("reciprocity")]
        public string Reciprocity { get; set; }

        [JsonProperty("tense")]
        public string Tense { get; set; }

        [JsonProperty("voice")]
        public string Voice { get; set; }
    }

    public class DependencyEdge
    {

        [JsonProperty("headTokenIndex")]
        public int HeadTokenIndex { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public class Token
    {

        [JsonProperty("text")]
        public Text2 Text { get; set; }

        [JsonProperty("partOfSpeech")]
        public PartOfSpeech PartOfSpeech { get; set; }

        [JsonProperty("dependencyEdge")]
        public DependencyEdge DependencyEdge { get; set; }

        [JsonProperty("lemma")]
        public string Lemma { get; set; }
    }

    public class Metadata
    {

        [JsonProperty("mid")]
        public string Mid { get; set; }

        [JsonProperty("wikipedia_url")]
        public string WikipediaUrl { get; set; }
    }

    public class Text3
    {

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("beginOffset")]
        public int BeginOffset { get; set; }
    }

    public class Mention
    {

        [JsonProperty("text")]
        public Text3 Text { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Entity
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("salience")]
        public double Salience { get; set; }

        [JsonProperty("mentions")]
        public Mention[] Mentions { get; set; }
    }

    public class DocumentSentiment
    {

        [JsonProperty("polarity")]
        public int Polarity { get; set; }

        [JsonProperty("magnitude")]
        public double Magnitude { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }
    }

    public class annotateText
    {

        [JsonProperty("sentences")]
        public Sentence[] Sentences { get; set; }

        [JsonProperty("tokens")]
        public Token[] Tokens { get; set; }

        [JsonProperty("entities")]
        public Entity[] Entities { get; set; }

        [JsonProperty("documentSentiment")]
        public DocumentSentiment DocumentSentiment { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }
    }

}