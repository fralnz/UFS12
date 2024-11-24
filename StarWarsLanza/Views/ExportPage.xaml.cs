using StarWarsLanza.Models;
using StarWarsLanza.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace StarWarsLanza.Views
{
    public sealed partial class ExportPage : Page
    {
        private string _selectedFormat;
        private const string JsonExtension = ".json";
        private const string XmlExtension = ".xml";
        private PersonViewModel _viewModel;

        public ExportPage()
        {
            this.InitializeComponent();
            _viewModel = new PersonViewModel();
            DataContext = _viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Verify if we have data to export
            if (PersonViewModel.PeopleListSelected == null || PersonViewModel.PeopleListSelected.Count == 0)
            {
                ShowErrorMessage("No data available to export. Please select characters first.");
                SaveFileButton.IsEnabled = false;
                SaveFilePickerButton.IsEnabled = false;
            }
        }

        private async void ShowErrorMessage(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Export Error",
                Content = message,
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
        }

        private async void ShowSuccessMessage(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.IsChecked == true)
            {
                _selectedFormat = radioButton.Content.ToString().ToUpper();
                UpdateSaveButtonState();
            }
        }

        private void checkName(object sender, TextChangedEventArgs e)
        {
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            bool isEnabled = !string.IsNullOrWhiteSpace(fileName.Text) &&
                           !string.IsNullOrEmpty(_selectedFormat) &&
                           PersonViewModel.PeopleListSelected != null &&
                           PersonViewModel.PeopleListSelected.Count > 0;

            SaveFileButton.IsEnabled = isEnabled;
            SaveFilePickerButton.IsEnabled = isEnabled;
        }

        private async void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileButton.IsEnabled = false;
                loadingRing.IsActive = true;

                if (PersonViewModel.PeopleListSelected == null || PersonViewModel.PeopleListSelected.Count == 0)
                {
                    throw new InvalidOperationException("No characters selected for export");
                }

                switch (_selectedFormat)
                {
                    case "JSON":
                        await SaveToLocalStorageAsync(JsonExtension);
                        break;
                    case "XML":
                        await SaveToLocalStorageAsync(XmlExtension);
                        break;
                    default:
                        throw new InvalidOperationException("Please select a file format (JSON or XML)");
                }

                ShowSuccessMessage($"File saved successfully in {_selectedFormat} format");
                Debug.WriteLine($"File {_selectedFormat} saved successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving file: {ex.Message}");
                ShowErrorMessage($"Error saving file: {ex.Message}");
            }
            finally
            {
                SaveFileButton.IsEnabled = true;
                loadingRing.IsActive = false;
            }
        }

        private async void Button_Click_SaveWithPicker(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFilePickerButton.IsEnabled = false;
                loadingRing.IsActive = true;

                if (PersonViewModel.PeopleListSelected == null || PersonViewModel.PeopleListSelected.Count == 0)
                {
                    throw new InvalidOperationException("No characters selected for export");
                }

                var file = await PickSaveFileAsync();
                if (file == null)
                {
                    Debug.WriteLine("File picker was cancelled");
                    return;
                }

                if (file.FileType.Equals(JsonExtension, StringComparison.OrdinalIgnoreCase))
                {
                    await SaveAsJsonAsync(file);
                }
                else if (file.FileType.Equals(XmlExtension, StringComparison.OrdinalIgnoreCase))
                {
                    await SaveAsXmlAsync(file);
                }
                else
                {
                    throw new InvalidOperationException("Invalid file format selected");
                }

                ShowSuccessMessage($"File saved successfully at {file.Path}");
                Debug.WriteLine($"File saved successfully at {file.Path}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving file: {ex.Message}");
                ShowErrorMessage($"Error saving file: {ex.Message}");
            }
            finally
            {
                SaveFilePickerButton.IsEnabled = true;
                loadingRing.IsActive = false;
            }
        }

        private async Task<StorageFile> PickSaveFileAsync()
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = fileName.Text
            };

            savePicker.FileTypeChoices.Add("JSON File", new[] { JsonExtension });
            savePicker.FileTypeChoices.Add("XML File", new[] { XmlExtension });

            return await savePicker.PickSaveFileAsync();
        }

        private async Task SaveToLocalStorageAsync(string fileExtension)
        {
            if (string.IsNullOrWhiteSpace(fileName.Text))
            {
                throw new InvalidOperationException("Please enter a file name");
            }

            string fileNameText = fileName.Text;
            string filePath = $"{fileNameText}{fileExtension}";

            var storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                filePath, CreationCollisionOption.ReplaceExisting);

            if (fileExtension.Equals(JsonExtension, StringComparison.OrdinalIgnoreCase))
            {
                await SaveAsJsonAsync(storageFile);
            }
            else if (fileExtension.Equals(XmlExtension, StringComparison.OrdinalIgnoreCase))
            {
                await SaveAsXmlAsync(storageFile);
            }
            else
            {
                throw new InvalidOperationException("Invalid file extension");
            }
        }

        private async Task SaveAsJsonAsync(StorageFile file)
        {
            ObservableCollection<Character> peopleList = new ObservableCollection<Character>();

            try
            {
                // Try to read existing file if it exists
                try
                {
                    var existingJson = await FileIO.ReadTextAsync(file);
                    if (!string.IsNullOrEmpty(existingJson))
                    {
                        var existingList = JsonSerializer.Deserialize<ObservableCollection<Character>>(existingJson);
                        if (existingList != null)
                        {
                            foreach (var person in existingList)
                            {
                                peopleList.Add(person);
                            }
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    // New file will be created
                }

                // Add new selections
                foreach (var person in PersonViewModel.PeopleListSelected)
                {
                    if (person != null)
                    {
                        peopleList.Add(person);
                    }
                }

                if (peopleList.Count == 0)
                {
                    throw new InvalidOperationException("No valid data to save");
                }

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IgnoreNullValues = true
                };

                var updatedJson = JsonSerializer.Serialize(peopleList, jsonOptions);
                await FileIO.WriteTextAsync(file, updatedJson);

                Debug.WriteLine($"JSON file saved at: {file.Path} with {peopleList.Count} items");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SaveAsJsonAsync: {ex.Message}");
                throw;
            }
        }

        private async Task SaveAsXmlAsync(StorageFile file)
        {
            var xmlSerializer = new XmlSerializer(typeof(ObservableCollection<Character>));
            ObservableCollection<Character> peopleList = new ObservableCollection<Character>();

            try
            {
                // Try to read existing file if it exists
                try
                {
                    var existingXml = await FileIO.ReadTextAsync(file);
                    if (!string.IsNullOrEmpty(existingXml))
                    {
                        using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(existingXml)))
                        {
                            var existingList = (ObservableCollection<Character>)xmlSerializer.Deserialize(memoryStream);
                            if (existingList != null)
                            {
                                foreach (var person in existingList)
                                {
                                    peopleList.Add(person);
                                }
                            }
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    // New file will be created
                }

                // Add new selections
                foreach (var person in PersonViewModel.PeopleListSelected)
                {
                    if (person != null)
                    {
                        peopleList.Add(person);
                    }
                }

                if (peopleList.Count == 0)
                {
                    throw new InvalidOperationException("No valid data to save");
                }

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    xmlSerializer.Serialize(stream, peopleList);
                }

                Debug.WriteLine($"XML file saved at: {file.Path} with {peopleList.Count} items");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SaveAsXmlAsync: {ex.Message}");
                throw;
            }
        }

        private void Button_Click_Back(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(CharacterRequest));
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clean up
            PersonViewModel.PeopleListSelected?.Clear();
        }
    }
}