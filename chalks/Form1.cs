using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        public struct Win32Point
        {
            public int x;
            public int y;
        }
        public const int WM_USER = 0x400;
        public const int EM_GETSCROLLPOS = (WM_USER + 221);
        public const int EM_SETSCROLLPOS = (WM_USER + 222);

        [DllImport("user32")]
        public static extern int LockWindowUpdate(System.IntPtr hwnd);
        [DllImport("user32")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, ref Win32Point pt);


        string file_name = "";
        Color back;
        Color tab_back;
        Color space_back;
        Color icon_back;
        Color comment_back;
        Color string_back;
        Color char_back;

        string[] keywords = { "def", "for", "if", "then", "elif", "else", "endif", "to", "in", "return", "import", "while", "do", "raise" };

        bool key_pressed = false;

        const int MAX_UNDO = 100;
        string[] undo_redo = new string[MAX_UNDO];
        int undo_redo_index = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private bool ok_for_name(char c)
        {
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || c == '_')
            {
                return true;
            }
            return false;
        }

        private void recolor(int from, int to)
        {
            int sel_start = richTextBox1.SelectionStart;
            int sel_len = richTextBox1.SelectionLength;
            Win32Point scroll_pos = new Win32Point();
            SendMessage(richTextBox1.Handle, EM_GETSCROLLPOS, 0, ref scroll_pos);
            LockWindowUpdate(richTextBox1.Handle);
            richTextBox1.Hide();

            bool is_name = false;
            int name_start = 0;
            string the_name = "";
            richTextBox1.Select(from, to - from);
            richTextBox1.SelectionBackColor = space_back;
            richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
            bool is_string = false;
            int string_start = 0;
            bool is_char = false;
            int char_start = 0;
            bool is_comment = false;
            int comment_start = 0;

            string richTextBox1_Text = richTextBox1.Text;

            char prev_char = '\0';
            char cur_char = '\0';
            for (int i = from; i < to + 1; i++)
            {
                if (i > from)
                {
                    prev_char = cur_char;
                }
                if (i < to)
                {
                    cur_char = richTextBox1_Text[i];
                }
                else
                {
                    cur_char = '\0';
                }

                if (ok_for_name(cur_char))
                {
                    if (!is_name)
                    {
                        name_start = i;
                        is_name = true;
                    }
                    the_name += cur_char;
                }
                else
                {
                    if (spacesToolStripMenuItem.Checked)
                    {
                        if (!(is_comment || is_string || is_char))
                        {
                            if (cur_char == '\t')
                            {
                                richTextBox1.Select(i, 1);
                                richTextBox1.SelectionBackColor = tab_back;
                            }
                        }
                    }

                    if (is_name)
                    {
                        int name_len = i - name_start;

                        if (syntaxHighlightingToolStripMenuItem.Checked && keywords.Contains(the_name))
                        {
                            richTextBox1.Select(name_start, name_len);
                            if (is_comment)
                            {
                                richTextBox1.SelectionColor = Color.Silver;
                            }
                            else
                            {
                                richTextBox1.SelectionColor = Color.White;
                            }
                        }
                        else if (perWordHighlightingToolStripMenuItem.Checked)
                        {
                            int hash = the_name.GetHashCode();
                            int name_R = 192 + (hash & 0x3f);
                            int name_G = 192 + ((hash >> 6) & 0x3f);
                            int name_B = 192 + ((hash >> 12) & 0x3f);

                            if (is_comment || is_string || is_char) {
                                name_R = 192 + (name_R - 192) / 2;
                                name_G = 192 + (name_G - 192) / 2;
                                name_B = 192 + (name_B - 192) / 2;
                            }

                            richTextBox1.Select(name_start, name_len);
                            richTextBox1.SelectionColor = Color.FromArgb(name_R, name_G, name_B);
                        }
                        else {
                            richTextBox1.Select(name_start, name_len);
                            richTextBox1.SelectionColor = Color.Silver;
                        }
                        is_name = false;
                        the_name = "";
                    }

                    if (commentsToolStripMenuItem.Checked)
                    {
                        if ((cur_char == '#' || cur_char == '-' || cur_char == '/' || cur_char == '%') && (prev_char == '\n' || i == from))
                        {
                            is_comment = true;
                            comment_start = i;
                        }
                        if (cur_char == '\n' || i == to)
                        {
                            if (is_comment) 
                            {
                                richTextBox1.Select(comment_start, i - comment_start);
                                richTextBox1.SelectionBackColor = comment_back;
                                richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Italic);
                            }
                            is_comment = false;
                            is_string = false;
                            is_char = false;
                        }
                    }

                    if (stringsToolStripMenuItem.Checked)
                    {
                        if (cur_char == '\"' && prev_char != '\\')
                        {
                            is_string = !is_string;
                            if (is_string)
                            {
                                string_start = i;
                            }
                            else
                            {
                                richTextBox1.Select(string_start+1, i - string_start-1);
                                richTextBox1.SelectionBackColor = string_back;
                                richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Italic);
                            }
                        }
                        if (cur_char == '\'' && prev_char != '\\')
                        {
                            is_char = !is_char;
                            if (is_char)
                            {
                                char_start = i;
                            }
                            else
                            {
                                richTextBox1.Select(char_start+1, i - char_start-1);
                                richTextBox1.SelectionBackColor = char_back;
                                richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, FontStyle.Italic);
                            }
                        }
                    }
                }
            }
            richTextBox1.Show();
            richTextBox1.Select(sel_start, sel_len);
            SendMessage(richTextBox1.Handle, EM_SETSCROLLPOS, 0, ref scroll_pos);
            LockWindowUpdate((IntPtr)0);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (undo_redo_index < MAX_UNDO - 1)
            {
                undo_redo_index++;
            }
            else 
            {
                for (int i = 0; i < MAX_UNDO - 1; i++) 
                {
                    undo_redo[i] = undo_redo[i + 1];
                }
            }
            undo_redo[undo_redo_index] = richTextBox1.Rtf;
        }

        public void open(string local_file_name)
        {            
            file_name = local_file_name;            
            richTextBox1.LoadFile(file_name, RichTextBoxStreamType.PlainText);
            saveFileDialog1.FileName = file_name;
            recolor(0, richTextBox1.Text.Length);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = file_name;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                open(openFileDialog1.FileName);
                Text = openFileDialog1.SafeFileName + " (" + openFileDialog1.FileName + ")";
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                file_name = saveFileDialog1.FileName;
                Text = saveFileDialog1.FileName;
                richTextBox1.SaveFile(file_name, RichTextBoxStreamType.PlainText);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            file_name = "";
            int hash = Math.Abs(DateTime.Now.TimeOfDay.GetHashCode());
            int base_R = hash % 32;
            int base_G = (hash / 32) % 32;
            int base_B = (hash / 32 / 32) % 32;
            back = Color.FromArgb(base_R + 4, base_G + 4, base_B + 4);
            tab_back = Color.FromArgb(base_R + 4, base_G + 4, base_B + 4);
            space_back = Color.FromArgb(base_R + 0, base_G + 0, base_B + 0);
            icon_back = Color.FromArgb(base_R * 2, base_G * 2, base_B * 2);
            comment_back = Color.FromArgb(base_R + 6, base_G + 6, base_B + 24);
            string_back = space_back;
            char_back = space_back;
            richTextBox1.BackColor = back;

            Bitmap bmp = new Bitmap(64, 64);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Pen b = new Pen(icon_back);
                b.Width = 64;
                g.DrawLine(b, 1, 1, 62, 62);
                g.DrawLine(b, 62, 1, 1, 62);
            }
            Icon = Icon.FromHandle(bmp.GetHicon());

            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ChalksPreferences");
            int width = Width;
            if (Int32.TryParse((string)key.GetValue("Width"), out width))
            {
                Width = width;
            }
            int height = Height;
            if (Int32.TryParse((string)key.GetValue("Height"), out height))
            {
                Height = height;
            }
            key.Close();
        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (key_pressed)
            {
                int from = richTextBox1.SelectionStart;
                int to = from;
                while (from > 0 && richTextBox1.Text[from - 1] != '\n')
                {
                    from--;
                }
                while (to < richTextBox1.Text.Length && richTextBox1.Text[to] != '\n')
                {
                    to++;
                }
                recolor(from, to);
                key_pressed = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ChalksPreferences");
            key.SetValue("Width", Width.ToString());
            key.SetValue("Height", Height.ToString());
            key.Close();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
            Clipboard.SetText(Clipboard.GetText(TextDataFormat.Text), TextDataFormat.Text);
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Cut();
            Clipboard.SetText(Clipboard.GetText(TextDataFormat.Text), TextDataFormat.Text);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Clipboard.GetText(TextDataFormat.Text), TextDataFormat.Text);
            richTextBox1.SelectionColor = Color.Silver;
            richTextBox1.Paste();
        }

        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            key_pressed = true;            
        }

        private void recolorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            recolor(0, richTextBox1.Text.Length);
            stopwatch.Stop();
            MessageBox.Show(stopwatch.Elapsed.ToString());
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (undo_redo_index > 0) 
            {
                richTextBox1.Rtf = undo_redo[undo_redo_index];
                undo_redo[undo_redo_index] = "";
                undo_redo_index--;
            }
        }
    }
}
