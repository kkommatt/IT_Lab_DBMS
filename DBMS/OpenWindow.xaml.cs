using System;
using System.IO;
using System.Windows;
using DBMS.database;
using Microsoft.Win32;
using System.Text.Json;

namespace DBMS;

public partial class OpenWindow : Window
{
    public string SelectedFilePath { get; private set; }
    public string SelectedFolderPath { get; private set; }

    public OpenWindow()
    {
        InitializeComponent();
    }

    private void btnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Select a JSON Database File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            TxtFilePath.Text = openFileDialog.FileName;
            SelectedFilePath = openFileDialog.FileName;
            BtnOpen.IsEnabled = true; // Enable the Open button
        }
    }

    private void btnOpen_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedFilePath) || !File.Exists(SelectedFilePath))
            {
                MessageBox.Show("Please select a valid JSON file.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var jsonContent = File.ReadAllText(SelectedFilePath);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                MessageBox.Show("The selected file is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var db = JsonSerializer.Deserialize<Database>(jsonContent);
            var mainWindow = new MainWindow(SelectedFilePath);
            mainWindow.Show();

            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnCreate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string dbName = TxtDbName.Text.Trim();
            if (string.IsNullOrWhiteSpace(dbName))
            {
                MessageBox.Show("Please enter a valid database name.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            string folderPath = Path.GetDirectoryName(SelectedFolderPath);
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                MessageBox.Show("Please select a valid folder path first.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            string newDbPath = Path.Combine(folderPath, $"{dbName}.json");

            if (File.Exists(newDbPath))
            {
                MessageBox.Show("A database with this name already exists. Please choose a different name.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            File.WriteAllText(newDbPath, JsonSerializer.Serialize(new Database()));
            MessageBox.Show("New database created successfully.", "Success", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select any file in selected folder",
        };

        if (openFileDialog.ShowDialog() == true)
        {
            string folderPath = Path.GetDirectoryName(openFileDialog.FileName);
            if (Directory.Exists(folderPath))
            {
                SelectedFolderPath = openFileDialog.FileName;
                TxtFolderPath.Text = folderPath;
                BtnCreate.IsEnabled = !string.IsNullOrWhiteSpace(TxtDbName.Text);
            }
        }
    }
}