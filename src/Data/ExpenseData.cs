using System;

namespace ExpenseTracker.Data;

public class ExpenseData
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; } = "";
    public int Amount { get; set; }
    public string Currency { get; set; } = "$";

    public ExpenseData() {}

    public void SetData(int id, DateOnly date, string description, int amount)
    {
        Id = id;
        Date = date;
        Description = description;
        Amount = amount;
    }

    public string ReturnDate()
    {
        return Date.ToString("yyyy-MM-dd");
    }

    public override string ToString()
    {
        return $"{Id},{ReturnDate()},{Description},{Currency}{Amount}";
    }
}