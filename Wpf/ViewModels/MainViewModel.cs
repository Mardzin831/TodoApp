using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Wpf.Data;
using Wpf.Models;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Wpf.ViewModels
{
    // Widok-model aplikacji
    public class MainViewModel : ObservableObject
    {
        private readonly AppDbContext _db = new();

        public enum SortField { None, IsDone, Title }

        private SortField _currentSort = SortField.None;
        public SortField CurrentSort
        {
            get => _currentSort;
            set
            {
                if (SetProperty(ref _currentSort, value))
                {
                    OnPropertyChanged(nameof(IsDoneSortLabel));
                    OnPropertyChanged(nameof(TitleSortLabel));
                }
            }
        }

        private bool _sortDescending;
        public bool SortDescending
        {
            get => _sortDescending;
            set
            {
                if (SetProperty(ref _sortDescending, value))
                {
                    OnPropertyChanged(nameof(IsDoneSortLabel));
                    OnPropertyChanged(nameof(TitleSortLabel));
                }
            }
        }

        // Sta³e etykiety nag³ówków
        public string IsDoneSortLabel => "Status";
        public string TitleSortLabel => "Opis zadania";

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    LoadItems(); // prze³aduj elementy dla nowej daty
                }
            }
        }

        private TodoItem? _selectedItem;
        public TodoItem? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ObservableCollection<TodoItem> Items { get; } = new();

        // Komendy u¿ywane w widoku
        public ICommand ChangeDateCommand { get; }
        public ICommand AddItemCommand { get; private set; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ToggleDoneCommand { get; }
        public ICommand EditItemCommand { get; }
        public ICommand SetSortFieldCommand { get; }

        // Zdarzenie: ustaw fokus na nowo dodanym elemencie
        public event Action<TodoItem>? RequestFocusOnItem;

        public MainViewModel()
        {
            // Upewnij siê, ¿e baza danych jest utworzona
            _db.Database.EnsureCreated();

            ChangeDateCommand = new RelayCommand<object?>(param =>
            {
                if (param is DateTime dt) SelectedDate = dt;
            });

            // Dodaj nowy element
            AddItemCommand = new RelayCommand(() =>
            {
                var existingEmpty = Items.FirstOrDefault(i => string.IsNullOrWhiteSpace(i.Title));
                if (existingEmpty != null)
                {
                    RequestFocusOnItem?.Invoke(existingEmpty);
                    return;
                }

                var item = new TodoItem { Date = SelectedDate, Title = string.Empty };
                _db.TodoItems.Add(item);
                _db.SaveChanges();
                Attach(item);
                Items.Add(item);
                RequestFocusOnItem?.Invoke(item);
            });

            // Usuñ element
            DeleteItemCommand = new RelayCommand<TodoItem?>(item =>
            {
                if (item == null) return;
                Detach(item);
                _db.TodoItems.Remove(item);
                _db.SaveChanges();
                Items.Remove(item);
            });

            ToggleDoneCommand = new RelayCommand<TodoItem?>(item =>
            {
                if (item == null) return;
                _db.TodoItems.Update(item);
                _db.SaveChanges();
            });

            EditItemCommand = new RelayCommand<TodoItem?>(item =>
            {
                if (item == null) return;
                _db.TodoItems.Update(item);
                _db.SaveChanges();
            });

            // Ustawianie sortowania
            SetSortFieldCommand = new RelayCommand<string>(param =>
            {
                if (string.IsNullOrEmpty(param)) return;
                if (Enum.TryParse<SortField>(param, out var sf))
                {
                    if (CurrentSort == sf)
                    {
                        SortDescending = !SortDescending; // prze³¹cz kierunek
                    }
                    else
                    {
                        CurrentSort = sf;
                        SortDescending = false;
                    }
                    LoadItems();
                }
            });

            Items.CollectionChanged += Items_CollectionChanged;
            LoadItems();
        }

        // Obs³uga dodawania/usuñæ przy zmianie kolekcji
        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e?.NewItems != null)
            {
                foreach (TodoItem? ni in e.NewItems)
                {
                    if (ni != null) Attach(ni);
                }
            }
            if (e?.OldItems != null)
            {
                foreach (TodoItem? oi in e.OldItems)
                {
                    if (oi != null) Detach(oi);
                }
            }
        }

        // Za³aduj elementy z bazy dla wybranej daty
        private void LoadItems()
        {
            foreach (var existing in Items.ToList()) Detach(existing);
            Items.Clear();
            var start = SelectedDate.Date;
            var end = start.AddDays(1);
            var query = _db.TodoItems.Where(t => t.Date >= start && t.Date < end);

            switch (CurrentSort)
            {
                case SortField.IsDone:
                    query = SortDescending ? query.OrderByDescending(t => t.IsDone).ThenBy(t => t.Id) : query.OrderBy(t => t.IsDone).ThenBy(t => t.Id);
                    break;
                case SortField.Title:
                    query = SortDescending ? query.OrderByDescending(t => t.Title).ThenBy(t => t.Id) : query.OrderBy(t => t.Title).ThenBy(t => t.Id);
                    break;
                default:
                    query = query.OrderBy(t => t.Id);
                    break;
            }

            var items = query.ToList();
            foreach (var it in items)
            {
                Attach(it);
                Items.Add(it);
            }
        }

        // Subskrybuj zmiany pojedynczego elementu
        private void Attach(TodoItem item)
        {
            if (item == null) return;
            item.PropertyChanged -= Item_PropertyChanged;
            item.PropertyChanged += Item_PropertyChanged;
        }

        // Odepnij subskrypcjê
        private void Detach(TodoItem item)
        {
            if (item == null) return;
            item.PropertyChanged -= Item_PropertyChanged;
        }

        // Zapis zmian w³aœciwoœci do bazy
        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TodoItem item)
            {
                if (e.PropertyName == nameof(TodoItem.IsDone) || e.PropertyName == nameof(TodoItem.Title) || e.PropertyName == nameof(TodoItem.Notes))
                {
                    try
                    {
                        _db.TodoItems.Update(item);
                        _db.SaveChanges();
                    }
                    catch
                    {
                        // ignoruj b³êdy zapisu
                    }
                }
            }
        }
    }
}
