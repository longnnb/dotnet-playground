using static System.Console;

namespace CSharpFeatures.Csharp7;
public class PatternMatching
{
    public void Demo()
    {
        object[] data = { 1, "two", 3, "four", 5 };
        int sum = 0;
        foreach (var item in data)
        {
            if (item is int value) // check and assign
            {
                sum += value;
            }
        }
        WriteLine($"Sum of integers: {sum}");

        // switch statement
        foreach (var item in data)
        {
            switch (item)
            {
                case int value: // check and assign
                    sum += value;
                    break;
                case string text: // check and assign
                    WriteLine($"Text: {text}");
                    break;
            }
        }
        WriteLine($"Sum of integers: {sum}");
    }

    public void DisplayShape(Shape shape)
    {
        // old way
        if (shape is Rectangle)
        {
            var rc = (Rectangle)shape;

        }
        else if (shape is Circle)
        {
            // ...
        }

        // old way
        var rect = shape as Rectangle; 
        if (rect != null) // nonnull
        {
            //...
        }


        // new way
        if (shape is Rectangle r) 
        {
            // use r
        }

        // can also do the invserse
        if (!(shape is Circle cc))
        {
            // not a circle!
        }

        switch (shape)
        {
            case Circle c:
                // use c
                break;
            case Rectangle sq when (sq.Width == sq.Height): // assign with condition
                // square!
                break;
            case Rectangle rr:
                // use rr
                break;
        }
    }

}


public class Shape
{

}

public class Rectangle : Shape
{
    public int Width, Height;
}

public class Circle : Shape
{
    public int Diameter;
}