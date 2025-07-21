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
    CheckBox recursive;
    ListBox fileList;
    Label hitCount;
    System.Windows.Forms.Timer refreshTimer;

    public Form1()
    {
        InitializeComponent();
        this.Text = "File Janitor";

        folderTextBox = new TextBox { Left = 10, Top = 10, Width = 300 };
        browseButton = new Button { Text = "Browse", Left = 320, Top = 10 };
        refreshButton = new Button { Text = "Refresh", Left = 120, Top = 50 };
        extensionTextBox = new TextBox { Left = 10, Top = 50, Width = 100, PlaceholderText = "File Extension" };
        deleteButton = new Button { Text = "Delete Files", Left = 10, Top = 90 };
        permaDelete = new CheckBox { Left = 100, Top = 90, Width = 200, Checked = false, Text = "Delete Permanently" };
        recursive = new CheckBox { Left = 240, Top = 90, Width = 150, Checked = false, Text = "Recursive" };
        fileList = new ListBox { Left = 420, Top = 10, Width = 300, Height = 150 };
        refreshTimer = new System.Windows.Forms.Timer { Interval = 500 };
        hitCount = new Label { Left = 417, Top = 152, AutoSize = true, Text = "No files found" };


        browseButton.Click += Browse_Click;
        deleteButton.Click += Delete_Click;
        refreshButton.Click += Refresh_Click;
        recursive.CheckStateChanged += Refresh_Timer;
        extensionTextBox.TextChanged += Refresh_Timer;
        refreshTimer.Tick += RefreshTimer_Tick;
        refreshTimer.Enabled = false;

        this.Controls.Add(folderTextBox);
        this.Controls.Add(browseButton);
        this.Controls.Add(refreshButton);
        this.Controls.Add(extensionTextBox);
        this.Controls.Add(deleteButton);
        this.Controls.Add(recursive);
        this.Controls.Add(permaDelete);
        this.Controls.Add(fileList);
        this.Controls.Add(hitCount);

    }

    /// <summary>
    /// A method to validate the current selected folder path allowing the functionalty to be defined in one place and avioding repeated use of try-catch
    /// in all methods that require this check.
    /// </summary>
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

    /// <summary>
    /// Queries a specified and recursively queries all available subfolders for all files of a specified extension
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="searchPattern"></param>
    /// <returns></returns>
    private List<string> Recursive_Query_Files(string folder, string searchPattern)
    {
        List<string> result = new List<string>();
        string[] subFolders = Directory.GetDirectories(folder, searchPattern);
        result.AddRange(Directory.GetFiles(folder, searchPattern));
        foreach (var subFolder in subFolders)
        {
            result.AddRange(Recursive_Query_Files(subFolder, searchPattern));
        }
        return result;
    }

    /// <summary>
    /// Queries a specified folder for all files of a specified extension
    /// </summary>
    private void Query_Files()
    {
        string folder = folderTextBox.Text;
        string ext = extensionTextBox.Text.TrimStart('.');
        string searchPattern = string.IsNullOrWhiteSpace(ext) ? "*" : $"*.{ext.TrimStart('.')}";

        fileList.Items.Clear();

        if (Validate_Path())
        {
            string[] files;
            if (recursive.Checked)
            {
                files = Recursive_Query_Files(folder, searchPattern).ToArray();
            }
            else
            {
                files = Directory.GetFiles(folder, searchPattern);
            }
            foreach (var file in files)
            {
                fileList.Items.Add(file.Remove(0, folder.Length + 1));
            }
        }

        switch (fileList.Items.Count)
        {
            case 0:
                hitCount.Text = "No files found";
                break;

            case 1:
                hitCount.Text = "1 file found";
                break;

            default:
                hitCount.Text = $"{fileList.Items.Count} files found";
                break;
        }
    }
    
    /// <summary>
    /// Initiates the auto refresh timer when a change in the file extension text box is detected
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Refresh_Timer(object? sender, EventArgs e)
    {
        refreshTimer.Stop();
        refreshTimer.Start();
    }

    /// <summary>
    /// Initiate the auto refresh once the timer hits its intervan (Tick)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RefreshTimer_Tick(object? sender, EventArgs e)
    {
        refreshTimer.Stop();
        if (Validate_Path())
        {
            Query_Files();
        }
    }

    /// <summary>
    /// Browse the file system through a Windows Explorer GUI popup to select the desired folder.
    /// Automatically will fill the folder path text box once a folder is selected.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Browse_Click(object? sender, EventArgs e)
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

    /// <summary>
    /// Deletes all files of specified extension in the specified folder. Can be configured to send to recycle bin or permanently delete based on permaDelete.Checked status.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Delete_Click(object? sender, EventArgs e)
    {
        string folder = folderTextBox.Text;
        string ext = extensionTextBox.Text.TrimStart('.');
        string searchPattern = string.IsNullOrWhiteSpace(ext) ? "*" : $"*.{ext.TrimStart('.')}";
        bool perma = permaDelete.Checked;

        if (Validate_Path())
        {
            string[] files = Directory.GetFiles(folder, searchPattern);

            string deleteType = perma ? "permanently delete" : "recycle";
            var confirmation = MessageBox.Show(
                    $"Are you sure you want to {deleteType} {fileList.Items.Count} files?",
                    "Please Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmation == DialogResult.Yes)
            {
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

                MessageBox.Show(
                    $"Deleted {files.Length} file(s) of type .{ext}",
                    "Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
    }

    /// <summary>
    /// Refreshes the file list by re-querying the specified folder if the extension is changed. Should eventually be replaced with an automated solution.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Refresh_Click(object? sender, EventArgs e)
    {
        if (Validate_Path())
        {
            Query_Files();
        }
    }
}
