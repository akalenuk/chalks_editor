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
            bool name = false;
            int name_start = 0;
            int sel_start = richTextBox1.SelectionStart;
            int sel_len = richTextBox1.SelectionLength;
            Win32Point scroll_pos = new Win32Point();
            SendMessage(richTextBox1.Handle, EM_GETSCROLLPOS, 0, ref scroll_pos);

            LockWindowUpdate(richTextBox1.Handle);

            for (int i = from; i < to; i++) { 
                if ( ok_for_name( richTextBox1.Text[i] ) ){
                    if(!name){
                        name_start = i;
                        name = true;
                    }
                }else{
                    if(name){
                        int name_len = i - name_start;
                        string the_name = richTextBox1.Text.Substring(name_start, name_len);

                        int hash = the_name.GetHashCode();                        
                        int name_R = 128 + (hash % 128);
                        int name_G = 128 + ((hash / 128) % 128);
                        int name_B = 128 + ((hash / 128 / 128) % 128);

                        richTextBox1.Select(name_start, name_len);
                        richTextBox1.SelectionColor = Color.FromArgb(name_R, name_G, name_B);
                        
                        name = false;
                    }
                }
            }
            
            richTextBox1.Select(sel_start, sel_len);
            SendMessage(richTextBox1.Handle, EM_SETSCROLLPOS, 0, ref scroll_pos);
            LockWindowUpdate((IntPtr)0);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) {
            recolor(0, richTextBox1.Text.Length);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog1.FileName = file_name;
            if(openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK ){                
                file_name = openFileDialog1.FileName;
                richTextBox1.LoadFile(file_name, RichTextBoxStreamType.PlainText);
                saveFileDialog1.FileName = file_name;
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
            int hash = DateTime.Now.TimeOfDay.GetHashCode();
            int name_R = hash % 32;
            int name_G = (hash / 32) % 32;
            int name_B = (hash / 32 / 32) % 32;
            richTextBox1.BackColor = Color.FromArgb( name_R, name_G, name_B );
            recolor(0, richTextBox1.Text.Length);
        }
    }
}
