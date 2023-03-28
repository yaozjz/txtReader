using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace txtReader
{
    public partial class ReaderForm : Form
    {
        public ReaderForm(string FilePath, int weizhi)
        {
            InitializeComponent();
            //记录大小
            Width = Properties.Settings.Default.reader_width;
            Height = Properties.Settings.Default.reader_height;
            //隐藏状态
            unDisplayTitle.Checked = Properties.Settings.Default.display_title;
            splitContainer1.Panel1Collapsed = unDisplayTitle.Checked;
            //打开文件
            scrol_index = weizhi;
            OpenFile(FilePath);
        }
        //隐藏状态
        private void unDisplayTitle_Click(object sender, EventArgs e)
        {
            unDisplayTitle.Checked = !unDisplayTitle.Checked;
            splitContainer1.Panel1Collapsed = unDisplayTitle.Checked;
        }
        //======

        [DllImport("user32.dll")]
        private static extern int GetScrollPos(IntPtr hWnd, Int32 nBar);
        [DllImport("user32.dll")]
        private static extern int SetScrollPos(IntPtr hWnd, Int32 nBar, Int32 nPos, bool bRedraw);
        [DllImport("user32.dll")]
        private static extern bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);
        int scrollRange = 0;
        int lineHeight = 0;
        public int scrol_index = 0;
        private void GetScrol()
        {
            int min, max;
            if (GetScrollRange(NovelShow.Handle, 1, out min, out max))
            {
                scrollRange = max - min;
                lineHeight = scrollRange / NovelShow.Lines.Length;
            }
        }
        private int GetIndex()
        {
            int pos = GetScrollPos(NovelShow.Handle, 1);
            int line = (int)(pos / (lineHeight));
            int index = NovelShow.GetFirstCharIndexFromLine(line);
            return index;
        }
        //=======
        /////文件打开
        private void OpenFile(string FilePath)
        {
            Encoding ecode = TextFormat.GetTextFileEncodingType(FilePath);
            try
            {
                FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, Encoding.GetEncoding(ecode.BodyName));

                string line = sr.ReadLine();
                StringBuilder sb = new StringBuilder();
                while (line != null)
                {
                    sb.Append(line + Environment.NewLine);
                    line = sr.ReadLine();
                }
                NovelShow.Text = sb.ToString();
                sb.Clear();
                sr.Close();
                fs.Close();
                NovelShow.SelectionStart = scrol_index;
                NovelShow.ScrollToCaret();
            }
            catch (Exception ex)
            {
                MessageBox.Show("发生错误：" + ex.Message);
            }
        }
        //章节显示
        private void AddTitle(int line_index, string ttName)
        {
            string[] msg = { ttName, line_index.ToString() };
            dataGridView1.Rows.Add(msg);
        }
        //获取当前字体格式
        private void get_font_set()
        {
            FontStatus.Text = NovelShow.Font.Name + " " + NovelShow.Font.Size + "pt";
        }
        //刷新当前聚焦的行数
        private void fresh_index()
        {
            int foucusIndex = NovelShow.SelectionStart;
            NowLine.Text = "第" + (NovelShow.GetLineFromCharIndex(foucusIndex) + 1) + "行";
        }
        //===========END
        //退出
        private void Exit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
        //获取章节
        private void GetTitle_Click(object sender, EventArgs e)
        {
            //清空表格
            dataGridView1.Rows.Clear();
            //检查章节是否正确
            int i = 0;
            foreach (string line in NovelShow.Lines)
            {
                //检查行中是否存在章节名称
                Match result = Regex.Match(line, Properties.Settings.Default.title_rule);
                if (result.Success)
                {
                    AddTitle(i, line);
                }
                i++;
            }
        }
        //加载项
        private void ReaderForm_Load(object sender, EventArgs e)
        {            
            fresh_index();
            get_font_set();
            GetScrol();
        }
        //字体设置
        private void FontSet_Click(object sender, EventArgs e)
        {
            FontDialog font_set = new FontDialog();
            font_set.ShowColor = true;
            font_set.Font = NovelShow.Font;
            font_set.Color = NovelShow.ForeColor;
            if (font_set.ShowDialog() == DialogResult.OK)
            {
                NovelShow.Font = font_set.Font;
                NovelShow.ForeColor = font_set.Color;
            }
            font_set.Dispose();
            get_font_set();
        }
        //隐藏状态
        private void NovelShow_SelectionChanged(object sender, EventArgs e)
        {
            fresh_index();
        }
        //双击定位章节
        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                //获取行数
                int select_index = dataGridView1.CurrentRow.Index;
                string index_str = dataGridView1.Rows[select_index].Cells["line"].Value.ToString();
                try
                {
                    int line_index = int.Parse(index_str);
                    //自动换行后，richtextbox的真实行数不等于原来的行数，因此需要先关掉自动换行，再将光标移动到章节所在行
                    if (NovelShow.WordWrap)
                        NovelShow.WordWrap = false;
                    NovelShow.SelectionStart = NovelShow.GetFirstCharIndexFromLine(line_index);
                    NovelShow.Focus();
                    //如果之前是开启了自动换行，则此时应该将变换状态转换回去
                    NovelShow.WordWrap = true;
                    NovelShow.ScrollToCaret();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message, "Error");
                }
            }
        }
        //窗口关闭时
        private void ReaderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //配置
            Properties.Settings.Default.reader_width = Size.Width;
            Properties.Settings.Default.reader_height = Size.Height;
            Properties.Settings.Default.display_title = unDisplayTitle.Checked;
            Properties.Settings.Default.Save();
            //scrol_index = NovelShow.SelectionStart;
            scrol_index = GetIndex();
        }
        //
    }
}
