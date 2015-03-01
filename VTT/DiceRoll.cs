using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace VTT
{
    public static class DiceRoll
    {
        private static Random r = new Random();
        private static string diceMatchPattern = "^([0-9])([0-9]*)d([0-9])([0-9]*)$";

        public static string Roll(string expression)
        {
            if (expression.Length > 3)
            {
                if (expression.Substring(0, 3) == "/r ")
                {
                    expression = expression.Substring(2, expression.Length - 2);
                    string result = expression;
                    result = result.Replace(" ", string.Empty);
                    //addition/subtraction
                    int sum = 0;
                    string[] splitExp = { string.Empty, string.Empty };
                    if (result.Contains('+'))
                    {
                        splitExp = result.Split('+');
                        if (splitExp[1] != string.Empty)
                            sum += Int32.Parse(splitExp[1]);
                        result = splitExp[0];
                    }
                    else if (result.Contains('-'))
                    {
                        splitExp = result.Split('-');
                        if (splitExp[1] != string.Empty)
                            sum -= Int32.Parse(splitExp[1]);
                        result = splitExp[0];
                    }
                    if (splitExp[0] == String.Empty)
                    {
                        splitExp[0] = result;
                    }
                    if (Regex.IsMatch(result, diceMatchPattern))
                    {
                        //multiplication
                        var d = splitExp[0].Split('d');
                        int numberOfRolls = Int32.Parse(d[0]);
                        int dieSides = Int32.Parse(d[1]);
                        for (int i = 0; i < numberOfRolls; ++i)
                        {
                            sum += r.Next(1, dieSides);
                        }
                        return "has rolled " + expression + " and got " + sum.ToString();
                    }
                }
            }
            return expression;
        }
    }
}
