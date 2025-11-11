using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace LightJockey.Views;

/// <summary>
/// Audio visualizer control for displaying spectral data
/// </summary>
public partial class AudioVisualizerControl : UserControl
{
    public static readonly DependencyProperty SpectralDataProperty =
        DependencyProperty.Register(
            nameof(SpectralData),
            typeof(float[]),
            typeof(AudioVisualizerControl),
            new PropertyMetadata(Array.Empty<float>(), OnSpectralDataChanged));

    public static readonly DependencyProperty IsBeatDetectedProperty =
        DependencyProperty.Register(
            nameof(IsBeatDetected),
            typeof(bool),
            typeof(AudioVisualizerControl),
            new PropertyMetadata(false, OnBeatDetectedChanged));

    public float[] SpectralData
    {
        get => (float[])GetValue(SpectralDataProperty);
        set => SetValue(SpectralDataProperty, value);
    }

    public bool IsBeatDetected
    {
        get => (bool)GetValue(IsBeatDetectedProperty);
        set => SetValue(IsBeatDetectedProperty, value);
    }

    public AudioVisualizerControl()
    {
        InitializeComponent();
    }

    private static void OnSpectralDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AudioVisualizerControl control && e.NewValue is float[] data)
        {
            control.UpdateVisualization(data);
        }
    }

    private static void OnBeatDetectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AudioVisualizerControl control && e.NewValue is bool isBeat && isBeat)
        {
            control.AnimateBeatIndicator();
        }
    }

    private void UpdateVisualization(float[] data)
    {
        if (data.Length < 3)
            return;

        // Maximum height for bars
        var maxHeight = ActualHeight - 30;

        // Animate the frequency bars
        AnimateBar(LowFreqBar, data[0] * maxHeight);
        AnimateBar(MidFreqBar, data[1] * maxHeight);
        AnimateBar(HighFreqBar, data[2] * maxHeight);
    }

    private void AnimateBar(FrameworkElement bar, double targetHeight)
    {
        var animation = new DoubleAnimation
        {
            To = targetHeight,
            Duration = TimeSpan.FromMilliseconds(50),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        bar.BeginAnimation(HeightProperty, animation);
    }

    private void AnimateBeatIndicator()
    {
        var storyboard = new Storyboard();

        // Opacity animation
        var opacityAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(opacityAnimation, BeatIndicator);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));

        // Scale animation
        var scaleAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.5,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleAnimation, BeatScale);
        Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("ScaleX"));

        var scaleAnimationY = new DoubleAnimation
        {
            From = 1.0,
            To = 1.5,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleAnimationY, BeatScale);
        Storyboard.SetTargetProperty(scaleAnimationY, new PropertyPath("ScaleY"));

        storyboard.Children.Add(opacityAnimation);
        storyboard.Children.Add(scaleAnimation);
        storyboard.Children.Add(scaleAnimationY);

        storyboard.Begin();
    }
}
