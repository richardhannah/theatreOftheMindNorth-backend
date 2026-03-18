using System.Text.RegularExpressions;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Services;

public static partial class DiceService
{
    [GeneratedRegex(@"#(\d*)d(\d+)(?:([+\-*])(\d+))?", RegexOptions.IgnoreCase)]
    private static partial Regex DiceCodeRegex();

    public static ChatMessage? ProcessDiceRolls(ChatMessage msg)
    {
        var matches = DiceCodeRegex().Matches(msg.Text);
        if (matches.Count == 0) return null;

        var results = new List<string>();

        foreach (Match m in matches)
        {
            var count = string.IsNullOrEmpty(m.Groups[1].Value) ? 1 : int.Parse(m.Groups[1].Value);
            var sides = int.Parse(m.Groups[2].Value);

            // Bounds check to prevent DoS
            count = Math.Clamp(count, 1, 100);
            sides = Math.Clamp(sides, 1, 1000);
            var op = m.Groups[3].Value;
            var modValue = string.IsNullOrEmpty(m.Groups[4].Value) ? 0 : int.Parse(m.Groups[4].Value);

            var rolls = RollDice(count, sides);
            var subtotal = rolls.Sum();
            var total = ApplyModifier(subtotal, op, modValue);

            var rollsPart = $"[{string.Join(", ", rolls)}]";
            var diceCode = m.Value;

            results.Add(string.IsNullOrEmpty(op)
                ? $"{diceCode} => {rollsPart} = {total}"
                : $"{diceCode} => {rollsPart} = {subtotal} {op} {modValue} = {total}");
        }

        return new ChatMessage
        {
            Name = msg.Name,
            PlayerName = msg.PlayerName,
            TokenId = msg.TokenId,
            Text = string.Join("  |  ", results),
            Ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            IsDiceRoll = true
        };
    }

    private static int[] RollDice(int count, int sides)
    {
        var rolls = new int[count];
        for (var i = 0; i < count; i++)
        {
            rolls[i] = Random.Shared.Next(1, sides + 1);
        }
        return rolls;
    }

    private static int ApplyModifier(int subtotal, string op, int value)
    {
        return op switch
        {
            "+" => subtotal + value,
            "-" => subtotal - value,
            "*" => subtotal * value,
            _ => subtotal
        };
    }
}
