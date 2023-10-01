using OpenApi.Tools.Core;
using OpenApi.Tools.MarkmapViewer.Services;
using System.Windows.Input;

namespace OpenApi.Tools.MarkmapViewer.ViewModels
{
    public class MarkmapViewerViewModel : ViewModelBase
    {
        private readonly IOpenApiTools _openApiTools;
        private readonly IAlertService _alertService;

        public ICommand LoadItemsCommand { get; set; }
        public ICommand ShowNextItemCommand { get; set; }
        public ICommand ShowPreviousItemCommand { get; set; }

        public MarkmapViewerViewModel(IOpenApiTools openApiTools, IAlertService alertService)
        {
            _openApiTools = openApiTools;
            _alertService = alertService;
            LoadItemsCommand = new Command(LoadItems);
            ShowNextItemCommand = new Command(ShowNextItem);
            ShowPreviousItemCommand = new Command(ShowPreviousItem);
        }

        private string _inputFolder;
        public string InputFolder
        {
            get => _inputFolder;
            set
            {
                _inputFolder = value;
                OnPropertyChanged();
            }
        }

        private string _searchPattern;
        public string SearchPattern
        {
            get => _searchPattern;
            set
            {
                _searchPattern = value;
                OnPropertyChanged();
            }
        }

        private IList<string> _items;
        public IList<string> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        private string _selectedItem;
        public string SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                ShowItem();
            }
        }

        private string _previewUrl;
        public string PreviewUrl
        {
            get => _previewUrl;
            set
            {
                _previewUrl = value;
                OnPropertyChanged();
            }
        }

        private bool _includeFileName;
        public bool IncludeFileName
        {
            get => _includeFileName;
            set
            {
                _includeFileName = value;
                OnPropertyChanged();
            }
        }

        private void ShowItem()
        {
            if (!string.IsNullOrEmpty(SelectedItem))
            {
                PreviewUrl = _currentItems[SelectedItem];
            }
        }

        private string _currentInputFolder;
        private string _currentSearchPattern;
        private IDictionary<string, string> _currentItems;

        private void LoadItems()
        {
            _currentInputFolder = InputFolder;
            _currentSearchPattern = SearchPattern;
            if (!string.IsNullOrEmpty(_currentInputFolder)
                && !string.IsNullOrEmpty(_currentSearchPattern))
            {
                try
                {
                    var files = _openApiTools.FindMarkmapFiles(_currentInputFolder, _currentSearchPattern);
                    if (files.Count > 0)
                    {
                        _currentItems = _openApiTools.ParseItemsFromMarkmapFilePath(_currentInputFolder, files, IncludeFileName);
                        Items = _currentItems.Keys.ToList();
                        if (Items.Any())
                        {
                            SelectedItem = Items[0];
                        }
                    }
                    else
                    {
                        _alertService.ShowAlert("No files found", $"No files that match '{_currentSearchPattern}' found in '{_currentInputFolder}'");
                    }
                }
                catch (DirectoryNotFoundException ex)
                {
                    _alertService.ShowAlert("Wrong directory", ex.Message);
                }
                catch (Exception ex)
                {
                    _alertService.ShowAlert("Failed to load items", ex.Message);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_currentInputFolder))
                {
                    _alertService.ShowAlert("Missing input", "No input folder set.");
                }
                else if(string.IsNullOrEmpty(_currentSearchPattern))
                {
                    _alertService.ShowAlert("Missing input", "No file pattern set.");
                }
            }
        }

        private void ShowNextItem()
        {
            if (!string.IsNullOrEmpty(SelectedItem))
            {
                var index = Items.IndexOf(SelectedItem);
                if (index == Items.Count - 1)
                {
                    SelectedItem = Items[0];
                }
                else
                {
                    SelectedItem = Items[index + 1];
                }
            }
        }

        private void ShowPreviousItem()
        {
            if (!string.IsNullOrEmpty(SelectedItem))
            {
                var index = Items.IndexOf(SelectedItem);
                if (index == 0)
                {
                    SelectedItem = Items[Items.Count - 1];
                }
                else
                {
                    SelectedItem = Items[index - 1];
                }
            }
        }

        protected override Task LoadDataAsync()
        {
            throw new NotImplementedException();
        }
    }
}
