# ASH

ASH stands for Application shell. It can be used to run applications (obviously).

## Features

ASH boasts a number of features, including:

- Customizable prompt (with colors<sub>1</sub> and all)
- A functional<sub>2</sub> scripting language
- Some built-in commands

And more!

## Usage

### Flags

These flags modify how ASH starts up.

- `--quiet|-q`: quiet startup (discussed below in `## Quiet Startup`.)
- `--execute|-x[:|=]<commands>`: runs `<commands>` before startup, and before parsing `ash.cfg`.
  - `-c`: exits immediately after commands finish executing.
- `--version|-v`: prints the version of ASH and exits.
- `--help|-h`: prints the description of every flag, and exits.

### Variables

In ASH, all variables are strings. You needn't enclose them with quotes, unless there is trailing whitespace (in which case, you do.)

To refer to a variable: `$variable`  
To assign to/create a variable: `variable = new value`

If the variable doesn't already exist in the first place, referring to it will just print the text `$variable` literally. For example, `echo`ing a non-existent `$hello` will just print `$hello` to the console.

It doesn't matter if the variable is inside quotes or not, it will get expanded.

### Customizable Prompt Format

To modify the prompt, you may change the value of the `prompt_fmt` variable, either in the `ash.cfg` file in your home directory or assigning it directly in the shell (will get reset the next time you open the shell.)

Currently, there are 4 format specifiers to be used in customizing the prompt.

- `%u`: The user name.
- `%m`: The NetBIOS name of the machine.
- `%c`: The current directory.
- `%e`: The exit code of the last command executed.

Use `\n` for newline.

The default is `prompt_fmt = "%u@%m:%c$ "`

It may display the following:

```
user@machine:/some/directory$
```

#### Colors

You can customize the colors of the prompt elements.

- `prompt_fmt_u_color`: color of the `%u` specifier
- `prompt_fmt_m_color`: color of the `%m` specifier
- `prompt_fmt_c_color`: color of the `%c` specifier
- `prompt_fmt_e_fail_color`: color of the `%e` specifier when a command fails (returns something other than 0)
- `prompt_fmt_e_success_color`: color of the `%e` specifier when a command succeeds (returns 0)

All of them can have the values `Black`, `DarkBlue`, `DarkGreen`, `DarkCyan`, `DarkRed`, `DarkMagenta`, `DarkYellow`, `Gray`, `DarkGray`, `Blue`, `Green`, `Cyan`, `Red`, `Magenta`, `Yellow`, `White`. The values are case-**in**sensitive.

### Built-in Commands

There are 5 built-in commands.

- `help [command]`: Displays the help page. If `[command]` is specified, it displays its description and usage.
- `echo [text]`: Prints `[text]` and succeeding arguments to the console and a new line..
- `strfmt <format> [arg1, [arg2, [...]]]`: Formats and prints the text according to the format string `<format>`. Quotes will be removed. Also un-escapes escape sequences (like \\n, \\r, etc.).
- `chdir <directory>`: Changes the current directory to `<directory>`.
- `list [-l] [-a] [-h] [directory]`: Lists the files in the current directory, or optionally, in `[directory]`. Use `-a` to list all files in the directory, including hidden files; `-l` to list the contents in list format; `-h` to print the sizes in human-readable format (with units like B, K, M, G, T, P, E). These flags can be combined like so: `list -al`.

#### `list` list Format

`list -l` will print something like this:

```txt
Mode    | Name   | Date Modified        | Date Created         | Size
-----------------------------------------------------------------------
darhsl  | a.txt  | 1980/01/02 10:37:00  | 1980/01/01 10:29:00  | 7522
darhsl  | b.a    | 1980/01/03 09:34:00  | 1980/01/03 11:22:00  | 10
```

With the `-h` flag, the `Size` header may become `7.5K` and `10B`, respectively.

## Quiet Startup

To run ASH without the header, run it with the `--quiet`/`-q` flag or with `--execute`/`-x "quiet_startup = true"`. Both of these override the value in `ash.cfg`.

Alternatively, you can set `quiet_startup` to `true` in `ash.cfg`.

## Changelog

The changelog can be found [here](CHANGELOG.md).

## License

This project is licensed under the MIT License.

## Notes

<sup>
1. Keep in mind that the colors of the customizable prompt depend on C#'s `System.ConsoleColor` enum. The values are not case-sensitive.
2. By functional, I mean "working".  
</sup>
