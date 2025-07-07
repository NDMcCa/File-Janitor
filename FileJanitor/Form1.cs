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
    Button refreshButton;
    Button deleteButton;
    CheckBox permaDelete;
    ListBox fileList;
    public Form1()
    {
        InitializeComponent();
        this.Text = "File Janitor";

        folderTextBox = new TextBox { Left = 10, Top = 10, Width = 300 };
        browseButton = new Button { Text = "Browse", Left = 320, Top = 10 };
        refreshButton = new Button { Text = "Refresh", Left = 120, Top = 50 };
        extensionTextBox = new TextBox { Left = 10, Top = 50, Width = 100, PlaceholderText = ".txt" };
        deleteButton = new Button { Text = "Delete Files", Left = 10, Top = 90 };
        permaDelete = new CheckBox { Left = 100, Top = 90, Width = 200, Checked = false, Text = "Delete Permanently" };
        fileList = new ListBox { Left = 420, Top = 10, Width = 300, Height = 150 };

        browseButton.Click += Browse_Click;
        deleteButton.Click += Delete_Click;
        refreshButton.Click += Refresh_Click;

        this.Controls.Add(folderTextBox);
        this.Controls.Add(browseButton);
        this.Controls.Add(refreshButton);
        this.Controls.Add(extensionTextBox);
        this.Controls.Add(deleteButton);
        this.Controls.Add(permaDelete);
        this.Controls.Add(fileList);

    }

    private bool Validate_Path()
    {
        if (string.IsNullOrWhiteSpace(folderTextBox.Text))
        {
            MessageBox.Show("No selected. Please select a folder valid folder first", "Missing Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        else if (!Directory.Exists(folderTextBox.Text))
        {
            MessageBox.Show("Invalid folder");
            return false;
        }
        else
        {
            return true;
        }
    }

    private void Query_Files()
    {

        string folder = folderTextBox.Text;
        string ext = extensionTextBox.Text.TrimStart('.');
        string[] files = Directory.GetFiles(folder, "*." + ext);

        fileList.Items.Clear();

        if (Validate_Path());
        {
            foreach (var file in files)
            {
                fileList.Items.Add(file.Remove(0, folder.Length + 1));
            }
        }
    }

    private void Refresh_Click(object sender, EventArgs e)
    {
        if (Validate_Path()){
            Query_Files();             
        }
    }

    private void Browse_Click(object sender, EventArgs e)
    {
        using (FolderBrowserDialog dialog = new FolderBrowserDialog())
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                folderTextBox.Text = dialog.SelectedPath;
                Query_Files();
            }
        }
    }

    private void Delete_Click(object sender, EventArgs e)
    {
        string folder = folderTextBox.Text;
        string ext = extensionTextBox.Text.TrimStart('.');
        bool perma = permaDelete.Checked;


        if (Validate_Path())
        {
            string[] files = Directory.GetFiles(folder, "*." + ext);
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
            Query_Files();

            MessageBox.Show($"Deleted {files.Length} file(s) of type .{ext}");
        }
    }
}
