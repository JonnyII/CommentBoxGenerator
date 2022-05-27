using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CommentBoxGenerator;

internal class BoxSettingsVm : ReactiveObject
{
    [Reactive] public char UpperLeft { get; set; } = '┏';
    [Reactive] public char UpperRight { get; set; } = '┓';
    [Reactive] public char LowerLeft { get; set; } = '┗';
    [Reactive] public char LowerRight { get; set; } = '┛';
    [Reactive] public char Left { get; set; } = '┃';
    [Reactive] public char Right { get; set; } = '┃';
    [Reactive] public char Top { get; set; } = '━';
    [Reactive] public char Bottom { get; set; } = '━';

    public IObservable<BoxSettings> SettingsChanges
        => this.WhenAnyValue(
            vm => vm.UpperLeft,
            vm => vm.UpperRight,
            vm => vm.LowerLeft,
            vm => vm.LowerRight,
            vm => vm.Left,
            vm => vm.Right,
            vm => vm.Top,
            vm => vm.Bottom,
            (
                    upperLeft,
                    upperRight,
                    lowerLeft,
                    lowerRight,
                    left,
                    right,
                    top,
                    bottom
                ) =>
                new BoxSettings(upperLeft,
                    upperRight,
                    lowerLeft,
                    lowerRight,
                    left,
                    right,
                    top,
                    bottom)
        );
}

public record BoxSettings(
    char TopLeft,
    char TopRight,
    char BottomLeft,
    char BottomRight,
    char Left,
    char Right,
    char Top,
    char Bottom);

internal class CommentBoxGeneratorVm : ReactiveObject
{
    [Reactive] public ushort MinWidth { get; set; } = 100;
    [Reactive] public ushort Padding { get; set; } = 2;
    [Reactive] public string Content { get; set; } = "Content";
    [Reactive] public bool IsComment { get; set; } = true;
    [Reactive] public bool IsCentered { get; set; } = true;
    [Reactive] public bool AutoCopy { get; set; } = true;
    [Reactive] public string Result { get; private set; } = string.Empty;
    [Reactive] public BoxSettingsVm BoxSettingsVm { get; set; } = new();
    public ICommand CopyToClipboardCommand { get; }
    public CommentBoxGeneratorVm()
    {
        CopyToClipboardCommand = ReactiveCommand.Create(() => CopyToClipboard());

        this.WhenAnyValue(
                vm => vm.MinWidth,
                vm => vm.Padding,
                vm => vm.Content,
                vm => vm.IsComment,
                vm => vm.BoxSettingsVm,
            (minWidth, padding, content, isComment, boxSettingsVm) =>
                boxSettingsVm.SettingsChanges.Select(boxSettings =>
                {

                    var lines = content.Split(Environment.NewLine);

                    var actualWidth = lines.Max(x => x.Length) + padding * 2;
                    if (actualWidth < minWidth)
                        actualWidth = minWidth;
                    var prefix = "/*" + Environment.NewLine;
                    var suffix = Environment.NewLine + "*/";
                    if (!isComment)
                        prefix = suffix = string.Empty;

                    return prefix + string.Join(Environment.NewLine,
                        boxSettings.TopLeft + boxSettings.Top.Repeat(actualWidth - 2) + boxSettings.TopRight,
                        boxSettings.Left + ' '.Repeat(actualWidth - 2) + boxSettings.Right,
                        string.Join(Environment.NewLine,
                            lines.Select(line =>
                                IsCentered
                                    ? boxSettings.Left + ' '.Repeat(Padding) + ' '.Repeat((actualWidth - padding - 2 - line.Length) / 2) + line + ' '.Repeat((int)Math.Ceiling((decimal)(actualWidth - padding - 2 - line.Length) / 2)) + boxSettings.Right
                                    : boxSettings.Left + ' '.Repeat(Padding) + line + ' '.Repeat(actualWidth - padding - 2 - line.Length) + boxSettings.Right
                                )
                        ),
                        boxSettings.Left + ' '.Repeat(actualWidth - 2) + boxSettings.Right,
                        boxSettings.BottomLeft + boxSettings.Bottom.Repeat(actualWidth - 2) + boxSettings.BottomRight
                    ) + suffix;
                }))
            .Switch()
            .Do(result =>
            {
                if (AutoCopy)
                    CopyToClipboard(result);
            })
            .BindTo(this, vm => vm.Result);
    }

    private void CopyToClipboard(string? result = null)
        => Clipboard.SetText(result ?? this.Result);
}

static class Helper
{
    public static string Repeat(this char character, int count)
        => new(character, count);
}