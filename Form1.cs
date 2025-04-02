using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CodeViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "Kod Satır Sayacı & Analiz Aracı 🚀";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Proje klasörünü seçin.";

            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
                return;

            string selectedPath = folderBrowserDialog1.SelectedPath;
            labelSelectedPath.Text = $"📁 Seçilen klasör: {selectedPath}";

            bool isDetailed = checkBox1.Checked;
            progressBar1.Visible = true;
            Application.DoEvents();
            button1.Enabled = false;

            try
            {
                string result = string.Empty;

                if (radioButton1.Checked)
                {
                    var solutionRoot = FindSolutionDirectory(selectedPath);
                    if (solutionRoot == null)
                    {
                        textBox1.Text = ".sln dosyası bulunamadı.";
                        return;
                    }

                    result = isDetailed
                        ? CountDotNetDetailed(solutionRoot)
                        : CountDotNetClean(solutionRoot);
                }
                else if (radioButton2.Checked)
                {
                    result = isDetailed
                        ? CountFrontEndDetailed(selectedPath)
                        : CountFrontEndClean(selectedPath);
                }
                else if (radioButton3.Checked)
                {
                    result = isDetailed
                        ? CountJavaSpringDetailed(selectedPath)
                        : CountJavaSpringClean(selectedPath);
                }
                else if (radioButton4.Checked)
                {
                    result = isDetailed
                        ? CountPureAndroidDetailed(selectedPath)
                        : CountPureAndroidClean(selectedPath);
                }
                else if (radioButton5.Checked)
                {
                    result = isDetailed
                        ? CountKotlinFlutterDetailed(selectedPath)
                        : CountKotlinFlutterClean(selectedPath);
                }
                else
                {
                    MessageBox.Show("Lütfen bir proje türü seçin.");
                    return;
                }

                textBox1.Text = result;
            }
            finally
            {
                progressBar1.Visible = false;
                button1.Enabled = true;
            }
        }


        private string CountDotNetDetailed(string root)
        {
            var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                                 .Where(f => f.EndsWith(".cs") || f.EndsWith(".json") || f.EndsWith(".xml") ||
                                             f.EndsWith(".xaml") || f.EndsWith(".config") || f.EndsWith(".cshtml"))
                                 .Where(f => !f.Contains(@"\obj\") && !f.Contains(@"\bin\"))
                                 .ToList();

            int totalLines = 0;
            int maxLines = 0;
            string largestFile = "";
            var fileData = new List<(string, int)>();

            foreach (var file in files)
            {
                try
                {
                    int lineCount = File.ReadAllLines(file).Length;
                    totalLines += lineCount;
                    fileData.Add((Path.GetFileName(file), lineCount));

                    if (lineCount > maxLines)
                    {
                        maxLines = lineCount;
                        largestFile = Path.GetFileName(file);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Dosya okunamadı: {file} - Hata: {ex.Message}");
                }
            }

            double average = files.Count > 0 ? (double)totalLines / files.Count : 0;

            UpdateListView(fileData);


            return $"🟦 .NET Kapsamlı Sayım\r\n" +
                   $"📁 Dosya sayısı: {files.Count}\r\n" +
                   $"📏 Toplam satır: {totalLines}\r\n" +
                   $"📊 Ortalama satır/dosya: {average:F2}\r\n" +
                   $"📄 En uzun dosya: {largestFile} ({maxLines} satır)";
        }
        private string CountDotNetClean(string root)
        {
            var files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
                                 .Where(f => !f.EndsWith(".Designer.cs"))
                                 .Where(f => !f.Contains(@"\obj\") && !f.Contains(@"\bin\"))
                                 .ToList();

            int totalLines = 0;
            int maxLines = 0;
            string largestFile = "";
            var fileData = new List<(string, int)>();

            foreach (var file in files)
            {
                try
                {
                    var lines = File.ReadAllLines(file)
                                    .Select(line => line.Trim())
                                    .Where(line => !string.IsNullOrWhiteSpace(line))
                                    .Where(line => !line.StartsWith("//"))
                                    .ToList();

                    int lineCount = lines.Count;
                    totalLines += lineCount;
                    fileData.Add((Path.GetFileName(file), lineCount));

                    if (lineCount > maxLines)
                    {
                        maxLines = lineCount;
                        largestFile = Path.GetFileName(file);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Dosya analizinde hata: {file} - {ex.Message}");
                }
            }

            double average = files.Count > 0 ? (double)totalLines / files.Count : 0;

            UpdateListView(fileData);


            return $"🟦 .NET Sade Sayım (.cs)\r\n" +
                   $"📁 Dosya sayısı: {files.Count}\r\n" +
                   $"📏 Toplam anlamlı kod satırı: {totalLines}\r\n" +
                   $"📊 Ortalama satır/dosya: {average:F2}\r\n" +
                   $"📄 En uzun dosya: {largestFile} ({maxLines} satır)";
        }


        private string CountFrontEndDetailed(string root)
        {
            var exts = new[] { ".js", ".ts", ".jsx", ".tsx", ".html", ".css", ".scss", ".json", ".vue", ".svelte" };

            var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                                 .Where(f => exts.Contains(Path.GetExtension(f)))
                                 .Where(f => !f.Contains("node_modules") && !f.Contains("dist") && !f.Contains("build"))
                                 .ToList();

            int totalLines = 0;
            int maxLines = 0;
            string largestFile = "";
            var fileData = new List<(string, int)>();
            Dictionary<string, int> extensionCounts = new Dictionary<string, int>();

            foreach (var file in files)
            {
                try
                {
                    int lines = File.ReadAllLines(file).Length;
                    totalLines += lines;
                    fileData.Add((Path.GetFileName(file), lines));

                    if (lines > maxLines)
                    {
                        maxLines = lines;
                        largestFile = Path.GetFileName(file);
                    }

                    string ext = Path.GetExtension(file);
                    if (extensionCounts.ContainsKey(ext))
                        extensionCounts[ext]++;
                    else
                        extensionCounts[ext] = 1;
                }
                catch { }
            }

            double average = files.Count > 0 ? (double)totalLines / files.Count : 0;
            string mostCommonExt = extensionCounts.OrderByDescending(e => e.Value).FirstOrDefault().Key;

            UpdateListView(fileData);


            return $"🟨 FrontEnd Kapsamlı Sayım\r\n" +
                   $"📁 Dosya sayısı: {files.Count}\r\n" +
                   $"📏 Toplam satır: {totalLines}\r\n" +
                   $"📊 Ortalama satır/dosya: {average:F2}\r\n" +
                   $"📄 En uzun dosya: {largestFile} ({maxLines} satır)\r\n" +
                   $"🔁 En çok uzantı: {mostCommonExt}";
        }
        private string CountFrontEndClean(string root)
        {
            var exts = new[] { ".js", ".jsx", ".ts", ".tsx" };

            var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                                 .Where(f => exts.Contains(Path.GetExtension(f)))
                                 .Where(f => !f.Contains("node_modules") && !f.Contains("dist") && !f.Contains("build"))
                                 .ToList();

            int totalLines = 0;
            int maxLines = 0;
            string largestFile = "";
            var fileData = new List<(string, int)>();

            foreach (var file in files)
            {
                try
                {
                    var lines = File.ReadAllLines(file)
                                    .Select(line => line.Trim())
                                    .Where(line => !string.IsNullOrWhiteSpace(line))
                                    .Where(line => !line.StartsWith("//"))
                                    .ToList();

                    int lineCount = lines.Count;
                    totalLines += lineCount;
                    fileData.Add((Path.GetFileName(file), lineCount));

                    if (lineCount > maxLines)
                    {
                        maxLines = lineCount;
                        largestFile = Path.GetFileName(file);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Dosya analizinde hata: {file} - {ex.Message}");
                }
            }

            double average = files.Count > 0 ? (double)totalLines / files.Count : 0;

            UpdateListView(fileData);


            return $"🟨 FrontEnd Sade Sayım\r\n" +
                   $"📁 Dosya sayısı: {files.Count}\r\n" +
                   $"📏 Toplam anlamlı kod satırı: {totalLines}\r\n" +
                   $"📊 Ortalama satır/dosya: {average:F2}\r\n" +
                   $"📄 En uzun dosya: {largestFile} ({maxLines} satır)";
        }


        private string CountJavaSpringClean(string root)
        {
            var files = Directory.GetFiles(root, "*.java", SearchOption.AllDirectories)
                                 .Where(f => !f.Contains("target"))
                                 .ToList();

            int totalLines = 0;
            int maxLines = 0;
            string largestFile = "";
            var fileData = new List<(string, int)>();

            foreach (var file in files)
            {
                try
                {
                    var lines = File.ReadAllLines(file)
                                    .Select(line => line.Trim())
                                    .Where(line => !string.IsNullOrWhiteSpace(line))
                                    .Where(line => !line.StartsWith("//"))
                                    .ToList();

                    int lineCount = lines.Count;
                    totalLines += lineCount;
                    fileData.Add((Path.GetFileName(file), lineCount));

                    if (lineCount > maxLines)
                    {
                        maxLines = lineCount;
                        largestFile = Path.GetFileName(file);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Dosya analizinde hata: {file} - {ex.Message}");
                }
            }

            double average = files.Count > 0 ? (double)totalLines / files.Count : 0;

            UpdateListView(fileData);

            return $"🟫 Java Spring Sade Sayım\r\n" +
                   $"📁 Dosya sayısı: {files.Count}\r\n" +
                   $"📏 Toplam anlamlı kod satırı: {totalLines}\r\n" +
                   $"📊 Ortalama satır/dosya: {average:F2}\r\n" +
                   $"📄 En uzun dosya: {largestFile} ({maxLines} satır)";
        }
        private string CountJavaSpringDetailed(string root)
        {
            var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                                 .Where(f => f.EndsWith(".java") || f.EndsWith(".xml") || f.EndsWith(".properties"))
                                 .Where(f => !f.Contains("target"))
                                 .ToList();

            int totalLines = 0;
            int maxLines = 0;
            string largestFile = "";
            var fileData = new List<(string, int)>();

            foreach (var file in files)
            {
                try
                {
                    int lines = File.ReadAllLines(file).Length;
                    totalLines += lines;
                    fileData.Add((Path.GetFileName(file), lines));

                    if (lines > maxLines)
                    {
                        maxLines = lines;
                        largestFile = Path.GetFileName(file);
                    }
                }
                catch { }
            }

            double average = files.Count > 0 ? (double)totalLines / files.Count : 0;
            UpdateListView(fileData);

            return $"🟫 Java Spring Kapsamlı Sayım\r\n" +
                   $"📁 Dosya sayısı: {files.Count}\r\n" +
                   $"📏 Toplam satır: {totalLines}\r\n" +
                   $"📊 Ortalama satır/dosya: {average:F2}\r\n" +
                   $"📄 En uzun dosya: {largestFile} ({maxLines} satır)";
        }


        private string CountPureAndroidClean(string root)
        {
            var files = Directory.GetFiles(root, "*.java", SearchOption.AllDirectories)
                                 .Where(f => !f.Contains("build"))
                                 .ToList();

            int totalLines = 0;
            int maxLines = 0;
            string largestFile = "";
            var fileData = new List<(string, int)>();

            foreach (var file in files)
            {
                try
                {
                    var lines = File.ReadAllLines(file)
                                    .Select(line => line.Trim())
                                    .Where(line => !string.IsNullOrWhiteSpace(line))
                                    .Where(line => !line.StartsWith("//"))
                                    .ToList();

                    int lineCount = lines.Count;
                    totalLines += lineCount;
                    fileData.Add((Path.GetFileName(file), lineCount));

                    if (lineCount > maxLines)
                    {
                        maxLines = lineCount;
                        largestFile = Path.GetFileName(file);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Dosya analizinde hata: {file} - {ex.Message}");
                }
            }

            double average = files.Count > 0 ? (double)totalLines / files.Count : 0;

            UpdateListView(fileData);

            return $"📱 Pure Java Android Sade Sayım\r\n" +
                   $"📁 Dosya sayısı: {files.Count}\r\n" +
                   $"📏 Toplam anlamlı kod satırı: {totalLines}\r\n" +
                   $"📊 Ortalama satır/dosya: {average:F2}\r\n" +
                   $"📄 En uzun dosya: {largestFile} ({maxLines} satır)";
        }
        private string CountPureAndroidDetailed(string root)
        {
            var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                                 .Where(f => f.EndsWith(".java") || f.EndsWith(".xml"))
                                 .Where(f => !f.Contains("build"))
                                 .ToList();

            int totalLines = 0;
            int maxLines = 0;
            string largestFile = "";
            var fileData = new List<(string, int)>();

            foreach (var file in files)
            {
                try
                {
                    int lines = File.ReadAllLines(file).Length;
                    totalLines += lines;
                    fileData.Add((Path.GetFileName(file), lines));
                    if (lines > maxLines)
                    {
                        maxLines = lines;
                        largestFile = Path.GetFileName(file);
                    }
                }
                catch { }
            }

            double average = files.Count > 0 ? (double)totalLines / files.Count : 0;
            UpdateListView(fileData);

            return $"📱 Pure Java Android Kapsamlı Sayım\r\n" +
                   $"📁 Dosya sayısı: {files.Count}\r\n" +
                   $"📏 Toplam satır: {totalLines}\r\n" +
                   $"📊 Ortalama satır/dosya: {average:F2}\r\n" +
                   $"📄 En uzun dosya: {largestFile} ({maxLines} satır)";
        }


        private string CountKotlinFlutterClean(string root)
        {
            var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                                 .Where(f => f.EndsWith(".kt") || f.EndsWith(".dart"))
                                 .Where(f => !f.Contains("build"))
                                 .ToList();

            int totalLines = 0;
            int maxLines = 0;
            string largestFile = "";
            var fileData = new List<(string, int)>();

            foreach (var file in files)
            {
                try
                {
                    var lines = File.ReadAllLines(file)
                                    .Select(line => line.Trim())
                                    .Where(line => !string.IsNullOrWhiteSpace(line))
                                    .Where(line => !line.StartsWith("//"))
                                    .ToList();

                    int lineCount = lines.Count;
                    totalLines += lineCount;
                    fileData.Add((Path.GetFileName(file), lineCount));

                    if (lineCount > maxLines)
                    {
                        maxLines = lineCount;
                        largestFile = Path.GetFileName(file);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Dosya analizinde hata: {file} - {ex.Message}");
                }
            }

            double average = files.Count > 0 ? (double)totalLines / files.Count : 0;

            UpdateListView(fileData);

            return $"🟣 Kotlin / Flutter Sade Sayım\r\n" +
                   $"📁 Dosya sayısı: {files.Count}\r\n" +
                   $"📏 Toplam anlamlı kod satırı: {totalLines}\r\n" +
                   $"📊 Ortalama satır/dosya: {average:F2}\r\n" +
                   $"📄 En uzun dosya: {largestFile} ({maxLines} satır)";
        }
        private string CountKotlinFlutterDetailed(string root)
        {
            var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                                 .Where(f => f.EndsWith(".kt") || f.EndsWith(".dart") || f.EndsWith(".yaml"))
                                 .Where(f => !f.Contains("build"))
                                 .ToList();

            int totalLines = 0;
            int maxLines = 0;
            string largestFile = "";
            var fileData = new List<(string, int)>();

            foreach (var file in files)
            {
                try
                {
                    int lines = File.ReadAllLines(file).Length;
                    totalLines += lines;
                    fileData.Add((Path.GetFileName(file), lines));
                    if (lines > maxLines)
                    {
                        maxLines = lines;
                        largestFile = Path.GetFileName(file);
                    }
                }
                catch { }
            }

            double average = files.Count > 0 ? (double)totalLines / files.Count : 0;
            UpdateListView(fileData);

            return $"🟣 Kotlin / Flutter Kapsamlı Sayım\r\n" +
                   $"📁 Dosya sayısı: {files.Count}\r\n" +
                   $"📏 Toplam satır: {totalLines}\r\n" +
                   $"📊 Ortalama satır/dosya: {average:F2}\r\n" +
                   $"📄 En uzun dosya: {largestFile} ({maxLines} satır)";
        }


        private string FindSolutionDirectory(string startDir)
        {
            var dir = new DirectoryInfo(startDir);

            while (dir != null && !dir.GetFiles("*.sln").Any())
            {
                dir = dir.Parent;
            }
            return dir?.FullName;
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {
            MessageBox.Show("Bir klasör seçmeniz gerekiyor.");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ApplySoftTheme();
            ApplyTooltips();
            SetupListView();
        }

        private void SetupListView()
        {
            listView1.View = View.Details;
            listView1.Columns.Clear();
            listView1.Columns.Add("📄 Dosya Adı", 400);
            listView1.Columns.Add("📏 Satır Sayısı", 100);
        }

        private void UpdateListView(List<(string fileName, int lineCount)> fileData)
        {
            listView1.Items.Clear();

            foreach (var item in fileData)
            {
                ListViewItem lvi = new ListViewItem(item.fileName);
                lvi.SubItems.Add(item.lineCount.ToString());
                listView1.Items.Add(lvi);
            }
        }

        private void ApplyTooltips()
        {
            toolTip1.IsBalloon = true;
            toolTip1.ToolTipTitle = "Açıklama";
            toolTip1.BackColor = Color.LightYellow;

            toolTip1.SetToolTip(button1, "Projeyi analiz eder ve toplam satır sayısını gösterir.");
            toolTip1.SetToolTip(checkBox1, "Kapsamlı sayım: yorum ve boşluklar da dahil edilir.");
            toolTip1.SetToolTip(radioButton1, ".NET projesi (.sln içeren solution) seçin.");
            toolTip1.SetToolTip(radioButton2, "FrontEnd projesi (React, Angular, Vue, Svelte, JS/TS vb.) klasörünü seçin.");
            toolTip1.SetToolTip(textBox1, "Analiz sonuçları burada gösterilir.");
            toolTip1.SetToolTip(labelSelectedPath, "Analiz yapılan klasörün tam yolu.");
            toolTip1.SetToolTip(buttonExportCsv, "Tabloda listelenen dosya verilerini .csv dosyasına aktar.");
            toolTip1.SetToolTip(listView1, "Analiz edilen dosyaların adları ve satır sayıları burada gösterilir.");
            toolTip1.SetToolTip(treeViewStructure, "Analiz edilen klasör yapısı burada gösterilir.");
            toolTip1.SetToolTip(radioButton3, "Java Spring projesi seçin.");
            toolTip1.SetToolTip(radioButton4, "Pure Java Android projesi seçin.");
            toolTip1.SetToolTip(radioButton5, "Kotlin ya da Flutter projesi seçin.");
        }

        private void ApplySoftTheme()
        {
            this.BackColor = Color.FromArgb(240, 240, 240);

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is GroupBox)
                {
                    ctrl.BackColor = Color.FromArgb(245, 245, 245);
                    ctrl.ForeColor = Color.FromArgb(30, 30, 30);

                    foreach (Control inner in ctrl.Controls)
                    {
                        inner.BackColor = Color.FromArgb(245, 245, 245);
                        inner.ForeColor = Color.FromArgb(30, 30, 30);
                    }
                }
                else if (ctrl is TextBox)
                {
                    ctrl.BackColor = Color.WhiteSmoke;
                    ctrl.ForeColor = Color.Black;
                }
                else if (ctrl is Button)
                {
                    ctrl.BackColor = Color.LightSlateGray;
                    ctrl.ForeColor = Color.White;
                }
                else if (ctrl is CheckBox || ctrl is RadioButton || ctrl is Label)
                {
                    ctrl.ForeColor = Color.FromArgb(30, 30, 30);
                    ctrl.BackColor = Color.Transparent;
                }
            }

            Font defaultFont = new Font("Segoe UI", 10, FontStyle.Regular);

            Font monospaceFont = new Font("Consolas", 10, FontStyle.Regular);

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is TextBox)
                {
                    ctrl.Font = monospaceFont;
                }
                else
                {
                    ctrl.Font = defaultFont;
                }

                if (ctrl is GroupBox groupBox)
                {
                    foreach (Control inner in groupBox.Controls)
                    {
                        if (inner is TextBox)
                            inner.Font = monospaceFont;
                        else
                            inner.Font = defaultFont;
                    }
                }
            }
        }

        private void buttonExportCsv_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                MessageBox.Show("Önce bir analiz yapmalısınız.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            saveFileDialog1.FileName = "SatirAnalizi.csv";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        sw.WriteLine("Dosya Adı,Satır Sayısı");

                        foreach (ListViewItem item in listView1.Items)
                        {
                            string fileName = item.SubItems[0].Text;
                            string lineCount = item.SubItems[1].Text;

                            fileName = fileName.Replace(",", " ");
                            sw.WriteLine($"{fileName},{lineCount}");
                        }
                    }

                    MessageBox.Show("CSV dosyası başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Bir hata oluştu:\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // İstersen buraya log at, ama zorunlu değil
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // Bu checkbox değişince yapılacak bir şey varsa buraya yaz.
            // Şimdilik boş bırakıyoruz.
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {
            // GroupBox'a giriş yapıldığında çalışır.
            // Şimdilik bir şey yapmasına gerek yok.
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Label'a tıklanınca çalışır.
            // Şimdilik bir işlem yapmıyoruz.
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            // Pure Android seçildiğinde yapılacak işlemler buraya yazılabilir.
            // Şimdilik boş geçiyoruz.
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            // Bu alanı ihtiyaç varsa kullanabilirsin, şimdilik boş bırakıyoruz.
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            // Gerekirse buraya kod eklenebilir
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Gerekirse buraya kod eklenebilir
        }
        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {
            // ToolTip popup olduğunda yapılacaklar
        }

    }
}
