using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AssemblyBrowser.Commands;
using AssemblyBrowserLib;
using Microsoft.Win32;

namespace AssemblyBrowser.ViewModels
{
    public class AssemblyViewModel: INotifyPropertyChanged
    {
        private RelayCommand _openAssemblyCommand;

        public ObservableCollection<AssemblyNode> NamespaceNodes { get; set; } = new();

        public RelayCommand OpenAssembly => 
            _openAssemblyCommand ??= new RelayCommand(_ =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "dll|*.dll"
                };
                openFileDialog.ShowDialog();

                if (openFileDialog.FileName != string.Empty)
                {
                    var root = AssemblyParser.Parse(openFileDialog.FileName);

                    NamespaceNodes = new ObservableCollection<AssemblyNode>(root.Children);
                    
                    OnPropertyChanged(nameof(NamespaceNodes));
                }
            });

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}