﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarWarsLanza.Models
{
    public class Character : INotifyPropertyChanged
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("height")]
        public string Height { get; set; }

        [JsonPropertyName("mass")]
        public string Mass { get; set; }

        [JsonPropertyName("hair_color")]
        public string HairColor { get; set; }

        [JsonPropertyName("skin_color")]
        public string SkinColor { get; set; }

        [JsonPropertyName("eye_color")]
        public string EyeColor { get; set; }

        [JsonPropertyName("birth_year")]
        public string BirthYear { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("homeworld")]
        public string Homeworld { get; set; }

        [JsonPropertyName("films")]
        public List<string> Films { get; set; } = new();

        [JsonPropertyName("species")]
        public List<string> Species { get; set; } = new();

        [JsonPropertyName("vehicles")]
        public List<string> Vehicles { get; set; } = new();

        [JsonPropertyName("starships")]
        public List<string> Starships { get; set; } = new();

        [JsonPropertyName("created")]
        public string Created { get; set; }

        [JsonPropertyName("edited")]
        public string Edited { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        public Planet Planet { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
