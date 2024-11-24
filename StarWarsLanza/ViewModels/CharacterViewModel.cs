using StarWarsLanza.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarWarsLanza.ViewModels
{
    internal class CharacterViewModel : INotifyPropertyChanged
    {
        // Inizializzo delle ObservableCollection dei modelli (definiti nella cartella Models)
        public ObservableCollection<Planet> PlanetsList { get; set; } = new();
        public ObservableCollection<Character> PeopleList { get; set; } = new();
        public ObservableCollection<Vehicle> VehiclesList { get; set; } = new();
        public ObservableCollection<Starship> StarshipList { get; set; } = new();
        public static ObservableCollection<Character> PeopleListSelected { get; set; } = new();    // solo questa e' static perche' la devo richiamare in futuro

        // inizializzo le liste filtrate (necessarie per le ricerche)
        private ObservableCollection<Character> _filteredPeopleList = new();
        private ObservableCollection<Character> _currentPeopleList;

        // questo dizionario serve per associare la persona alla selezione
        private readonly Dictionary<Character, bool> _selectionStates = new();

        private string _searchText;
        private bool _isSaveButtonEnabled;

        public CharacterViewModel()
        {
            // inizializza la lista di personaggi
            CurrentPeopleList = PeopleList;

            // chiama la funzione per il cambiamento della lista di persona
            PeopleList.CollectionChanged += PeopleList_Change;
        }

        // parte della barra di ricerca
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    FilterPeopleList(); // chiama la funzione per filtrare il risultato della ricerca
                }
            }
        }

        // lista visualizzata al momento
        public ObservableCollection<Character> CurrentPeopleList
        {
            get => _currentPeopleList;
            set
            {
                _currentPeopleList = value;
                OnPropertyChanged(nameof(CurrentPeopleList));
            }
        }

        // lista filtrata, non viene visualizzata
        public ObservableCollection<Character> FilteredPeopleList
        {
            get => _filteredPeopleList;
            set
            {
                _filteredPeopleList = value;
                OnPropertyChanged(nameof(FilteredPeopleList));
            }
        }

        // funzione per filtrare i risultati di ricerca
        private void FilterPeopleList()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                // se vuoto, mostra tutti
                CurrentPeopleList = PeopleList;
            }
            else
            {
                // Filtra le persone per nome o per nome del pianeta
                var filtered = PeopleList
                    .Where(p => p.Name.StartsWith(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                (p.Planet?.Name != null && p.Planet.Name.StartsWith(SearchText, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // aggiorna la lista visualizzata
                CurrentPeopleList = new ObservableCollection<Character>(filtered);
            }
        }

        // funzione per i cambiamenti nella collezione di persone (handler)
        private void PeopleList_Change(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Character person in e.NewItems)
                {
                    person.PropertyChanged += Person_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                // togli le persone rimosse
                foreach (Character person in e.OldItems)
                {
                    person.PropertyChanged -= Person_PropertyChanged;
                }
            }
        }

        // funzione per la modifica delle proprieta
        private void Person_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Character.IsSelected))
            {
                if (sender is Character person)
                {
                    if (person.IsSelected && !PeopleListSelected.Contains(person))
                    {
                        // aggiungi la persona alla lista selezionata quando viene selezionata
                        PeopleListSelected.Add(person);
                    }
                    else if (!person.IsSelected && PeopleListSelected.Contains(person))
                    {
                        // rimuovi la persona dalla lista selezionata quando viene deselezionata
                        PeopleListSelected.Remove(person);
                    }

                    // aggiorna lo stato del bottone di salvataggio
                    IsSaveButtonEnabled = PeopleListSelected.Any();
                }
            }
        }

        // gestione del pulsante di salvataggio
        public bool IsSaveButtonEnabled
        {
            get => _isSaveButtonEnabled;
            set
            {
                if (_isSaveButtonEnabled != value)
                {
                    _isSaveButtonEnabled = value;
                    OnPropertyChanged(nameof(IsSaveButtonEnabled));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
