using System;
using System.Collections.Generic;

namespace BankingSystem
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Banking System - Day 8";
            var accounts = new List<Account>
            {
                new SavingsAccount("Ali", 1000, 0.02),
                new CheckingAccount("Sara", 500, 200)
            };

            while (true)
            {
                ShowMenu();
                switch ((Console.ReadLine() ?? "").Trim())
                {
                    case "1": CreateAccount(accounts); break;
                    case "2": ListAccounts(accounts); break;
                    case "3": Deposit(accounts); break;
                    case "4": Withdraw(accounts); break;
                    case "5": Transfer(accounts); break;
                    case "0": Info("Bye 👋"); return;
                    default: Warn("Invalid choice."); break;
                }
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("=== Banking System ===");
            Console.WriteLine("1) Create account");
            Console.WriteLine("2) List accounts");
            Console.WriteLine("3) Deposit");
            Console.WriteLine("4) Withdraw");
            Console.WriteLine("5) Transfer");
            Console.WriteLine("0) Exit");
            Console.Write("Choose: ");
        }

        static void CreateAccount(List<Account> accounts)
        {
            Console.WriteLine("Choose type: 1) Savings  2) Checking");
            string type = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Owner name: ");
            string owner = Console.ReadLine() ?? "";

            double initial = ReadDouble("Initial deposit: ", 0);

            if (type == "1")
            {
                double rate = ReadDouble("Interest rate (e.g., 0.02): ", 0);
                accounts.Add(new SavingsAccount(owner, initial, rate));
                Notify("Savings account created ✅");
            }
            else if (type == "2")
            {
                double overdraft = ReadDouble("Overdraft limit: ", 0);
                accounts.Add(new CheckingAccount(owner, initial, overdraft));
                Notify("Checking account created ✅");
            }
            else
            {
                Warn("Unknown account type.");
            }
        }

        static void ListAccounts(List<Account> accounts)
        {
            if (accounts.Count == 0) { Info("No accounts yet."); return; }

            Console.WriteLine("\n#   Type      Owner        Balance   Extra");
            Console.WriteLine("----------------------------------------------");

            for (int i = 0; i < accounts.Count; i++)
            {
                var a = accounts[i];
                string extra = a switch
                {
                    SavingsAccount s => $"Rate={s.InterestRate:P0}",
                    CheckingAccount c => $"Overdraft={c.OverdraftLimit}",
                    _ => ""
                };
                Console.WriteLine($"{i + 1,2}. {a.GetType().Name,-9} {a.Owner,-10} {a.Balance,8:C}  {extra}");
            }
        }

        static void Deposit(List<Account> accounts)
        {
            if (!TryPickAccount(accounts, out var acc)) return;
            double amt = ReadDouble("Deposit amount: ", 1);
            acc.Deposit(amt);
            Notify($"Deposited {amt:C}. New balance = {acc.Balance:C}");
        }

        static void Withdraw(List<Account> accounts)
        {
            if (!TryPickAccount(accounts, out var acc)) return;
            double amt = ReadDouble("Withdraw amount: ", 1);
            if (acc.Withdraw(amt))
                Notify($"Withdrew {amt:C}. New balance = {acc.Balance:C}");
            else
                Warn("❌ Withdrawal failed (insufficient balance/limit).");
        }

        static void Transfer(List<Account> accounts)
        {
            if (!TryPickAccount(accounts, out var from)) return;
            if (!TryPickAccount(accounts, out var to)) return;
            if (from == to) { Warn("Cannot transfer to the same account."); return; }

            double amt = ReadDouble("Transfer amount: ", 1);
            if (from.Withdraw(amt))
            {
                to.Deposit(amt);
                Notify($"Transferred {amt:C} from {from.Owner} to {to.Owner}");
            }
            else
                Warn("❌ Transfer failed (insufficient funds).");
        }

        static bool TryPickAccount(List<Account> accounts, out Account acc)
        {
            acc = null!;
            if (accounts.Count == 0) { Warn("No accounts available."); return false; }
            ListAccounts(accounts);
            int idx = ReadInt("Choose account number: ", 1, accounts.Count) - 1;
            acc = accounts[idx];
            return true;
        }

        // Helpers
        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v) && v >= min && v <= max)
                    return v;
                Warn($"Enter a number between {min} and {max}.");
            }
        }

        static double ReadDouble(string prompt, double min)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine(), out double v) && v >= min)
                    return v;
                Warn($"Enter a number ≥ {min}.");
            }
        }

        static void Warn(string msg) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine(msg); Console.ResetColor(); }
        static void Notify(string msg) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine(msg); Console.ResetColor(); }
        static void Info(string msg) { Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine(msg); Console.ResetColor(); }
    }

    // ===== OOP Core =====
    interface IAccount
    {
        string Owner { get; }
        double Balance { get; }
        void Deposit(double amount);
        bool Withdraw(double amount);
    }

    abstract class Account : IAccount
    {
        public string Owner { get; set; }
        public double Balance { get; protected set; }

        public Account(string owner, double initialBalance)
        {
            Owner = owner;
            Balance = initialBalance;
        }

        public virtual void Deposit(double amount) => Balance += amount;
        public abstract bool Withdraw(double amount);
    }

    class SavingsAccount : Account
    {
        public double InterestRate { get; set; }
        public SavingsAccount(string owner, double initialBalance, double rate)
            : base(owner, initialBalance) => InterestRate = rate;

        public override bool Withdraw(double amount)
        {
            if (amount <= Balance)
            {
                Balance -= amount;
                return true;
            }
            return false;
        }
    }

    class CheckingAccount : Account
    {
        public double OverdraftLimit { get; set; }
        public CheckingAccount(string owner, double initialBalance, double overdraft)
            : base(owner, initialBalance) => OverdraftLimit = overdraft;

        public override bool Withdraw(double amount)
        {
            if (amount <= Balance + OverdraftLimit)
            {
                Balance -= amount;
                return true;
            }
            return false;
        }
    }
}
