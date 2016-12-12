using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using ApiAi.Common;
using ApiAiSDK;
using App2.ApiDotAi;
using App2.Google;
using App2.Google.NaturalLanguage;
using App2.Google.NaturalLanguage.AnalyzeEntities;
using App2.Google.NaturalLanguage.AnalyzeSentiment;
using App2.Google.NaturalLanguage.AnnotateText;
using App2.Google.Search;
using App2.Google.Speech;
using App2.Google.Translate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace App2
{
    [Activity(Label = "CSS", ScreenOrientation = ScreenOrientation.Locked, MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        #region upper section

        static int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            Scenario_7(); //recording & playing audio using AudioRecorder
        }

        #endregion

        Button _start;
        Button _stop;
        ScrollView _scrollView1;
        ScrollView _scrollView2;
        TextView _textView1;
        TextView _textView2;
        TextView _textView3;

        short[] audioBuffer = null;
        List<short> currentRecoringSession = new List<short>();
        Queue<Tuple<List<short>, long>> recoringSessions = new Queue<Tuple<List<short>, long>>();
        AudioRecord audioRecord = null;
        AudioTrack audioTrack = null; //playback
        bool endRecording = false;
        bool isRecording = false;
        const string PrerecordedAudioFile = "sdcard/Music/audio.wav";
        Func<string> GetNewTranscriptionFile = () => $"sdcard/Music/{DateTime.Now.Month.ToString("00")}{DateTime.Now.Day.ToString("00")} {DateTime.Now.Hour.ToString("00")}{DateTime.Now.Minute.ToString("00")}{DateTime.Now.Second.ToString("00")}.txt";
        Func<bool> UseMic = () => !File.Exists(PrerecordedAudioFile); //if the file doesn't exist, use mic
        long currentTicks = 0;

        readonly int SampleRate = int.Parse(GoogleSettings.SampleRate); //44100; //32000; //22050; //16000; //8000; 
        readonly double DefinitionOfPauseInSeconds = 0.25 * (int.Parse(GoogleSettings.SampleRate) / 8000);

        System.Text.StringBuilder scenario8;
        Dictionary<string, Tuple<int/*occurenceCount*/, long/*lastTimeUpdated*/>> keywords;
        long AgeOfKeyword = TimeSpan.FromHours(1).Ticks;
        TextMatching textMatching;
        const bool doNextWordPrediction = false;

        ChannelIn channelIn = ChannelIn.Mono;
        ChannelOut channelOut = ChannelOut.Mono;
        Encoding encoding = Encoding.Pcm16bit;
        AudioTrackMode audioTrackMode = AudioTrackMode.Stream;

        //const short FILTER_FREQ_LOW = short.MinValue;
        //const short FILTER_FREQ_HIGH = short.MaxValue;

        /* LIVE speaking to mic */
        const short FILTER_FREQ_LOW = -5000; //-2000; -5000; -8000;
        const short FILTER_FREQ_HIGH = 5000; // 2000;  5000;  8000; 

        /* Audio FILE - Human Voice */
        //const short FILTER_FREQ_LOW = -500;
        //const short FILTER_FREQ_HIGH = 500;

        /* MICROPHONE CHECK */
        //const short FILTER_FREQ_LOW = -1;
        //const short FILTER_FREQ_HIGH = 1;

        private void Scenario_7()
        {
            _start = FindViewById<Button>(Resource.Id._startButton);
            _stop = FindViewById<Button>(Resource.Id._stopButton);
            _scrollView1 = FindViewById<ScrollView>(Resource.Id._scrollView1);
            _scrollView2 = FindViewById<ScrollView>(Resource.Id._scrollView2);
            _textView1 = FindViewById<TextView>(Resource.Id._textView1);
            _textView2 = FindViewById<TextView>(Resource.Id._textView2);
            _textView3 = FindViewById<TextView>(Resource.Id._textView3);

            _textView1.MovementMethod = new ScrollingMovementMethod();
            _textView1.Gravity = GravityFlags.VerticalGravityMask;
            _textView1.SetTextIsSelectable(false);

            _textView2.MovementMethod = new ScrollingMovementMethod();
            _textView2.Gravity = GravityFlags.VerticalGravityMask;
            _textView2.SetTextIsSelectable(false);

            _stop.Visibility = ViewStates.Gone;

            //var documentsPath = "sdcard/DataSetARFF/";// System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            //scen7_fullPath = System.IO.Path.Combine(documentsPath, scen7_fileName);
            //if (File.Exists(scen7_fullPath))
            //    File.Delete(scen7_fullPath);
            ////File.Create(scen7_fullPath);

            RecordingStateChanged = (bool isRecording) =>
            {
                if (!isRecording)
                {
                    _start.Enabled = true;
                }
            };

            _start.Click += async delegate
            {
                await StartButton_OnClick();
            };

            _stop.Click += delegate
            {
                Stop();
                _start.Visibility = ViewStates.Visible;
                _start.Enabled = true;
                _stop.Visibility = ViewStates.Gone;
                _stop.Enabled = false;
                File.WriteAllText(GetNewTranscriptionFile(), _textView2.Text, System.Text.Encoding.UTF8);
            };
        }

        private async Task StartButton_OnClick()
        {
            _start.Enabled = false;
            _start.Visibility = ViewStates.Gone;
            _stop.Enabled = true;
            _stop.Visibility = ViewStates.Visible;
            _textView1.Text = string.Empty;
            _textView2.Text = string.Empty;
            scenario8 = new System.Text.StringBuilder();
            keywords = new Dictionary<string, Tuple<int, long>>();
            currentTicks = UseMic() ? DateTime.Now.Ticks : 0;
            count = 0;
            textMatching = new TextMatching();
            await Task.Delay(10);
            await StartRecorderAsync(useMic: UseMic());
        }

        public Action<bool> RecordingStateChanged;

        protected async Task StartRecorderAsync(bool useMic)
        {
            endRecording = false;
            isRecording = true;

            RaiseRecordingStateChangedEvent();

            audioBuffer = new short[256]; /* you can experiment with 256 or 255 */

            if (useMic)
            {
                /* don't change anything from these two lines! */
                int buffsize = AudioRecord.GetMinBufferSize(SampleRate, channelIn, encoding);
                audioRecord = new AudioRecord(AudioSource.Mic, SampleRate, channelIn, encoding, buffsize);

                audioRecord.StartRecording();
            }
            // Off line this so that we do not block the UI thread.
            await ReadAudioAsync(useMic);
        }

        void DEBUG_FREQUENCY()
        {
            var _range = audioBuffer.Select(snd => Convert.ToInt16((~snd | 1)));
            var min = _range.Min();
            var max = _range.Max();
            if (min < FILTER_FREQ_LOW && max > FILTER_FREQ_HIGH)
            {
                string text = min + "\t\t" + max;
                _textView3.Text = text;
            }
        }

        async Task ReadAudioAsync(bool useMic)
        {
            short[] wholeAudioFromFile = null;
            int lenSoFarFromFile = 0;
            if (!useMic)
            {
                Func<string, short[]> ReadWaveFile = (path) =>
                {
                    var waveArr = File.ReadAllBytes(path).Skip(44).ToArray();

                    short[] rawArr = new short[waveArr.Length / 2];

                    for (int idxWave = 0, idxRaw = 0; idxWave < waveArr.Length; idxRaw += 1, idxWave += 2)
                    {
                        byte tail_byte = waveArr[idxWave];
                        byte head_byte = waveArr[idxWave + 1];
                        short val = (short)((head_byte * 256) + tail_byte);
                        rawArr[idxRaw] = val;
                    }

                    return rawArr;
                };

                wholeAudioFromFile = ReadWaveFile(PrerecordedAudioFile);
                //var wave = new Wave(16000, 1, wholeAudioFromFile, 0, wholeAudioFromFile.Length - 1);
                //wave.writeToFile("sdcard/Music/i_am_a_copy.wav");
            }

            while (true)
            {
                if (endRecording)
                {
                    endRecording = false;
                    break;
                }

                // Keep reading the buffer while there is audio input.
                if (useMic)
                {
                    int numBytes = await audioRecord.ReadAsync(audioBuffer, 0, audioBuffer.Length);
                }
                else
                {
                    if (lenSoFarFromFile + audioBuffer.Length >= wholeAudioFromFile.Length)
                    {
                        break;
                    }
                    Array.Copy(wholeAudioFromFile, lenSoFarFromFile, audioBuffer, 0, audioBuffer.Length);
                    lenSoFarFromFile += audioBuffer.Length;
                }

                if (useMic)
                {
                    currentTicks = DateTime.Now.Ticks;
                }
                else
                {
                    currentTicks += (long)((32000 / ((double)SampleRate)) * TimeSpan.FromMilliseconds(audioBuffer.Length / 32).Ticks);
                }

                await ProcessIncomingStream(useMic);


                // Do something with the audio input.
                //_textArea.Append($"[{count}] There's an incoming sound on {DateTime.Now.ToLongTimeString()}\n");
                //count++;

                if (!doNextWordPrediction)
                {
                    DEBUG_FREQUENCY();
                    //_textArea.Append($"[{list_shorts.Count}] \n");
                }
            }

            if (useMic)
            {
                audioRecord.Release();
            }
            else
            {
                _stop.PerformClick();
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            isRecording = false;

            RaiseRecordingStateChangedEvent();
        }

        private bool isSilence(short snd)
        {
            // 2's complement to normal signed value
            if (snd != 0)
                snd = Convert.ToInt16((~snd | 1));

            return snd > FILTER_FREQ_LOW && snd < FILTER_FREQ_HIGH;
        }

        private async Task ProcessIncomingStream(bool useMic)
        {
            currentRecoringSession.AddRange(audioBuffer);

            Func<double, short> inRecordingSeconds = (duration) => (short)(duration * 10000);
            int minimumRecordingSeconds = inRecordingSeconds(DefinitionOfPauseInSeconds);// 0.25 for 8000, 0.5 for 16000, 1.4 for 44100
            if (currentRecoringSession.Count > minimumRecordingSeconds)
            {
                int split = currentRecoringSession.Count - minimumRecordingSeconds;

                bool hasSilenceRecently = currentRecoringSession.Skip(split).All(isSilence);
                if (hasSilenceRecently)
                {
                    bool wasSilent = currentRecoringSession.Take(split).All(isSilence);
                    if (wasSilent)
                    {
                        currentRecoringSession.Clear();
                    }
                    else
                    {
                        //create new recording session
                        recoringSessions.Enqueue(new Tuple<List<short>, long>(currentRecoringSession, currentTicks));
                        currentRecoringSession = new List<short>();


                        /* MAJOR BREAKTHROUGH: have garbage collector run after transcribing!!! 
                         *      GC.Collect(2, GCCollectionMode.Forced, true);
                         */
                        if (useMic)
                        {
                            /* NEVER EVER await the following line. It's on purpose to be parallel! 
                             * Except if you're using Thread instead of Task...
                             */

                            new Thread(
                                //async 
                                () =>
                                {
                                    //await 
                                    TranscribeAndPlayback(playback: !useMic); GC.Collect(2, GCCollectionMode.Forced, true);
                                })
                            { IsBackground = false, Priority = System.Threading.ThreadPriority.Normal }.Start();
                        }
                        else
                        {
                            /* This one SHOULD be awaited, otherwise too many http connections're opened simultaneously */

                            //TranscribeAndPlayback(playback: !useMic); GC.Collect(2, GCCollectionMode.Forced, true);
                            await TranscribeAndPlayback(playback: !useMic); GC.Collect(2, GCCollectionMode.Forced, true);
                        }
                        //Task.Factory.StartNew(async () => { await TranscribeAndPlayback(); GC.Collect(2, GCCollectionMode.Forced, true); }, TaskCreationOptions.PreferFairness);
                        #region Threading options for the Async transcription
                        //TranscribeAndPlayback();
                        //new Thread(() => TranscribeAndPlayback()) { IsBackground = false, Priority = System.Threading.ThreadPriority.Normal }.Start();
                        //new Thread(() => TranscribeAndPlayback()) { IsBackground = true, Priority = System.Threading.ThreadPriority.Normal }.Start();
                        //Task.Run(() => TranscribeAndPlayback());
                        //Task.Factory.StartNew(() => TranscribeAndPlayback());
                        //Task.Factory.StartNew(() => TranscribeAndPlayback(), TaskCreationOptions.PreferFairness);
                        //Task.Factory.StartNew(() => TranscribeAndPlayback()).Unwrap();
                        //Task.Factory.StartNew(() => TranscribeAndPlayback(), TaskCreationOptions.PreferFairness).Unwrap();

                        //new Thread(async () => await TranscribeAndPlayback()) { IsBackground = false, Priority = System.Threading.ThreadPriority.Normal }.Start();
                        //new Thread(async () => await TranscribeAndPlayback()) { IsBackground = true, Priority = System.Threading.ThreadPriority.Normal }.Start();
                        //Task.Run(async () => await TranscribeAndPlayback());
                        //Task.Factory.StartNew(async () => await TranscribeAndPlayback());
                        //Task.Factory.StartNew(async () => await TranscribeAndPlayback(), TaskCreationOptions.PreferFairness); //when paired with en-US, this one doesn't forget 10 when you say 1..10 :)
                        //Task.Factory.StartNew(async () => await TranscribeAndPlayback()).Unwrap();
                        //Task.Factory.StartNew(async () => await TranscribeAndPlayback(), TaskCreationOptions.PreferFairness).Unwrap();
                        #endregion
                    }
                }
            }
        }

        public async Task TranscribeAndPlayback(bool playback)
        //public async void TranscribeAndPlayback(bool playback)
        {
            #region not our main concern
            Tuple<List<short>, long> sessionToPlayback = null;
            lock (recoringSessions)
                sessionToPlayback = recoringSessions.Dequeue();

            short[] buffer = sessionToPlayback.Item1.ToArray();
            var wave = new Wave(SampleRate, 1, buffer, 0, buffer.Length - 1);
            //wave.writeToFile(mp3);

            //if (playback)
            //    await PlayAudioTrackAsync(buffer);

            //return; //testing purpose
            #endregion
            /**********************************************************************************************************************************/
            try
            {
                // Google Speech API (v1beta1)
                // Google Speech API (Chromium v2)
                // CSS
                // Stanford NLP Parser
                // Google Natural Language API: analyzeEntities
                // Google Natural Language API: analyzeSentiment
                // Google Natural Language API: annotateText
                // Google Natural Language API: annotateText (colored)
                // Google Search I'm feeling lucky (get the URL)
                // Thesaurus Synonim
                // API.AI
                // Google Search Mispelling Suggestion
                var Post_SpeechRecognition_Action = new string[] {
                    //"Google Speech API (v1beta1)",
                    "Google Speech API (Chromium v2)",
                    "Google Translate API",
                    "CSS"
                };
                string google_result = "";
                string original_transcription = ""; //this is a backup for the transcription result
                bool printGoogleSpeechAndTranslateResult = false;

                string output = string.Empty;
                //var talkDuration = TimeSpan.FromMilliseconds(buffer.Length / 32);
                var talkDuration = TimeSpan.FromTicks((long)((32000 / ((double)SampleRate)) * TimeSpan.FromMilliseconds(buffer.Length / 32).Ticks));
                long endTimeInTicks = sessionToPlayback.Item2;
                long startTimeInTicks = endTimeInTicks - talkDuration.Ticks;

                TimeSpan startTime = TimeSpan.FromTicks(startTimeInTicks);
                TimeSpan endTime = TimeSpan.FromTicks(endTimeInTicks);

                #region not our concern
                if (Post_SpeechRecognition_Action.Contains("Google Speech API (v1beta1)"))
                {
                    string[] inofficialWords = new string[] { "makanan", "minuman", "karage", "katsu", "udon" };
                    var resultBeta1 = (await Google.Speech.SpeechV1Beta1Utils.RequestGoogleSpeechAPIAsync(wave.output, inofficialWords));
                    if (!(resultBeta1 == null || resultBeta1.Results == null || resultBeta1.Results.Length == 0))
                    {
                        //google_result = resultBeta1.Results.SelectMany(_ => _.Alternatives).OrderByDescending(_ => _.Confidence).First().Transcript;
                        google_result = resultBeta1.Results[0].Alternatives[0].Transcript;
                        original_transcription = google_result;

                        if (printGoogleSpeechAndTranslateResult)
                        {
                            RunOnUiThread(() => _textView1.Append($"[Google\tv1] {google_result}\n"));
                        }
                    }
                }

                if (Post_SpeechRecognition_Action.Contains("Google Speech API (Chromium v2)"))
                {
                    string json = //ConsumeWCF.Simple_WCF_Client.GoogleSpeechV2Using16KHz16BitMonoWave(wave.output); 
                                  await Google.Speech.Chromium.SpeechV2Utils.RequestGoogleSpeechAPIAsync(wave.output);
                    google_result = Google.Speech.Chromium.SpeechV2Utils.ParseJson(json);
                    original_transcription = google_result;

                    if (printGoogleSpeechAndTranslateResult)
                    {
                        RunOnUiThread(() => _textView1.Append($"[Chrome\tv2] {google_result}\n"));
                    }
                }

                if (string.IsNullOrEmpty(google_result)) return;

                if (Post_SpeechRecognition_Action.Contains("Google Translate API"))
                {
                    if (!GoogleSettings.TranslateFrom.Equals(GoogleSettings.TranslateTo))
                    {
                        var translation = JsonConvert.DeserializeObject<translations>(await TranslateUtils.Translate(google_result, GoogleSettings.TranslateFrom, GoogleSettings.TranslateTo)).Data.Translations[0].TranslatedText;

                        if (printGoogleSpeechAndTranslateResult)
                            output += $"[Google Translate API (from {GoogleSettings.TranslateFrom} to {GoogleSettings.TranslateTo})]\n\n{translation}\n\n";
                        else
                            google_result = translation;
                    }
                }
                #endregion

                if (Post_SpeechRecognition_Action.Contains("CSS"))
                {
                    try
                    {
                        var _annotateText = await NaturalLanguageUtils.Execute_AnnotateText(google_result);

                        var tokenIndicesForExclusion = new bool[_annotateText.Tokens.Length];
                        var lengthOfExclusions = new int?[_annotateText.Tokens.Length];
                        var exclusions = new string[_annotateText.Tokens.Length];

                        var knownEntities = _annotateText.Entities.Select(entity => entity.Name);
                        var knownTokens = _annotateText.Tokens.Select(token => token.Text.Content);
                        var knownLemmas = new List<string>();

                        {
                            //I'm giving a timeout to how long the system remembers the keywords
                            //Otherwise there will be too many keywords stored in the dictionary
                            var outdatedKeywords = keywords.Where(kvp => (startTimeInTicks - kvp.Value.Item2) > AgeOfKeyword).Select(kvp => kvp.Key).ToList();

                            //TODO: insert pre-delete action here
                            //  e.g. Action 1: sending such keywords to a server database

                            outdatedKeywords.ForEach(keyword => keywords.Remove(keyword));
                            if (keywords.Count == 0)
                                textMatching = new TextMatching();
                            else
                            {
                                outdatedKeywords.ForEach(keyword =>
                                {
                                    //Step 1: delete from pattern history
                                    textMatching.possibilities.RemoveAll(entry => entry.next.Equals(keyword));

                                    //Step 2: delete from overall stream history
                                    //  Example: {Str1 Str2 Str3 StrWanted StrWanted Str4 StrWanted Str5 Str6}
                                    //  We want to remove everything before each "StringWanted"
                                    //  In the end, the stream is {String4 String5}
                                    int idx = textMatching.stream.LastIndexOf(keyword);
                                    if (idx != -1)
                                        textMatching.stream.RemoveRange(0, idx + 1);
                                });
                            }
                        }

                        foreach (var item in _annotateText.Entities)
                        {
                            var sections = item.Name.Split(' ');
                            for (int i = 0; i < _annotateText.Tokens.Length;)
                            {
                                try
                                {
                                    var tokensToMatch = _annotateText.Tokens.Skip(i).Take(sections.Length).Select(_ => _.Text.Content)/*.ToArray()*/;
                                    if (Enumerable.SequenceEqual(sections, tokensToMatch)
                                            && tokenIndicesForExclusion.Skip(i).Take(sections.Length).All(_ => _ == false))
                                    {
                                        for (int j = i; j < i + sections.Length; j++)
                                        {
                                            tokenIndicesForExclusion[j] = true;
                                        }

                                        lengthOfExclusions[i] = sections.Length;
                                        exclusions[i] = item.Name;
                                        i += sections.Length;
                                        continue;
                                    }
                                }
                                catch { /* don't even bother */ }

                                i++;
                            }
                        }

                        for (int i = 0; i < _annotateText.Tokens.Length; i++)
                        {
                            //var entry = $"[{item.Text.Content}/{item.PartOfSpeech.Tag}/{item.DependencyEdge.Label}]";
                            string entry;

                            var item = _annotateText.Tokens[i];

                            //var arr = new string[] {
                            //    item.PartOfSpeech.Aspect,
                            //    item.PartOfSpeech.Case,
                            //    item.PartOfSpeech.Form,
                            //    item.PartOfSpeech.Gender,
                            //    item.PartOfSpeech.Mood,
                            //    item.PartOfSpeech.Number,
                            //    item.PartOfSpeech.Person,
                            //    item.PartOfSpeech.Proper,
                            //    item.PartOfSpeech.Reciprocity,
                            //    item.PartOfSpeech.Tense,
                            //    item.PartOfSpeech.Voice
                            //};

                            // Already marked for another usage
                            if (tokenIndicesForExclusion[i])
                            {
                                entry = exclusions[i];

                                if (keywords.ContainsKey(entry))
                                {
                                    keywords[entry] = new Tuple<int, long>(keywords[entry].Item1 + 1, startTimeInTicks);
                                }
                                else
                                {
                                    keywords.Add(entry, new Tuple<int, long>(1, startTimeInTicks));
                                }

                                i += (lengthOfExclusions[i].Value - 1); //minus 1, because there is "i++" up there

                                continue;
                            }

                            // LABELS
                            if (new string[] {
                                DependencyEdgeEnums.AUX,
                                DependencyEdgeEnums.P,
                            }.Any(_ => item.DependencyEdge.Label.Equals(_)))
                                continue;

                            // TAGS
                            if (new string[] {
                                TagEnums.PRON,
                                TagEnums.CONJ,
                                TagEnums.ADV,
                                TagEnums.ADP,
                            }.Any(_ => item.PartOfSpeech.Tag.Equals(_)))
                                continue;

                            if (TagEnums.VERB.Equals(item.PartOfSpeech.Tag))
                            {
                                entry = item.Lemma;
                                knownLemmas.Add(entry);
                            }
                            else
                            {
                                entry = item.Text.Content;
                            }

                            if (keywords.ContainsKey(entry))
                            {
                                keywords[entry] = new Tuple<int, long>(keywords[entry].Item1 + 1, endTimeInTicks);
                            }
                            else
                            {
                                keywords.Add(entry, new Tuple<int, long>(1, endTimeInTicks));
                            }
                        }

                        var sb = new System.Text.StringBuilder();

                        foreach (var item in keywords.OrderByDescending(kvp => kvp.Value))
                        {
                            if (knownLemmas.Contains(item.Key))
                            {
                                sb.Append($"<b><font color='lime'>{item.Key}</font> (<font color='olive'>{item.Value.Item1}</font>)</b><br/>");
                            }
                            else if (knownEntities.Contains(item.Key))
                            {
                                string entityType = _annotateText.Entities.Single(_ => _.Name.Equals(item.Key)).Type;
                                switch (entityType)
                                {
                                    case EntityEnums.CONSUMER_GOOD:
                                        sb.Append($"<b><font color='#800080'>{item.Key}</font> (<font color='#9933FF'>{item.Value.Item1}</font>)</b><br/>");
                                        break;
                                    case EntityEnums.EVENT:
                                        sb.Append($"<b><font color='#b29838'>{item.Key}</font> (<font color='#FFE5CC'>{item.Value.Item1}</font>)</b><br/>");
                                        break;
                                    case EntityEnums.LOCATION:
                                        sb.Append($"<b><font color='#008000'>{item.Key}</font> (<font color='#CCFFCC'>{item.Value.Item1}</font>)</b><br/>");
                                        break;
                                    case EntityEnums.ORGANIZATION:
                                        sb.Append($"<b><font color='#0000ff'>{item.Key}</font> (<font color='#CCE5FF'>{item.Value.Item1}</font>)</b><br/>");
                                        break;
                                    case EntityEnums.OTHER:
                                        sb.Append($"<b><font color='#CC6600'>{item.Key}</font> (<font color='#FF8000'>{item.Value.Item1}</font>)</b><br/>");
                                        break;
                                    case EntityEnums.PERSON:
                                        sb.Append($"<b><font color='#db4437'>{item.Key}</font> (<font color='#FF9999'>{item.Value.Item1}</font>)</b><br/>");
                                        break;
                                    case EntityEnums.UNKNOWN:
                                        sb.Append($"<b><font color='#d6be40'>{item.Key}</font> (<font color='#99FFCC'>{item.Value.Item1}</font>)</b><br/>");
                                        break;
                                    case EntityEnums.WORK_OF_ART:
                                        sb.Append($"<b><font color='#ff8c00'>{item.Key}</font> (<font color='#FFE5CC'>{item.Value.Item1}</font>)</b><br/>");
                                        break;
                                    default:
                                        sb.Append($"<b><font color='red'>{item.Key}</font> (<font color='fuchsia'>{item.Value.Item1}</font>)</b><br/>");
                                        break;
                                }
                            }
                            else if (knownTokens.Contains(item.Key) &&
                                    !knownEntities.SelectMany(entity => entity.Split(' ')).Contains(item.Key))
                            {
                                sb.Append($"<b><font color='cyan'>{item.Key}</font> (<font color='blue'>{item.Value.Item1}</font>)</b><br/>");
                            }
                            else
                            {
                                sb.Append($"{item.Key} ({item.Value.Item1})<br/>");
                            }

                            //sb.Append(item.Key);
                            //sb.Append(' ');
                            //sb.Append('(');
                            //sb.Append(item.Value.Item1);
                            //sb.Append(')');
                            //sb.AppendLine();
                        }

                        string annotationResult = sb.ToString();

                        RunOnUiThread(() =>
                        {
                            _textView1.SetText(Html.FromHtml(annotationResult), TextView.BufferType.Spannable);
                            //_textView1.Text = sb.ToString();

                            _textView2.Append($"{++count}\n");
                            _textView2.Append($"{startTime.Hours.ToString("00")}:{startTime.Minutes.ToString("00")}:{startTime.Seconds.ToString("00")},{startTime.Milliseconds.ToString("000")} --> {endTime.Hours.ToString("00")}:{endTime.Minutes.ToString("00")}:{endTime.Seconds.ToString("00")},{endTime.Milliseconds.ToString("000")}\n");
                            _textView2.Append($"{original_transcription /*google_result*/}\n\n");
                        });

                        if (doNextWordPrediction)
                        {
                            foreach (var token in _annotateText.Tokens)
                            {
                                textMatching.NextToLearn(token.Text.Content);
                            }
                            var predictions = textMatching.PredictNext();
                            RunOnUiThread(() =>
                            {
                                var memoryUsage = $"(uses {System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / 1048576} MB)";
                                if (predictions == null)
                                {
                                    _textView3.Text = $"I can predict nothing! {memoryUsage}";
                                }
                                else
                                {
                                    //_textView3.Text = $"Predicting: [{predictions.Select(entry => entry.next).Aggregate((a, b) => a + ", " + b)}]";
                                    _textView3.Text = $"Predicting: [{predictions.next}] {memoryUsage}";
                                }
                            });
                        }
                    }
                    catch (Exception err)
                    {
                        //RunOnUiThread(() => _textView1.Append(err.ToString()));
                    }
                }
                
                //OtherEvents();

                _scrollView1.Post(() => _scrollView1.FullScroll(FocusSearchDirection.Down));
                _scrollView2.Post(() => _scrollView2.FullScroll(FocusSearchDirection.Down));
            }
            catch (Exception err)
            {
                RunOnUiThread(() => _textView1.Append(err.ToString()));
            }
        }

        private void RaiseRecordingStateChangedEvent()
        {
            RecordingStateChanged?.Invoke(isRecording);
        }

        public void StopPlayback()
        {
            if (audioTrack != null)
            {
                audioTrack.Stop();
                audioTrack.Release();
                audioTrack = null;
            }
        }

        public void Stop()
        {
            endRecording = true;
            Thread.Sleep(500); // Give it time to drop out.
        }

        protected async Task PlayAudioTrackAsync(short[] buffer)
        {
            /* jangan rubah2 settingan channel in/out dari Mono, dan yang encoding dari PCM 16 Bit !!! */
            int buffsize = AudioTrack.GetMinBufferSize(SampleRate, channelOut, encoding);
            audioTrack = new AudioTrack(Android.Media.Stream.Music, SampleRate, channelOut, encoding, buffsize, audioTrackMode);

            audioTrack.Play();

            audioTrack.Flush(); // ini opsional
            await audioTrack.WriteAsync(buffer, 0, buffer.Length);
        }

        #region not our concern
        void OtherEvents()
        {
            /*if (Post_SpeechRecognition_Action.Contains("Stanford NLP Parser"))
                {
                    try
                    {
                        output = (await StanfordNLP.Execute_HttpClient(google_result)).Item1;
                        RunOnUiThread(() => _textView1.Append($"[Stanford NLP]\n{output}\n\n"));
                    }
                    catch
                    {
                        RunOnUiThread(() => _textView1.Append($"[Stanford NLP thinks your speech is malformed]\n\n"));
                    }
                }

                if (Post_SpeechRecognition_Action.Contains("Google Natural Language API: analyzeEntities"))
                {
                    try
                    {
                        var _analyzeEntities = await NaturalLanguageUtils.Execute_AnalyzeEntities(google_result);

                        output = "[Google Natural Language API analyzeEntities]\n\n";
                        foreach (var item in _analyzeEntities.Entities)
                        {
                            output += $"{item.Name} - {item.Type}\n";
                        }
                        output += $"\n";

                        RunOnUiThread(() => _textView1.Append(output));
                    }
                    catch
                    {
                        RunOnUiThread(() => _textView1.Append($"[Google Natural Language API analyzeEntities didn't detect anything in particular]\n\n"));
                    }
                }

                if (Post_SpeechRecognition_Action.Contains("Google Natural Language API: analyzeSentiment"))
                {
                    try
                    {
                        var _analyzeSentiment = await NaturalLanguageUtils.Execute_AnalyzeSentiment(google_result);

                        output += "[Google Natural Language API analyzeSentiment]\n\n";
                        output += $"Magnitude = {_analyzeSentiment.DocumentSentiment.Magnitude} & Polarity = {_analyzeSentiment.DocumentSentiment.Polarity}\n";
                        output += $"\n";

                        RunOnUiThread(() => _textView1.Append(output));
                    }
                    catch
                    {
                        RunOnUiThread(() => _textView1.Append($"[Google Natural Language API analyzeSentiment didn't detect anything in particular]\n\n"));
                    }
                }

                if (Post_SpeechRecognition_Action.Contains("Google Natural Language API: annotateText"))
                {
                    try
                    {
                        var _annotateText = await NaturalLanguageUtils.Execute_AnnotateText(google_result);

                        output += "[Google Natural Language API annotateText]\n\n";
                        foreach (var item in _annotateText.Tokens)
                        {
                            output += $"[{item.Text.Content}/{item.DependencyEdge.Label}] ";
                        }
                        output += $"\n\n";

                        foreach (var item in _annotateText.Entities)
                        {
                            output += $"{item.Name} - {item.Type}\n";
                        }
                        output += $"\n";

                        output += $"Magnitude = {_annotateText.DocumentSentiment.Magnitude} & Polarity = {_annotateText.DocumentSentiment.Polarity}\n";
                        output += $"\n";

                        RunOnUiThread(() => _textView1.Append(output));
                    }
                    catch
                    {
                        RunOnUiThread(() => _textView1.Append($"[Google Natural Language API annotateText didn't detect anything in particular]\n\n"));
                    }
                }

                if (Post_SpeechRecognition_Action.Contains("Google Natural Language API: annotateText (colored)"))
                {
                    try
                    {
                        var _annotateText = await NaturalLanguageUtils.Execute_AnnotateText(google_result);

                        if (scenario8.Length != 0)
                            scenario8.Append($"<br/>");

                        for (int i = 0; i < _annotateText.Tokens.Length; i++)
                        {
                            var item = _annotateText.Tokens[i];

                            var arr = new string[] {
                                item.PartOfSpeech.Aspect,
                                item.PartOfSpeech.Case,
                                item.PartOfSpeech.Form,
                                item.PartOfSpeech.Gender,
                                item.PartOfSpeech.Mood,
                                item.PartOfSpeech.Number,
                                item.PartOfSpeech.Person,
                                item.PartOfSpeech.Proper,
                                item.PartOfSpeech.Reciprocity,
                                item.PartOfSpeech.Tense,
                                item.PartOfSpeech.Voice
                            };
                            if (arr.Any(pos => !pos.EndsWith("UNKNOWN")))
                                scenario8.Append($"<b><font color='cyan'>{item.Text.Content}</font></b>");
                            else if (item.DependencyEdge.Label.Equals(DependencyEdgeEnums.NEG))
                                scenario8.Append($"<b><font color='red'>{item.Text.Content}</font></b>");
                            else
                                scenario8.Append($"<b>{item.Text.Content}</b>");

                            if (i + 1 < _annotateText.Tokens.Length - 1 && new string[] { "n't", "'ve", "'m", "'s", "'re", "'d" }.Contains(_annotateText.Tokens[i + 1].Text.Content))
                            {
                                //shortcuts.
                                //no space after.
                            }
                            else if ((i + 1 == _annotateText.Tokens.Length - 1) && _annotateText.Tokens[i + 1].DependencyEdge.Label.Equals("."))
                            {
                                //the last word before punctuation (end of statement).
                                //no space after.
                            }
                            else
                            {
                                scenario8.Append(" ");
                            }
                        }

                        RunOnUiThread(() => _textView1.SetText(Html.FromHtml(scenario8.ToString()), TextView.BufferType.Spannable));
                    }
                    catch
                    {
                        RunOnUiThread(() => _textView1.Append($"[Google Natural Language API annotateText didn't detect anything in particular]\n\n"));
                    }
                }

                if (Post_SpeechRecognition_Action.Contains("Google Search I'm feeling lucky (get the URL)"))
                {
                    try
                    {
                        output = await SearchUtils.Get_IamFeelingLucky_URL(query: google_result, useMonkeyTricks: true);
                        RunOnUiThread(() => _textView1.Append($"[Google Search I'm Feeling Lucky]\n{output}\n\n"));
                    }
                    catch
                    {
                        RunOnUiThread(() => _textView1.Append($"[Google Search I'm Feeling Lucky can't recommend anything]\n\n"));
                    }
                }

                if (Post_SpeechRecognition_Action.Contains("Thesaurus Synonim"))
                {
                    try
                    {
                        var rootWord = (await NaturalLanguageUtils.Execute_AnnotateText(google_result))
                                        .Tokens.First(_ => _.DependencyEdge.Label.Equals("ROOT")).Lemma;

                        ////////////////////////////////////////////////////////////////////////////

                        string queryString = rootWord; //rootWord; //google_result;
                                                       //queryString = "nothing"; // valid word
                                                       //queryString = "nothiin"; // probably means "nothing"
                                                       //queryString = "qwrtyplkjhgfdszxcvbnm"; // invalid word

                        queryString = queryString.Replace(" ", "+").Replace(",", "%20");
                        var outputList = await Thesaurus.GetSynonims(queryString);

                        if (outputList != null)
                        {
                            output = outputList.Aggregate((a, b) => $"{a}\n{b}");
                            RunOnUiThread(() => _textView1.Append($"[Thesaurus Synonims for '{queryString}']\n{output}\n\n"));
                        }
                        else
                            RunOnUiThread(() => _textView1.Append($"[Thesaurus Synonims]\n\n"));
                    }
                    catch
                    {
                        RunOnUiThread(() => _textView1.Append($"[Thesaurus can find no synonim for that word]\n\n"));
                    }
                    finally
                    {
                        await Task.Delay(100); //update the UI
                    }
                }

                if (Post_SpeechRecognition_Action.Contains("API.AI"))
                {
                    try
                    {
                        var result = ApiAiUtils.Recognize(this.BaseContext, google_result);

                        if (!result.Action.Equals("input.unknown"))
                        {
                            //RunOnUiThread(() => _textArea.Append("[API.AI]\n\n"));
                            //RunOnUiThread(() => _textArea.Append($"ResolvedQuery = {result.ResolvedQuery}\n"));
                            RunOnUiThread(() => _textView1.Append($"Action = {result.Action}, with Parameters:\n"));
                            foreach (var kvp in result.Parameters)
                            {
                                RunOnUiThread(() => _textView1.Append($"[{kvp.Key} - {kvp.Value}]\n"));
                            }
                        }
                        else
                        {
                            RunOnUiThread(() => _textView1.Append($"No intent recognized: [{result.ResolvedQuery}]\n"));
                        }
                    }
                    catch
                    {
                        RunOnUiThread(() => _textView1.Append($"[API.AI thinks your speech is malformed]\n\n"));
                    }
                    finally
                    {
                        await Task.Delay(100); //update the UI
                    }
                }

                if (Post_SpeechRecognition_Action.Contains("Google Search Mispelling Suggestion"))
                {
                    try
                    {
                        var result = await SearchUtils.GetSuggestionForMispelling(google_result);

                        if (!string.IsNullOrEmpty(result))
                        {
                            RunOnUiThread(() => _textView1.Append($"Did you mean {result} instead of {google_result}\n\n"));
                        }
                        else
                        {
                            RunOnUiThread(() => _textView1.Append($"[Google Search didn't find any mispelling]\n\n"));
                        }
                    }
                    catch
                    {
                        RunOnUiThread(() => _textView1.Append($"[Google Search didn't understand that]\n\n"));
                    }
                    finally
                    {
                        await Task.Delay(100); //update the UI
                    }
                }*/
        }
        #endregion
    }
}