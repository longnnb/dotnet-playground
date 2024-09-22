public class Exercise
{
    public static int FindMax(int[,] numbers)
    {
        if (numbers.GetLength(0) == 0 || numbers.GetLength(1) == 0) return -1;
        var max = numbers[0, 0];
        for (var i = 0; i < numbers.GetLength(0); i++)
        for (var j = 0; j < numbers.GetLength(1); j++)
            if (numbers[i, j] > max)
                max = numbers[i, j];
        return max;
    }
}