using System;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Unicode;
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

public enum LineStyle : byte
{
    Light = 00,
    Double = 50,
    TripleDashed = 4,
    QuadDashed = 8,
}

public enum LineThickness : byte
{
    Light = 0b0,
    Heavy = 0b1
}

public enum LineSection : byte
{
    Horizontal = 0b_0000_0000,
    Vertical = 0b___0000_0010,

    UpperLeft = 0b_0000_1100,
    UpperRight = 0b__0001_0000,
    LowerLeft = 0b_0001_0100,
    LowerRight = 0b__0001_1000,
}

public class Box
{
    public LineThickness Left { get; set; }
    public LineThickness Right { get; set; }
    public LineThickness Top { get; set; }
    public LineThickness Bottom { get; set; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }

    public string UpperRight
        => ToUnicode((byte)LineSection.UpperRight | ((byte)Top) | (byte)Right << 1);
    public string UpperLeft
        => ToUnicode((byte)LineSection.UpperLeft | ((byte)Top) | (byte)Left << 1);
    public string LowerRight
        => ToUnicode((byte)LineSection.LowerRight | ((byte)Bottom) | (byte)Right << 1);
    public string LowerLeft
        => ToUnicode((byte)LineSection.LowerLeft | ((byte)Bottom) | (byte)Left << 1);

    public string TopLine
        => ToUnicode((byte)LineSection.Horizontal | (byte)Top);
    public string BottomLine
        => ToUnicode((byte)LineSection.Horizontal | (byte)Bottom);
    public string LeftLine
        => ToUnicode((byte)LineSection.Vertical | (byte)Left);
    public string RightLine
        => ToUnicode((byte)LineSection.Vertical | (byte)Right);

    public string BoxString
        => new StringBuilder()
            .Append(UpperLeft).Append(repeat(TopLine, Width - 2)).Append(UpperRight).Append(Environment.NewLine)
            .AppendJoin(Environment.NewLine, Enumerable.Range(0, Height - 2).Select(_ => new StringBuilder()
                .Append(LeftLine).Append(new string(' ', Width - 2)).Append(RightLine)
            )).Append(Environment.NewLine)
            .Append(LowerLeft).Append(repeat(BottomLine, Width - 2)).Append(LowerRight).Append(Environment.NewLine)
            .ToString();

    private string ToUnicode(int boxCharNumber)
    {
        int offset = UnicodeRanges.BoxDrawing.FirstCodePoint;
        return char.ConvertFromUtf32(offset + boxCharNumber);

    }

    private StringBuilder repeat(string str, int count)
        => new StringBuilder().AppendJoin(string.Empty, Enumerable.Range(0, count).Select(_ => str));

}
public record LineConfig
{
    /// <summary>
    /// thickness is ignored when style = double
    /// </summary>
    public LineConfig(LineStyle style, LineThickness thickness = LineThickness.Light)
    {
        this.Style = style;
        this.Thickness = style == LineStyle.Double
            ? LineThickness.Light
            : thickness;
    }

    public LineStyle Style { get; init; }
    public LineThickness Thickness { get; init; }

    public void Deconstruct(out LineStyle Style, out LineThickness Thickness)
    {
        Style = this.Style;
        Thickness = this.Thickness;
    }
}

public class BoxMapper
{
    public string MapBorder(LineConfig config)
    {
        int offset = UnicodeRanges.BoxDrawing.FirstCodePoint;
        int result = offset + (int)config.Thickness + (int)config.Style;
        return char.ConvertFromUtf32(result);
    }
    public string MapEdge(LineConfig config)
    {


        int offset = UnicodeRanges.BoxDrawing.FirstCodePoint;
        int result = offset + (int)config.Thickness + (int)config.Style;
        return char.ConvertFromUtf32(result);
    }

    public LineStyle Left { get; set; } = LineStyle.Light;
    public LineStyle Right { get; set; } = LineStyle.Light;
    public LineStyle Top { get; set; } = LineStyle.Light;
    public LineStyle Bottom { get; set; } = LineStyle.Light;
}

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
                vm => vm.IsCentered,
            (minWidth, padding, content, isComment, boxSettingsVm, isCentered) =>
                boxSettingsVm.SettingsChanges.Select(boxSettings =>
                {
                    //return new Box()
                    //{
                    //    Width = 10,
                    //    Height = 10,
                    //    Top = LineThickness.Light,
                    //    Bottom = LineThickness.Light,
                    //    Left = LineThickness.Light,
                    //    Right = LineThickness.Heavy
                    //}.BoxString;


                    var lines = content.Split(Environment.NewLine);

                    var actualWidth = lines.Max(x => x.Length) + (padding * 2) + 2;
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
                                isCentered
                                    ? boxSettings.Left
                                    + ' '.Repeat((actualWidth - 2 - line.Length) / 2)
                                    + line.Trim()
                                    + ' '.Repeat((int)Math.Ceiling((decimal)(actualWidth - 2 - line.Length) / 2))
                                    + boxSettings.Right

                                    : boxSettings.Left
                                    + ' '.Repeat(Padding)
                                    + line.Trim()
                                    + ' '.Repeat(actualWidth - padding - 2 - line.Length)
                                    + boxSettings.Right
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