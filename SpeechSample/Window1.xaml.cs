﻿using System;
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
using System.Speech;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Windows.Forms;
using System.Xml;

namespace SpeechSample
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        protected SpeechSynthesizer talker;

        public Window1()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            talker = new SpeechSynthesizer();
            talker.Speak("Welcome to Speech in WPF.");

            // Load all installed voices
            System.Collections.ObjectModel.ReadOnlyCollection<InstalledVoice>
                voices = talker.GetInstalledVoices();
            foreach (InstalledVoice voice  in voices)
            {
                VoicesComboBox.Items.Add(voice.VoiceInfo.Name);
            }
           
        }

        private void TalkButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem volumeItem = (ComboBoxItem)VolumeList.Items[VolumeList.SelectedIndex];
            Int32 vol = Convert.ToInt32(volumeItem.Content.ToString());
            ComboBoxItem rateItem = (ComboBoxItem)RateList.Items[RateList.SelectedIndex];
            Int32 rate = Convert.ToInt32(rateItem.Content.ToString());
            talker.Volume = vol;
            talker.Rate = rate;
            talker.SelectVoice(VoicesComboBox.Text);
            talker.Speak(ConvertRichTextBoxContentsToString());
        }

        private void OpenTextFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Text files (*.txt)|*.txt|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadTextDocument(dlg.FileName);
                FileNameTextBox.Text = dlg.FileName;
            }
        }
              

        private void LoadTextDocument(string fileName)
        {
            TextRange range;
            System.IO.FileStream fStream = null;
            if (System.IO.File.Exists(fileName))            
            {
                try
                {
                    range = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd);
                    fStream = new System.IO.FileStream(fileName, System.IO.FileMode.OpenOrCreate);
                    range.Load(fStream, System.Windows.DataFormats.Text);
                }
                catch
                {
                }
                finally
                {
                   fStream.Close();
                }
            }
        }

        string ConvertRichTextBoxContentsToString()
        {
            TextRange textRange = new TextRange(richTextBox1.Document.ContentStart,
                richTextBox1.Document.ContentEnd);
            return textRange.Text;
        }


        private void OpenWavFileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VoicesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private PromptBuilder BuildPromptBuilder()
        {
            PromptBuilder builder = new PromptBuilder();
            builder.AppendText("WPF Speech API");
            builder.AppendTextWithHint("Hello! This is WPF. How may I help you?"
                                            , SayAs.Telephone);
            builder.AppendTextWithHint("A B C D E F G", SayAs.SpellOut);

            builder.AppendText("Let's take a quick break.");
            builder.AppendBreak(new TimeSpan(0, 0, 5));
            builder.AppendText("We are back. Now we are joined with Sam.");
            builder.AppendBreak(new TimeSpan(0, 0, 2));
            builder.AppendText("Sam. How are you? ");

            builder.StartVoice("Microsoft Sam");
            builder.AppendText("I am fine Mary. Thank you");
            builder.EndVoice();

            builder.AppendText("Can we speed it up?");
 
            builder.StartStyle(new PromptStyle(PromptRate.ExtraSlow));
            builder.AppendText("This is slow reading.");
            builder.EndStyle();

            builder.AppendText("We are done");

            return builder;
        }

        private void PromptBuilderButton_Click(object sender, RoutedEventArgs e)
        {
            talker = new SpeechSynthesizer();
            talker.SpeakAsync(BuildPromptBuilder());
        }

        private void SSMLButton_Click(object sender, RoutedEventArgs e)
        {
            string str = BuildPromptBuilder().ToXml();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(str);
            doc.Save("SSML.xml");           
        }   


    }
}
