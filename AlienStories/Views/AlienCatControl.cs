using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace AlienStories.Views;

public class AlienCatControl : Control
{
    // ===== Свойства =====

    public static readonly StyledProperty<string> ColorProperty =
        AvaloniaProperty.Register<AlienCatControl, string>(nameof(Color), "#FFA500");

    public string Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly StyledProperty<bool> IsHappyProperty =
        AvaloniaProperty.Register<AlienCatControl, bool>(nameof(IsHappy), true);

    public bool IsHappy
    {
        get => GetValue(IsHappyProperty);
        set => SetValue(IsHappyProperty, value);
    }

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<AlienCatControl, double>(nameof(Size), 80.0);

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly StyledProperty<double> JumpProperty =
        AvaloniaProperty.Register<AlienCatControl, double>(nameof(Jump), 0);

    public double Jump
    {
        get => GetValue(JumpProperty);
        set => SetValue(JumpProperty, value);
    }

    public static readonly StyledProperty<int> ShapeTypeProperty =
        AvaloniaProperty.Register<AlienCatControl, int>(nameof(ShapeType), 0);

    public int ShapeType
    {
        get => GetValue(ShapeTypeProperty);
        set => SetValue(ShapeTypeProperty, value);
    }

    static AlienCatControl()
    {
        AffectsRender<AlienCatControl>(ColorProperty, IsHappyProperty, SizeProperty, JumpProperty, ShapeTypeProperty);
    }

    public override void Render(DrawingContext context)
    {
        var size = Size;
        var centerX = size / 2;
        var centerY = size / 2;

        var jumpOffset = -Jump * size * 0.3;
        var color = Avalonia.Media.Color.Parse(Color);
        var brush = new SolidColorBrush(color);

        // 🎨 РИСУЕМ ТЕЛО В ЗАВИСИМОСТИ ОТ ФОРМЫ
        // ShapeType: 0=Круг, 1=Треугольник, 2=Квадрат, 3=Звезда, 4=Капля
        switch (ShapeType)
        {
            case 0:
                DrawCircle(context, brush, centerX, centerY + jumpOffset, size);
                break;
            case 1:
                DrawTriangle(context, brush, centerX, centerY + jumpOffset, size);
                break;
            case 2:
                DrawSquare(context, brush, centerX, centerY + jumpOffset, size);
                break;
            case 3:
                DrawStar(context, brush, centerX, centerY + jumpOffset, size);
                break;
            case 4:
                DrawDrop(context, brush, centerX, centerY + jumpOffset, size);
                break;
            default:
                DrawCircle(context, brush, centerX, centerY + jumpOffset, size);
                break;
        }

        // ---- Уши (всегда одинаковые) ----
        var earSize = size * 0.3;
        var earY = centerY + jumpOffset - size / 2.2;

        var ear1 = new StreamGeometry();
        using (var ctx = ear1.Open())
        {
            ctx.BeginFigure(new Point(centerX - size * 0.32, earY + 5), true);
            ctx.LineTo(new Point(centerX - size * 0.45, earY - earSize));
            ctx.LineTo(new Point(centerX - size * 0.12, earY + 5));
            ctx.EndFigure(true);
        }
        context.DrawGeometry(brush, null, ear1);

        var ear2 = new StreamGeometry();
        using (var ctx = ear2.Open())
        {
            ctx.BeginFigure(new Point(centerX + size * 0.32, earY + 5), true);
            ctx.LineTo(new Point(centerX + size * 0.45, earY - earSize));
            ctx.LineTo(new Point(centerX + size * 0.12, earY + 5));
            ctx.EndFigure(true);
        }
        context.DrawGeometry(brush, null, ear2);

        // ---- Глаза ----
        var eyeSize = size * 0.1;
        var eyeY = centerY + jumpOffset - size * 0.05;
        var eyeSpacing = size * 0.18;

        context.DrawEllipse(
            Brushes.White,
            null,
            new Point(centerX - eyeSpacing, eyeY),
            eyeSize * 1.3,
            eyeSize * 1.4
        );
        context.DrawEllipse(
            Brushes.White,
            null,
            new Point(centerX + eyeSpacing, eyeY),
            eyeSize * 1.3,
            eyeSize * 1.4
        );

        var pupilSize = eyeSize * 0.6;
        var pupilOffsetY = IsHappy ? 0 : eyeSize * 0.3;

        context.DrawEllipse(
            Brushes.Black,
            null,
            new Point(centerX - eyeSpacing, eyeY + pupilOffsetY * 0.5),
            pupilSize,
            pupilSize
        );
        context.DrawEllipse(
            Brushes.Black,
            null,
            new Point(centerX + eyeSpacing, eyeY + pupilOffsetY * 0.5),
            pupilSize,
            pupilSize
        );

        var highlightSize = pupilSize * 0.4;
        context.DrawEllipse(
            Brushes.White,
            null,
            new Point(centerX - eyeSpacing + pupilSize * 0.5, eyeY - pupilSize * 0.4),
            highlightSize,
            highlightSize
        );
        context.DrawEllipse(
            Brushes.White,
            null,
            new Point(centerX + eyeSpacing + pupilSize * 0.5, eyeY - pupilSize * 0.4),
            highlightSize,
            highlightSize
        );

        // ---- Рот ----
        var mouthY = centerY + jumpOffset + size * 0.12;

        if (IsHappy)
        {
            context.DrawLine(
                new Pen(Brushes.Black, 1.5),
                new Point(centerX - size * 0.12, mouthY),
                new Point(centerX, mouthY + size * 0.08)
            );
            context.DrawLine(
                new Pen(Brushes.Black, 1.5),
                new Point(centerX, mouthY + size * 0.08),
                new Point(centerX + size * 0.12, mouthY)
            );
        }
        else
        {
            context.DrawLine(
                new Pen(Brushes.Black, 1.5),
                new Point(centerX - size * 0.12, mouthY + size * 0.04),
                new Point(centerX, mouthY - size * 0.04)
            );
            context.DrawLine(
                new Pen(Brushes.Black, 1.5),
                new Point(centerX, mouthY - size * 0.04),
                new Point(centerX + size * 0.12, mouthY + size * 0.04)
            );
        }

        // ---- Усы ----
        var whiskerPen = new Pen(Brushes.Black, 1.2);
        var whiskerY = mouthY + size * 0.04;
        var whiskerLength = size * 0.2;

        context.DrawLine(
            whiskerPen,
            new Point(centerX - size * 0.08, whiskerY),
            new Point(centerX - size * 0.08 - whiskerLength, whiskerY - size * 0.04)
        );
        context.DrawLine(
            whiskerPen,
            new Point(centerX - size * 0.08, whiskerY),
            new Point(centerX - size * 0.08 - whiskerLength, whiskerY + size * 0.04)
        );
        context.DrawLine(
            whiskerPen,
            new Point(centerX + size * 0.08, whiskerY),
            new Point(centerX + size * 0.08 + whiskerLength, whiskerY - size * 0.04)
        );
        context.DrawLine(
            whiskerPen,
            new Point(centerX + size * 0.08, whiskerY),
            new Point(centerX + size * 0.08 + whiskerLength, whiskerY + size * 0.04)
        );
    }

    // ===== МЕТОДЫ ДЛЯ РИСОВАНИЯ РАЗНЫХ ФОРМ =====

    private void DrawCircle(DrawingContext context, IBrush brush, double cx, double cy, double size)
    {
        context.DrawEllipse(brush, null, new Point(cx, cy), size / 2.2, size / 2.2);
    }

    private void DrawTriangle(DrawingContext context, IBrush brush, double cx, double cy, double size)
    {
        var s = size / 2.2;
        var geom = new StreamGeometry();
        using (var ctx = geom.Open())
        {
            ctx.BeginFigure(new Point(cx, cy - s), true);
            ctx.LineTo(new Point(cx - s, cy + s));
            ctx.LineTo(new Point(cx + s, cy + s));
            ctx.EndFigure(true);
        }
        context.DrawGeometry(brush, null, geom);
    }

    private void DrawSquare(DrawingContext context, IBrush brush, double cx, double cy, double size)
    {
        var s = size / 2.5;
        context.DrawRectangle(brush, null, new Rect(cx - s, cy - s, s * 2, s * 2));
    }

    private void DrawStar(DrawingContext context, IBrush brush, double cx, double cy, double size)
    {
        var outer = size / 2.2;
        var inner = outer * 0.4;
        var points = new List<Point>();
        for (int i = 0; i < 10; i++)
        {
            var r = i % 2 == 0 ? outer : inner;
            var angle = Math.PI / 2 + i * 2 * Math.PI / 10;
            points.Add(new Point(cx + r * Math.Cos(angle), cy + r * Math.Sin(angle)));
        }
        var geom = new StreamGeometry();
        using (var ctx = geom.Open())
        {
            ctx.BeginFigure(points[0], true);
            for (int i = 1; i < points.Count; i++)
                ctx.LineTo(points[i]);
            ctx.EndFigure(true);
        }
        context.DrawGeometry(brush, null, geom);
    }

    private void DrawDrop(DrawingContext context, IBrush brush, double cx, double cy, double size)
    {
        var s = size / 2.2;
        var geom = new StreamGeometry();
        using (var ctx = geom.Open())
        {
            ctx.BeginFigure(new Point(cx, cy - s), true);
            ctx.QuadraticBezierTo(new Point(cx + s, cy + s * 0.3), new Point(cx, cy + s));
            ctx.QuadraticBezierTo(new Point(cx - s, cy + s * 0.3), new Point(cx, cy - s));
            ctx.EndFigure(true);
        }
        context.DrawGeometry(brush, null, geom);
    }
}