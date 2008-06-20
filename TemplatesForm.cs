using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace gep
{
    partial class TemplatesForm : Form
    {
        public TemplatesForm()
        {
            InitializeComponent();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            int i = languageCombo.Items.Add(new CodeGenCPP());
            languageCombo.Items.Add(new CodeGenCS());
            languageCombo.SelectedIndex = i;            
        }

        private void OnSelChange(object sender, EventArgs e)
        {
            CodeSnippet snp = (CodeSnippet)snippetsList.SelectedItem;
            if (snp == null) return;
            templateText.Text = snp.Text;
            variablesText.Text = snp.Description;
            exampleText.Text = snp.Generate();
        }

        private void OnTemplateChanged(object sender, EventArgs e)
        {
            CodeSnippet snp = (CodeSnippet)snippetsList.SelectedItem;
            if (snp == null) return;
            snp.Text = templateText.Text;
            exampleText.Text = snp.Generate();
        }

        private void OnOK(object sender, EventArgs e)
        {
            foreach (object o in languageCombo.Items)
            {
                CodeGenBase cg = (CodeGenBase)o;
                cg.SaveTemplates();
            }
            Close();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            Close();
        }

        private void OnRestoreDefaults(object sender, EventArgs e)
        {
            foreach (object o in snippetsList.Items)
            {
                CodeSnippet snp = (CodeSnippet)o;
                snp.RestoreDefault();
            }
            OnSelChange(null, null);
        }

        private void OnLangChange(object sender, EventArgs e)
        {
            CodeGenBase cg = (CodeGenBase)languageCombo.SelectedItem;
            if (cg == null) return;
            snippetsList.Items.Clear();
            foreach (CodeSnippet snp in cg.snippets)
                snippetsList.Items.Add(snp);
            templateText.Clear();
            variablesText.Clear();
            exampleText.Clear();
        }
    }

    class CodeSnippet
    {
        string caption;
        string default_text;
        string text;
        string description;
        string codename;
        Dictionary<string, string> vars = new Dictionary<string, string>();

        public CodeSnippet(string _caption, string _codename, string _deftext, string _descr)
        {
            caption = _caption; text = default_text = _deftext; description = _descr;
            codename = _codename;
        }

        public override string ToString()
        {
            return caption;
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public string Caption { get { return caption; } }
        public string Description { get { return description; } }
        public string Codename { get { return codename; } }

        public void SetVar(string var, string val)
        {
            vars[var] = val;
        }

        public void Clear()
        {
            vars.Clear();
        }

        public void SetVars(string[] vrs)
        {
            for (int i = 0; i < vrs.Length / 2; i++)
                SetVar(vrs[i * 2], vrs[i * 2 + 1]);
        }

        public void RestoreDefault()
        {
            text = default_text;
        }

        public string Generate()
        {
            string s = text;
            foreach (string var in vars.Keys)
                s = s.Replace(var, vars[var]);
            return s;    
        }

        public string GenerateWith(string[] vrs)
        {
            return Translate(text, vrs);
        }

        public static string Translate(string s, string[] vrs)
        {
            for (int i = 0; i < vrs.Length / 2; i++)
                s = s.Replace(vrs[i * 2], vrs[i * 2 + 1]);
            return s;
        }
    }
}