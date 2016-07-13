using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DicomImageViewer
{
   public class Dialog
    {
        public static List<string> ShowPromptDialog(List<string> lbl, string text)
        {
            Form prompt = new Form();
            prompt.Width = 400;
            prompt.Height = 400;
            prompt.Text = text;


            int top = 20;
            foreach (var item in lbl)
            {
                Label textLabel = new Label() { Left = 20, Top = top, Text = item };
                TextBox textBox = new TextBox() { Left = 20, Top = top + 25, Width = 100 };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                top += 50;
            }
            Button confirmation = new Button() { Text = "Go", Left = 20, Width = 100, Top = top + 25 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(confirmation);

            prompt.ShowDialog();
            var results = new List<string>();
            var ctrls = prompt.Controls.OfType<TextBox>();
            foreach (var item in ctrls)
            {
                results.Add(item.Text);
            }
            return results;
        }

        public static List<bool> ShowPromptCheckBoxDialog(List<string> lbl, string text)
        {
            Form prompt = new Form();
            prompt.Width = 400;
            prompt.Height = 400;
            prompt.Text = text;


            int top = 20;
            foreach (var item in lbl)
            {
                Label textLabel = new Label() { Left = 20, Top = top, Text = item };
                CheckBox textBox = new CheckBox() { Left = 20, Top = top + 25, Width = 100 };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                top += 50;
            }
            Button confirmation = new Button() { Text = "Go", Left = 20, Width = 100, Top = top + 25 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(confirmation);

            prompt.ShowDialog();
            var results = new List<bool>();
            var ctrls = prompt.Controls.OfType<CheckBox>();
            foreach (var item in ctrls)
            {
                results.Add(item.Checked);
            }
            return results;
        }
    }
}
