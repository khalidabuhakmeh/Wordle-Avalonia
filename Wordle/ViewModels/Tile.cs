using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Wordle.ViewModels;

public partial class Tile : ObservableObject
{
    [ObservableProperty, 
     NotifyPropertyChangedFor(nameof(IsExact)),
     NotifyPropertyChangedFor(nameof(HasValue))
    ]
    private char _actual = '_';

    [ObservableProperty] private char? _expected;
    [ObservableProperty] private bool _isPartial;

    public bool IsExact => Actual == Expected;
    public bool HasValue => Actual is not '_';

    public void Set(char? actual, char? expected)
    {
        Actual = actual ?? '_';
        Expected = expected ?? null;
    }

    public void Undo()
    {
        Actual = '_';
    }
}