namespace App2.Google
{
    public class GoogleSettings
    {
        public const string SampleRate = "16000";  //48000 //44100; //32000; //22050; //16000; //8000; 
        public const string SpeechLanguage = "id-ID"; //"id-ID"; //"ja"; //"en-US"; //"sv"; //"nl"; //"en-IN"; //"en-ID";
        public static string TranslateFrom { get { return SpeechLanguage.Substring(0, 2).ToLower(); } }
        public const string TranslateTo = "en";
    }
}