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

namespace WindowsFormsApplication1 {
    public partial class Form1 : Form {

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

        public Form1() {
            InitializeComponent();
        }

        private bool ok_for_name(char c){
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || c=='_') {
                return true;
            }
            return false;
        }

        private void recolor(int from, int to){            
            int sel_start = richTextBox1.SelectionStart;
            int sel_len = richTextBox1.SelectionLength;
            Win32Point scroll_pos = new Win32Point();
            SendMessage(richTextBox1.Handle, EM_GETSCROLLPOS, 0, ref scroll_pos);
            LockWindowUpdate(richTextBox1.Handle);

            bool is_name = false;
            int name_start = 0;
            string the_name = "";
            richTextBox1.Select(from, to-from);
            richTextBox1.SelectionBackColor = back;

            string richTextBox1_Text = richTextBox1.Text;

            for (int i = from; i < to + 1; i++) {
                char cur_char = '\0';
                if (i < to) {
                    cur_char = richTextBox1_Text[i];
                }
                if ( ok_for_name( cur_char ) ){
                    if (!is_name) {
                        name_start = i;
                        is_name = true;
                        the_name = "";
                    } else {
                        the_name += cur_char;
                    }
                }else{
                    if (cur_char == '\t') {
                        richTextBox1.Select(i, 1);
                        richTextBox1.SelectionBackColor = tab_back;
                    }
                    if (cur_char == ' ') {
                        richTextBox1.Select(i, 1);
                        richTextBox1.SelectionBackColor = space_back;
                    }
                    if (is_name) {
                        int name_len = i - name_start;                        

                        int hash = the_name.GetHashCode();                        
                        int name_R = 192 + (hash % 64);
                        int name_G = 192 + ((hash / 64) % 64);
                        int name_B = 192 + ((hash / 64 / 64) % 64);

                        richTextBox1.Select(name_start, name_len);
                        richTextBox1.SelectionColor = Color.FromArgb(name_R, name_G, name_B);

                        is_name = false;
                    }                    
                }
            }
            richTextBox1.Select(sel_start, sel_len);
            SendMessage(richTextBox1.Handle, EM_SETSCROLLPOS, 0, ref scroll_pos);
            LockWindowUpdate((IntPtr)0);            
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) {
            // backup plan is to do: recolor(0, richTextBox1.Text.Length);
        }

        public void open(string local_file_name) {
            file_name = local_file_name;
            richTextBox1.LoadFile(file_name, RichTextBoxStreamType.PlainText);
            saveFileDialog1.FileName = file_name;
            recolor(0, richTextBox1.Text.Length);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog1.FileName = file_name;
            if(openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK ){
                open(openFileDialog1.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            if(saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK ){
                file_name = saveFileDialog1.FileName;
                richTextBox1.SaveFile(file_name, RichTextBoxStreamType.PlainText);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }

        private void Form1_Load(object sender, EventArgs e) {
            file_name = "";
            int hash = Math.Abs(DateTime.Now.TimeOfDay.GetHashCode());
            int base_R = hash % 32;
            int base_G = (hash / 32) % 32;
            int base_B = (hash / 32 / 32) % 32;
            back = Color.FromArgb(base_R + 6, base_G + 6, base_B + 6);
            tab_back = Color.FromArgb(base_R + 0, base_G + 0, base_B + 0);
            space_back = Color.FromArgb(base_R + 12, base_G + 12, base_B + 12);
            richTextBox1.BackColor = back;

            Bitmap bmp = new Bitmap(64, 64);
            using (Graphics g = Graphics.FromImage(bmp)) {
                Pen b = new Pen(back);
                b.Width = 64;
                g.DrawLine(b, 1, 1, 62, 62);
                g.DrawLine(b, 62, 1, 1, 62);
            }
            Icon = Icon.FromHandle(bmp.GetHicon());

            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ChalksPreferences");
            int width = Width;            
            if (Int32.TryParse((string)key.GetValue("Width"), out width)) {
                Width = width;
            }
            int height = Height;
            if (Int32.TryParse((string)key.GetValue("Height"), out height)) {
                Height = height;
            }
            key.Close();          
        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            
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
            Clipboard.SetText( Clipboard.GetText( TextDataFormat.Text ), TextDataFormat.Text);
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
        }
    }
}
