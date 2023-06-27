using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace XamarinBasicCalculator
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        string entry = "0";
        string operate = "";
        int stackState = 0; // 0: empty or firstNumber after operator 1: can be replaced 2: result in stack
        Stack<double> stack = new Stack<double>(); 
        public event PropertyChangedEventHandler PropertyChanged;
        public string Entry
        {
            get => entry;
            set
            {
                SetProperty(ref entry, value);
                //RefreshCanExecutes();
            }
        }
        public ICommand ClearCommand { get; }
        public ICommand PercentageCommand { get; }
        public Command BackspaceCommand { get; }
        public Command ResultCommand { get; }
        public Command DigitNumberCommand { get; }
        public Command BinaryOperation { get; }
        public MainPageViewModel()
        {
            ClearCommand = new Command(
                execute: () =>
                {
                    Entry = "0";
                    stack.Clear();
                    stackState = 0;
                    RefreshCanExecutes();
                });

            BackspaceCommand = new Command(
                execute: () =>
                {
                    Entry = Entry.Substring(0, Entry.Length - 1);
                    if (string.IsNullOrEmpty(Entry) || Entry == "-")
                    {
                        Entry = "0";
                        stack.Clear();
                        stackState = 0;
                    }
                    if(stackState == 2)
                    {
                        stack.Pop();
                        if(Entry != "0")
                        {
                            stack.Push(double.Parse(Entry));
                            stackState = 2;
                        }
                        else 
                            stackState = 0;
                    }
                    RefreshCanExecutes();
                },
                canExecute: () =>
                {
                    return Entry.Length > 1 || Entry != "0";
                });

            ResultCommand = new Command(
                execute: () =>
                {
                    Calculate(operate);
                    Entry = stack.Peek().ToString();
                    stackState = 2;
                    operate = "";
                    RefreshCanExecutes();
                },
                canExecute: () =>
                {
                    return stack.Count > 1 && !string.IsNullOrEmpty(operate);
                });

            DigitNumberCommand = new Command<string>(
                execute: (string arg) =>
                {
                    Entry += arg;
                    if (Entry.StartsWith("0") && !Entry.StartsWith("0."))
                    {
                        Entry = Entry.Substring(1);
                    }
                    if (stack.Count > 0 && stackState == 1)
                        stack.Pop();
                    stack.Push(double.Parse(Entry));
                    stackState = 1;
                    RefreshCanExecutes();
                },
                canExecute: (string arg) =>
                {
                    return !(arg == "." && Entry.Contains("."));
                });
            
            BinaryOperation = new Command<string>(
                execute: (string op) =>
                {
                    if (stack.Count > 1)
                    {
                        Calculate(operate);
                        stackState = 0;
                    }
                    if (stackState == 0 && !(stack.Count > 1))
                        stack.Push(double.Parse(Entry));

                    operate = op;
                    stackState = 0;
                    Entry = "0";
                    RefreshCanExecutes();
                },
                canExecute: (string op) =>
                {
                    return stack.Count >= 1 && string.IsNullOrEmpty(operate);
                });

            PercentageCommand = new Command(
                execute: () =>
                {
                    double result = double.Parse(Entry) / 100;
                    Entry = result.ToString();
                    if (stack.Count > 0)
                        stack.Pop();
                    stack.Push(result);
                },
                canExecute: () =>
                {
                    return !string.IsNullOrEmpty(Entry);
                });
        }

        void RefreshCanExecutes()
        {
            BackspaceCommand.ChangeCanExecute();
            DigitNumberCommand.ChangeCanExecute();
            BinaryOperation.ChangeCanExecute();
            ResultCommand.ChangeCanExecute();
        }

        void Calculate(string op)
        {
            double x = stack.Pop();
            double y = stack.Pop();
            double result = 0;
            switch (op)
            {
                case "divide": result = y / x; break;
                case "multiply": result = y * x; break;
                case "subtract": result = y - x; break;
                case "add": result = y + x; break;
            }
            stack.Push(result);
        }

        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
