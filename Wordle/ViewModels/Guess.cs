using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Wordle.ViewModels;

public partial class Guess : ObservableObject
{
    [ObservableProperty] private bool _hasGuessed;

    private bool IsFull => Tiles.TrueForAll(t => t.HasValue);
    private string Expected { get; set; } = "";

    public List<Tile> Tiles { get; set; }
        = new() { new(), new(), new(), new(), new() };

    public void Reset(string expected)
    {
        HasGuessed = false;
        Expected = expected;

        for (var i = 0; i < expected.Length; i++)
        {
            var tile = Tiles[i];
            tile.Set(actual: null, expected[i]);
        }
    }

    public void AddLetter(char character)
    {
        if (IsFull) return;

        var currentTile = Tiles.FirstOrDefault(t => !t.HasValue);
        if (currentTile is not null)
        {
            currentTile.Actual = char.ToUpperInvariant(character);
        }
    }

    public void Undo()
    {
        var currentTile = Tiles.LastOrDefault(t => t.HasValue);
        currentTile?.Undo();
    }

    public bool GuessWord()
    {
        // did not guess
        if (!IsFull) return false;

        var remainingCharacters = Expected
            .Select(char.ToUpperInvariant)
            .ToList();

        // get all the exact matches first
        var exactMatches = Tiles.Where(t => t.IsExact).ToList();
        foreach (var match in exactMatches)
        {
            remainingCharacters.Remove(match.Actual);
        }

        // we pass in a mutable list
        foreach (var tile in Tiles.Except(exactMatches))
        {
            var exists = remainingCharacters.IndexOf(tile.Actual);
            if (exists < 0) continue;
            
            // remove the character from the list
            tile.IsPartial = true;
            remainingCharacters.Remove(tile.Actual);
        }

        // build guess into string
        HasGuessed = true;
        return Tiles.TrueForAll(x => x.IsExact);
    }
}