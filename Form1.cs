using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace txtReader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //读取窗口大小
            Width = Properties.Settings.Default.width;
            Height = Properties.Settings.Default.height;
            //刷新书架
            PrintBooks();
        }
        private void Exit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        //关闭保存
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //保存到文件
            SaveBooks();
            //配置
            Properties.Settings.Default.width = Size.Width;
            Properties.Settings.Default.height = Size.Height;
            Properties.Settings.Default.Save();
        }
        //=========变量
        public string libPath = "./lib/";
        private string config_txt = "./lib/sql.txt";
        private int index = -1;
        //=========常用函数
        private void SaveBooks()
        {
            try
            {
                FileStream fs = new FileStream(config_txt, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, Encoding.GetEncoding("utf-8"));
                int rows_count = dataGridView1.Rows.Count;
                for (int i = 0; i < rows_count; i++)
                {
                    try
                    {
                        var now_Rows = dataGridView1.Rows[i];
                        object _name = now_Rows.Cells[0].Value;
                        object _path = now_Rows.Cells[1].Value;
                        object _index = now_Rows.Cells[2].Value;
                        if (_name != null && _path != null && _index != null)
                        {
                            sw.WriteLine(string.Format("{0};{1};{2}", _name, _path, _index));
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                sw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误：" + ex.Message);
            }
        }
        private void ShowList(string Str1, string Str2, string Str3)
        {
            string[] msg = { Str1, Str2 , Str3};
            dataGridView1.Rows.Add(msg);
        }

        private void PrintBooks()
        {
            dataGridView1.Rows.Clear();
            //string[] files = Directory.GetFiles(@"./lib/books/", "*.txt");
            //foreach (string name in files)
            //{
            //    ShowList(name, "0");
            //}
            try
            {
                FileStream fs = new FileStream(config_txt, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("UTF-8"));

                string line = sr.ReadLine();
                while (line != null)
                {
                    string[] r = line.Split(';');
                    ShowList(r[0], r[1], r[2]);
                    line = sr.ReadLine();
                }
                sr.Close();
                fs.Close();
            }
            catch
            {
                MessageBox.Show("配置文件不完整！");
            }
        }

        //=========常用函数END
        //书架刷新按钮
        private void FreshList_Click(object sender, EventArgs e)
        {
            PrintBooks();
        }
        //双击打开
        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if(e.RowIndex > -1)
            {
                //获取行数
                int select_index = dataGridView1.CurrentRow.Index;
                var _selection = dataGridView1.Rows[select_index];
                string novel_path = _selection.Cells[1].Value.ToString();
                string readline = _selection.Cells[2].Value.ToString();
                ReaderForm reader = new ReaderForm(novel_path, int.Parse(readline));
                this.Hide();
                reader.ShowDialog();
                this.Show();
                _selection.Cells[2].Value = reader.scrol_index;
                reader.Dispose();
            }
        }
        //添加数据
        private void AddNovel_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "文本文件";
            dlg.DefaultExt = ".txt";
            dlg.Filter = "保存当前文本|*.txt";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string novel_path = dlg.FileName;
                string novel_name = System.IO.Path.GetFileNameWithoutExtension(novel_path);
                FileInfo f1 = new FileInfo(novel_path);
                string f2_name = libPath + "books/" + novel_name + ".txt";
                try
                {
                    f1.CopyTo(f2_name);
                }
                catch
                {
                    //删除再复制
                    FileInfo f2 = new FileInfo(f2_name);
                    f2.Delete();
                    f1.CopyTo(f2_name);
                }
                ShowList(novel_name, f2_name, "0");
            }
        }
        //保存书架列表
        private void SaveList_Click(object sender, EventArgs e)
        {
            SaveBooks();
        }
        //删除选中行
        private void DeleteRows_Click(object sender, EventArgs e)
        {
            if (!dataGridView1.Rows[index].IsNewRow && index > -1)
            {
                string file_path = dataGridView1.Rows[index].Cells[1].Value.ToString();
                FileInfo f = new FileInfo(file_path);
                f.Delete();
                dataGridView1.Rows.RemoveAt(index);                
            }
        }
        //右键菜单
        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                index = e.RowIndex;
                dataGridView1.Rows[index].Selected = true;
                dataGridView1.CurrentCell = dataGridView1.Rows[index].Cells[0];
                contextMenuStrip1.Show(dataGridView1, e.Location);
                contextMenuStrip1.Show(Cursor.Position);
            }
        }
    }
}
