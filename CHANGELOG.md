# ASH Changelog

## Latest: version 1.4.1

### New features

- Added a better command-line parser
- You can now specify short options together (such as `ls -al`), separate long options with an equals sign or a colon (`ash --execute:date`), and specify a short option and its argument together (`ash -xls`).

## version 1.4.0

### New and changed features

- Changed the name of some built-in commands so as to not conflict with existing executables of the same name.
- Implemented an actual lexer and parser for more efficient execution of the shell language.
- Added a pipe operator.
- `echo` now removes quotes due to the lexer removing them early on. The quotes may be escaped though.
- Added escape sequences (only popular ones like `\n`, `\t`, and `\r` for now).
- Added the logical OR operator (short-circuit).
- Added the ability to surround expressions with parentheses for higher evaluation precedence.
- ASH will now not close when CTRL-C is pressed.

## version 1.3.0

This release includes some breaking changes.

### New and removed features

- Added multi-line commands.
- Added exit built-in command.
- Added long arguments (e.g. `--help`, `--quiet`, etc).
- Removed the `-run` argument.

### Bug fixes

- Fixed ASH trying to find the .exe extension on programs when running on OSes other than Windows.
- Fixed revision number getting displayed.
- Fixed grammar on help prompts.

## version 1.2.1

Minor update.

### New features

- Added support for macOS.

### Bug fixes

- Fixed PATH environment variable not working in Unix or Unix-like environments.

## version 1.2.0

### New features and changes

- Added `path` variable (separated by the pipe symbol `|`.)
- Added the ability to execute programs in `$path`.
- Added ability to execute shell scripts.
- Changed the default prompt and colors.
- Added 2 new command-line flags, `-c` and `-e`.

### Bug fixes

- Fixed `-q` getting ignored.

## version 1.1.0

Minor changes.

- Improved `help` command
- Added `ls` and `cd`
- Fixed spelling and grammar in `help` command

## version 1.0.0

First release of ASH (Application shell).
