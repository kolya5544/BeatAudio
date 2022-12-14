# BeatAudio
## Audio beat editing software.

BeatAudio is a software that allows you to change the configuration of beats in a song.

## Features

- Remove every Nth beat (in a group of K beats)
- Swap Nth beat with Kth beat (in a group of C beats)
- Double beats in a group of N beats
- Reverse beats in a group of N beats
- Real-time playback with metronome tick sound

You can create videos like "[song name] but every other beat is swapped", "[song name] but beats 3 and 4 are gone" etc using this software. It allows you to experience a song you've already heard, in a new light.

It also supports several transformations being applied, so there are no limits on your creativity!

## Installation and usage

You will need [.NET 6.0 Runtime](https://dotnet.microsoft.com/en-us/download) installed for this software to work. Download a binary file from [Releases](https://github.com/kolya5544/BeatAudio/releases) or compile the source code using `dotnet build`.

To run the binary file, on Windows you should use your terminal (for example, `cmd.exe`) and open `BeatAudio.exe`.

### Linux

Due to NAudio limitations, traditional methods of launching a .NET Core program **will not work**. Currently a known and somewhat working way to make BeatAudio work is to compile it using [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) with `dotnet publish -c Release --self-contained true --runtime win10-x64`, then run `wine BeatAudio.exe` in the generated folder.

From there you'll be able to access the interface of the program. It should be self-explanatory.

## Contributions

We don't accept any major contributions.

## Documentation

Every action contains a short usage guide/tutorial. This should allow you to understand the logic behind transformations applied. Additionally, a YT video tutorial will soon come out...