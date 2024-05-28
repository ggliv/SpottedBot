# SpottedBot :telescope:

This repo contains the code for a Discord bot which manages games of spotted.

## What?
Spotted is a competitive game where you snap pictures of your friends when you see them out and about. Spotting someone nets you one point while getting spotted makes you lose one. It's particularly fun to play on a college campus.

## Why?
I'm not sure if spotted is an especially common game, but I've been playing it with friends at school and it's been pretty fun. The logistical side of things, though, usually is not. It's a bit of a pain to enforce rules, make sure people tag one another, and tally up the points at the end of the semester. I also wanted to be able to see things like statistics, leaderboards, and the like before the end of a season. Bots are good at that sort of thing.

## How (do I run this thing)?
SpottedBot is written in C#. If you're familiar with the `dotnet` toolchain already, you should be pretty much set to get the bot up and running. If not, the process is pretty simple.

Start by grabbing a copy of .NET. This project currently targets net8.0, so you can go [visit Microsoft's download page](https://dotnet.microsoft.com/en-us/download) and follow the directions they provide for your platform. You can also poke around other adjacent pages to get some advice on how to set up a nice dev environment in Visual Studio or VS Code. I'm using Jetbrains Rider on a student license right now, but any competent text editor should be good enough to get started with. These instructions will be targeting the the CLI tool `dotnet`.

To register your new bot on Discord's side, flip through [these instructions](https://docs.discordnet.dev/guides/getting_started/first-bot.html#creating-a-discord-bot). Copy the token that Discord gives you before leaving the page, otherwise you'll have to regenerate it.

SpottedBot will take configuration from two places: environment variables and a config JSON. Any environment variable prefixed with `SpottedBot_` will be read as a config value. For example, if you wanted to set the value `token` for this app, you'd add an environment variable that looks like `SpottedBot_token=[your_token_here]`. The JSON file included in this repo (`config.json`) sets values in a typical key-value pair fashion. A freshly cloned repo should contain a setting for `db_path`. Use that as an example.

I encourage you to set your token as an environment variable. Please be careful to avoid pushing files that expose your token, there are people that scan repos and snap those up pretty quick.

After you've set your token, you're ready to run the bot. Open a terminal, navigate to where this README is located on your disk, and execute `dotnet run`.
