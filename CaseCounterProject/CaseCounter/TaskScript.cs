using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.IO;


namespace CaseCounter {
    public abstract class TaskScript {

        private ListBox listBox;
        protected Config config;

        public TaskScript(ListBox listBox) {
            this.listBox = listBox;
            listBox.Items.Clear();
            config = new();
        }

        public abstract bool Build();

        protected void ReportStep(string str) {
            _ = listBox.Items.Add(str);

        }

        protected static string CreateOutputDirectories(string[] directories, out bool error) {
            error = false;

            _ = MessageBox.Show("Select directory for output");
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();
            string path = folderDialog.SelectedPath;
            if (result == System.Windows.Forms.DialogResult.OK) {

                foreach (string dir in directories) {
                    string path1 = Path.Combine(path, dir);
                    _ = Directory.CreateDirectory(path1);
                }
            } else {
                error = true;
                return "";
            }
            return path;

        }

        protected static string GetTopLevelInputDirectory(out bool error) {

            _ = MessageBox.Show("Select directory for input files");

            string path = "";
            error = false;

            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();


            if (result == System.Windows.Forms.DialogResult.OK) {
                path = folderDialog.SelectedPath;
            } else {
                error = true;
            }
            return path;
        }

    }
}
