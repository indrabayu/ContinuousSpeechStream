namespace App2.Google.NaturalLanguage.AnnotateText
{
    public static class TagEnums
    {
        public const string UNKNOWN = "UNKNOWN";    // Unknown
        public const string ADJ = "ADJ";        // Adjective
        public const string ADP = "ADP";        // Adposition (preposition and postposition)
        public const string ADV = "ADV";        // Adverb
        public const string CONJ = "CONJ";       // Conjunction
        public const string DET = "DET";        // Determiner
        public const string NOUN = "NOUN";       // Noun (common and proper)
        public const string NUM = "NUM";        // Cardinal number
        public const string PRON = "PRON";       // Pronoun
        public const string PRT = "PRT";        // Particle or other function word
        public const string PUNCT = "PUNCT";      // Punctuation
        public const string VERB = "VERB";       // Verb (all tenses and modes)
        public const string X = "X";          // Other: foreign words, typos, abbreviations
        public const string AFFIX = "AFFIX";      // Affix
    }

    public static class AspectEnums
    {
        public const string ASPECT_UNKNOWN = "ASPECT_UNKNOWN"; // Aspect is not applicable in the analyzed language or is not predicted.
        public const string PERFECTIVE = "PERFECTIVE";     // Perfective
        public const string IMPERFECTIVE = "IMPERFECTIVE";   // Imperfective
        public const string PROGRESSIVE = "PROGRESSIVE";    // Progressive
    }

    public static class CaseEnums
    {
        public const string CASE_UNKNOWN = "CASE_UNKNOWN";   // Case is not applicable in the analyzed language or is not predicted.
        public const string ACCUSATIVE = "ACCUSATIVE";     // Accusative
        public const string ADVERBIAL = "ADVERBIAL";      // Adverbial
        public const string COMPLEMENTIVE = "COMPLEMENTIVE";  // Complementive
        public const string DATIVE = "DATIVE";         // Dative
        public const string GENITIVE = "GENITIVE";       // Genitive
        public const string INSTRUMENTAL = "INSTRUMENTAL";   // Instrumental
        public const string LOCATIVE = "LOCATIVE";       // Locative
        public const string NOMINATIVE = "NOMINATIVE";     // Nominative
        public const string OBLIQUE = "OBLIQUE";        // Oblique
        public const string PARTITIVE = "PARTITIVE";      // Partitive
        public const string PREPOSITIONAL = "PREPOSITIONAL";  // Prepositional
        public const string REFLEXIVE_CASE = "REFLEXIVE_CASE"; // Reflexive
        public const string RELATIVE_CASE = "RELATIVE_CASE";  // Relative
        public const string VOCATIVE = "VOCATIVE";       // Vocative
    }

    public static class FormEnums
    {
        public static string FORM_UNKNOWN = "FORM_UNKNOWN";
        public static string ADNOMIAL = "ADNOMIAL";
        public static string AUXILIARY = "AUXILIARY";
        public static string COMPLEMENTIZER = "COMPLEMENTIZER";
        public static string FINAL_ENDING = "FINAL_ENDING";
        public static string GERUND = "GERUND";
        public static string REALIS = "REALIS";
        public static string IRREALIS = "IRREALIS";
        public static string LONG = "LONG";
        public static string ORDER = "ORDER";
        public static string SPECIFIC = "SPECIFIC";
    }

    public static class GenderEnums
    {
        public static string GENDER_UNKNOWN = "GENDER_UNKNOWN";
        public static string FEMININE = "FEMININE";
        public static string MASCULINE = "MASCULINE";
        public static string NEUTER = "NEUTER";
    }

    public static class MoodEnums
    {
        public static string MOOD_UNKNOWN = "MOOD_UNKNOWN";
        public static string CONDITIONAL_MOOD = "CONDITIONAL_MOOD";
        public static string IMPERATIVE = "IMPERATIVE";
        public static string INDICATIVE = "INDICATIVE";
        public static string INTERROGATIVE = "INTERROGATIVE";
        public static string JUSSIVE = "JUSSIVE";
        public static string SUBJUNCTIVE = "SUBJUNCTIVE";
    }

    public static class NumberEnums
    {
        public static string NUMBER_UNKNOWN = "NUMBER_UNKNOWN";
        public static string SINGULAR = "SINGULAR";
        public static string PLURAL = "PLURAL";
        public static string DUAL = "DUAL";
    }

    public static class PersonEnums
    {
        public static string PERSON_UNKNOWN = "PERSON_UNKNOWN";
        public static string FIRST = "FIRST";
        public static string SECOND = "SECOND";
        public static string THIRD = "THIRD";
        public static string REFLEXIVE_PERSON = "REFLEXIVE_PERSON";
    }

    public static class ProperEnums
    {
        public static string PROPER_UNKNOWN = "PROPER_UNKNOWN";
        public static string PROPER = "PROPER";
        public static string NOT_PROPER = "NOT_PROPER";
    }

    public static class ReciprocityEnums
    {
        public static string RECIPROCITY_UNKNOWN = "RECIPROCITY_UNKNOWN";
        public static string RECIPROCAL = "RECIPROCAL";
        public static string NON_RECIPROCAL = "NON_RECIPROCAL";
    }

    public static class TenseEnums
    {
        public static string TENSE_UNKNOWN = "TENSE_UNKNOWN";
        public static string CONDITIONAL_TENSE = "CONDITIONAL_TENSE";
        public static string FUTURE = "FUTURE";
        public static string PAST = "PAST";
        public static string PRESENT = "PRESENT";
        public static string IMPERFECT = "IMPERFECT";
        public static string PLUPERFECT = "PLUPERFECT";
    }

    public static class VoiceEnums
    {
        public static string VOICE_UNKNOWN = "VOICE_UNKNOWN";
        public static string ACTIVE = "ACTIVE";
        public static string CAUSATIVE = "CAUSATIVE";
        public static string PASSIVE = "PASSIVE";
    }

    public static class DependencyEdgeEnums
    {
        public static string UNKNOWN = "UNKNOWN";
        public static string ABBREV = "ABBREV";
        public static string ACOMP = "ACOMP";
        public static string ADVCL = "ADVCL";
        public static string ADVMOD = "ADVMOD";
        public static string AMOD = "AMOD";
        public static string APPOS = "APPOS";
        public static string ATTR = "ATTR";
        public static string AUX = "AUX";
        public static string AUXPASS = "AUXPASS";
        public static string CC = "CC";
        public static string CCOMP = "CCOMP";
        public static string CONJ = "CONJ";
        public static string CSUBJ = "CSUBJ";
        public static string CSUBJPASS = "CSUBJPASS";
        public static string DEP = "DEP";
        public static string DET = "DET";
        public static string DISCOURSE = "DISCOURSE";
        public static string DOBJ = "DOBJ";
        public static string EXPL = "EXPL";
        public static string GOESWITH = "GOESWITH";
        public static string IOBJ = "IOBJ";
        public static string MARK = "MARK";
        public static string MWE = "MWE";
        public static string MWV = "MWV";
        public static string NEG = "NEG";
        public static string NN = "NN";
        public static string NPADVMOD = "NPADVMOD";
        public static string NSUBJ = "NSUBJ";
        public static string NSUBJPASS = "NSUBJPASS";
        public static string NUM = "NUM";
        public static string NUMBER = "NUMBER";
        public static string P = "P";
        public static string PARATAXIS = "PARATAXIS";
        public static string PARTMOD = "PARTMOD";
        public static string PCOMP = "PCOMP";
        public static string POBJ = "POBJ";
        public static string POSS = "POSS";
        public static string POSTNEG = "POSTNEG";
        public static string PRECOMP = "PRECOMP";
        public static string PRECONJ = "PRECONJ";
        public static string PREDET = "PREDET";
        public static string PREF = "PREF";
        public static string PREP = "PREP";
        public static string PRONL = "PRONL";
        public static string PRT = "PRT";
        public static string PS = "PS";
        public static string QUANTMOD = "QUANTMOD";
        public static string RCMOD = "RCMOD";
        public static string RCMODREL = "RCMODREL";
        public static string RDROP = "RDROP";
        public static string REF = "REF";
        public static string REMNANT = "REMNANT";
        public static string REPARANDUM = "REPARANDUM";
        public static string ROOT = "ROOT";
        public static string SNUM = "SNUM";
        public static string SUFF = "SUFF";
        public static string TMOD = "TMOD";
        public static string TOPIC = "TOPIC";
        public static string VMOD = "VMOD";
        public static string VOCATIVE = "VOCATIVE";
        public static string XCOMP = "XCOMP";
        public static string SUFFIX = "SUFFIX";
        public static string TITLE = "TITLE";
        public static string ADVPHMOD = "ADVPHMOD";
        public static string AUXCAUS = "AUXCAUS";
        public static string AUXVV = "AUXVV";
        public static string DTMOD = "DTMOD";
        public static string FOREIGN = "FOREIGN";
        public static string KW = "KW";
        public static string LIST = "LIST";
        public static string NOMC = "NOMC";
        public static string NOMCSUBJ = "NOMCSUBJ";
        public static string NOMCSUBJPASS = "NOMCSUBJPASS";
        public static string NUMC = "NUMC";
        public static string COP = "COP";
        public static string DISLOCATED = "DISLOCATED";
    }
}