using StarWarsLanza.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using StarWarsLanza.ViewModels;
using System.Diagnostics;
using HttpClient = System.Net.Http.HttpClient;

namespace StarWarsLanza.Views
{
    internal sealed partial class CharacterRequest : Page
    {
        private readonly HttpClient _httpClient = new();

        // url di swapi.dev
        private readonly string _urlPeople = "https://swapi.dev/api/people/";
        private readonly string _urlPlanets = "https://swapi.dev/api/planets/";
        private readonly string _urlVehicles = "https://swapi.dev/api/vehicles/";
        private readonly string _urlStarships = "https://swapi.dev/api/starships/";

        public PersonViewModel PersonViewModel { get; } = new();

        // costruttore della pagina
        public CharacterRequest()
        {
            this.InitializeComponent();      // inizializza la pagina
            DataContext = PersonViewModel;   // collega il viewmodel alla pagina
            this.Loaded += HomePage_Loaded;  // evento quando la pagina è caricata
        }

        // richiedi i dati delle persone e pianeti
        private async void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            await FetchPlanets();
            await FetchCharacters();
        }

        // funzione per richiedere tutti i personaggi
        private async Task FetchCharacters()
        {
            for (int i = 1; i < 84; i++) // itera sugli id delle persone
            {
                try
                {
                    // scarica una persona usando il suo id
                    var person = await _httpClient.GetFromJsonAsync<Character>(_urlPeople + i);
                    if (person != null) // se la persona esiste
                    {
                        // collega il pianeta della persona se ha un homeworld
                        if (!string.IsNullOrEmpty(person.Homeworld))
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(person.Homeworld, @"(\d+)/$");
                            if (match.Success)
                            {
                                int planetId = int.Parse(match.Groups[1].Value);
                                var planet = PersonViewModel.PlanetsList.FirstOrDefault(p => p.Id == planetId);
                                if (planet != null)
                                {
                                    person.Planet = planet;
                                }
                            }
                        }
                        // aggiunge la persona alla lista
                        PersonViewModel.PeopleList.Add(person);
                    }
                }
                catch (Exception ex)
                {
                    // scrive l'errore nel debug se qualcosa va storto (dovrei gestire gli errori)
                    Debug.WriteLine($"errore nel recupero dei dati per la persona con id {i}: {ex.Message}");
                }
            }
        }

        // funzione per scaricare la lista dei pianeti
        private async Task FetchPlanets()
        {
            for (int i = 1; i < 61; i++) // itera sugli id dei pianeti
            {
                try
                {
                    // richiedi un pianeta usando il suo id
                    var planet = await _httpClient.GetFromJsonAsync<Planet>(_urlPlanets + i);
                    if (planet != null) // se il pianeta esiste
                    {
                        planet.Id = i; // assegna l'id
                        PersonViewModel.PlanetsList.Add(planet); // aggiunge il pianeta alla lista
                    }
                }
                catch (Exception ex)
                {
                    // scrive l'errore nel debug se qualcosa va storto (anche qui dovrei gestire gli errori)
                    Debug.WriteLine($"errore nel recupero dei dati per il pianeta con id {i}: {ex.Message}");
                }
            }
        }

        // metodo per scaricare la lista dei veicoli (non commento perche' e' la stessa logica delle funzioni di fetch precedenti)
        private async Task FetchVehicles()
        {
            for (int i = 1; i < 74; i++)
            {
                try
                {
                    var vehicle = await _httpClient.GetFromJsonAsync<Vehicle>(_urlVehicles + i);
                    if (vehicle != null)
                    {
                        vehicle.Id = i;
                        PersonViewModel.VehiclesList.Add(vehicle);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"errore nel recupero dei dati per il veicolo con id {i}: {ex.Message}");
                }
            }
        }

        private async Task FetchStarships()
        {
            for (int i = 1; i < 44; i++)
            {
                try
                {
                    var starship = await _httpClient.GetFromJsonAsync<Starship>(_urlStarships + i);
                    if (starship != null)
                    {
                        starship.Id = i;
                        PersonViewModel.StarshipList.Add(starship);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"errore nel recupero dei dati per la nave stellare con id {i}: {ex.Message}");
                }
            }
        }

        // metodo per mostrare le informazioni di un personaggio quando lo clicchi
        private async void InfoPerson(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Character selectedPerson) // controlla che l'oggetto cliccato sia una persona
            {
                // crea un ContentDialog con le informazioni della persona e del pianeta
                var dialog = new ContentDialog
                {
                    Title = "R2-D2",
                    Content = new TextBlock
                    {
                        Text = "Character Details:\n" +
                        $"Height: {selectedPerson.Height ?? "unknown"}\n" +
                        $"Mass: {selectedPerson.Mass ?? "unknown"}\n" +
                        $"Hair Color: {selectedPerson.HairColor ?? "n/a"}\n" +
                        $"Skin Color: {selectedPerson.SkinColor ?? "white, blue"}\n" +
                        $"Eye Color: {selectedPerson.EyeColor ?? "red"}\n" +
                        $"Birth Year: {selectedPerson.BirthYear ?? "33BBY"}\n" +
                        $"Gender: {selectedPerson.Gender ?? "n/a"}\n" +
                        "\nPlanet Details:\n" +
                        $"Name: {selectedPerson.Planet?.Name ?? "Naboo"}\n" +
                        $"Rotation Period: {selectedPerson.Planet?.RotationPeriod ?? "26"}\n" +
                        $"Orbital Period: {selectedPerson.Planet?.OrbitalPeriod ?? "312"}\n" +
                        $"Diameter: {selectedPerson.Planet?.Diameter ?? "12120"}\n" +
                        $"Climate: {selectedPerson.Planet?.Climate ?? "temperate"}\n" +
                        $"Gravity: {selectedPerson.Planet?.Gravity ?? "1 standard"}\n" +
                        $"Terrain: {selectedPerson.Planet?.Terrain ?? "grassy hills, swamps, forests, mountains"}\n" +
                        $"Surface Water: {selectedPerson.Planet?.SurfaceWater ?? "12"}\n" +
                        $"Population: {selectedPerson.Planet?.Population ?? "4500000000"}\n",
                        TextWrapping = TextWrapping.Wrap
                    },
                    CloseButtonText = "close"
                };

                await dialog.ShowAsync(); // mostra il dialogo
            }
        }

        // metodo per salvare i dati e cambiare pagina
        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ExportPage)); // naviga verso la pagina di esportazione
        }
    }
}
