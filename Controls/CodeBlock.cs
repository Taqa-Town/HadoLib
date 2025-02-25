
using HadoLib.Tokenizer;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI;

namespace HadoLib.Controls;

public sealed partial class CodeBlock : Control
{
    private Button _copyButton;
    private TextBlock TextSource;
    private TokenizingModel _tokenizingModel = GetTokenizingModel();
    private readonly DispatcherTimer _timer = new();
    private readonly string _copyIcon = "🗎";
    private readonly string _checkIcon = "✓";

    public CodeBlock()
    {
        this.DefaultStyleKey = typeof(CodeBlock);
    }

    public static TokenizingModel GetTokenizingModel()
    {
        var source = new Uri(@"ms-appx:///Tokenizer/CSharpSyntax.json");
        var result = StorageFile.GetFileFromApplicationUriAsync(source);
        var file = result.AsTask().Result;
        string json = File.ReadAllText(file.Path);
        LanguageSyntaxModel syntaxModel = JsonSerializer.Deserialize<LanguageSyntaxModel>(json);
        return new TokenizingModel(syntaxModel);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        
        _copyButton = (Button)GetTemplateChild("Part_CopyButton");
        TextSource = (TextBlock)GetTemplateChild("Part_TextSource");
        if (_copyButton is not null)
            _copyButton.Click += CopyButton_Click;

        if (ColorizeText is true)
            ColorTheText(this);
        else
            DeColorTheText(this);

    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(Text);
        Clipboard.SetContent(package);

        _copyButton.Content = _checkIcon;
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Time_Tick;
        _timer.Start();
    }

    private void Time_Tick(object sender, object e)
    {
        _copyButton.Content = _copyIcon;
        _timer.Stop();
    }

    #region Properties
    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public static readonly DependencyProperty TitleProperty = 
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(CodeBlock), new PropertyMetadata(string.Empty));



    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(CodeBlock), new PropertyMetadata(string.Empty, OnTextChanged));

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CodeBlock control)
            return;
        if (control.TextSource is not null && control.ColorizeText is true)
            ColorTheText(control);
    }



    public string JsonSource
    {
        get { return (string)GetValue(JsonSourceProperty); }
        set { SetValue(JsonSourceProperty, value); }
    }
    public static readonly DependencyProperty JsonSourceProperty =
        DependencyProperty.Register(nameof(JsonSource), typeof(string), typeof(CodeBlock), new PropertyMetadata(string.Empty, OnSourceChanged));

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CodeBlock control)
            return;
        var value = e.NewValue.ToString();
        var source = new Uri(value);
        var result = StorageFile.GetFileFromApplicationUriAsync(source);
        var file = result.AsTask().Result;
        string json = File.ReadAllText(file.Path);
        LanguageSyntaxModel syntaxModel = JsonSerializer.Deserialize<LanguageSyntaxModel>(json);
        control._tokenizingModel = new TokenizingModel(syntaxModel);
    }

    public Brush TitleBackground
    {
        get { return (Brush)GetValue(TitleBackgroundProperty); }
        set { SetValue(TitleBackgroundProperty, value); }
    }

    public static readonly DependencyProperty TitleBackgroundProperty =
        DependencyProperty.Register(nameof(TitleBackground), typeof(Brush), typeof(CodeBlock), new PropertyMetadata(null));

    public Brush MainBackground
    {
        get { return (Brush)GetValue(MainBackgroundProperty); }
        set { SetValue(MainBackgroundProperty, value); }
    }

    public static readonly DependencyProperty MainBackgroundProperty =
        DependencyProperty.Register(nameof(MainBackground), typeof(Brush), typeof(CodeBlock), new PropertyMetadata(null));

    public Brush MainForeground
    {
        get { return (Brush)GetValue(MainForegroundProperty); }
        set { SetValue(MainForegroundProperty, value); }
    }

    public static readonly DependencyProperty MainForegroundProperty =
        DependencyProperty.Register(nameof(MainForeground), typeof(Brush), typeof(CodeBlock), new PropertyMetadata(null));

    public Brush TitleForeground
    {
        get { return (Brush)GetValue(TitleForegroundProperty); }
        set { SetValue(TitleForegroundProperty, value); }
    }

    public static readonly DependencyProperty TitleForegroundProperty =
        DependencyProperty.Register(nameof(TitleForeground), typeof(Brush), typeof(CodeBlock), new PropertyMetadata(null));

    public Brush ButtonForeground
    {
        get { return (Brush)GetValue(ButtonForegroundProperty); }
        set { SetValue(ButtonForegroundProperty, value); }
    }

    public static readonly DependencyProperty ButtonForegroundProperty =
        DependencyProperty.Register(nameof(ButtonForeground), typeof(Brush), typeof(CodeBlock), new PropertyMetadata(null));


    public Brush ButtonBackground
    {
        get { return (Brush)GetValue(ButtonBackgroundProperty); }
        set { SetValue(ButtonBackgroundProperty, value); }
    }

    public static readonly DependencyProperty ButtonBackgroundProperty =
        DependencyProperty.Register(nameof(ButtonBackground), typeof(Brush), typeof(CodeBlock), new PropertyMetadata(Colors.Transparent));



    public bool ColorizeText
    {
        get { return (bool)GetValue(ColorizeTextProperty); }
        set { SetValue(ColorizeTextProperty, value); }
    }

    public static readonly DependencyProperty ColorizeTextProperty =
        DependencyProperty.Register(nameof(ColorizeText), typeof(bool), typeof(CodeBlock), new PropertyMetadata(false, OnColorizeChanged));

    private static void OnColorizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CodeBlock)
            return;

        var control = d as CodeBlock;
        if (control.TextSource is not null)
        {
            if (control.ColorizeText is true)
                ColorTheText(control);
            else if (control.ColorizeText is false)
                DeColorTheText(control);
        }

    }

    private static void DeColorTheText(CodeBlock control)
    {
        string text = control.Text;
        control.TextSource.Inlines.Clear();
        control.TextSource.Text = text;
    }

    private static void ColorTheText(CodeBlock control)
    {
        var tokens = control._tokenizingModel.Tokenize(control.Text);
        control.TextSource.Inlines.Clear();
        foreach (var token in tokens)
        {
            string text;
            if (token.Text.Contains('\n'))
                text = "\n";
            else
                text = token.Text;
            control.TextSource.Inlines.Add(new Run
            {
                Text = text,
                Foreground = token.Color
            });
        }
    }



    #endregion

}
