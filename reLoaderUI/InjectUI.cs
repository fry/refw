using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace reLoaderUI {
    public partial class InjectUI : Form {
        class ProcessListEntry {
            static Dictionary<Tuple<int, string>, string> MainModuleFilenameCache = new Dictionary<Tuple<int, string>, string>();

            public Process Process;
            public override string ToString() {
                var key = new Tuple<int, string>(Process.Id, Process.ProcessName);
                if (!MainModuleFilenameCache.ContainsKey(key)) {
                    try {
                        MainModuleFilenameCache[key] = this.Process.MainModule.FileName;
                    } catch (Win32Exception e) {
                        MainModuleFilenameCache[key] = "";
                    }
                }
                return String.Format("{0} - {1} ({2})", Process.ProcessName, MainModuleFilenameCache[key], Process.Id);
            }
        }

        public InjectUI() {
            InitializeComponent();

            RefreshProcessList();
        }

        void RefreshProcessList() {
            comboProcessList.BeginUpdate();
            comboProcessList.Items.Clear();

            foreach (var process in Process.GetProcesses()) {
                comboProcessList.Items.Add(new ProcessListEntry {
                    Process = process
                });
            }

            comboProcessList.EndUpdate();

            comboProcessName.BeginUpdate();
            comboProcessName.Items.Clear();

            foreach (var process in Process.GetProcesses().Select(p => p.ProcessName).Distinct()) {
                comboProcessName.Items.Add(process);
            }

            comboProcessName.EndUpdate();
        }

        private void comboProcessList_DropDown(object sender, EventArgs e) {
            RefreshProcessList();
        }

        private void btnPickDLL_Click(object sender, EventArgs e) {
            if (pickDLLDialog.ShowDialog() == DialogResult.OK) {
                textInjectDLL.Text = pickDLLDialog.FileName;
            }
        }

        private void btnInject_Click(object sender, EventArgs e) {
            Process selected_process = null;
            if (radioTargetProcess.Checked) {
                selected_process = (comboProcessList.SelectedItem as ProcessListEntry).Process;
            } else if (radioProcessName.Checked) {
                selected_process = Process.GetProcessesByName(comboProcessName.SelectedItem as String).FirstOrDefault();
            }

            if (selected_process == null)
                return;

            try {
                refw.Loader.Inject(selected_process.Id, textInjectDLL.Text, textDLLArguments.Lines, true);
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Injection Failed");
            }
        }

        private void comboProcessName_DropDown(object sender, EventArgs e) {
            RefreshProcessList();
        }

        private void textInjectDLL_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effect = DragDropEffects.Link;
            } else {
                e.Effect = DragDropEffects.None;
            }
        }

        private void textInjectDLL_DragDrop(object sender, DragEventArgs e) {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0) {
                textInjectDLL.Text = files[0];
            }
        }
    }
}
