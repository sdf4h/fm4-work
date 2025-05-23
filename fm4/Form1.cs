using System;
using System.Net;
using System.Windows.Forms;

namespace DocDownloader
{
    public partial class MainForm : Form
    {
        private TextBox txtUrl;
        private Button btnDownload;
        private Label lblUrl;
        private ProgressBar progressBar;
        private Label lblStatus;
        private CheckBox chkAsync;

        public MainForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Настройка формы
            this.Text = "Загрузчик документов";
            this.ClientSize = new System.Drawing.Size(500, 180);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Создание и настройка элементов управления

            // Label для URL
            lblUrl = new Label
            {
                Text = "URL документа:",
                Location = new System.Drawing.Point(15, 15),
                AutoSize = true
            };

            // TextBox для ввода URL
            txtUrl = new TextBox
            {
                Location = new System.Drawing.Point(15, 40),
                Size = new System.Drawing.Size(470, 25),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };

            // CheckBox для асинхронной загрузки
            chkAsync = new CheckBox
            {
                Text = "Асинхронная загрузка",
                Location = new System.Drawing.Point(15, 70),
                AutoSize = true,
                Checked = true
            };

            // Кнопка загрузки
            btnDownload = new Button
            {
                Text = "Скачать",
                Location = new System.Drawing.Point(15, 100),
                Size = new System.Drawing.Size(100, 30),
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            btnDownload.Click += BtnDownload_Click;

            // ProgressBar
            progressBar = new ProgressBar
            {
                Location = new System.Drawing.Point(15, 140),
                Size = new System.Drawing.Size(470, 20),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Style = ProgressBarStyle.Continuous
            };

            // Label для статуса
            lblStatus = new Label
            {
                Text = "Готово к загрузке",
                Location = new System.Drawing.Point(120, 105),
                AutoSize = true
            };

            // Добавление элементов на форму
            this.Controls.Add(lblUrl);
            this.Controls.Add(txtUrl);
            this.Controls.Add(chkAsync);
            this.Controls.Add(btnDownload);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblStatus);
        }

        private async void BtnDownload_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();

            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Введите URL документа!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show("Некорректный URL! Должен начинаться с http:// или https://",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Все файлы (*.*)|*.*",
                Title = "Сохранить файл как..."
            };

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                btnDownload.Enabled = false;
                lblStatus.Text = "Загрузка...";
                progressBar.Value = 0;

                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, args) =>
                    {
                        progressBar.Value = args.ProgressPercentage;
                    };

                    client.DownloadFileCompleted += (s, args) =>
                    {
                        if (args.Error != null)
                        {
                            lblStatus.Text = "Ошибка загрузки";
                            MessageBox.Show($"Ошибка: {args.Error.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            lblStatus.Text = "Загрузка завершена!";
                            MessageBox.Show($"Файл успешно сохранён как:\n{saveDialog.FileName}",
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        btnDownload.Enabled = true;
                    };

                    if (chkAsync.Checked)
                    {
                        await client.DownloadFileTaskAsync(new Uri(url), saveDialog.FileName);
                    }
                    else
                    {
                        client.DownloadFile(new Uri(url), saveDialog.FileName);
                        lblStatus.Text = "Загрузка завершена!";
                        MessageBox.Show($"Файл успешно сохранён как:\n{saveDialog.FileName}",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        btnDownload.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Ошибка загрузки";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnDownload.Enabled = true;
            }
        }
    }
}