using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Avalonia;
using Avalonia.Input;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wordle.Models;

namespace Wordle.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty,
     NotifyPropertyChangedFor(nameof(IsGameOver)),
     NotifyPropertyChangedFor(nameof(HasWon)),
     NotifyPropertyChangedFor(nameof(HasLost)),
     NotifyPropertyChangedFor(nameof(GameOverText))]
    private GameState _gameState = GameState.Playing;

    [ObservableProperty] private string _currentWord = "";
    public bool IsGameOver => GameState != GameState.Playing;

    public string GameOverText => HasWon
        ? "🥳 You guessed the word correctly!"
        : "😭 You did not guess correctly. Try again.";

    public bool HasWon => GameState is GameState.Win;
    public bool HasLost => GameState is GameState.Lost;

    private static readonly List<string> Words;

    static MainWindowViewModel()
    {
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        var reader = new StreamReader(assets!.Open(new Uri("avares://Wordle/Assets/valid-words.csv")));
        Words = new List<string>();
        while (reader.ReadLine() is { } line) {
            Words.Add(line.Trim().ToUpperInvariant());
        }
    }

    public MainWindowViewModel()
    {
        Reset();
    }

    public List<Guess> Guesses { get; set; }
        = new() { new(), new(), new(), new(), new() };

    private Guess? CurrentRound => Guesses.FirstOrDefault(g => !g.HasGuessed);

    [RelayCommand]
    private void Reset()
    {
        // get a random word
        CurrentWord = Words[RandomNumberGenerator.GetInt32(0, Words.Count)].ToUpperInvariant();

        // Reset all the guesses
        Guesses.ForEach(g => { g.Reset(CurrentWord); });

        GameState = GameState.Playing;
    }

    [RelayCommand]
    private void GuessLetter(KeyEventArgs? args)
    {
        args ??= new KeyEventArgs { Key = Key.Enter };
        args.Handled = true;

        // tell the user to reset the game
        if (GameState is GameState.Win or GameState.Lost)
        {
            return;
        }

        if (CurrentRound is null) return;
        
        if (args.Key is Key.Back or Key.Delete)
        {
            CurrentRound.Undo();
        }

        if (char.TryParse(args.Key.ToString(), out var letter))
        {
            CurrentRound.AddLetter(letter);
        }
            
        if (args.Key == Key.Enter && CurrentRound.GuessWord())
        {
            GameState = GameState.Win;
        }

        // check if we're out of guesses
        var nextRound = CurrentRound;
        if (nextRound is null)
        {
            GameState = GameState.Lost;    
        }
    }

    [RelayCommand]
    private void QuietTheBeep(KeyEventArgs args)
    {
        args.Handled = true;
        // a command to quiet the beep
    }
}