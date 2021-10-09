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

using System.Speech;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Xml;

namespace SpeechRecognitionSample
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        protected SpeechRecognizer spRecognizer ;


        public Window1()
        {
            InitializeComponent();

            InitializeSR();
            textBox1.Focus();
        }

        private void InitializeSR()
        {
            spRecognizer = new SpeechRecognizer();
            spRecognizer.Enabled = true; 
            spRecognizer.SpeechRecognized += 
                new EventHandler<SpeechRecognizedEventArgs>(spRecognizer_SpeechRecognized);
        }

        void spRecognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            textBox1.Text = e.Result.Text;
        }

        private void EnableSRCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (EnableSRCheckBox.IsChecked != true)
            {
                //if (spRecognizer != null)
                //    spRecognizer.Enabled = false; 
            }
            else
            {
                InitializeSR();                
            }
        }

        private void LoadGrammarButton_Click(object sender, RoutedEventArgs e)
        {
            SpeechRecognizer spRecognizer = new SpeechRecognizer();
            Grammar g = CreateGrammarDocument();
            spRecognizer.LoadGrammar(g);
            spRecognizer.UnloadGrammar(g);
            spRecognizer.UnloadAllGrammars();
        }

        private void LoadGrammarAsyncButton_Click(object sender, RoutedEventArgs e)
        {
            SpeechRecognizer spRecognizer = new SpeechRecognizer();
            Grammar g = CreateGrammarDocument();
            spRecognizer.LoadGrammarAsync(g); 

        }

        private Grammar CreateGrammarDocument()
        {
            GrammarBuilder gBuilder = new GrammarBuilder();
            gBuilder.Append(new Choices("Phone", "Email", "Text"));
            gBuilder.Append("my");
            gBuilder.Append(new Choices("Mom", "Dad", "Brother", "Sister"));

            // Создание грамматики из GrammarBuilder
            Grammar speechGrammar = new Grammar(gBuilder);
            return speechGrammar;
        }

        private void CreateSRGS_Click(object sender, RoutedEventArgs e)
        {
            BuildDynamicSRGSDocument();
        }

        private SrgsDocument BuildDynamicSRGSDocument()
        {
            SrgsDocument document = new SrgsDocument();
            SrgsRule rootRule = new SrgsRule("MonthsandDays");
            rootRule.Scope = SrgsRuleScope.Public;

            rootRule.Elements.Add(new SrgsItem("Months and Days Grammar "));

            SrgsOneOf oneOfMonths = new SrgsOneOf(
                new SrgsItem("January"),
                new SrgsItem("February"),
                new SrgsItem("March"),
                new SrgsItem("April"),
                new SrgsItem("May"),
                new SrgsItem("June"),
                new SrgsItem("July"),
                new SrgsItem("August"),
                new SrgsItem("September"),
                new SrgsItem("October"),
                new SrgsItem("November"),
                new SrgsItem("December")
                );
            SrgsRule ruleMonths = new SrgsRule("Months", oneOfMonths);
            SrgsItem of = new SrgsItem("of");
            SrgsItem year = new SrgsItem("year");
            SrgsItem ruleMonthsItem = new SrgsItem(new SrgsRuleRef(ruleMonths), of, year);

            SrgsOneOf oneOfDays = new SrgsOneOf(
                new SrgsItem("Monday"),
                new SrgsItem("Tuesday"),
                new SrgsItem("Wednesday"),
                new SrgsItem("Thursday"),
                new SrgsItem("Friday"),
                new SrgsItem("Saturday"),
                new SrgsItem("Sunday")
                );
            SrgsRule ruleDays = new SrgsRule("Days", oneOfDays);
            SrgsItem week = new SrgsItem("week");
            SrgsItem ruleDaysItem = new SrgsItem(new SrgsRuleRef(ruleDays), of, week);

            rootRule.Elements.Add(ruleMonthsItem);
            rootRule.Elements.Add(ruleDaysItem);


            // Добавить все правила в документ
            document.Rules.Add(rootRule, ruleMonths, ruleDays);
            // Дополнительные правила
            SrgsText textItem = new SrgsText("Start of the Document.");
            SrgsRule textRule = new SrgsRule("TextItem");
            textRule.Elements.Add(textItem);
            document.Rules.Add(textRule);

            SrgsItem stringItem = new SrgsItem("Item as String.");
            SrgsRule itemRule = new SrgsRule("ItemRule");
            itemRule.Elements.Add(stringItem);
            document.Rules.Add(itemRule);

            SrgsItem elementItem = new SrgsItem();
            SrgsRule elementRule = new SrgsRule("ElementRule");
            elementRule.Elements.Add(elementItem);
            document.Rules.Add(elementRule);

            // Установить корень документа
            document.Root = rootRule;

            // Сохранить созданный документ SRGS в файл XML
            XmlWriter writer = XmlWriter.Create("DynamicSRGSDocument.Xml");
            document.WriteSrgs(writer);
            writer.Close();

            return document;
        }

        private void LoadSRGS_Click(object sender, RoutedEventArgs e)
        {
            SpeechRecognizer spRecognizer = new SpeechRecognizer();
            spRecognizer.LoadGrammar(new Grammar(BuildDynamicSRGSDocument()));  
        }
    }
}
