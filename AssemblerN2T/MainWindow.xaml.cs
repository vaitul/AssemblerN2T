using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AssemblerN2T
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string codeFile = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Hack file (*.asm)|*.asm";
            if (openFileDialog.ShowDialog() == true)
            {
                var fldoc = new FlowDocument();
                foreach (string line in File.ReadAllLines(openFileDialog.FileName))
                {
                    fldoc.Blocks.Add(new Paragraph(new Run(line)));
                }
                codeEditor.Document = fldoc;
                this.codeFile = openFileDialog.FileName;
                this.Title = openFileDialog.SafeFileName + " -My Assembler";
            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            string text = new TextRange(codeEditor.Document.ContentStart, codeEditor.Document.ContentEnd).Text;
            if (codeFile == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Hack file (*.asm)|*.asm";
                if (saveFileDialog.ShowDialog() == true)
                {
                    this.codeFile = saveFileDialog.FileName;
                    File.WriteAllText(this.codeFile, text);
                    this.Title = saveFileDialog.SafeFileName + " -My Assembler";
                }
                return;
            }
            File.WriteAllText(this.codeFile, text);
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            saveBtn_Click(null, null);
            string text = new TextRange(codeEditor.Document.ContentStart, codeEditor.Document.ContentEnd).Text;
            errorBox.Text = "";
            try
            {
                var Instructions = AssemblerSpecs.Assemble(text);
                output.ItemsSource = Instructions;
                using (var fs = new FileStream(codeFile.Replace(".asm", ".hack"), FileMode.OpenOrCreate & FileMode.Truncate, FileAccess.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fs))
                    {
                        foreach (var i in Instructions)
                            streamWriter.WriteLine(i);
                        streamWriter.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                errorBox.Text = ex.Message;
            }
        }


    }
}
