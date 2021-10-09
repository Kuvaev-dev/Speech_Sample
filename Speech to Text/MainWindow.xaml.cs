using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Recognition;
using System.Threading;
using System.Speech.Synthesis;

namespace Speech_to_Text
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Enum для хранения состояния распознавателя
        private enum State
        {
            Idle = 0,
            Accepting = 1,
            Off = 2,
        }

        private State RecogState = State.Off;
        private SpeechRecognitionEngine recognizer;
        private SpeechSynthesizer synthesizer = null;        
        private int Hypothesized = 0;
        private int Recognized = 0;
        public MainWindow()
        {
            InitializeComponent();            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Инициализация распознавателя и синтезатора
            InitializeRecognizerSynthesizer();

            // Проверка на доступность девайса
            if (SelectInputDevice())
            {
                LoadDictationGrammar();
                ButtonStart.IsEnabled = true;
                ReadAloud("Speech Engine Ready for Input");
            }
        }

        /// <summary>
        /// Инициализация распознавателя и синтезатора вместе с их событиями
        /// /// </summary>
        private void InitializeRecognizerSynthesizer()
        {
            var selectedRecognizer = (from e in SpeechRecognitionEngine.InstalledRecognizers()
                                      where e.Culture.Equals(Thread.CurrentThread.CurrentCulture)
                                      select e).FirstOrDefault();
            recognizer = new SpeechRecognitionEngine(selectedRecognizer);
            recognizer.AudioStateChanged+=new EventHandler<AudioStateChangedEventArgs>(recognizer_AudioStateChanged);
            recognizer.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(recognizer_SpeechHypothesized);
            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

            synthesizer = new SpeechSynthesizer();
        }

        /// <summary>
        /// Выбор устройства ввода, если оно доступно
        /// </summary>
        /// <returns></returns>
        private bool SelectInputDevice()
        {
            bool proceedLoading = true;
            // если ОС выше XP
            if (IsOscompatible())
            {
                try
                {
                    recognizer.SetInputToDefaultAudioDevice();
                }
                catch
                {
                    proceedLoading = false; // нет устройства ввода звука
                }
            }
            // если ОС XP или ниже
            else
                ThreadPool.QueueUserWorkItem(InitSpeechRecogniser);
            return proceedLoading;
        }

        /// <summary>
        /// Узнаём, совместима ли ОС.
        /// </summary>
        /// <returns>истина, если больше, чем XP, иначе ложь</returns>
        private bool IsOscompatible()
        {
            OperatingSystem osInfo = Environment.OSVersion;
            if (osInfo.Version > new Version("6.0"))
                return true;
            else
                return false;
        }

        private void InitSpeechRecogniser(object o)
        {
            recognizer.SetInputToDefaultAudioDevice();
        }

        /// <summary>
        /// Загрузка грамматик, одна для команд, другая для диктовки
        /// </summary>
        private void LoadDictationGrammar()
        {
            GrammarBuilder grammarBuilder = new GrammarBuilder();
            grammarBuilder.Append(new Choices("End Dictate"));
            Grammar commandGrammar = new Grammar(grammarBuilder);
            commandGrammar.Name = "main command grammar";
            recognizer.LoadGrammar(commandGrammar);

            DictationGrammar dictationGrammar = new DictationGrammar();
            dictationGrammar.Name = "dictation";
            recognizer.LoadGrammar(dictationGrammar);
        }

        #region Recognizer events
        private void recognizer_AudioStateChanged(object sender, AudioStateChangedEventArgs e)
        {
            switch (e.AudioState)
            {
                case AudioState.Speech:
                    LabelStatus.Content = "Listening";
                    break;
                case AudioState.Silence:
                    LabelStatus.Content = "Idle";
                    break;
                case AudioState.Stopped:
                    LabelStatus.Content = "Stopped";
                    break;
            }
        }

        private void recognizer_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Hypothesized++;
            LabelHypothesized.Content = "Hypothesized: " + Hypothesized.ToString();
        }
        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Recognized++;
            LabelRecognized.Content = "Recognized: " + Recognized.ToString();

            if (RecogState == State.Off)
                return;
            float accuracy = (float)e.Result.Confidence;
            string phrase = e.Result.Text;
            {
                if (phrase == "End Dictate")
                {
                    RecogState = State.Off;
                    recognizer.RecognizeAsyncStop();
                    ReadAloud("Dictation Ended");
                    return;
                }
                TextBox1.AppendText(" " + e.Result.Text);
            }
        }
        #endregion

        /// <summary>
        /// Приостановить распознавание и озвучить отправленный текст
        /// </summary>
        /// <param name="speakText"></param>
        public void ReadAloud(string speakText)
        {
            try
            {
                recognizer.RecognizeAsyncCancel();
                synthesizer.SpeakAsync(speakText);
            }
            catch { }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            switch (RecogState)
            {
                case State.Off:
                    RecogState = State.Accepting;
                    ButtonStart.Content = "Stop";
                    recognizer.RecognizeAsync(RecognizeMode.Multiple);
                    break;
                case State.Accepting:
                    RecogState = State.Off;
                    ButtonStart.Content = "Start";
                    recognizer.RecognizeAsyncStop();
                    break;
            }
        }
    }
}
