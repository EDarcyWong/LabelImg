using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
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
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;

namespace LabelImg.Views.UserControls
{
    /// <summary>
    /// CodeEditor.xaml 的交互逻辑
    /// </summary>
    public partial class CodeEditor : UserControl
    {
        private FoldingManager _foldingManager;
        private XmlFoldingStrategy _foldingStrategy;

        public CodeEditor()
        {
            InitializeComponent();
            InitializeTextEditor();
        }

        private void InitializeTextEditor()
        {
            // 设置语法高亮
            textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Python");

            // 设置初始文本
            textEditor.Text = @"using System;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, world!"");
        }
    }
}";

            // 启用代码折叠
            _foldingManager = FoldingManager.Install(textEditor.TextArea);
            _foldingStrategy = new XmlFoldingStrategy();
            UpdateFoldings();
        }

        public string Text
        {
            get => textEditor.Text;
            set => textEditor.Text = value;
        }

        public void UpdateFoldings()
        {
            _foldingStrategy.UpdateFoldings(_foldingManager, textEditor.Document);
        }
    }

    public class XmlFoldingStrategy
    {
        public void UpdateFoldings(FoldingManager manager, ICSharpCode.AvalonEdit.Document.TextDocument document)
        {
            var newFoldings = CreateNewFoldings(document);
            manager.UpdateFoldings(newFoldings, -1);
        }

        private IEnumerable<NewFolding> CreateNewFoldings(ICSharpCode.AvalonEdit.Document.TextDocument document)
        {
            List<NewFolding> foldings = new List<NewFolding>();

            Stack<int> startOffsets = new Stack<int>();
            for (int i = 0; i < document.LineCount; i++)
            {
                var line = document.GetLineByNumber(i + 1);
                var text = document.GetText(line);

                if (text.Trim().StartsWith("{"))
                {
                    startOffsets.Push(line.Offset);
                }
                else if (text.Trim().StartsWith("}"))
                {
                    if (startOffsets.Count > 0)
                    {
                        int startOffset = startOffsets.Pop();
                        foldings.Add(new NewFolding(startOffset, line.EndOffset));
                    }
                }
            }

            return foldings.OrderBy(f => f.StartOffset);
        }
    }
}