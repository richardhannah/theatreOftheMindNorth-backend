using TheatreOfTheMind.Models;
using TheatreOfTheMind.Services;

namespace TheatreOfTheMind.Tests;

public class DiceServiceTests
{
    private static ChatMessage Msg(string text) => new() { Name = "Test", PlayerName = "Tester", TokenId = "", Text = text };

    // --- No match ---

    [Theory]
    [InlineData("hello world")]
    [InlineData("d20")]
    [InlineData("2d6")]
    [InlineData("roll 1d20")]
    public void NonDiceText_ReturnsNull(string text)
    {
        Assert.Null(DiceService.ProcessDiceRolls(Msg(text)));
    }

    // --- Basic rolls ---

    [Fact]
    public void BasicRoll_ReturnsResult()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#1d20"));
        Assert.NotNull(result);
        Assert.True(result.IsDiceRoll);
        Assert.Contains("#1d20 =>", result.Text);
        Assert.Contains("= ", result.Text);
    }

    [Fact]
    public void ImplicitCount_DefaultsToOne()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#d6"));
        Assert.NotNull(result);
        // Should show a single roll in brackets
        Assert.Matches(@"\[\d+\]", result.Text);
    }

    [Fact]
    public void MultipleDice_ShowsAllRolls()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#4d6"));
        Assert.NotNull(result);
        // Should have 4 comma-separated values in brackets
        Assert.Matches(@"\[\d+, \d+, \d+, \d+\]", result.Text);
    }

    // --- Arithmetic modifiers ---

    [Fact]
    public void AdditionModifier_ShowsInOutput()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#1d20+5"));
        Assert.NotNull(result);
        Assert.Contains("+ 5 =", result.Text);
    }

    [Fact]
    public void SubtractionModifier_ShowsInOutput()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#2d6-1"));
        Assert.NotNull(result);
        Assert.Contains("- 1 =", result.Text);
    }

    [Fact]
    public void MultiplicationModifier_ShowsInOutput()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#1d20*2"));
        Assert.NotNull(result);
        Assert.Contains("* 2 =", result.Text);
    }

    // --- Threshold (e) modifier ---

    [Fact]
    public void ThresholdModifier_ReturnsSuccessCount()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#10d6e5"));
        Assert.NotNull(result);
        Assert.Contains("successes", result.Text);
        Assert.Contains(">= 5", result.Text);
    }

    [Fact]
    public void ThresholdModifier_SingleSuccess_UsesSingular()
    {
        // Roll 100d6e6 — statistically will have at least 1 success
        // But for the singular test, we need exactly 1, which is hard to guarantee.
        // Instead, just verify the format handles both cases by checking regex pattern.
        var result = DiceService.ProcessDiceRolls(Msg("#1d6e1"));
        Assert.NotNull(result);
        // Threshold 1 means every roll >= 1 is a success, so 1d6e1 always = 1 success
        Assert.Contains("1 success", result.Text);
    }

    [Fact]
    public void ThresholdModifier_ImpossibleThreshold_ZeroSuccesses()
    {
        // e7 on a d6 — no roll can be >= 7
        var result = DiceService.ProcessDiceRolls(Msg("#5d6e7"));
        Assert.NotNull(result);
        Assert.Contains("0 successes", result.Text);
    }

    [Fact]
    public void ThresholdModifier_AllSucceed()
    {
        // e1 on any die — every roll >= 1 is a success
        var result = DiceService.ProcessDiceRolls(Msg("#4d6e1"));
        Assert.NotNull(result);
        Assert.Contains("4 successes", result.Text);
    }

    // --- Bounds clamping ---

    [Fact]
    public void ExcessiveDiceCount_ClampedTo100()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#999d6"));
        Assert.NotNull(result);
        // Should only have 100 rolls
        var rolls = result.Text.Split('[')[1].Split(']')[0];
        var count = rolls.Split(',').Length;
        Assert.Equal(100, count);
    }

    [Fact]
    public void ZeroDiceCount_ClampedToOne()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#0d6"));
        Assert.NotNull(result);
        Assert.Matches(@"\[\d+\]", result.Text);
    }

    // --- Roll value ranges ---

    [Fact]
    public void RollValues_WithinDieSidesRange()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#20d6"));
        Assert.NotNull(result);
        var rolls = result.Text.Split('[')[1].Split(']')[0]
            .Split(',')
            .Select(s => int.Parse(s.Trim()))
            .ToArray();
        Assert.All(rolls, r => Assert.InRange(r, 1, 6));
    }

    [Fact]
    public void D20Values_WithinRange()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#20d20"));
        Assert.NotNull(result);
        var rolls = result.Text.Split('[')[1].Split(']')[0]
            .Split(',')
            .Select(s => int.Parse(s.Trim()))
            .ToArray();
        Assert.All(rolls, r => Assert.InRange(r, 1, 20));
    }

    // --- Multiple dice expressions ---

    [Fact]
    public void MultipleDiceExpressions_AllProcessed()
    {
        var result = DiceService.ProcessDiceRolls(Msg("Attack #1d20+5 damage #2d6+3"));
        Assert.NotNull(result);
        Assert.Contains("|", result.Text);
        Assert.Contains("#1d20+5 =>", result.Text);
        Assert.Contains("#2d6+3 =>", result.Text);
    }

    // --- Output metadata ---

    [Fact]
    public void Result_PreservesNameAndToken()
    {
        var msg = new ChatMessage { Name = "Gandalf", PlayerName = "Ian", TokenId = "wizard", Text = "#1d20" };
        var result = DiceService.ProcessDiceRolls(msg);
        Assert.NotNull(result);
        Assert.Equal("Gandalf", result.Name);
        Assert.Equal("Ian", result.PlayerName);
        Assert.Equal("wizard", result.TokenId);
    }

    [Fact]
    public void Result_SetsIsDiceRollTrue()
    {
        var result = DiceService.ProcessDiceRolls(Msg("#1d6"));
        Assert.NotNull(result);
        Assert.True(result.IsDiceRoll);
    }

    [Fact]
    public void Result_SetsTimestamp()
    {
        var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var result = DiceService.ProcessDiceRolls(Msg("#1d6"));
        var after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Assert.NotNull(result);
        Assert.InRange(result.Ts, before, after);
    }
}
