namespace Challenges;
public class ListFilterer
{
    public static IEnumerable<int> GetIntegersFromList(List<object> listOfItems)
    {
        //my solution
        //var result = new List<int>();
        //foreach (var item in listOfItems)
        //{
        //    if (item is int)
        //    {
        //        result.Add((int)item);
        //    }
        //}
        //return result;

        //Linq
        return listOfItems.OfType<int>().ToList();
    }
}
