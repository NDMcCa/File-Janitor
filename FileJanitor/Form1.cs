using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace FileJanitor;

public partial class Form1 : Form
{
    TextBox folderTextBox;
    TextBox extensionTextBox;
    Button browseButton;
    Button deleteButton;
    CheckBox permaDelete;
    public Form1()
    {
        InitializeComponent();
        this.Text = "File Janitor";

        folderTextBox = new TextBox { Left = 10, Top = 10, Width = 300 };
        browseButton = new Button { Text = "Browse", Left = 320, Top = 10 };
        extensionTextBox = new TextBox { Left = 10, Top = 50, Width = 100, PlaceholderText = ".txt" };
        deleteButton = new Button { Text = "Delete Files", Left = 10, Top = 90 };
        permaDelete = new CheckBox { Left = 100, Top = 90, Checked = false };

        browseButton.Click += Browse_Click;
        deleteButton.Click += Delete_Click;

        this.Controls.Add(folderTextBox);
        this.Controls.Add(browseButton);
        this.Controls.Add(extensionTextBox);
        this.Controls.Add(deleteButton);
        this.Controls.Add(permaDelete);

    }

    private void Browse_Click(object sender, EventArgs e)
    {
        using (FolderBrowserDialog dialog = new FolderBrowserDialog())
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                folderTextBox.Text = dialog.SelectedPath;
            }
        }
    }

    private void Delete_Click(object sender, EventArgs e)
    {
        string folder = folderTextBox.Text;
        string ext = extensionTextBox.Text.TrimStart('.');
        bool perma = permaDelete.Checked;

        if (!Directory.Exists(folder))
        {
            MessageBox.Show("Invalid folder");
        }

        var files = Directory.GetFiles(folder, "*." + ext);
        foreach (var file in files)
        {
            if (perma)
            {
                FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
            }
            else
            {
                FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                
            }
        }

        MessageBox.Show($"Deleted {files.Length} file(s) of type .{ext}");
    }
}
