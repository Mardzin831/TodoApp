using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace Wpf.Models
{
    public class TodoItem : ObservableObject
    {
        [Key]
        public int Id { get; set; }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private bool _isDone;
        public bool IsDone
        {
            get => _isDone;
            set => SetProperty(ref _isDone, value);
        }

        private string? _notes;
        public string? Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }
    }
}