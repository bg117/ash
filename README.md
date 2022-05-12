# ASH

ASH stands for Application shell. It can be used to run applications (obviously).

## Features

ASH boasts a number of features, including:

- Customizable prompt (with colors<sub>1</sub> and all)
- A functional<sub>2</sub> scripting language
- Some powerful built-in commands

And more!

## Usage

These flags modify how ASH starts up.

- `-q`: quiet startup (discussed below in `## Quiet Startup`)
- `-ex <commands>`: runs `<commands>` before startup, and before parsing `.apcrc`
- `-v`: prints the version of ASH and exits.
- `-h`: prints the description of every flag, and exits.

## Customizable Prompt Format

To modify the prompt, you may change the value of the `prompt_fmt` variable, either in the `.apcrc` file in your home directory or assigning it directly in the shell (will get reset the next time you open the shell.)

Currently, there are 4 format specifiers to be used in customizing the prompt.

- `%u`: The user name.
- `%m`: The NetBIOS name of the machine.
- `%c`: The current directory.
- `%e`: The exit code of the last command executed.

There is another miscellaneous specifier: `%nl`. It is used to represent a new line in the prompt format. It is subject to change in a future version of the shell.

The default is `prompt_fmt = "%u:%m@%c ~% "`

It may display the following:

`user:machine@/some/directory ~%`

### Colors

You can customize the colors of the prompt elements.

- `prompt_fmt_u_color`: color of the `%u` specifier
- `prompt_fmt_m_color`: color of the `%m` specifier
- `prompt_fmt_c_color`: color of the `%c` specifier
- `prompt_fmt_e_fail_color`: color of the `%e` specifier when a command fails (returns something other than 0)
- `prompt_fmt_e_success_color`: color of the `%e` specifier when a command succeeds (returns 0)

All of them can have the values `Black`, `DarkBlue`, `DarkGreen`, `DarkCyan`, `DarkRed`, `DarkMagenta`, `DarkYellow`, `Gray`, `DarkGray`, `Blue`, `Green`, `Cyan`, `Red`, `Magenta`, `Yellow`, `White`. The values are case-**in**sensitive.

## Quiet Startup

To run ASH without the header, run it with the `-q` flag OR with `-ex "quiet_startup = true"`. Both of these override the value in `.apcrc`.

Alternatively, you can set `quiet_startup` to `true` in `.apcrc`.

## Built-in Commands

<!-- TODO: Describe built-in commands. -->

## Changelog

The changelog can be found [here](CHANGELOG.md).

## License

This project is licensed under the MIT License.

## Notes

<sup>

1. Keep in mind that the colors of the customizable prompt depend on C#'s `System.ConsoleColor` enum. The values are not case-sensitive.  
2. By functional, I mean "working".  

</sup>
