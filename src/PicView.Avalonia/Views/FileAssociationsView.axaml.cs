using Avalonia.Controls;
using PicView.Avalonia.ViewModels;

namespace PicView.Avalonia.Views;

 public partial class FileAssociationsView : UserControl
    {
        private readonly List<(CheckBox CheckBox, string SearchText)> _allCheckBoxes = [];
        
        public FileAssociationsView()
        {
            InitializeComponent();
            
            FilterBox.TextChanged += FilterBox_TextChanged;
                
            // Clear button functionality
            var clearButton = ClearButton;
            if (clearButton != null)
            {
                clearButton.Click += (s, e) => 
                { 
                    FilterBox.Text = string.Empty;
                    FilterCheckBoxes(string.Empty);
                };
            }
            
            // Initialize the collection of checkboxes for filtering
            InitializeCheckBoxesCollection();
        }
        
        private void InitializeCheckBoxesCollection()
        {
            var container = FileTypesContainer;

            if (DataContext is not MainViewModel vm)
            {
                return;
            }
            
            foreach (var fileTypeGroup in vm.AssociationsViewModel.FileTypeGroups)
            {
            }
        }
        
        private void FilterBox_TextChanged(object? sender, EventArgs e)
        {
            FilterCheckBoxes(FilterBox.Text);
        }
        
        private void FilterCheckBoxes(string filterText)
        {
        }
    }
