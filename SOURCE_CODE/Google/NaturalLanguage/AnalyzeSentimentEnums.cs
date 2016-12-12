namespace App2.Google.NaturalLanguage.AnalyzeEntities
{
    public static class EntityEnums
    {
        public const string UNKNOWN = "UNKNOWN";        // Unknown
        public const string PERSON = "PERSON";         // Person
        public const string LOCATION = "LOCATION";       // Location
        public const string ORGANIZATION = "ORGANIZATION";   // Organization
        public const string EVENT = "EVENT";          // Event
        public const string WORK_OF_ART = "WORK_OF_ART";    // Work of art
        public const string CONSUMER_GOOD = "CONSUMER_GOOD";  // Consumer goods
        public const string OTHER = "OTHER";          // Other types
    }

    public static class EntityMentionEnums
    {
        public const string TYPE_UNKNOWN = "TYPE_UNKNOWN";   // Unknown
        public const string PROPER = "PROPER";         // Proper name
        public const string COMMON = "COMMON";         // Common noun (or noun compound)
    }
}