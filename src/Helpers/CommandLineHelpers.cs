namespace OpenProject.ApplicationShell.Helpers;

public static class CommandLineHelpers
{
    public static void ParseCommandLine(CommandLineOption[] options, string[] args, Action<string, string[]> optAction)
    {
        var optArgs = new Dictionary<string, List<string>>(); // key-value pairs of options and their arguments

        var shortOpts = string.Empty; // the list of short options in the current argument (if the first opt is short)
        var hasNextShortOpt = false; // if there is a short option after the current short option (e.g. -abc)
        var currentShortOptIdx = 0; // current short option index in the list of short opts

        var argumentIsAtNextPos =
            false; // if the argument is at the next position (e.g. --flag option, as opposed to --flag:option)

        var optName = string.Empty; // the name of the current option being parsed
        var argVal = string.Empty; // the value of the argument, if there is one

        // we don't use a foreach loop so we can control the current index
        for (var i = 0; i < args.Length;)
        {
            var opt = args[i];

            // empty option
            if (opt.Length == 0)
            {
                ++i;
                continue;
            }

            // if the current option is not an argument for the previous option
            if (!argumentIsAtNextPos && opt[0] == '-')
            {
                var isLongOpt = opt.Length > 2 && opt[1] == '-'; // checks if first two characters are -
                opt = isLongOpt
                    ? opt[2..]
                    : opt[1..]; // if long option, extracts text after the --, else, after the first -

                var optLen = opt.Length; // the length of the option
                // for long options, only get the length of the actual option name
                if (isLongOpt)
                {
                    if (opt.Contains('='))
                        optLen = opt.IndexOf('=');
                    else if (opt.Contains(':'))
                        optLen = opt.IndexOf(':');
                }

                // if there are no short options succeeding the current option
                if (!hasNextShortOpt)
                    shortOpts = opt;

                // extract option name
                optName = isLongOpt ? opt[..optLen] : shortOpts[currentShortOptIdx].ToString();

                var optProp = options.First(current => current.Option == optName);
                if (optProp.IsLongOption)
                {
                    if (optProp.RequiresArgument)
                        switch (argumentIsAtNextPos)
                        {
                            // check if argument for option is specified in the same token
                            // or if it's in the next position args[i + 1]
                            case false when opt.Length <= optLen:
                                argumentIsAtNextPos =
                                    true; // the overall option length (including the argument) is leq the opt name
                                ++i;
                                continue;
                            case false:
                                argVal = opt[(optLen + 1)..]; // argument is specified in the same token
                                break;
                        }
                }
                else
                {
                    if (optProp.RequiresArgument)
                    {
                        // short option with embedded argument like -DDEBUG where DEBUG is the arg
                        if (!argumentIsAtNextPos && opt.Length > 1)
                        {
                            argVal = opt[1..]; // extract arg which is just right after the opt
                        }
                        else
                        {
                            // arg is in the next position
                            argumentIsAtNextPos = true;
                            ++i;
                            continue;
                        }
                    }
                    else
                    {
                        hasNextShortOpt = false;
                        // check if there are more short opts in the same token after this one
                        if (!hasNextShortOpt && currentShortOptIdx < shortOpts.Length - 1)
                        {
                            hasNextShortOpt = true;
                            ++currentShortOptIdx;
                        }
                    }
                }

                // write to dictionary
                if (optArgs.ContainsKey(optProp.Option))
                    optArgs[optProp.Option].Add(argVal);
                else
                    optArgs.Add(optProp.Option, new List<string> { argVal });

                if (hasNextShortOpt)
                    continue;
            }
            else
            {
                argVal = opt;

                var optProp = options.First(current => current.Option == optName);
                if (optArgs.ContainsKey(optProp.Option))
                    optArgs[optProp.Option].Add(argVal);
                else
                    optArgs.Add(optProp.Option, new List<string> { argVal });
            }

            // reset values
            argumentIsAtNextPos = false;
            currentShortOptIdx = 0;
            argVal = string.Empty;

            ++i;
        }

        foreach (var optArg in optArgs)
            optAction(optArg.Key, optArg.Value.ToArray());
    }
}

public struct CommandLineOption
{
    public string Option { get; set; }
    public bool IsLongOption { get; set; }
    public bool RequiresArgument { get; set; }
    public string? Description { get; set; }
}